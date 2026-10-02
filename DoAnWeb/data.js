// ================================================================
// CƠ SỞ DỮ LIỆU ĐỒ ÁN QUẢN LÝ TÚI XÁCH (KHỚP CHUẨN VỚI quan_ly_tui_xach.sql)
// ================================================================

// 1. BẢNG THƯƠNG HIỆU (thuonghieu)
const thuongHieuList = [
    { mathuonghieu: 1, tenthuonghieu: "CHARLES & KEITH" },
    { mathuonghieu: 2, tenthuonghieu: "Natoli" },
    { mathuonghieu: 3, tenthuonghieu: "Pedro" },
    { mathuonghieu: 4, tenthuonghieu: "ELLY" },
    { mathuonghieu: 5, tenthuonghieu: "Jamlos" }
];

// 2. BẢNG LOẠI TÚI (loaitui)
const loaiTuiList = [
    { maloai: 1, tenloai: "Balo" },
    { maloai: 2, tenloai: "Túi đeo chéo" },
    { maloai: 3, tenloai: "Túi tote" },
    { maloai: 4, tenloai: "Túi dự tiệc" },
    { maloai: 5, tenloai: "Túi đeo vai" },
    { maloai: 6, tenloai: "Clutch" }
];

// 3. BẢNG KHUYẾN MÃI (khuyenmai)
const khuyenMaiList = [
    { makm: 1, tenkm: "Khuyến mãi tháng 9", phantramgiam: 10, ngaybatdau: "2026-09-01", ngayketthuc: "2026-09-30", trangthai: 1 },
    { makm: 2, tenkm: "Sale cuối tuần", phantramgiam: 15, ngaybatdau: "2026-09-05", ngayketthuc: "2026-09-30", trangthai: 1 },
    { makm: 3, tenkm: "Ưu đãi khách hàng mới", phantramgiam: 20, ngaybatdau: "2026-09-01", ngayketthuc: "2026-12-31", trangthai: 1 },
    { makm: 4, tenkm: "Sale mùa thu", phantramgiam: 25, ngaybatdau: "2026-09-15", ngayketthuc: "2026-10-15", trangthai: 1 },
    { makm: 5, tenkm: "Siêu sale cuối năm", phantramgiam: 30, ngaybatdau: "2026-12-01", ngayketthuc: "2026-12-31", trangthai: 1 }
];

// 4. BẢNG HÌNH ẢNH TÚI (hinhanhtui)
const hinhAnhTuiList = [];
for (let i = 1; i <= 15; i++) {
    hinhAnhTuiList.push({ mahinhanh: (i - 1) * 3 + 1, matui: i, tenhinhanh: `hinhAnh/tui${i}_1.png`, laanhchinh: 1, thutu: 1 });
    hinhAnhTuiList.push({ mahinhanh: (i - 1) * 3 + 2, matui: i, tenhinhanh: `hinhAnh/tui${i}_2.png`, laanhchinh: 0, thutu: 2 });
    hinhAnhTuiList.push({ mahinhanh: (i - 1) * 3 + 3, matui: i, tenhinhanh: `hinhAnh/tui${i}_3.png`, laanhchinh: 0, thutu: 3 });
}

