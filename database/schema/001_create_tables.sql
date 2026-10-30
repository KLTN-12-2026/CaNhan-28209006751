USE [Atomos];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

CREATE TABLE dbo.[Role] (
    IDRole int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Role PRIMARY KEY,
    TenRole nvarchar(30) NOT NULL,
    CONSTRAINT UQ_Role_TenRole UNIQUE (TenRole)
);
GO

CREATE TABLE dbo.NguoiDung (
    IDUser int IDENTITY(1,1) NOT NULL CONSTRAINT PK_NguoiDung PRIMARY KEY,
    IDRole int NOT NULL,
    Username nvarchar(50) NOT NULL,
    Email nvarchar(100) NOT NULL,
    MatKhau nvarchar(500) NULL,
    MatKhauTruocDo nvarchar(500) NULL,
    IDGoogle nvarchar(128) NULL,
    HoTen nvarchar(100) NULL,
    Avatar nvarchar(max) NULL,
    VIPHetHan datetime2 NULL,
    DaKhoa bit NOT NULL CONSTRAINT DF_NguoiDung_DaKhoa DEFAULT (0),
    DaXoa bit NOT NULL CONSTRAINT DF_NguoiDung_DaXoa DEFAULT (0),
    NgayTao datetime2 NOT NULL CONSTRAINT DF_NguoiDung_NgayTao DEFAULT (SYSUTCDATETIME()),
    NgayCapNhat datetime2 NOT NULL CONSTRAINT DF_NguoiDung_NgayCapNhat DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_NguoiDung_Role FOREIGN KEY (IDRole) REFERENCES dbo.[Role](IDRole),
    CONSTRAINT UQ_NguoiDung_Username UNIQUE (Username),
    CONSTRAINT UQ_NguoiDung_Email UNIQUE (Email)
);
GO

CREATE UNIQUE INDEX UX_NguoiDung_IDGoogle
    ON dbo.NguoiDung(IDGoogle)
    WHERE IDGoogle IS NOT NULL;
CREATE INDEX IX_NguoiDung_IDRole ON dbo.NguoiDung(IDRole);
GO

CREATE TABLE dbo.NhatKyQuanTri (
    IDAuditLog bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_NhatKyQuanTri PRIMARY KEY,
    IDAdmin int NOT NULL,
    HanhDong nvarchar(50) NOT NULL,
    LoaiDoiTuong nvarchar(50) NOT NULL,
    IDDoiTuong int NULL,
    ChiTiet nvarchar(500) NULL,
    NgayTao datetime2 NOT NULL CONSTRAINT DF_NhatKyQuanTri_NgayTao DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_NhatKyQuanTri_NguoiDung FOREIGN KEY (IDAdmin) REFERENCES dbo.NguoiDung(IDUser)
);
GO

CREATE INDEX IX_NhatKyQuanTri_NgayTao_HanhDong ON dbo.NhatKyQuanTri(NgayTao, HanhDong);
GO

CREATE TABLE dbo.NhatKyHoatDong (
    IDNhatKyHoatDong bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_NhatKyHoatDong PRIMARY KEY,
    IDUser int NULL,
    LoaiSuKien nvarchar(50) NOT NULL,
    XayRaLuc datetime2 NOT NULL CONSTRAINT DF_NhatKyHoatDong_XayRaLuc DEFAULT (SYSUTCDATETIME()),
    LoaiDoiTuong nvarchar(50) NULL,
    IDDoiTuong int NULL,
    MetadataJson nvarchar(max) NULL,
    CONSTRAINT FK_NhatKyHoatDong_NguoiDung FOREIGN KEY (IDUser) REFERENCES dbo.NguoiDung(IDUser) ON DELETE SET NULL
);
GO

CREATE INDEX IX_NhatKyHoatDong_XayRaLuc_LoaiSuKien ON dbo.NhatKyHoatDong(XayRaLuc, LoaiSuKien);
CREATE INDEX IX_NhatKyHoatDong_IDUser_XayRaLuc ON dbo.NhatKyHoatDong(IDUser, XayRaLuc);
CREATE INDEX IX_NhatKyHoatDong_DoiTuong ON dbo.NhatKyHoatDong(LoaiDoiTuong, IDDoiTuong);
GO

CREATE TABLE dbo.GioiHanSuDung (
    IDUser int NOT NULL CONSTRAINT PK_GioiHanSuDung PRIMARY KEY,
    SandboxCuaSoBatDauLuc datetime2 NULL,
    SandboxSoGiayDaDung int NOT NULL CONSTRAINT DF_GioiHanSuDung_SandboxSoGiayDaDung DEFAULT (0),
    SandboxHeartbeatLuc datetime2 NULL,
    SandboxDangHoatDong bit NOT NULL CONSTRAINT DF_GioiHanSuDung_SandboxDangHoatDong DEFAULT (0),
    CONSTRAINT FK_GioiHanSuDung_NguoiDung FOREIGN KEY (IDUser) REFERENCES dbo.NguoiDung(IDUser) ON DELETE CASCADE,
    CONSTRAINT CK_GioiHanSuDung_SandboxSoGiayDaDung CHECK (SandboxSoGiayDaDung BETWEEN 0 AND 600)
);
GO

CREATE TABLE dbo.CauHinhThanhToan (
    IDCauHinhThanhToan int IDENTITY(1,1) NOT NULL CONSTRAINT PK_CauHinhThanhToan PRIMARY KEY,
    NhaCungCap nvarchar(50) NOT NULL,
    GiaVip decimal(18,2) NOT NULL,
    TienTe nvarchar(3) NOT NULL CONSTRAINT DF_CauHinhThanhToan_TienTe DEFAULT (N'VND'),
    TenNguoiNhan nvarchar(150) NULL,
    TaiKhoanNhan nvarchar(100) NULL,
    NganHangNhan nvarchar(150) NULL,
    MaNganHang nvarchar(20) NULL,
    MaMerchant nvarchar(100) NULL,
    DangHoatDong bit NOT NULL CONSTRAINT DF_CauHinhThanhToan_DangHoatDong DEFAULT (1),
    TaoLuc datetime2 NOT NULL CONSTRAINT DF_CauHinhThanhToan_TaoLuc DEFAULT (SYSUTCDATETIME()),
    CapNhatLuc datetime2 NOT NULL CONSTRAINT DF_CauHinhThanhToan_CapNhatLuc DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT CK_CauHinhThanhToan_GiaVip CHECK (GiaVip > 0)
);
GO

CREATE INDEX IX_CauHinhThanhToan_NhaCungCap ON dbo.CauHinhThanhToan(NhaCungCap);
GO

CREATE TABLE dbo.DonThanhToan (
    IDDonThanhToan int IDENTITY(1,1) NOT NULL CONSTRAINT PK_DonThanhToan PRIMARY KEY,
    IDUser int NOT NULL,
    MaDonHang nvarchar(40) NOT NULL,
    SoTien decimal(18,2) NOT NULL,
    TienTe nvarchar(3) NOT NULL,
    TrangThai nvarchar(20) NOT NULL,
    NhaCungCap nvarchar(50) NOT NULL,
    MoTa nvarchar(250) NOT NULL,
    MaDonHangNhaCungCap nvarchar(100) NULL,
    MaThanhToanNhaCungCap nvarchar(100) NULL,
    MaGiaoDichNhaCungCap nvarchar(100) NULL,
    CheckoutUrl nvarchar(500) NULL,
    QrPayload nvarchar(max) NULL,
    NoiDungChuyenKhoan nvarchar(250) NULL,
    TaoLuc datetime2 NOT NULL,
    HetHanLuc datetime2 NOT NULL,
    ThanhToanLuc datetime2 NULL,
    XacNhanLuc datetime2 NULL,
    MetadataJson nvarchar(max) NULL,
    CONSTRAINT FK_DonThanhToan_NguoiDung FOREIGN KEY (IDUser) REFERENCES dbo.NguoiDung(IDUser),
    CONSTRAINT UX_DonThanhToan_MaDonHang UNIQUE (MaDonHang),
    CONSTRAINT CK_DonThanhToan_SoTien CHECK (SoTien > 0)
);
GO

CREATE INDEX IX_DonThanhToan_IDUser_TaoLuc ON dbo.DonThanhToan(IDUser, TaoLuc);
CREATE INDEX IX_DonThanhToan_TrangThai_TaoLuc ON dbo.DonThanhToan(TrangThai, TaoLuc);
GO

CREATE TABLE dbo.GiaoDichThanhToan (
    IDGiaoDichThanhToan bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_GiaoDichThanhToan PRIMARY KEY,
    IDDonThanhToan int NOT NULL,
    NhaCungCap nvarchar(50) NOT NULL,
    MaGiaoDichNhaCungCap nvarchar(100) NOT NULL,
    SoTien decimal(18,2) NOT NULL,
    TienTe nvarchar(3) NOT NULL,
    TrangThai nvarchar(20) NOT NULL,
    ThoiGianGiaoDich datetime2 NOT NULL,
    ThamChieuRaw nvarchar(500) NULL,
    MetadataJson nvarchar(max) NULL,
    TaoLuc datetime2 NOT NULL,
    XacNhanLuc datetime2 NULL,
    IDXacNhanBoiAdmin int NULL,
    CONSTRAINT FK_GiaoDichThanhToan_DonThanhToan FOREIGN KEY (IDDonThanhToan) REFERENCES dbo.DonThanhToan(IDDonThanhToan) ON DELETE CASCADE,
    CONSTRAINT FK_GiaoDichThanhToan_Admin FOREIGN KEY (IDXacNhanBoiAdmin) REFERENCES dbo.NguoiDung(IDUser),
    CONSTRAINT UX_GiaoDichThanhToan_NhaCungCap_MaGiaoDich UNIQUE (NhaCungCap, MaGiaoDichNhaCungCap),
    CONSTRAINT CK_GiaoDichThanhToan_SoTien CHECK (SoTien > 0)
);
GO

