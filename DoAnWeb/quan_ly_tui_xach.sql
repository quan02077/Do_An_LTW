CREATE DATABASE QuanLyTuiXach;
GO

USE QuanLyTuiXach;
GO

CREATE TABLE taikhoan (
    matk INT IDENTITY(1,1) PRIMARY KEY,
    tendangnhap VARCHAR(50) NOT NULL UNIQUE,
    matkhau VARCHAR(255) NOT NULL,
    vaitro NVARCHAR(20) NOT NULL,

    CONSTRAINT CK_taikhoan_vaitro CHECK (vaitro IN (N'Người bán', N'Khách hàng'))
);
GO

CREATE TABLE khachhang (
    makh INT IDENTITY(1,1) PRIMARY KEY,
    matk INT NOT NULL UNIQUE,
    hoten NVARCHAR(100) NOT NULL,
    email VARCHAR(100),
    sodienthoai VARCHAR(15),
    diachi NVARCHAR(255),

    CONSTRAINT FK_khachhang_taikhoan FOREIGN KEY (matk) REFERENCES taikhoan(matk)
);
GO

CREATE TABLE thuonghieu (
    mathuonghieu INT IDENTITY(1,1) PRIMARY KEY,
    tenthuonghieu NVARCHAR(100) NOT NULL UNIQUE
);
GO

CREATE TABLE loaitui (
    maloai INT IDENTITY(1,1) PRIMARY KEY,
    tenloai NVARCHAR(100) NOT NULL UNIQUE
);
GO

CREATE TABLE khuyenmai (
    makm INT IDENTITY(1,1) PRIMARY KEY,
    tenkm NVARCHAR(100) NOT NULL,
    phantramgiam DECIMAL(5,2) NOT NULL,
    ngaybatdau DATE NOT NULL,
    ngayketthuc DATE NOT NULL,
    trangthai BIT NOT NULL DEFAULT 1,

    CONSTRAINT CK_khuyenmai_phantram CHECK (phantramgiam >= 0 AND phantramgiam <= 100),

    CONSTRAINT CK_khuyenmai_ngay CHECK (ngaybatdau <= ngayketthuc)
);
GO

CREATE TABLE tuixach (
    matui INT IDENTITY(1,1) PRIMARY KEY,
    tentui NVARCHAR(150) NOT NULL,
    mathuonghieu INT NOT NULL,
    maloai INT NOT NULL,
    dongia DECIMAL(15,2) NOT NULL,
    soluong INT NOT NULL,
    mota NVARCHAR(MAX),
    makm INT NULL,

    CONSTRAINT CK_tuixach_dongia CHECK (dongia >= 0),

    CONSTRAINT CK_tuixach_soluong CHECK (soluong >= 0),

    CONSTRAINT FK_tuixach_thuonghieu FOREIGN KEY (mathuonghieu) REFERENCES thuonghieu(mathuonghieu),

    CONSTRAINT FK_tuixach_loaitui FOREIGN KEY (maloai) REFERENCES loaitui(maloai),

    CONSTRAINT FK_tuixach_khuyenmai FOREIGN KEY (makm) REFERENCES khuyenmai(makm)
);
GO

CREATE TABLE hinhanhtui (
    mahinhanh INT IDENTITY(1,1) PRIMARY KEY,
    matui INT NOT NULL,
    tenhinhanh VARCHAR(255) NOT NULL,
    laanhchinh BIT NOT NULL DEFAULT 0,
    thutu INT NOT NULL,

    CONSTRAINT CK_hinhanhtui_thutu CHECK (thutu > 0),

    CONSTRAINT FK_hinhanhtui_tuixach FOREIGN KEY (matui) REFERENCES tuixach(matui),

    CONSTRAINT UQ_hinhanhtui_thutu UNIQUE (matui, thutu)
);
GO

CREATE TABLE giohang (
    magiohang INT IDENTITY(1,1) PRIMARY KEY,
    makh INT NOT NULL UNIQUE,
    ngaytao DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_giohang_khachhang FOREIGN KEY (makh) REFERENCES khachhang(makh)
);
GO

CREATE TABLE chitietgiohang (
    magiohang INT NOT NULL,
    matui INT NOT NULL,
    soluong INT NOT NULL,

    PRIMARY KEY (magiohang, matui),

    CONSTRAINT CK_chitietgiohang_soluong CHECK (soluong > 0),

    CONSTRAINT FK_chitietgiohang_giohang FOREIGN KEY (magiohang) REFERENCES giohang(magiohang),

    CONSTRAINT FK_chitietgiohang_tuixach FOREIGN KEY (matui) REFERENCES tuixach(matui)
);
GO