// 5. BẢNG TÚI XÁCH (tuixach) & SẢN PHẨM DATABASE CHO WEBSITE
let productsDatabase = [
    // CHARLES & KEITH
    {
        id: 1,
        matui: 1,
        name: "Túi đeo chéo nữ CHARLES & KEITH",
        mathuonghieu: 1,
        brand: "CHARLES & KEITH",
        maloai: 2,
        category: "Túi đeo chéo",
        dongia: 1599000,
        soluong: 10,
        mota: "Túi đeo chéo thiết kế hiện đại, phù hợp sử dụng hằng ngày.",
        makm: 1,
        promotionName: "Khuyến mãi tháng 9",
        discountPercent: 10,
        oldPrice: 1599000,
        price: Math.round(1599000 * 0.9),
        badge: "Sale Off",
        img: "hinhAnh/tui1_1.png",
        thumbnails: ["hinhAnh/tui1_1.png", "hinhAnh/tui1_2.png", "hinhAnh/tui1_3.png"]
    },
    {
        id: 2,
        matui: 2,
        name: "Túi đeo vai nữ CHARLES & KEITH",
        mathuonghieu: 1,
        brand: "CHARLES & KEITH",
        maloai: 5,
        category: "Túi đeo vai",
        dongia: 1899000,
        soluong: 8,
        mota: "Túi đeo vai phong cách thanh lịch, phù hợp đi làm và đi chơi.",
        makm: 2,
        promotionName: "Sale cuối tuần",
        discountPercent: 15,
        oldPrice: 1899000,
        price: Math.round(1899000 * 0.85),
        badge: "Best Seller",
        img: "hinhAnh/tui2_1.png",
        thumbnails: ["hinhAnh/tui2_1.png", "hinhAnh/tui2_2.png", "hinhAnh/tui2_3.png"]
    },
    {
        id: 3,
        matui: 3,
        name: "Clutch dự tiệc nữ CHARLES & KEITH",
        mathuonghieu: 1,
        brand: "CHARLES & KEITH",
        maloai: 6,
        category: "Clutch",
        dongia: 1999000,
        soluong: 6,
        mota: "Clutch thiết kế sang trọng, phù hợp các buổi tiệc và sự kiện.",
        makm: 3,
        promotionName: "Ưu đãi khách hàng mới",
        discountPercent: 20,
        oldPrice: 1999000,
        price: Math.round(1999000 * 0.8),
        badge: "Sale Off",
        img: "hinhAnh/tui3_1.png",
        thumbnails: ["hinhAnh/tui3_1.png", "hinhAnh/tui3_2.png", "hinhAnh/tui3_3.png"]
    },

    // NATOLI
    {
        id: 4,
        matui: 4,
        name: "Túi đeo chéo nữ Natoli",
        mathuonghieu: 2,
        brand: "Natoli",
        maloai: 2,
        category: "Túi đeo chéo",
        dongia: 599000,
        soluong: 15,
        mota: "Túi đeo chéo nữ thiết kế trẻ trung, phù hợp sử dụng hằng ngày.",
        makm: 1,
        promotionName: "Khuyến mãi tháng 9",
        discountPercent: 10,
        oldPrice: 599000,
        price: Math.round(599000 * 0.9),
        badge: "Best Seller",
        img: "hinhAnh/tui4_1.png",
        thumbnails: ["hinhAnh/tui4_1.png", "hinhAnh/tui4_2.png", "hinhAnh/tui4_3.png"]
    },
    {
        id: 5,
        matui: 5,
        name: "Túi tote nam nữ Natoli",
        mathuonghieu: 2,
        brand: "Natoli",
        maloai: 3,
        category: "Túi tote",
        dongia: 699000,
        soluong: 12,
        mota: "Túi tote thiết kế đơn giản, phù hợp cho cả nam và nữ.",
        makm: 2,
        promotionName: "Sale cuối tuần",
        discountPercent: 15,
        oldPrice: 699000,
        price: Math.round(699000 * 0.85),
        badge: "Sale Off",
        img: "hinhAnh/tui5_1.png",
        thumbnails: ["hinhAnh/tui5_1.png", "hinhAnh/tui5_2.png", "hinhAnh/tui5_3.png"]
    },
    {
        id: 6,
        matui: 6,
        name: "Balo nam Natoli",
        mathuonghieu: 2,
        brand: "Natoli",
        maloai: 1,
        category: "Balo",
        dongia: 799000,
        soluong: 10,
        mota: "Balo nam kiểu dáng năng động, phù hợp đi học và đi làm.",
        makm: 3,
        promotionName: "Ưu đãi khách hàng mới",
        discountPercent: 20,
        oldPrice: 799000,
        price: Math.round(799000 * 0.8),
        badge: "Sale Off",
        img: "hinhAnh/tui6_1.png",
        thumbnails: ["hinhAnh/tui6_1.png", "hinhAnh/tui6_2.png", "hinhAnh/tui6_3.png"]
    },

    // PEDRO
    {
        id: 7,
        matui: 7,
        name: "Túi đeo chéo nam nữ Pedro",
        mathuonghieu: 3,
        brand: "Pedro",
        maloai: 2,
        category: "Túi đeo chéo",
        dongia: 799000,
        soluong: 12,
        mota: "Túi đeo chéo thiết kế hiện đại, phù hợp cho cả nam và nữ.",
        makm: 1,
        promotionName: "Khuyến mãi tháng 9",
        discountPercent: 10,
        oldPrice: 799000,
        price: Math.round(799000 * 0.9),
        badge: "Best Seller",
        img: "hinhAnh/tui7_1.png",
        thumbnails: ["hinhAnh/tui7_1.png", "hinhAnh/tui7_2.png", "hinhAnh/tui7_3.png"]
    },
    {
        id: 8,
        matui: 8,
        name: "Túi đeo vai nam Pedro",
        mathuonghieu: 3,
        brand: "Pedro",
        maloai: 5,
        category: "Túi đeo vai",
        dongia: 899000,
        soluong: 10,
        mota: "Túi đeo vai nam thiết kế thanh lịch, phù hợp đi làm và đi chơi.",
        makm: 2,
        promotionName: "Sale cuối tuần",
        discountPercent: 15,
        oldPrice: 899000,
        price: Math.round(899000 * 0.85),
        badge: "Sale Off",
        img: "hinhAnh/tui8_1.png",
        thumbnails: ["hinhAnh/tui8_1.png", "hinhAnh/tui8_2.png", "hinhAnh/tui8_3.png"]
    },
    {
        id: 9,
        matui: 9,
        name: "Clutch dự tiệc nữ Pedro",
        mathuonghieu: 3,
        brand: "Pedro",
        maloai: 6,
        category: "Clutch",
        dongia: 999000,
        soluong: 8,
        mota: "Clutch thiết kế sang trọng, phù hợp các sự kiện.",
        makm: 4,
        promotionName: "Sale mùa thu",
        discountPercent: 25,
        oldPrice: 999000,
        price: Math.round(999000 * 0.75),
        badge: "Sale Off",
        img: "hinhAnh/tui9_1.png",
        thumbnails: ["hinhAnh/tui9_1.png", "hinhAnh/tui9_2.png", "hinhAnh/tui9_3.png"]
    },

    // ELLY
    {
        id: 10,
        matui: 10,
        name: "Túi đeo chéo nữ ELLY",
        mathuonghieu: 4,
        brand: "ELLY",
        maloai: 2,
        category: "Túi đeo chéo",
        dongia: 899000,
        soluong: 10,
        mota: "Túi đeo chéo nữ thiết kế sang trọng, phù hợp nhiều dịp.",
        makm: 1,
        promotionName: "Khuyến mãi tháng 9",
        discountPercent: 10,
        oldPrice: 899000,
        price: Math.round(899000 * 0.9),
        badge: "Best Seller",
        img: "hinhAnh/tui10_1.png",
        thumbnails: ["hinhAnh/tui10_1.png", "hinhAnh/tui10_2.png", "hinhAnh/tui10_3.png"]
    },
    {
        id: 11,
        matui: 11,
        name: "Túi đeo vai nữ ELLY",
        mathuonghieu: 4,
        brand: "ELLY",
        maloai: 5,
        category: "Túi đeo vai",
        dongia: 1099000,
        soluong: 8,
        mota: "Túi đeo vai phong cách thanh lịch, phù hợp đi làm và dự tiệc.",
        makm: 3,
        promotionName: "Ưu đãi khách hàng mới",
        discountPercent: 20,
        oldPrice: 1099000,
        price: Math.round(1099000 * 0.8),
        badge: "Sale Off",
        img: "hinhAnh/tui11_1.png",
        thumbnails: ["hinhAnh/tui11_1.png", "hinhAnh/tui11_2.png", "hinhAnh/tui11_3.png"]
    },
    {
        id: 12,
        matui: 12,
        name: "Balo nam nữ ELLY",
        mathuonghieu: 4,
        brand: "ELLY",
        maloai: 1,
        category: "Balo",
        dongia: 1199000,
        soluong: 7,
        mota: "Balo thiết kế hiện đại, phù hợp cho cả nam và nữ.",
        makm: 4,
        promotionName: "Sale mùa thu",
        discountPercent: 25,
        oldPrice: 1199000,
        price: Math.round(1199000 * 0.75),
        badge: "Sale Off",
        img: "hinhAnh/tui12_1.png",
        thumbnails: ["hinhAnh/tui12_1.png", "hinhAnh/tui12_2.png", "hinhAnh/tui12_3.png"]
    },

    // JAMLOS
    {
        id: 13,
        matui: 13,
        name: "Balo nam Jamlos",
        mathuonghieu: 5,
        brand: "Jamlos",
        maloai: 1,
        category: "Balo",
        dongia: 699000,
        soluong: 15,
        mota: "Balo nam phong cách năng động, phù hợp đi học và đi làm.",
        makm: null,
        promotionName: "Không khuyến mãi",
        discountPercent: 0,
        oldPrice: 0,
        price: 699000,
        badge: "New Arrival",
        img: "hinhAnh/tui13_1.png",
        thumbnails: ["hinhAnh/tui13_1.png", "hinhAnh/tui13_2.png", "hinhAnh/tui13_3.png"]
    },
    {
        id: 14,
        matui: 14,
        name: "Túi đeo chéo nam nữ Jamlos",
        mathuonghieu: 5,
        brand: "Jamlos",
        maloai: 2,
        category: "Túi đeo chéo",
        dongia: 499000,
        soluong: 18,
        mota: "Túi đeo chéo thiết kế nhỏ gọn, phù hợp cho cả nam và nữ.",
        makm: 2,
        promotionName: "Sale cuối tuần",
        discountPercent: 15,
        oldPrice: 499000,
        price: Math.round(499000 * 0.85),
        badge: "Sale Off",
        img: "hinhAnh/tui14_1.png",
        thumbnails: ["hinhAnh/tui14_1.png", "hinhAnh/tui14_2.png", "hinhAnh/tui14_3.png"]
    },
    {
        id: 15,
        matui: 15,
        name: "Túi tote nam nữ Jamlos",
        mathuonghieu: 5,
        brand: "Jamlos",
        maloai: 3,
        category: "Túi tote",
        dongia: 599000,
        soluong: 12,
        mota: "Túi tote thiết kế đơn giản, tiện dụng và năng động.",
        makm: null,
        promotionName: "Không khuyến mãi",
        discountPercent: 0,
        oldPrice: 0,
        price: 599000,
        badge: "Best Seller",
        img: "hinhAnh/tui15_1.png",
        thumbnails: ["hinhAnh/tui15_1.png", "hinhAnh/tui15_2.png", "hinhAnh/tui15_3.png"]
    }
];