CREATE INDEX IX_GiaoDichThanhToan_TrangThai_ThoiGian ON dbo.GiaoDichThanhToan(TrangThai, ThoiGianGiaoDich);
CREATE INDEX IX_GiaoDichThanhToan_IDDon_TaoLuc ON dbo.GiaoDichThanhToan(IDDonThanhToan, TaoLuc);
CREATE UNIQUE INDEX UX_GiaoDichThanhToan_ConfirmedOrder
    ON dbo.GiaoDichThanhToan(IDDonThanhToan)
    WHERE TrangThai = N'Confirmed';
GO

CREATE TABLE dbo.PasswordResetTokens (
    IDReset int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PasswordResetTokens PRIMARY KEY,
    IDUser int NOT NULL,
    Token nvarchar(64) NOT NULL,
    HetHan datetime2 NOT NULL,
    DaSuDung bit NOT NULL CONSTRAINT DF_PasswordResetTokens_DaSuDung DEFAULT (0),
    FailedAttempts int NOT NULL CONSTRAINT DF_PasswordResetTokens_FailedAttempts DEFAULT (0),
    NgayTao datetime2 NOT NULL CONSTRAINT DF_PasswordResetTokens_NgayTao DEFAULT (SYSUTCDATETIME()),
    IpAddress nvarchar(45) NULL,
    UserAgent nvarchar(512) NULL,
    CONSTRAINT FK_PasswordResetTokens_NguoiDung FOREIGN KEY (IDUser) REFERENCES dbo.NguoiDung(IDUser) ON DELETE CASCADE,
    CONSTRAINT UQ_PasswordResetTokens_Token UNIQUE (Token)
);
GO

CREATE INDEX IX_PasswordResetTokens_IDUser ON dbo.PasswordResetTokens(IDUser);
CREATE INDEX IX_PasswordResetTokens_NgayTao ON dbo.PasswordResetTokens(NgayTao);
GO

CREATE TABLE dbo.AuthSessions (
    IDAuthSession bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuthSessions PRIMARY KEY,
    IDUser int NOT NULL,
    SessionSecretHash binary(32) NOT NULL,
    AuthMethod nvarchar(20) NOT NULL,
    CreatedAt datetime2(7) NOT NULL,
    ExpiresAt datetime2(7) NOT NULL,
    RevokedAt datetime2(7) NULL,
    RevokeReason nvarchar(200) NULL,
    CONSTRAINT FK_AuthSessions_NguoiDung FOREIGN KEY (IDUser)
        REFERENCES dbo.NguoiDung(IDUser) ON DELETE CASCADE,
    CONSTRAINT CK_AuthSessions_AuthMethod CHECK (
        AuthMethod COLLATE Latin1_General_100_BIN2 IN (N'Password', N'Google')),
    CONSTRAINT CK_AuthSessions_ExpiresAt CHECK (ExpiresAt > CreatedAt)
);
GO

CREATE UNIQUE INDEX UX_AuthSessions_SessionSecretHash
    ON dbo.AuthSessions(SessionSecretHash);
CREATE INDEX IX_AuthSessions_IDUser_RevokedAt_ExpiresAt
    ON dbo.AuthSessions(IDUser, RevokedAt, ExpiresAt);
GO

CREATE TABLE dbo.ChuongHoc (
    IDChuong int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ChuongHoc PRIMARY KEY,
    TenChuong nvarchar(150) NOT NULL,
    MoTa nvarchar(max) NULL,
    ThuTu int NOT NULL,
    CONSTRAINT CK_ChuongHoc_ThuTu CHECK (ThuTu > 0),
    CONSTRAINT UQ_ChuongHoc_ThuTu UNIQUE (ThuTu)
);
GO

CREATE TABLE dbo.BaiHoc (
    IDBaiHoc int IDENTITY(1,1) NOT NULL CONSTRAINT PK_BaiHoc PRIMARY KEY,
    IDChuong int NOT NULL,
    TenBaiHoc nvarchar(150) NOT NULL,
    NoiDung nvarchar(max) NULL,
    MinhHoaPhanUngJson nvarchar(max) NULL,
    ThuTu int NOT NULL,
    CONSTRAINT FK_BaiHoc_ChuongHoc FOREIGN KEY (IDChuong) REFERENCES dbo.ChuongHoc(IDChuong) ON DELETE CASCADE,
    CONSTRAINT CK_BaiHoc_ThuTu CHECK (ThuTu > 0),
    CONSTRAINT CK_BaiHoc_MinhHoaPhanUngJson CHECK (MinhHoaPhanUngJson IS NULL OR ISJSON(MinhHoaPhanUngJson) = 1),
    CONSTRAINT UQ_BaiHoc_Chuong_ThuTu UNIQUE (IDChuong, ThuTu)
);
GO

CREATE INDEX IX_BaiHoc_IDChuong ON dbo.BaiHoc(IDChuong);
GO

CREATE TABLE dbo.GhiChu (
    IDGhiChu int IDENTITY(1,1) NOT NULL CONSTRAINT PK_GhiChu PRIMARY KEY,
    IDUser int NOT NULL,
    IDBaiHoc int NULL,
    TieuDe nvarchar(150) NULL,
    NoiDung nvarchar(max) NOT NULL CONSTRAINT DF_GhiChu_NoiDung DEFAULT (N''),
    NgayTao datetime2 NOT NULL CONSTRAINT DF_GhiChu_NgayTao DEFAULT (SYSUTCDATETIME()),
    NgayCapNhat datetime2 NOT NULL CONSTRAINT DF_GhiChu_NgayCapNhat DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_GhiChu_NguoiDung FOREIGN KEY (IDUser) REFERENCES dbo.NguoiDung(IDUser) ON DELETE CASCADE,
    CONSTRAINT FK_GhiChu_BaiHoc FOREIGN KEY (IDBaiHoc) REFERENCES dbo.BaiHoc(IDBaiHoc) ON DELETE SET NULL
);
GO

CREATE INDEX IX_GhiChu_IDUser_NgayCapNhat ON dbo.GhiChu(IDUser, NgayCapNhat DESC);
GO

CREATE TABLE dbo.TienDoHocTap (
    IDUser int NOT NULL,
    IDBaiHoc int NOT NULL,
    DaHoanThanh bit NOT NULL CONSTRAINT DF_TienDoHocTap_DaHoanThanh DEFAULT (0),
    LanHocGanNhat datetime2 NULL,
    PhanTramHoanThanh tinyint NOT NULL CONSTRAINT DF_TienDoHocTap_PhanTramHoanThanh DEFAULT (0),
    CONSTRAINT PK_TienDoHocTap PRIMARY KEY (IDUser, IDBaiHoc),
    CONSTRAINT FK_TienDoHocTap_NguoiDung FOREIGN KEY (IDUser) REFERENCES dbo.NguoiDung(IDUser) ON DELETE CASCADE,
    CONSTRAINT FK_TienDoHocTap_BaiHoc FOREIGN KEY (IDBaiHoc) REFERENCES dbo.BaiHoc(IDBaiHoc),
    CONSTRAINT CK_TienDoHocTap_PhanTram CHECK (PhanTramHoanThanh BETWEEN 0 AND 100)
);
GO

CREATE TABLE dbo.HoSoHocTap (
    IDUser int NOT NULL CONSTRAINT PK_HoSoHocTap PRIMARY KEY,
    ChuoiHienTai int NOT NULL CONSTRAINT DF_HoSoHocTap_ChuoiHienTai DEFAULT (0),
    ChuoiCaoNhat int NOT NULL CONSTRAINT DF_HoSoHocTap_ChuoiCaoNhat DEFAULT (0),
    NgayHoanThanhOnTapCuoi date NULL,
    CONSTRAINT FK_HoSoHocTap_NguoiDung FOREIGN KEY (IDUser) REFERENCES dbo.NguoiDung(IDUser) ON DELETE CASCADE,
    CONSTRAINT CK_HoSoHocTap_Chuoi CHECK (ChuoiHienTai >= 0 AND ChuoiCaoNhat >= 0)
);
GO

CREATE TABLE dbo.DeBaiTap (
    IDDeBaiTap int IDENTITY(1,1) NOT NULL CONSTRAINT PK_DeBaiTap PRIMARY KEY,
    IDBaiHoc int NOT NULL,
    TieuDe nvarchar(200) NOT NULL,
    MoTa nvarchar(max) NULL,
    ThuTu int NOT NULL CONSTRAINT DF_DeBaiTap_ThuTu DEFAULT (1),
    DangHoatDong bit NOT NULL CONSTRAINT DF_DeBaiTap_DangHoatDong DEFAULT (1),
    NgayCapNhat datetime2 NOT NULL CONSTRAINT DF_DeBaiTap_NgayCapNhat DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_DeBaiTap_BaiHoc FOREIGN KEY (IDBaiHoc) REFERENCES dbo.BaiHoc(IDBaiHoc) ON DELETE CASCADE,
    CONSTRAINT CK_DeBaiTap_ThuTu CHECK (ThuTu > 0)
);
GO

CREATE UNIQUE INDEX UX_DeBaiTap_BaiHoc_ThuTu_Active
    ON dbo.DeBaiTap(IDBaiHoc, ThuTu)
    WHERE DangHoatDong = 1;