CREATE TABLE donhang (
    madh INT IDENTITY(1,1) PRIMARY KEY,
    makh INT NOT NULL,
    ngaydat DATETIME NOT NULL DEFAULT GETDATE(),
    tongtien DECIMAL(15,2) NOT NULL,
    trangthai NVARCHAR(50) NOT NULL,
    diachinhan NVARCHAR(255) NOT NULL,

    CONSTRAINT CK_donhang_tongtien CHECK (tongtien >= 0),

    CONSTRAINT FK_donhang_khachhang FOREIGN KEY (makh) REFERENCES khachhang(makh)
);
GO

CREATE TABLE chitietdonhang (
    madh INT NOT NULL,
    matui INT NOT NULL,
    soluong INT NOT NULL,
    dongia DECIMAL(15,2) NOT NULL,
    thanhtien DECIMAL(15,2) NOT NULL,

    PRIMARY KEY (madh, matui),

    CONSTRAINT CK_chitietdonhang_soluong CHECK (soluong > 0),

    CONSTRAINT CK_chitietdonhang_dongia CHECK (dongia >= 0),

    CONSTRAINT CK_chitietdonhang_thanhtien CHECK (thanhtien >= 0),

    CONSTRAINT FK_chitietdonhang_donhang FOREIGN KEY (madh) REFERENCES donhang(madh),

    CONSTRAINT FK_chitietdonhang_tuixach FOREIGN KEY (matui) REFERENCES tuixach(matui)
);
GO

INSERT INTO taikhoan(tendangnhap, matkhau, vaitro)
VALUES
('admin', '123456', N'Người bán'),
('minhquan', '123456', N'Khách hàng'),
('ngocanh', '123456', N'Khách hàng'),
('thanhhoa', '123456', N'Khách hàng'),
('quanghuy', '123456', N'Khách hàng'),
('thimai', '123456', N'Khách hàng');
GO

INSERT INTO khachhang(matk, hoten, email, sodienthoai, diachi)
VALUES
(2, N'Nguyễn Nhật Minh Quân', 'minhquan@gmail.com', '0901234567', N'TP. Hồ Chí Minh'),
(3, N'Trần Ngọc Anh', 'ngocanh@gmail.com', '0912345678', N'Bình Dương'),
(4, N'Lê Thanh Hòa', 'thanhhoa@gmail.com', '0923456789', N'Đồng Nai'),
(5, N'Phạm Quang Huy', 'quanghuy@gmail.com', '0934567890', N'Long An'),
(6, N'Võ Thị Mai', 'thimai@gmail.com', '0945678901', N'TP. Hồ Chí Minh');
GO

INSERT INTO thuonghieu(tenthuonghieu)
VALUES
(N'CHARLES & KEITH'),
(N'Natoli'),
(N'Pedro'),
(N'ELLY'),
(N'Jamlos');
GO

INSERT INTO loaitui(tenloai)
VALUES
(N'Balo'),
(N'Túi đeo chéo'),
(N'Túi tote'),
(N'Túi dự tiệc'),
(N'Túi đeo vai'),
(N'Clutch');
GO

INSERT INTO khuyenmai
(tenkm, phantramgiam, ngaybatdau, ngayketthuc, trangthai)
VALUES
(N'Khuyến mãi tháng 9', 10, '2026-09-01', '2026-09-30', 1),
(N'Sale cuối tuần', 15, '2026-09-05', '2026-09-30', 1),
(N'Ưu đãi khách hàng mới', 20, '2026-09-01', '2026-12-31', 1),
(N'Sale mùa thu', 25, '2026-09-15', '2026-10-15', 1),
(N'Siêu sale cuối năm', 30, '2026-12-01', '2026-12-31', 1);
GO

INSERT INTO tuixach
(tentui, mathuonghieu, maloai, dongia, soluong, mota, makm)
VALUES
-- CHARLES & KEITH
(N'Túi đeo chéo nữ CHARLES & KEITH', 1, 2, 1599000, 10, N'Túi đeo chéo thiết kế hiện đại, phù hợp sử dụng hằng ngày.', 1),
(N'Túi đeo vai nữ CHARLES & KEITH', 1, 5, 1899000, 8, N'Túi đeo vai phong cách thanh lịch, phù hợp đi làm và đi chơi.', 2),
(N'Clutch dự tiệc nữ CHARLES & KEITH', 1, 6, 1999000, 6, N'Clutch thiết kế sang trọng, phù hợp các buổi tiệc và sự kiện.', 3),

-- NATOLI
(N'Túi đeo chéo nữ Natoli', 2, 2, 599000, 15, N'Túi đeo chéo nữ thiết kế trẻ trung, phù hợp sử dụng hằng ngày.', 1),
(N'Túi tote nam nữ Natoli', 2, 3, 699000, 12, N'Túi tote thiết kế đơn giản, phù hợp cho cả nam và nữ.', 2),
(N'Balo nam Natoli', 2, 1, 799000, 10, N'Balo nam kiểu dáng năng động, phù hợp đi học và đi làm.', 3),