// 6. BẢNG TÀI KHOẢN & KHÁCH HÀNG (taikhoan, khachhang)
const taiKhoanInitialList = [
    { matk: 1, username: "admin", email: "admin@tuixach.vn", password: "123456", role: "Người bán" },
    { matk: 2, username: "minhquan", email: "minhquan@gmail.com", password: "123456", role: "Khách hàng", hoten: "Nguyễn Nhật Minh Quân", phone: "0901234567", address: "TP. Hồ Chí Minh" },
    { matk: 3, username: "ngocanh", email: "ngocanh@gmail.com", password: "123456", role: "Khách hàng", hoten: "Trần Ngọc Anh", phone: "0912345678", address: "Bình Dương" },
    { matk: 4, username: "thanhhoa", email: "thanhhoa@gmail.com", password: "123456", role: "Khách hàng", hoten: "Lê Thanh Hòa", phone: "0923456789", address: "Đồng Nai" },
    { matk: 5, username: "quanghuy", email: "quanghuy@gmail.com", password: "123456", role: "Khách hàng", hoten: "Phạm Quang Huy", phone: "0934567890", address: "Long An" },
    { matk: 6, username: "thimai", email: "thimai@gmail.com", password: "123456", role: "Khách hàng", hoten: "Võ Thị Mai", phone: "0945678901", address: "TP. Hồ Chí Minh" }
];