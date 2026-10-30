using System.Buffers.Binary;

namespace Atomos.API.Security;

public static class ImageUploadValidator
{
    private const int MaxFileBytes = 5_000_000;
    private const int MaxDimension = 8_192;
    private const long MaxPixels = 40_000_000;

    public static async Task<string?> GetVerifiedExtensionAsync(
        IFormFile file,
        bool allowGif,
        CancellationToken cancellationToken)
    {
        if (file.Length is <= 0 or > MaxFileBytes || file.Length > int.MaxValue) return null;

        await using var source = file.OpenReadStream();
        using var buffer = new MemoryStream((int)file.Length);
        await source.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length != file.Length) return null;

        var detected = DetectAndValidate(buffer.GetBuffer().AsSpan(0, (int)buffer.Length), allowGif);
        return detected is not null && MatchesContentType(detected, file.ContentType) ? detected : null;
    }

    private static string? DetectAndValidate(ReadOnlySpan<byte> bytes, bool allowGif)
    {
        if (IsPng(bytes)) return ".png";
        if (IsJpeg(bytes)) return ".jpg";
        if (IsWebP(bytes)) return ".webp";
        if (allowGif && IsGif(bytes)) return ".gif";
        return null;
    }

    private static bool IsPng(ReadOnlySpan<byte> bytes)
    {
        ReadOnlySpan<byte> signature = [0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a];
        if (bytes.Length < 45 || !bytes[..8].SequenceEqual(signature)) return false;

        var offset = 8;
        var sawHeader = false;
        var sawData = false;
        while (offset <= bytes.Length - 12)
        {
            var lengthValue = BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(offset, 4));
            if (lengthValue > int.MaxValue) return false;
            var length = (int)lengthValue;
            var chunkEnd = (long)offset + 12 + length;
            if (chunkEnd > bytes.Length) return false;

            var type = bytes.Slice(offset + 4, 4);
            var data = bytes.Slice(offset + 8, length);
            var expectedCrc = BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(offset + 8 + length, 4));
            if (ComputeCrc32(bytes.Slice(offset + 4, 4 + length)) != expectedCrc) return false;

            if (type.SequenceEqual("IHDR"u8))
            {
                if (sawHeader || offset != 8 || length != 13) return false;
                var width = BinaryPrimitives.ReadUInt32BigEndian(data[..4]);
                var height = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(4, 4));
                if (!ValidDimensions(width, height) || data[10] != 0 || data[11] != 0 || data[12] > 1) return false;
                sawHeader = true;
            }
            else if (type.SequenceEqual("IDAT"u8))
            {
                if (!sawHeader || length == 0) return false;
                sawData = true;
            }
            else if (type.SequenceEqual("IEND"u8))
            {
                return sawHeader && sawData && length == 0 && chunkEnd == bytes.Length;
            }

            offset = (int)chunkEnd;
        }

        return false;
    }

    private static bool IsJpeg(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 20 || bytes[0] != 0xff || bytes[1] != 0xd8) return false;
        var offset = 2;
        var sawFrame = false;
        var sawScan = false;

        while (offset < bytes.Length)
        {
            if (bytes[offset++] != 0xff) return false;
            while (offset < bytes.Length && bytes[offset] == 0xff) offset++;
            if (offset >= bytes.Length) return false;
            var marker = bytes[offset++];

            if (marker == 0xd9) return sawFrame && sawScan && offset == bytes.Length;
            if (marker is 0x00 or 0xd8 || marker is >= 0xd0 and <= 0xd7 || marker == 0x01) return false;
            if (offset > bytes.Length - 2) return false;
            var segmentLength = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset, 2));
            if (segmentLength < 2 || offset + segmentLength > bytes.Length) return false;

            if (IsStartOfFrame(marker))
            {
                if (segmentLength < 8) return false;
                var height = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset + 3, 2));
                var width = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset + 5, 2));
                if (!ValidDimensions(width, height)) return false;
                sawFrame = true;
            }

            offset += segmentLength;
            if (marker != 0xda) continue;
            sawScan = true;
            while (offset < bytes.Length)
            {
                if (bytes[offset++] != 0xff) continue;
                if (offset >= bytes.Length) return false;
                var scanMarker = bytes[offset];
                if (scanMarker == 0x00 || scanMarker is >= 0xd0 and <= 0xd7)
                {
                    offset++;
                    continue;
                }
                offset--;
                break;
            }
        }

        return false;
    }

    private static bool IsGif(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 14 ||
            (!bytes[..6].SequenceEqual("GIF87a"u8) && !bytes[..6].SequenceEqual("GIF89a"u8))) return false;
        var width = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(6, 2));
        var height = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(8, 2));
        if (!ValidDimensions(width, height)) return false;

        var offset = 13;
        if ((bytes[10] & 0x80) != 0)
        {
            offset += 3 * (1 << ((bytes[10] & 0x07) + 1));
            if (offset > bytes.Length) return false;
        }

        var sawImage = false;
        while (offset < bytes.Length)
        {
            var marker = bytes[offset++];
            if (marker == 0x3b) return sawImage && offset == bytes.Length;
            if (marker == 0x21)
            {
                if (offset >= bytes.Length) return false;
                offset++;
                if (!SkipGifSubBlocks(bytes, ref offset)) return false;
                continue;
            }
            if (marker != 0x2c || offset > bytes.Length - 9) return false;
            var imageWidth = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(offset + 4, 2));
            var imageHeight = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(offset + 6, 2));
            if (!ValidDimensions(imageWidth, imageHeight)) return false;
            sawImage = true;
            var packed = bytes[offset + 8];
            offset += 9;
            if ((packed & 0x80) != 0)
            {
                offset += 3 * (1 << ((packed & 0x07) + 1));
                if (offset > bytes.Length) return false;
            }
            if (offset >= bytes.Length || bytes[offset++] is < 2 or > 12) return false;
            if (!SkipGifSubBlocks(bytes, ref offset)) return false;
        }

        return false;
    }

    private static bool IsWebP(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 30 || !bytes[..4].SequenceEqual("RIFF"u8) || !bytes.Slice(8, 4).SequenceEqual("WEBP"u8)) return false;
        var declaredLength = BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(4, 4));
        if ((long)declaredLength + 8 != bytes.Length) return false;

        var offset = 12;
        var sawImage = false;
        while (offset <= bytes.Length - 8)
        {
            var type = bytes.Slice(offset, 4);
            var lengthValue = BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(offset + 4, 4));
            if (lengthValue > int.MaxValue) return false;
            var length = (int)lengthValue;
            var dataStart = offset + 8;
            var chunkEnd = (long)dataStart + length + (length & 1);
            if (chunkEnd > bytes.Length) return false;
            var data = bytes.Slice(dataStart, length);

            if (type.SequenceEqual("VP8X"u8))
            {
                if (length < 10) return false;
                var width = 1u + ReadUInt24LittleEndian(data.Slice(4, 3));
                var height = 1u + ReadUInt24LittleEndian(data.Slice(7, 3));
                if (!ValidDimensions(width, height)) return false;
            }
            else if (type.SequenceEqual("VP8L"u8))
            {
                if (length < 5 || data[0] != 0x2f) return false;
                var bits = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(1, 4));
                var width = (bits & 0x3fff) + 1;
                var height = ((bits >> 14) & 0x3fff) + 1;
                if (!ValidDimensions(width, height)) return false;
                sawImage = true;
            }
            else if (type.SequenceEqual("VP8 "u8))
            {
                if (length < 10 || !data.Slice(3, 3).SequenceEqual(new byte[] { 0x9d, 0x01, 0x2a })) return false;
                var width = (uint)(BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(6, 2)) & 0x3fff);
                var height = (uint)(BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(8, 2)) & 0x3fff);
                if (!ValidDimensions(width, height)) return false;
                sawImage = true;
            }

            offset = (int)chunkEnd;
        }

        return sawImage && offset == bytes.Length;
    }

    private static bool SkipGifSubBlocks(ReadOnlySpan<byte> bytes, ref int offset)
    {
        while (offset < bytes.Length)
        {
            var length = bytes[offset++];
            if (length == 0) return true;
            offset += length;
            if (offset > bytes.Length) return false;
        }
        return false;
    }

    private static bool IsStartOfFrame(byte marker) =>
        marker is >= 0xc0 and <= 0xcf && marker is not (0xc4 or 0xc8 or 0xcc);

    private static bool ValidDimensions(uint width, uint height) =>
        width is > 0 and <= MaxDimension && height is > 0 and <= MaxDimension && (long)width * height <= MaxPixels;

    private static uint ReadUInt24LittleEndian(ReadOnlySpan<byte> bytes) =>
        (uint)(bytes[0] | bytes[1] << 8 | bytes[2] << 16);

    private static uint ComputeCrc32(ReadOnlySpan<byte> bytes)
    {
        var crc = uint.MaxValue;
        foreach (var value in bytes)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
            {
                crc = (crc >> 1) ^ ((crc & 1) == 0 ? 0u : 0xedb88320u);
            }
        }
        return ~crc;
    }

    private static bool MatchesContentType(string extension, string contentType) => extension switch
    {
        ".jpg" => contentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase),
        ".png" => contentType.Equals("image/png", StringComparison.OrdinalIgnoreCase),
        ".webp" => contentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase),
        ".gif" => contentType.Equals("image/gif", StringComparison.OrdinalIgnoreCase),
        _ => false
    };
}