GO

CREATE TABLE dbo.BaiTap (
    IDBaiTap int IDENTITY(1,1) NOT NULL CONSTRAINT PK_BaiTap PRIMARY KEY,
    IDBaiHoc int NULL,
    LoaiBaiTap nvarchar(50) NOT NULL,
    CauHoi nvarchar(max) NOT NULL,
    DapAn nvarchar(max) NULL,
    LoiGiai nvarchar(max) NULL,
    GoiYHocLai nvarchar(max) NULL,
    DonViDapAn nvarchar(30) NULL,
    SaiSoChoPhep decimal(12,4) NULL,
    DangHoatDong bit NOT NULL CONSTRAINT DF_BaiTap_DangHoatDong DEFAULT (1),
    NgayCapNhat datetime2 NOT NULL CONSTRAINT DF_BaiTap_NgayCapNhat DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_BaiTap_BaiHoc FOREIGN KEY (IDBaiHoc) REFERENCES dbo.BaiHoc(IDBaiHoc),
    CONSTRAINT CK_BaiTap_LoaiBaiTap CHECK (LoaiBaiTap IN (N'MultipleChoice', N'NumericAnswer', N'BalanceEquation')),
    CONSTRAINT CK_BaiTap_SaiSo CHECK (SaiSoChoPhep IS NULL OR SaiSoChoPhep >= 0)
);
GO

CREATE INDEX IX_BaiTap_IDBaiHoc ON dbo.BaiTap(IDBaiHoc);
GO

CREATE TABLE dbo.ChiTietDeBaiTap (
    IDDeBaiTap int NOT NULL,
    IDBaiTap int NOT NULL,
    ThuTu int NOT NULL,
    DiemToiDa decimal(5,2) NOT NULL CONSTRAINT DF_ChiTietDeBaiTap_DiemToiDa DEFAULT (1),
    CONSTRAINT PK_ChiTietDeBaiTap PRIMARY KEY (IDDeBaiTap, IDBaiTap),
    CONSTRAINT FK_ChiTietDeBaiTap_DeBaiTap FOREIGN KEY (IDDeBaiTap) REFERENCES dbo.DeBaiTap(IDDeBaiTap) ON DELETE CASCADE,
    CONSTRAINT FK_ChiTietDeBaiTap_BaiTap FOREIGN KEY (IDBaiTap) REFERENCES dbo.BaiTap(IDBaiTap),
    CONSTRAINT CK_ChiTietDeBaiTap_ThuTu CHECK (ThuTu > 0),
    CONSTRAINT CK_ChiTietDeBaiTap_Diem CHECK (DiemToiDa > 0),
    CONSTRAINT UQ_ChiTietDeBaiTap_ThuTu UNIQUE (IDDeBaiTap, ThuTu)
);
GO

CREATE INDEX IX_ChiTietDeBaiTap_IDBaiTap ON dbo.ChiTietDeBaiTap(IDBaiTap);
GO

CREATE TABLE dbo.LuaChon (
    IDLuaChon int IDENTITY(1,1) NOT NULL CONSTRAINT PK_LuaChon PRIMARY KEY,
    IDBaiTap int NOT NULL,
    NoiDung nvarchar(max) NOT NULL,
    LaDapAn bit NOT NULL CONSTRAINT DF_LuaChon_LaDapAn DEFAULT (0),
    ThuTu int NOT NULL,
    CONSTRAINT FK_LuaChon_BaiTap FOREIGN KEY (IDBaiTap) REFERENCES dbo.BaiTap(IDBaiTap) ON DELETE CASCADE,
    CONSTRAINT CK_LuaChon_ThuTu CHECK (ThuTu > 0),
    CONSTRAINT UQ_LuaChon_BaiTap_ThuTu UNIQUE (IDBaiTap, ThuTu)
);
GO

CREATE TABLE dbo.BaiKiemTra (
    IDBaiKiemTra int IDENTITY(1,1) NOT NULL CONSTRAINT PK_BaiKiemTra PRIMARY KEY,
    IDChuong int NULL,
    IDBaiHoc int NULL,
    TenBaiKiemTra nvarchar(150) NOT NULL,
    MoTa nvarchar(max) NULL,
    ThoiLuongPhut int NOT NULL CONSTRAINT DF_BaiKiemTra_ThoiLuongPhut DEFAULT (30),
    DangHoatDong bit NOT NULL CONSTRAINT DF_BaiKiemTra_DangHoatDong DEFAULT (1),
    NgayTao datetime2 NOT NULL CONSTRAINT DF_BaiKiemTra_NgayTao DEFAULT (SYSUTCDATETIME()),
    KieuChonCauHoi nvarchar(20) NOT NULL CONSTRAINT DF_BaiKiemTra_KieuChonCauHoi DEFAULT (N'Fixed'),
    SoCauHoi int NULL,
    CONSTRAINT FK_BaiKiemTra_ChuongHoc FOREIGN KEY (IDChuong) REFERENCES dbo.ChuongHoc(IDChuong) ON DELETE SET NULL,
    CONSTRAINT FK_BaiKiemTra_BaiHoc FOREIGN KEY (IDBaiHoc) REFERENCES dbo.BaiHoc(IDBaiHoc),
    CONSTRAINT CK_BaiKiemTra_ThoiLuong CHECK (ThoiLuongPhut > 0),
    CONSTRAINT CK_BaiKiemTra_KieuChonCauHoi CHECK (KieuChonCauHoi IN (N'Fixed', N'Random')),
    CONSTRAINT CK_BaiKiemTra_SoCauHoi CHECK (SoCauHoi IS NULL OR SoCauHoi > 0)
);
GO

CREATE TABLE dbo.ChiTietBaiKiemTra (
    IDBaiKiemTra int NOT NULL,
    IDBaiTap int NOT NULL,
    ThuTu int NOT NULL,
    DiemToiDa decimal(5,2) NOT NULL CONSTRAINT DF_ChiTietBaiKiemTra_DiemToiDa DEFAULT (1),
    CONSTRAINT PK_ChiTietBaiKiemTra PRIMARY KEY (IDBaiKiemTra, IDBaiTap),
    CONSTRAINT FK_ChiTietBaiKiemTra_BaiKiemTra FOREIGN KEY (IDBaiKiemTra) REFERENCES dbo.BaiKiemTra(IDBaiKiemTra) ON DELETE CASCADE,
    CONSTRAINT FK_ChiTietBaiKiemTra_BaiTap FOREIGN KEY (IDBaiTap) REFERENCES dbo.BaiTap(IDBaiTap),
    CONSTRAINT CK_ChiTietBaiKiemTra_ThuTu CHECK (ThuTu > 0),
    CONSTRAINT CK_ChiTietBaiKiemTra_Diem CHECK (DiemToiDa > 0),
    CONSTRAINT UQ_ChiTietBaiKiemTra_ThuTu UNIQUE (IDBaiKiemTra, ThuTu)
);
GO

CREATE INDEX IX_ChiTietBaiKiemTra_IDBaiTap ON dbo.ChiTietBaiKiemTra(IDBaiTap);
GO

CREATE TABLE dbo.LanLamBai (
    IDLanLam int IDENTITY(1,1) NOT NULL CONSTRAINT PK_LanLamBai PRIMARY KEY,
    IDUser int NOT NULL,
    IDBaiKiemTra int NULL,
    IDBaiHoc int NULL,
    IDDeBaiTap int NULL,
    LoaiLanLam nvarchar(20) NOT NULL,
    TrangThai nvarchar(20) NOT NULL CONSTRAINT DF_LanLamBai_TrangThai DEFAULT (N'Draft'),
    BatDauLuc datetime2 NOT NULL CONSTRAINT DF_LanLamBai_BatDauLuc DEFAULT (SYSUTCDATETIME()),
    NopLuc datetime2 NULL,
    ThoiLuongGiay int NULL,
    CapNhatLuc datetime2 NOT NULL CONSTRAINT DF_LanLamBai_CapNhatLuc DEFAULT (SYSUTCDATETIME()),
    HanNopLuc datetime2 NULL,
    TongDiem decimal(6,2) NULL,
    SoCauDung int NOT NULL CONSTRAINT DF_LanLamBai_SoCauDung DEFAULT (0),
    SoCauSai int NOT NULL CONSTRAINT DF_LanLamBai_SoCauSai DEFAULT (0),
    PhanTramDung decimal(5,2) NULL,
    CONSTRAINT FK_LanLamBai_NguoiDung FOREIGN KEY (IDUser) REFERENCES dbo.NguoiDung(IDUser) ON DELETE CASCADE,
    CONSTRAINT FK_LanLamBai_BaiKiemTra FOREIGN KEY (IDBaiKiemTra) REFERENCES dbo.BaiKiemTra(IDBaiKiemTra) ON DELETE SET NULL,
    CONSTRAINT FK_LanLamBai_BaiHoc FOREIGN KEY (IDBaiHoc) REFERENCES dbo.BaiHoc(IDBaiHoc),
    CONSTRAINT FK_LanLamBai_DeBaiTap FOREIGN KEY (IDDeBaiTap) REFERENCES dbo.DeBaiTap(IDDeBaiTap) ON DELETE SET NULL,
    CONSTRAINT CK_LanLamBai_Loai CHECK (LoaiLanLam IN (N'Practice', N'DailyReview', N'Test')),
    CONSTRAINT CK_LanLamBai_TrangThai CHECK (TrangThai IN (N'Draft', N'Submitted', N'Expired')),
    CONSTRAINT CK_LanLamBai_ThoiLuong CHECK (ThoiLuongGiay IS NULL OR ThoiLuongGiay >= 0),
    CONSTRAINT CK_LanLamBai_SoCau CHECK (SoCauDung >= 0 AND SoCauSai >= 0),
    CONSTRAINT CK_LanLamBai_PhanTramDung CHECK (PhanTramDung IS NULL OR PhanTramDung BETWEEN 0 AND 100)
);
GO

