USE [Atomos];
GO

SET IDENTITY_INSERT dbo.ChuongHoc ON;

MERGE dbo.ChuongHoc AS target
USING (VALUES
    (1, N'Phản ứng hóa học', N'Sử dụng an toàn hóa chất và thiết bị phòng thí nghiệm; các khái niệm về phản ứng, mol, dung dịch, phương trình hóa học, tốc độ phản ứng và chất xúc tác.', 1),
    (2, N'Một số hợp chất thông dụng', N'Acid, base, thang pH, oxide, muối và phân bón hóa học.', 2)
) AS source (IDChuong, TenChuong, MoTa, ThuTu)
ON target.IDChuong = source.IDChuong
WHEN MATCHED THEN UPDATE SET
    TenChuong = source.TenChuong,
    MoTa = source.MoTa,
    ThuTu = source.ThuTu
WHEN NOT MATCHED THEN
    INSERT (IDChuong, TenChuong, MoTa, ThuTu)
    VALUES (source.IDChuong, source.TenChuong, source.MoTa, source.ThuTu);

SET IDENTITY_INSERT dbo.ChuongHoc OFF;
GO
