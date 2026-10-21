namespace DoAnLTW.Models
{
    public class ChiTietGioHang
    {
        public int maGioHang { get; set; }
        public GioHang? GioHang { get; set; }

        public int maTui { get; set; }
        public TuiXach? TuiXach { get; set; }

        public int soLuong { get; set; } = 1;
    }
}