CREATE INDEX IX_LanLamBai_IDUser ON dbo.LanLamBai(IDUser);
GO

CREATE INDEX IX_LanLamBai_IDDeBaiTap ON dbo.LanLamBai(IDDeBaiTap);
CREATE INDEX IX_LanLamBai_Dashboard ON dbo.LanLamBai(TrangThai, LoaiLanLam, NopLuc);
GO

CREATE TABLE dbo.ChiTietLanLamBai (
    IDLanLam int NOT NULL,
    IDBaiTap int NOT NULL,
    CauTraLoi nvarchar(max) NULL,
    Diem decimal(5,2) NOT NULL CONSTRAINT DF_ChiTietLanLamBai_Diem DEFAULT (0),
    ThuTu int NOT NULL CONSTRAINT DF_ChiTietLanLamBai_ThuTu DEFAULT (1),
    LaDung bit NULL,
    DiemToiDa decimal(5,2) NOT NULL CONSTRAINT DF_ChiTietLanLamBai_DiemToiDa DEFAULT (1),
    DaXemGoiY bit NOT NULL CONSTRAINT DF_ChiTietLanLamBai_DaXemGoiY DEFAULT (0),
    DaXemLoiGiai bit NOT NULL CONSTRAINT DF_ChiTietLanLamBai_DaXemLoiGiai DEFAULT (0),
    CauHoiSnapshot nvarchar(max) NULL,
    DapAnDungSnapshot nvarchar(max) NULL,
    LoiGiaiSnapshot nvarchar(max) NULL,
    LuaChonSnapshot nvarchar(max) NULL,
    LoaiBaiTapSnapshot nvarchar(50) NULL,
    SaiSoChoPhepSnapshot decimal(18,6) NULL,
    CONSTRAINT PK_ChiTietLanLamBai PRIMARY KEY (IDLanLam, IDBaiTap),
    CONSTRAINT FK_ChiTietLanLamBai_LanLamBai FOREIGN KEY (IDLanLam) REFERENCES dbo.LanLamBai(IDLanLam) ON DELETE CASCADE,
    CONSTRAINT FK_ChiTietLanLamBai_BaiTap FOREIGN KEY (IDBaiTap) REFERENCES dbo.BaiTap(IDBaiTap),
    CONSTRAINT CK_ChiTietLanLamBai_ThuTu CHECK (ThuTu > 0),
    CONSTRAINT CK_ChiTietLanLamBai_Diem CHECK (Diem >= 0 AND DiemToiDa > 0)
);
GO

CREATE INDEX IX_ChiTietLanLamBai_IDBaiTap ON dbo.ChiTietLanLamBai(IDBaiTap);
GO

CREATE TABLE dbo.HoaChat (
    IDHoaChat int IDENTITY(1,1) NOT NULL CONSTRAINT PK_HoaChat PRIMARY KEY,
    TenChat nvarchar(100) NOT NULL,
    CongThuc nvarchar(50) NOT NULL,
    LoaiChat nvarchar(50) NULL,
    KhoiLuongMol decimal(10,4) NULL,
    TrangThai nvarchar(20) NULL,
    MauSac nvarchar(100) NULL,
    TinhChat nvarchar(max) NULL,
    UngDung nvarchar(max) NULL,
    MoTa nvarchar(max) NULL,
    DanhMucSandbox nvarchar(50) NULL,
    LuonHienThiSandbox bit NOT NULL CONSTRAINT DF_HoaChat_LuonHienThiSandbox DEFAULT (0),
    LaChatKhoiDau bit NOT NULL CONSTRAINT DF_HoaChat_LaChatKhoiDau DEFAULT (0),
    DoPHMacDinh decimal(4,2) NULL,
    NongDoMacDinh decimal(8,4) NULL,
    CoTheKeoTha bit NOT NULL CONSTRAINT DF_HoaChat_CoTheKeoTha DEFAULT (1),
    QuyTac_Cation nvarchar(20) NULL,
    QuyTac_DienTichCation int NULL,
    QuyTac_Anion nvarchar(20) NULL,
    QuyTac_DienTichAnion int NULL,
    QuyTac_SoHAcid int NULL,
    QuyTac_SoOHBazo int NULL,
    QuyTac_DoHoatDongKimLoai int NULL,
    QuyTac_DoTan nvarchar(30) NULL,
    QuyTac_HienTuong nvarchar(max) NULL,
    QuyTac_ChoPhep bit NOT NULL CONSTRAINT DF_HoaChat_QuyTac_ChoPhep DEFAULT (1),
    CONSTRAINT UQ_HoaChat_CongThuc UNIQUE (CongThuc),
    CONSTRAINT CK_HoaChat_DoPH CHECK (DoPHMacDinh IS NULL OR DoPHMacDinh BETWEEN 0 AND 14),
    CONSTRAINT CK_HoaChat_NongDo CHECK (NongDoMacDinh IS NULL OR NongDoMacDinh > 0)
);
GO

CREATE INDEX IX_HoaChat_DanhMucSandbox ON dbo.HoaChat(DanhMucSandbox);
CREATE INDEX IX_HoaChat_TenChat ON dbo.HoaChat(TenChat);
GO

CREATE TABLE dbo.KhoHoaChatNguoiDung (
    IDUser int NOT NULL,
    IDHoaChat int NOT NULL,
    NgayMoKhoa datetime2 NOT NULL CONSTRAINT DF_KhoHoaChatNguoiDung_NgayMoKhoa DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_KhoHoaChatNguoiDung PRIMARY KEY (IDUser, IDHoaChat),
    CONSTRAINT FK_KhoHoaChatNguoiDung_NguoiDung FOREIGN KEY (IDUser) REFERENCES dbo.NguoiDung(IDUser) ON DELETE CASCADE,
    CONSTRAINT FK_KhoHoaChatNguoiDung_HoaChat FOREIGN KEY (IDHoaChat) REFERENCES dbo.HoaChat(IDHoaChat) ON DELETE CASCADE
);
GO

CREATE INDEX IX_KhoHoaChatNguoiDung_IDHoaChat ON dbo.KhoHoaChatNguoiDung(IDHoaChat);
GO

CREATE TABLE dbo.ThuThachXuong (
    IDThuThach int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ThuThachXuong PRIMARY KEY,
    TenThuThach nvarchar(150) NOT NULL,
    MoTa nvarchar(max) NULL,
    DangHoatDong bit NOT NULL CONSTRAINT DF_ThuThachXuong_DangHoatDong DEFAULT (1),
    DoKho nvarchar(30) NOT NULL,
    NgayThuThach date NULL,
    SlotThuThach int NULL,
    Seed nvarchar(120) NULL,
    PhienBanGenerator nvarchar(40) NULL,
    MetadataJson nvarchar(max) NULL,
    NguonTao nvarchar(40) NOT NULL CONSTRAINT DF_ThuThachXuong_NguonTao DEFAULT (N'AdminLegacy'),
    ThoiGianGioiHanGiay int NOT NULL,
    IDHoaChatMucTieu int NOT NULL,
    LuongMucTieu decimal(18,4) NOT NULL,
    DonViMucTieu nvarchar(20) NOT NULL,
    DiemCoBan int NOT NULL CONSTRAINT DF_ThuThachXuong_DiemCoBan DEFAULT (100),
    DiemThuongSom int NOT NULL CONSTRAINT DF_ThuThachXuong_DiemThuongSom DEFAULT (0),
    NguongThuongSomGiay int NOT NULL CONSTRAINT DF_ThuThachXuong_NguongThuongSomGiay DEFAULT (0),
    DiemPhatTre int NOT NULL CONSTRAINT DF_ThuThachXuong_DiemPhatTre DEFAULT (0),
    NgayTao datetime2 NOT NULL CONSTRAINT DF_ThuThachXuong_NgayTao DEFAULT SYSUTCDATETIME(),
    NgayCapNhat datetime2 NOT NULL CONSTRAINT DF_ThuThachXuong_NgayCapNhat DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ThuThachXuong_HoaChatMucTieu FOREIGN KEY (IDHoaChatMucTieu) REFERENCES dbo.HoaChat(IDHoaChat),
    CONSTRAINT CK_ThuThachXuong_ThoiGian CHECK (ThoiGianGioiHanGiay > 0),
    CONSTRAINT CK_ThuThachXuong_Slot CHECK (SlotThuThach IS NULL OR SlotThuThach BETWEEN 1 AND 5),
    CONSTRAINT CK_ThuThachXuong_LuongMucTieu CHECK (LuongMucTieu > 0),
    CONSTRAINT CK_ThuThachXuong_Diem CHECK (DiemCoBan >= 0 AND DiemThuongSom >= 0 AND NguongThuongSomGiay >= 0 AND DiemPhatTre >= 0),
    CONSTRAINT CK_ThuThachXuong_DonVi CHECK (DonViMucTieu IN (N'g', N'ml', N'atm'))
);
GO

