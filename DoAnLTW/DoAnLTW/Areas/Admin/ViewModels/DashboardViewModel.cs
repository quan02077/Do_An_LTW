namespace DoAnLTW.Areas.Admin.ViewModels
{
    // viewmodel thong ke cho trang dashboard admin
    public class DashboardViewModel
    {
        // 1. Tong so don hang trong he thong
        public int TongDonHang { get; set; }

        // 2. Tong doanh thu thuc te tu cac don hang hoan tat (vnd)
        public decimal TongDoanhThu { get; set; }

        // 3. Don hang theo tung trang thai
        public int DonChoDuyet { get; set; }
        public int DonDangGiao { get; set; }
        public int DonHoanTat { get; set; }
        public int DonDaHuy { get; set; }

        public int NamDuocChon { get; set; }
        public List<int> DanhSachNam { get; set; } = [];
        public decimal[] DoanhThuTheoThang { get; set; } = new decimal[12];
        public List<SanPhamBanChayViewModel> SanPhamBanChay { get; set; } = [];
    }

    public class SanPhamBanChayViewModel
    {
        public string TenTui { get; set; } = string.Empty;
        public int SoLuongDaBan { get; set; }
        public decimal DoanhThu { get; set; }
    }
}