-- PEDRO
(N'Túi đeo chéo nam nữ Pedro', 3, 2, 799000, 12, N'Túi đeo chéo thiết kế hiện đại, phù hợp cho cả nam và nữ.', 1),
(N'Túi đeo vai nam Pedro', 3, 5, 899000, 10, N'Túi đeo vai nam thiết kế thanh lịch, phù hợp đi làm và đi chơi.', 2),
(N'Clutch dự tiệc nữ Pedro', 3, 6, 999000, 8, N'Clutch thiết kế sang trọng, phù hợp các sự kiện.', 4),

-- ELLY
(N'Túi đeo chéo nữ ELLY', 4, 2, 899000, 10, N'Túi đeo chéo nữ thiết kế sang trọng, phù hợp nhiều dịp.', 1),
(N'Túi đeo vai nữ ELLY', 4, 5, 1099000, 8, N'Túi đeo vai phong cách thanh lịch, phù hợp đi làm và dự tiệc.', 3),
(N'Balo nam nữ ELLY', 4, 1, 1199000, 7, N'Balo thiết kế hiện đại, phù hợp cho cả nam và nữ.', 4),

-- JAMLOS
(N'Balo nam Jamlos', 5, 1, 699000, 15, N'Balo nam phong cách năng động, phù hợp đi học và đi làm.', NULL),
(N'Túi đeo chéo nam nữ Jamlos', 5, 2, 499000, 18, N'Túi đeo chéo thiết kế nhỏ gọn, phù hợp cho cả nam và nữ.', 2),
(N'Túi tote nam nữ Jamlos', 5, 3, 599000, 12, N'Túi tote thiết kế đơn giản, tiện dụng và năng động.', NULL);

GO

INSERT INTO hinhanhtui
(matui, tenhinhanh, laanhchinh, thutu)
VALUES
(1, 'tui1_1.png', 1, 1),
(1, 'tui1_2.png', 0, 2),
(1, 'tui1_3.png', 0, 3),

(2, 'tui2_1.png', 1, 1),
(2, 'tui2_2.png', 0, 2),
(2, 'tui2_3.png', 0, 3),

(3, 'tui3_1.png', 1, 1),
(3, 'tui3_2.png', 0, 2),
(3, 'tui3_3.png', 0, 3),

(4, 'tui4_1.png', 1, 1),
(4, 'tui4_2.png', 0, 2),
(4, 'tui4_3.png', 0, 3),

(5, 'tui5_1.png', 1, 1),
(5, 'tui5_2.png', 0, 2),
(5, 'tui5_3.png', 0, 3),

(6, 'tui6_1.png', 1, 1),
(6, 'tui6_2.png', 0, 2),
(6, 'tui6_3.png', 0, 3),

(7, 'tui7_1.png', 1, 1),
(7, 'tui7_2.png', 0, 2),
(7, 'tui7_3.png', 0, 3),

(8, 'tui8_1.png', 1, 1),
(8, 'tui8_2.png', 0, 2),
(8, 'tui8_3.png', 0, 3),

(9, 'tui9_1.png', 1, 1),
(9, 'tui9_2.png', 0, 2),
(9, 'tui9_3.png', 0, 3),

(10, 'tui10_1.png', 1, 1),
(10, 'tui10_2.png', 0, 2),
(10, 'tui10_3.png', 0, 3),

(11, 'tui11_1.png', 1, 1),
(11, 'tui11_2.png', 0, 2),
(11, 'tui11_3.png', 0, 3),

(12, 'tui12_1.png', 1, 1),
(12, 'tui12_2.png', 0, 2),
(12, 'tui12_3.png', 0, 3),

(13, 'tui13_1.png', 1, 1),
(13, 'tui13_2.png', 0, 2),
(13, 'tui13_3.png', 0, 3),

(14, 'tui14_1.png', 1, 1),
(14, 'tui14_2.png', 0, 2),
(14, 'tui14_3.png', 0, 3),

(15, 'tui15_1.png', 1, 1),
(15, 'tui15_2.png', 0, 2),
(15, 'tui15_3.png', 0, 3);

GO


SELECT * FROM taikhoan;
SELECT * FROM khachhang;
SELECT * FROM thuonghieu;
SELECT * FROM loaitui;
SELECT * FROM khuyenmai;
SELECT * FROM tuixach;
SELECT * FROM hinhanhtui;
SELECT * FROM giohang;
SELECT * FROM chitietgiohang;
SELECT * FROM donhang;
SELECT * FROM chitietdonhang;

GO