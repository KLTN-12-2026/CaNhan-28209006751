using Atomos.Application.Lookup;
using Atomos.Infrastructure.Persistence;
using Dapper;

namespace Atomos.Infrastructure.Queries;

public sealed class LookupQueries(ISqlConnectionFactory connectionFactory) : ILookupQueries
{
    public async Task<List<ElementResponse>> GetElementsAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                IDNguyenTo AS Id,
                SoHieuNguyenTu AS AtomicNumber,
                KyHieu AS Symbol,
                TenNguyenTo AS Name,
                TenTiengAnh AS EnglishName,
                KhoiLuongNguyenTu AS AtomicMass,
                ChuKy AS Period,
                Nhom AS [Group],
                Khoi AS Block,
                LoaiNguyenTo AS Type,
                TrangThaiODieuKienThuong AS State,
                MauSac AS Color,
                DoAmDien AS Electronegativity,
                CauHinhElectron AS ElectronConfiguration,
                MoTa AS Description,
                UngDung AS Applications
            FROM dbo.BangTuanHoan
            ORDER BY SoHieuNguyenTu;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<ElementResponse>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<List<ChemicalResponse>> GetChemicalsAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                IDHoaChat AS Id,
                TenChat AS Name,
                CongThuc AS Formula,
                LoaiChat AS Type,
                KhoiLuongMol AS MolarMass,
                TrangThai AS State,
                MauSac AS Color,
                DoPHMacDinh AS DefaultPh,
                NongDoMacDinh AS DefaultConcentration,
                QuyTac_DoTan AS Solubility,
                TinhChat AS Properties,
                UngDung AS Applications,
                MoTa AS Description,
                QuyTac_HienTuong AS Phenomenon
            FROM dbo.HoaChat
            ORDER BY LoaiChat, TenChat;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<ChemicalResponse>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToList();
    }
}