CREATE INDEX IX_ThuThachXuong_DangHoatDong_DoKho ON dbo.ThuThachXuong(DangHoatDong, DoKho);
CREATE INDEX IX_ThuThachXuong_IDHoaChatMucTieu ON dbo.ThuThachXuong(IDHoaChatMucTieu);
CREATE UNIQUE INDEX UX_ThuThachXuong_Ngay_DoKho_Slot ON dbo.ThuThachXuong(NgayThuThach, DoKho, SlotThuThach)
    WHERE NgayThuThach IS NOT NULL AND SlotThuThach IS NOT NULL;
GO

CREATE TABLE dbo.ThuThachXuongTaiNguyen (
    IDTaiNguyen int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ThuThachXuongTaiNguyen PRIMARY KEY,
    IDThuThach int NOT NULL,
    IDHoaChat int NOT NULL,
    Luong decimal(18,4) NOT NULL,
    DonVi nvarchar(20) NOT NULL,
    ThuTu int NOT NULL,
    CONSTRAINT FK_ThuThachXuongTaiNguyen_ThuThachXuong FOREIGN KEY (IDThuThach) REFERENCES dbo.ThuThachXuong(IDThuThach) ON DELETE CASCADE,
    CONSTRAINT FK_ThuThachXuongTaiNguyen_HoaChat FOREIGN KEY (IDHoaChat) REFERENCES dbo.HoaChat(IDHoaChat),
    CONSTRAINT UX_ThuThachXuongTaiNguyen_ThuThach_HoaChat UNIQUE (IDThuThach, IDHoaChat),
    CONSTRAINT CK_ThuThachXuongTaiNguyen_Luong CHECK (Luong > 0),
    CONSTRAINT CK_ThuThachXuongTaiNguyen_DonVi CHECK (DonVi IN (N'g', N'ml', N'atm')),
    CONSTRAINT CK_ThuThachXuongTaiNguyen_ThuTu CHECK (ThuTu > 0)
);
GO

CREATE INDEX IX_ThuThachXuongTaiNguyen_IDHoaChat ON dbo.ThuThachXuongTaiNguyen(IDHoaChat);
GO

CREATE TABLE dbo.PhienThuThachXuong (
    IDPhien int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PhienThuThachXuong PRIMARY KEY,
    IDThuThach int NOT NULL,
    IDUser int NOT NULL,
    TrangThai nvarchar(30) NOT NULL,
    BatDauLuc datetime2 NOT NULL,
    HetHanLuc datetime2 NOT NULL,
    HoanThanhLuc datetime2 NULL,
    InventoryJson nvarchar(max) NOT NULL,
    LuongMucTieuDatDuoc decimal(18,4) NULL,
    DatMucTieu bit NOT NULL CONSTRAINT DF_PhienThuThachXuong_DatMucTieu DEFAULT (0),
    SoPhanUng int NOT NULL CONSTRAINT DF_PhienThuThachXuong_SoPhanUng DEFAULT (0),
    SoLoi int NOT NULL CONSTRAINT DF_PhienThuThachXuong_SoLoi DEFAULT (0),
    TrangThaiNghiVan nvarchar(30) NOT NULL CONSTRAINT DF_PhienThuThachXuong_TrangThaiNghiVan DEFAULT (N'Normal'),
    LichSuSuKienJson nvarchar(max) NOT NULL CONSTRAINT DF_PhienThuThachXuong_LichSuSuKienJson DEFAULT (N'[]'),
    DiemCoBan int NOT NULL CONSTRAINT DF_PhienThuThachXuong_DiemCoBan DEFAULT (0),
    DiemThuong int NOT NULL CONSTRAINT DF_PhienThuThachXuong_DiemThuong DEFAULT (0),
    DiemPhat int NOT NULL CONSTRAINT DF_PhienThuThachXuong_DiemPhat DEFAULT (0),
    DiemCuoi int NOT NULL CONSTRAINT DF_PhienThuThachXuong_DiemCuoi DEFAULT (0),
    ThoiLuongGiay int NULL,
    LyDoKetThuc nvarchar(200) NULL,
    CONSTRAINT FK_PhienThuThachXuong_ThuThachXuong FOREIGN KEY (IDThuThach) REFERENCES dbo.ThuThachXuong(IDThuThach),
    CONSTRAINT FK_PhienThuThachXuong_NguoiDung FOREIGN KEY (IDUser) REFERENCES dbo.NguoiDung(IDUser) ON DELETE CASCADE,
    CONSTRAINT CK_PhienThuThachXuong_TrangThai CHECK (TrangThai IN (N'Running', N'Completed', N'Failed', N'Expired', N'Dead')),
    CONSTRAINT CK_PhienThuThachXuong_NghiVan CHECK (TrangThaiNghiVan IN (N'Normal', N'Suspicious', N'Invalid')),
    CONSTRAINT CK_PhienThuThachXuong_Diem CHECK (DiemCoBan >= 0 AND DiemThuong >= 0 AND DiemPhat >= 0 AND DiemCuoi >= 0),
    CONSTRAINT CK_PhienThuThachXuong_Dem CHECK (SoPhanUng >= 0 AND SoLoi >= 0),
    CONSTRAINT CK_PhienThuThachXuong_ThoiLuong CHECK (ThoiLuongGiay IS NULL OR ThoiLuongGiay >= 0)
);
GO

CREATE INDEX IX_PhienThuThachXuong_User_Challenge_Status ON dbo.PhienThuThachXuong(IDUser, IDThuThach, TrangThai);
CREATE UNIQUE INDEX UX_PhienThuThachXuong_User_Challenge ON dbo.PhienThuThachXuong(IDUser, IDThuThach);
CREATE INDEX IX_PhienThuThachXuong_Leaderboard ON dbo.PhienThuThachXuong(IDThuThach, TrangThai, DiemCuoi DESC, ThoiLuongGiay ASC, HoanThanhLuc ASC);
GO

CREATE TABLE dbo.PhanUng (
    IDPhanUng int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PhanUng PRIMARY KEY,
    PhuongTrinh nvarchar(500) NOT NULL,
    HienTuong nvarchar(max) NULL,
    GiaiThich nvarchar(max) NULL,
    CoKetTua bit NOT NULL CONSTRAINT DF_PhanUng_CoKetTua DEFAULT (0),
    CoKhi bit NOT NULL CONSTRAINT DF_PhanUng_CoKhi DEFAULT (0),
    DoiMau bit NOT NULL CONSTRAINT DF_PhanUng_DoiMau DEFAULT (0),
    ToaNhiet bit NOT NULL CONSTRAINT DF_PhanUng_ToaNhiet DEFAULT (0),
    GhiChu nvarchar(max) NULL,
    MaQuyTac nvarchar(50) NULL,
    LaNgoaiLeSandbox bit NOT NULL CONSTRAINT DF_PhanUng_LaNgoaiLeSandbox DEFAULT (0),
    DoUuTienSandbox int NOT NULL CONSTRAINT DF_PhanUng_DoUuTienSandbox DEFAULT (50),
    CONSTRAINT UQ_PhanUng_PhuongTrinh UNIQUE (PhuongTrinh)
);
GO

CREATE TABLE dbo.ChiTietPhanUng (
    IDPhanUng int NOT NULL,
    IDHoaChat int NOT NULL,
    VaiTro nvarchar(20) NOT NULL,
    HeSo int NOT NULL,
    CONSTRAINT PK_ChiTietPhanUng PRIMARY KEY (IDPhanUng, IDHoaChat, VaiTro),
    CONSTRAINT FK_ChiTietPhanUng_PhanUng FOREIGN KEY (IDPhanUng) REFERENCES dbo.PhanUng(IDPhanUng) ON DELETE CASCADE,
    CONSTRAINT FK_ChiTietPhanUng_HoaChat FOREIGN KEY (IDHoaChat) REFERENCES dbo.HoaChat(IDHoaChat),
    CONSTRAINT CK_ChiTietPhanUng_VaiTro CHECK (VaiTro IN (N'Reactant', N'Product')),
    CONSTRAINT CK_ChiTietPhanUng_HeSo CHECK (HeSo > 0)
);
GO

CREATE INDEX IX_ChiTietPhanUng_IDHoaChat ON dbo.ChiTietPhanUng(IDHoaChat);
GO

CREATE TABLE dbo.QuyLuatPhanUng (
    IDQuyLuat int IDENTITY(1,1) NOT NULL CONSTRAINT PK_QuyLuatPhanUng PRIMARY KEY,
    MaQuyLuat nvarchar(80) NOT NULL,
    TenQuyLuat nvarchar(150) NOT NULL,
    MoTa nvarchar(max) NULL,
    DangHoatDong bit NOT NULL CONSTRAINT DF_QuyLuatPhanUng_DangHoatDong DEFAULT (1),
    DoUuTien int NOT NULL CONSTRAINT DF_QuyLuatPhanUng_DoUuTien DEFAULT (100),
    CanNhiet bit NULL,
    KetQua_CoKhi bit NULL,
    KetQua_CoKetTua bit NULL,
    KetQua_DoiMau bit NULL,
    KetQua_ToaNhiet bit NULL,
    KetQua_NhietDoTangC decimal(8,2) NULL,
    HienTuongTemplate nvarchar(max) NULL,
    GiaiThichTemplate nvarchar(max) NULL,
    NgayTao datetime2 NOT NULL CONSTRAINT DF_QuyLuatPhanUng_NgayTao DEFAULT SYSUTCDATETIME(),
    NgayCapNhat datetime2 NOT NULL CONSTRAINT DF_QuyLuatPhanUng_NgayCapNhat DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_QuyLuatPhanUng_MaQuyLuat UNIQUE (MaQuyLuat),
    CONSTRAINT CK_QuyLuatPhanUng_DoUuTien CHECK (DoUuTien >= 0)
);
GO

