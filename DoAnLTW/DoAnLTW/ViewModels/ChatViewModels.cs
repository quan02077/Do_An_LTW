namespace DoAnLTW.ViewModels;

// ---- yêu cầu từ trình duyệt (không tin, server tự kiểm tra lại) ----
public class ChatRequest
{
    public string? Message { get; set; }
    public List<ChatHistoryItem>? History { get; set; }
}

public class ChatHistoryItem
{
    public string? Role { get; set; }
    public string? Content { get; set; }
}

// ---- phản hồi gửi xuống trình duyệt (camelCase theo mặc định của ASP.NET Core) ----
// thẻ sản phẩm lấy từ CSDL, không do AI sinh ra, nên không thể bị AI bịa tên/giá/đường dẫn.
public record ChatProductDto(
    int MaTui,
    string TenTui,
    decimal DonGia,
    decimal GiaGoc,
    bool DangGiam,
    int PhanTramGiam,
    string? AnhUrl,
    string LinkChiTiet,
    bool ConHang);

public record ChatResponse(string Reply, string Source, IReadOnlyList<ChatProductDto> Products);
