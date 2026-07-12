namespace TechExchangeApp.ViewModel
{
    public class OcopOrderVm
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string DienThoai { get; set; } = "";
        public string DiaChiGiao { get; set; } = "";
        public int SoLuong { get; set; }
        public string? GhiChu { get; set; }
        public int StatusId { get; set; }
        public int HinhThucThanhToan { get; set; }
        public DateTime NgayTao { get; set; }
    }
}