CREATE TABLE dbo.QuyLuatPhanUngSlot (
    IDSlot int IDENTITY(1,1) NOT NULL CONSTRAINT PK_QuyLuatPhanUngSlot PRIMARY KEY,
    IDQuyLuat int NOT NULL,
    KyHieuSlot nvarchar(20) NOT NULL,
    VaiTro nvarchar(20) NOT NULL,
    ThuTu int NOT NULL,
    CONSTRAINT FK_QuyLuatPhanUngSlot_QuyLuatPhanUng FOREIGN KEY (IDQuyLuat) REFERENCES dbo.QuyLuatPhanUng(IDQuyLuat) ON DELETE CASCADE,
    CONSTRAINT UQ_QuyLuatPhanUngSlot_Key UNIQUE (IDQuyLuat, KyHieuSlot),
    CONSTRAINT CK_QuyLuatPhanUngSlot_VaiTro CHECK (VaiTro IN (N'Reactant'))
);
GO

CREATE TABLE dbo.QuyLuatPhanUngDieuKien (
    IDDieuKien int IDENTITY(1,1) NOT NULL CONSTRAINT PK_QuyLuatPhanUngDieuKien PRIMARY KEY,
    IDQuyLuat int NOT NULL,
    TargetKind nvarchar(30) NOT NULL,
    TargetSlot nvarchar(20) NULL,
    TargetProduct nvarchar(20) NULL,
    Field nvarchar(80) NOT NULL,
    Operator nvarchar(30) NOT NULL,
    ValueKind nvarchar(30) NOT NULL,
    ValueText nvarchar(200) NULL,
    CompareSlot nvarchar(20) NULL,
    CompareField nvarchar(80) NULL,
    GroupKey nvarchar(40) NULL,
    GroupOperator nvarchar(10) NULL,
    CONSTRAINT FK_QuyLuatPhanUngDieuKien_QuyLuatPhanUng FOREIGN KEY (IDQuyLuat) REFERENCES dbo.QuyLuatPhanUng(IDQuyLuat) ON DELETE CASCADE,
    CONSTRAINT CK_QuyLuatPhanUngDieuKien_TargetKind CHECK (TargetKind IN (N'Slot', N'Request', N'Product', N'Inventory')),
    CONSTRAINT CK_QuyLuatPhanUngDieuKien_Operator CHECK (Operator IN (N'Equals', N'NotEquals', N'GreaterThan', N'LessThan', N'GreaterThanOrEqual', N'LessThanOrEqual', N'In', N'Exists')),
    CONSTRAINT CK_QuyLuatPhanUngDieuKien_ValueKind CHECK (ValueKind IN (N'Literal', N'ChemicalField', N'SlotField', N'ReferencedChemicalField', N'InventoryField')),
    CONSTRAINT CK_QuyLuatPhanUngDieuKien_GroupOperator CHECK (GroupOperator IS NULL OR GroupOperator IN (N'All', N'Any'))
);
GO

CREATE TABLE dbo.QuyLuatPhanUngSanPham (
    IDSanPham int IDENTITY(1,1) NOT NULL CONSTRAINT PK_QuyLuatPhanUngSanPham PRIMARY KEY,
    IDQuyLuat int NOT NULL,
    ThuTu int NOT NULL,
    KieuSanPham nvarchar(40) NOT NULL,
    FixedFormula nvarchar(80) NULL,
    CationSourceSlot nvarchar(20) NULL,
    AnionSourceSlot nvarchar(20) NULL,
    SourceField nvarchar(80) NULL,
    HeSoHint int NULL,
    CONSTRAINT FK_QuyLuatPhanUngSanPham_QuyLuatPhanUng FOREIGN KEY (IDQuyLuat) REFERENCES dbo.QuyLuatPhanUng(IDQuyLuat) ON DELETE CASCADE,
    CONSTRAINT CK_QuyLuatPhanUngSanPham_Kieu CHECK (KieuSanPham IN (N'FixedFormula', N'IonicFromSlots', N'HydroxideFromCation', N'OxideFromCation', N'IonicFromCationAndFixedAnion', N'AcidFromAnion', N'MetalFromCation', N'Expression')),
    CONSTRAINT CK_QuyLuatPhanUngSanPham_HeSo CHECK (HeSoHint IS NULL OR HeSoHint > 0)
);
GO

CREATE INDEX IX_QuyLuatPhanUng_DangHoatDong_DoUuTien ON dbo.QuyLuatPhanUng(DangHoatDong, DoUuTien);
GO

CREATE INDEX IX_QuyLuatPhanUngDieuKien_IDQuyLuat ON dbo.QuyLuatPhanUngDieuKien(IDQuyLuat);
GO

CREATE INDEX IX_QuyLuatPhanUngSanPham_IDQuyLuat ON dbo.QuyLuatPhanUngSanPham(IDQuyLuat);
GO

CREATE TABLE dbo.QuyLuatPhanUngKetQuaDieuKien (
    IDKetQua int IDENTITY(1,1) NOT NULL CONSTRAINT PK_QuyLuatPhanUngKetQuaDieuKien PRIMARY KEY,
    IDQuyLuat int NOT NULL,
    ThuTu int NOT NULL,
    OutputField nvarchar(40) NOT NULL,
    BoolValue bit NULL,
    DecimalValue decimal(8,2) NULL,
    TextValue nvarchar(max) NULL,
    TargetKind nvarchar(30) NOT NULL,
    TargetSlot nvarchar(20) NULL,
    TargetProduct nvarchar(20) NULL,
    Field nvarchar(80) NOT NULL,
    Operator nvarchar(30) NOT NULL,
    ValueKind nvarchar(30) NOT NULL,
    ValueText nvarchar(200) NULL,
    CompareSlot nvarchar(20) NULL,
    CompareField nvarchar(80) NULL,
    CONSTRAINT FK_QuyLuatPhanUngKetQuaDieuKien_QuyLuatPhanUng FOREIGN KEY (IDQuyLuat) REFERENCES dbo.QuyLuatPhanUng(IDQuyLuat) ON DELETE CASCADE,
    CONSTRAINT CK_QuyLuatPhanUngKetQuaDieuKien_Output CHECK (OutputField IN (N'HasGas', N'KetQua_CoKhi', N'HasPrecipitate', N'KetQua_CoKetTua', N'HasColorChange', N'KetQua_DoiMau', N'IsExothermic', N'KetQua_ToaNhiet', N'TemperatureDeltaC', N'KetQua_NhietDoTangC', N'Phenomenon', N'HienTuong', N'Explanation', N'GiaiThich')),
    CONSTRAINT CK_QuyLuatPhanUngKetQuaDieuKien_TargetKind CHECK (TargetKind IN (N'Slot', N'Request', N'Product', N'Inventory')),
    CONSTRAINT CK_QuyLuatPhanUngKetQuaDieuKien_Operator CHECK (Operator IN (N'Equals', N'NotEquals', N'GreaterThan', N'LessThan', N'GreaterThanOrEqual', N'LessThanOrEqual', N'In', N'Exists')),
    CONSTRAINT CK_QuyLuatPhanUngKetQuaDieuKien_ValueKind CHECK (ValueKind IN (N'Literal', N'ChemicalField', N'SlotField', N'ReferencedChemicalField', N'InventoryField')),
    CONSTRAINT CK_QuyLuatPhanUngKetQuaDieuKien_Value CHECK (BoolValue IS NOT NULL OR DecimalValue IS NOT NULL OR TextValue IS NOT NULL)
);
GO

CREATE INDEX IX_QuyLuatPhanUngKetQuaDieuKien_IDQuyLuat ON dbo.QuyLuatPhanUngKetQuaDieuKien(IDQuyLuat);
GO

CREATE TABLE dbo.DungCu (
    IDDungCu int IDENTITY(1,1) NOT NULL CONSTRAINT PK_DungCu PRIMARY KEY,
    TenDungCu nvarchar(100) NOT NULL,
    MoTa nvarchar(max) NULL,
    NhomDungCu tinyint NULL,
    VaiTroSandbox nvarchar(max) NULL,
    LogicCode nvarchar(max) NULL,
    FrontendHint nvarchar(max) NULL,
    CoTheKeoTha bit NOT NULL CONSTRAINT DF_DungCu_CoTheKeoTha DEFAULT (1),
    ThuTuHienThi int NULL,
    CONSTRAINT UQ_DungCu_TenDungCu UNIQUE (TenDungCu),
    CONSTRAINT CK_DungCu_Nhom CHECK (NhomDungCu IS NULL OR NhomDungCu BETWEEN 1 AND 4),
    CONSTRAINT CK_DungCu_ThuTu CHECK (ThuTuHienThi IS NULL OR ThuTuHienThi > 0)
);
GO

CREATE INDEX IX_DungCu_Nhom_ThuTu ON dbo.DungCu(NhomDungCu, ThuTuHienThi);
GO

CREATE TABLE dbo.BangTuanHoan (
    IDNguyenTo int IDENTITY(1,1) NOT NULL CONSTRAINT PK_BangTuanHoan PRIMARY KEY,
    SoHieuNguyenTu int NOT NULL,
    KyHieu nvarchar(3) NOT NULL,
    TenNguyenTo nvarchar(100) NOT NULL,
    TenTiengAnh nvarchar(100) NULL,
    KhoiLuongNguyenTu decimal(10,4) NULL,
    ChuKy tinyint NULL,
    Nhom tinyint NULL,
    Khoi nvarchar(5) NULL,
    LoaiNguyenTo nvarchar(50) NULL,
    TrangThaiODieuKienThuong nvarchar(20) NULL,
    MauSac nvarchar(100) NULL,
    DoAmDien decimal(4,2) NULL,
    CauHinhElectron nvarchar(max) NULL,
    MoTa nvarchar(max) NULL,
    UngDung nvarchar(max) NULL,
    ImageUrl nvarchar(max) NULL,
    CONSTRAINT UQ_BangTuanHoan_SoHieu UNIQUE (SoHieuNguyenTu),
    CONSTRAINT UQ_BangTuanHoan_KyHieu UNIQUE (KyHieu),
    CONSTRAINT CK_BangTuanHoan_SoHieu CHECK (SoHieuNguyenTu > 0),
    CONSTRAINT CK_BangTuanHoan_ChuKy CHECK (ChuKy IS NULL OR ChuKy BETWEEN 1 AND 7),
    CONSTRAINT CK_BangTuanHoan_Nhom CHECK (Nhom IS NULL OR Nhom BETWEEN 1 AND 18)
);
GO

CREATE INDEX IX_BangTuanHoan_TenNguyenTo ON dbo.BangTuanHoan(TenNguyenTo);
CREATE INDEX IX_BangTuanHoan_LoaiNguyenTo ON dbo.BangTuanHoan(LoaiNguyenTo);
GO

CREATE TABLE dbo.ThanhPhanHoaChat (
    IDHoaChat int NOT NULL,
    IDNguyenTo int NOT NULL,
    SoNguyenTu int NOT NULL,
    CONSTRAINT PK_ThanhPhanHoaChat PRIMARY KEY (IDHoaChat, IDNguyenTo),
    CONSTRAINT FK_ThanhPhanHoaChat_HoaChat FOREIGN KEY (IDHoaChat) REFERENCES dbo.HoaChat(IDHoaChat) ON DELETE CASCADE,
    CONSTRAINT FK_ThanhPhanHoaChat_BangTuanHoan FOREIGN KEY (IDNguyenTo) REFERENCES dbo.BangTuanHoan(IDNguyenTo) ON DELETE CASCADE,
    CONSTRAINT CK_ThanhPhanHoaChat_SoNguyenTu CHECK (SoNguyenTu > 0)
);
GO

CREATE INDEX IX_ThanhPhanHoaChat_IDNguyenTo ON dbo.ThanhPhanHoaChat(IDNguyenTo);
GO

CREATE TABLE dbo.NguyenToTrangThaiOxiHoa (
    IDTrangThaiOxiHoa int IDENTITY(1,1) NOT NULL CONSTRAINT PK_NguyenToTrangThaiOxiHoa PRIMARY KEY,
    IDNguyenTo int NOT NULL,
    SoOxiHoa int NOT NULL,
    CONSTRAINT FK_NguyenToTrangThaiOxiHoa_BangTuanHoan FOREIGN KEY (IDNguyenTo) REFERENCES dbo.BangTuanHoan(IDNguyenTo) ON DELETE CASCADE,
    CONSTRAINT UQ_NguyenToTrangThaiOxiHoa_Key UNIQUE (IDNguyenTo, SoOxiHoa),
    CONSTRAINT CK_NguyenToTrangThaiOxiHoa_SoOxiHoa CHECK (SoOxiHoa BETWEEN -8 AND 8)
);
GO

CREATE TABLE dbo.HoaChatTrangThaiOxiHoa (
    IDTrangThaiOxiHoa int IDENTITY(1,1) NOT NULL CONSTRAINT PK_HoaChatTrangThaiOxiHoa PRIMARY KEY,
    IDHoaChat int NOT NULL,
    IDNguyenTo int NOT NULL,
    SoOxiHoa int NOT NULL,
    CONSTRAINT FK_HoaChatTrangThaiOxiHoa_HoaChat FOREIGN KEY (IDHoaChat) REFERENCES dbo.HoaChat(IDHoaChat) ON DELETE CASCADE,
    CONSTRAINT FK_HoaChatTrangThaiOxiHoa_BangTuanHoan FOREIGN KEY (IDNguyenTo) REFERENCES dbo.BangTuanHoan(IDNguyenTo) ON DELETE CASCADE,
    CONSTRAINT UQ_HoaChatTrangThaiOxiHoa_Key UNIQUE (IDHoaChat, IDNguyenTo, SoOxiHoa),
    CONSTRAINT CK_HoaChatTrangThaiOxiHoa_SoOxiHoa CHECK (SoOxiHoa BETWEEN -8 AND 8)
);
GO

CREATE INDEX IX_HoaChatTrangThaiOxiHoa_IDNguyenTo ON dbo.HoaChatTrangThaiOxiHoa(IDNguyenTo);
GO

CREATE TABLE dbo.HoaChatKhaNangRedox (
    IDKhaNangRedox int IDENTITY(1,1) NOT NULL CONSTRAINT PK_HoaChatKhaNangRedox PRIMARY KEY,
    IDHoaChat int NOT NULL,
    VaiTro nvarchar(30) NOT NULL,
    DieuKien nvarchar(50) NULL,
    CONSTRAINT FK_HoaChatKhaNangRedox_HoaChat FOREIGN KEY (IDHoaChat) REFERENCES dbo.HoaChat(IDHoaChat) ON DELETE CASCADE,
    CONSTRAINT UQ_HoaChatKhaNangRedox_Key UNIQUE (IDHoaChat, VaiTro, DieuKien),
    CONSTRAINT CK_HoaChatKhaNangRedox_VaiTro CHECK (VaiTro IN (N'OxidizingAgent', N'ReducingAgent', N'Both', N'Neither'))
);
GO

CREATE INDEX IX_HoaChatKhaNangRedox_VaiTro ON dbo.HoaChatKhaNangRedox(VaiTro);
GO

CREATE TABLE dbo.HoaChatPhanUngVoiNuoc (
    IDPhanUngVoiNuoc int IDENTITY(1,1) NOT NULL CONSTRAINT PK_HoaChatPhanUngVoiNuoc PRIMARY KEY,
    IDHoaChat int NOT NULL,
    PhaNuoc nvarchar(20) NOT NULL,
    NhietDoToiThieuC decimal(6,2) NULL,
    NhietDoToiDaC decimal(6,2) NULL,
    HoSanPham nvarchar(40) NOT NULL,
    DangHoatDong bit NOT NULL CONSTRAINT DF_HoaChatPhanUngVoiNuoc_DangHoatDong DEFAULT (1),
    CONSTRAINT FK_HoaChatPhanUngVoiNuoc_HoaChat FOREIGN KEY (IDHoaChat) REFERENCES dbo.HoaChat(IDHoaChat) ON DELETE CASCADE,
    CONSTRAINT UQ_HoaChatPhanUngVoiNuoc_Key UNIQUE (IDHoaChat, PhaNuoc, HoSanPham),
    CONSTRAINT CK_HoaChatPhanUngVoiNuoc_PhaNuoc CHECK (PhaNuoc IN (N'ColdWater', N'HotWater', N'Steam')),
    CONSTRAINT CK_HoaChatPhanUngVoiNuoc_NhietDo CHECK (NhietDoToiThieuC IS NULL OR NhietDoToiDaC IS NULL OR NhietDoToiThieuC <= NhietDoToiDaC)
);
GO

CREATE INDEX IX_HoaChatPhanUngVoiNuoc_PhaNuoc ON dbo.HoaChatPhanUngVoiNuoc(PhaNuoc);
GO

CREATE TABLE dbo.HoaChatTinhLuongTinh (
    IDTinhLuongTinh int IDENTITY(1,1) NOT NULL CONSTRAINT PK_HoaChatTinhLuongTinh PRIMARY KEY,
    IDHoaChat int NOT NULL,
    DangChat nvarchar(30) NOT NULL,
    MoiTruong nvarchar(30) NOT NULL,
    CanNuoc bit NOT NULL CONSTRAINT DF_HoaChatTinhLuongTinh_CanNuoc DEFAULT (0),
    HoSanPham nvarchar(40) NOT NULL,
    DangHoatDong bit NOT NULL CONSTRAINT DF_HoaChatTinhLuongTinh_DangHoatDong DEFAULT (1),
    CONSTRAINT FK_HoaChatTinhLuongTinh_HoaChat FOREIGN KEY (IDHoaChat) REFERENCES dbo.HoaChat(IDHoaChat) ON DELETE CASCADE,
    CONSTRAINT UQ_HoaChatTinhLuongTinh_Key UNIQUE (IDHoaChat, DangChat, MoiTruong, HoSanPham),
    CONSTRAINT CK_HoaChatTinhLuongTinh_DangChat CHECK (DangChat IN (N'Metal', N'Oxide', N'Hydroxide')),
    CONSTRAINT CK_HoaChatTinhLuongTinh_MoiTruong CHECK (MoiTruong IN (N'Acid', N'StrongBase'))
);
GO

CREATE INDEX IX_HoaChatTinhLuongTinh_MoiTruong ON dbo.HoaChatTinhLuongTinh(MoiTruong);
GO

CREATE TABLE dbo.HoaChatTinhAcid (
    IDTinhAcid int IDENTITY(1,1) NOT NULL CONSTRAINT PK_HoaChatTinhAcid PRIMARY KEY,
    IDHoaChat int NOT NULL,
    KieuAcid nvarchar(30) NOT NULL,
    DieuKien nvarchar(50) NULL,
    SanPhamKhu nvarchar(80) NULL,
    CONSTRAINT FK_HoaChatTinhAcid_HoaChat FOREIGN KEY (IDHoaChat) REFERENCES dbo.HoaChat(IDHoaChat) ON DELETE CASCADE,
    CONSTRAINT UQ_HoaChatTinhAcid_Key UNIQUE (IDHoaChat, KieuAcid, DieuKien, SanPhamKhu),
    CONSTRAINT CK_HoaChatTinhAcid_KieuAcid CHECK (KieuAcid IN (N'NonOxidizing', N'Oxidizing', N'ConditionDependent'))
);
GO

CREATE INDEX IX_HoaChatTinhAcid_KieuAcid ON dbo.HoaChatTinhAcid(KieuAcid);
GO

CREATE TABLE dbo.HoaChatHoSanPham (
    IDHoSanPham int IDENTITY(1,1) NOT NULL CONSTRAINT PK_HoaChatHoSanPham PRIMARY KEY,
    IDHoaChat int NOT NULL,
    HoSanPham nvarchar(40) NOT NULL,
    CONSTRAINT FK_HoaChatHoSanPham_HoaChat FOREIGN KEY (IDHoaChat) REFERENCES dbo.HoaChat(IDHoaChat) ON DELETE CASCADE,
    CONSTRAINT UQ_HoaChatHoSanPham_Key UNIQUE (IDHoaChat, HoSanPham)
);
GO

CREATE INDEX IX_HoaChatHoSanPham_HoSanPham ON dbo.HoaChatHoSanPham(HoSanPham);
GO

CREATE TABLE dbo.VeChungToi (
    IDVeChungToi int IDENTITY(1,1) NOT NULL CONSTRAINT PK_VeChungToi PRIMARY KEY,
    TieuDe nvarchar(150) NOT NULL,
    NoiDung nvarchar(max) NOT NULL,
    ImageUrl nvarchar(max) NULL,
    ThuTu int NOT NULL,
    DangHienThi bit NOT NULL CONSTRAINT DF_VeChungToi_DangHienThi DEFAULT (1),
    NgayCapNhat datetime2 NOT NULL CONSTRAINT DF_VeChungToi_NgayCapNhat DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT CK_VeChungToi_ThuTu CHECK (ThuTu > 0)
);
GO

CREATE INDEX IX_VeChungToi_DangHienThi_ThuTu ON dbo.VeChungToi(DangHienThi, ThuTu);
GO

CREATE TABLE dbo.DeXuatTriThucAI (
    IDDeXuat int IDENTITY(1,1) NOT NULL CONSTRAINT PK_DeXuatTriThucAI PRIMARY KEY,
    LoaiThucThe nvarchar(20) NOT NULL,
    TrangThai nvarchar(30) NOT NULL,
    KhoaUngVien nvarchar(200) NOT NULL,
    TuKhoa nvarchar(200) NOT NULL,
    NguonYeuCauJson nvarchar(max) NULL,
    PayloadJson nvarchar(max) NULL,
    PhienBanSchema int NOT NULL CONSTRAINT DF_DeXuatTriThucAI_PhienBanSchema DEFAULT (1),
    KetQuaKiemTraJson nvarchar(max) NULL,
    Revision int NOT NULL CONSTRAINT DF_DeXuatTriThucAI_Revision DEFAULT (0),
    NguonGoc nvarchar(20) NOT NULL,
    IDNguoiYeuCau int NULL,
    IDNguoiDuyet int NULL,
    LyDoQuyetDinh nvarchar(1000) NULL,
    IDBanGhiDich int NULL,
    NgayTao datetime2 NOT NULL CONSTRAINT DF_DeXuatTriThucAI_NgayTao DEFAULT (SYSUTCDATETIME()),
    NgayCapNhat datetime2 NOT NULL CONSTRAINT DF_DeXuatTriThucAI_NgayCapNhat DEFAULT (SYSUTCDATETIME()),
    NgayQuyetDinh datetime2 NULL,
    RowVer rowversion NOT NULL,
    CONSTRAINT FK_DeXuatTriThucAI_NguoiYeuCau FOREIGN KEY (IDNguoiYeuCau) REFERENCES dbo.NguoiDung(IDUser),
    CONSTRAINT FK_DeXuatTriThucAI_NguoiDuyet FOREIGN KEY (IDNguoiDuyet) REFERENCES dbo.NguoiDung(IDUser),
    CONSTRAINT CK_DeXuatTriThucAI_LoaiThucThe CHECK (LoaiThucThe IN (N'Chemical', N'Element', N'ReactionRule', N'Reaction')),
    CONSTRAINT CK_DeXuatTriThucAI_TrangThai CHECK (TrangThai IN (N'Collecting', N'InsufficientEvidence', N'ValidationFailed', N'Pending', N'Approved', N'Denied', N'ApplyFailed')),
    CONSTRAINT CK_DeXuatTriThucAI_NguonGoc CHECK (NguonGoc IN (N'Admin', N'Discovery'))
);
GO

CREATE UNIQUE INDEX UX_DeXuatTriThucAI_DangMo
    ON dbo.DeXuatTriThucAI(LoaiThucThe, KhoaUngVien)
    WHERE TrangThai IN (N'Collecting', N'InsufficientEvidence', N'ValidationFailed', N'Pending', N'ApplyFailed');
CREATE INDEX IX_DeXuatTriThucAI_TrangThai_NgayTao ON dbo.DeXuatTriThucAI(TrangThai, NgayTao);
GO

CREATE TABLE dbo.NguonDeXuatTriThucAI (
    IDNguon bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_NguonDeXuatTriThucAI PRIMARY KEY,
    IDDeXuat int NOT NULL,
    CoQuan nvarchar(100) NOT NULL,
    Url nvarchar(1000) NOT NULL,
    DinhDanh nvarchar(200) NULL,
    NgayTruyCap datetime2 NOT NULL,
    PhienBan nvarchar(100) NULL,
    HashNoiDung char(64) NOT NULL,
    TrichDoan nvarchar(max) NULL,
    TruongHoTroJson nvarchar(max) NULL,
    CONSTRAINT FK_NguonDeXuatTriThucAI_DeXuat FOREIGN KEY (IDDeXuat) REFERENCES dbo.DeXuatTriThucAI(IDDeXuat) ON DELETE CASCADE
);
GO

CREATE INDEX IX_NguonDeXuatTriThucAI_IDDeXuat ON dbo.NguonDeXuatTriThucAI(IDDeXuat);
GO

CREATE TABLE dbo.PhienBanDeXuatTriThucAI (
    IDPhienBan bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_PhienBanDeXuatTriThucAI PRIMARY KEY,
    IDDeXuat int NOT NULL,
    Revision int NOT NULL,
    PayloadJson nvarchar(max) NOT NULL,
    NguonGoc nvarchar(20) NOT NULL,
    IDNguoiSua int NULL,
    NgayTao datetime2 NOT NULL CONSTRAINT DF_PhienBanDeXuatTriThucAI_NgayTao DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_PhienBanDeXuatTriThucAI_DeXuat FOREIGN KEY (IDDeXuat) REFERENCES dbo.DeXuatTriThucAI(IDDeXuat) ON DELETE CASCADE,
    CONSTRAINT FK_PhienBanDeXuatTriThucAI_NguoiSua FOREIGN KEY (IDNguoiSua) REFERENCES dbo.NguoiDung(IDUser),
    CONSTRAINT UQ_PhienBanDeXuatTriThucAI_Revision UNIQUE (IDDeXuat, Revision),
    CONSTRAINT CK_PhienBanDeXuatTriThucAI_NguonGoc CHECK (NguonGoc IN (N'Extraction', N'AdminEdit'))
);
GO

CREATE TABLE dbo.CongViecDeXuatTriThucAI (
    IDCongViec bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_CongViecDeXuatTriThucAI PRIMARY KEY,
    IDDeXuat int NOT NULL,
    TrangThai nvarchar(20) NOT NULL,
    SoLanThu int NOT NULL CONSTRAINT DF_CongViecDeXuatTriThucAI_SoLanThu DEFAULT (0),
    GiuDen datetime2 NULL,
    ChayLaiSau datetime2 NULL,
    Loi nvarchar(1000) NULL,
    Provider nvarchar(50) NULL,
    Model nvarchar(100) NULL,
    PhienBanPrompt nvarchar(20) NULL,
    ThoiLuongMs int NULL,
    TokenVao int NULL,
    TokenRa int NULL,
    NgayTao datetime2 NOT NULL CONSTRAINT DF_CongViecDeXuatTriThucAI_NgayTao DEFAULT (SYSUTCDATETIME()),
    BatDauLuc datetime2 NULL,
    KetThucLuc datetime2 NULL,
    CONSTRAINT FK_CongViecDeXuatTriThucAI_DeXuat FOREIGN KEY (IDDeXuat) REFERENCES dbo.DeXuatTriThucAI(IDDeXuat) ON DELETE CASCADE,
    CONSTRAINT CK_CongViecDeXuatTriThucAI_TrangThai CHECK (TrangThai IN (N'Queued', N'Running', N'Succeeded', N'Failed', N'Cancelled'))
);
GO

CREATE UNIQUE INDEX UX_CongViecDeXuatTriThucAI_DangChay
    ON dbo.CongViecDeXuatTriThucAI(IDDeXuat)
    WHERE TrangThai IN (N'Queued', N'Running');
CREATE INDEX IX_CongViecDeXuatTriThucAI_TrangThai ON dbo.CongViecDeXuatTriThucAI(TrangThai, ChayLaiSau);
GO
