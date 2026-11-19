namespace DoAnLTW.Areas.Admin.ViewModels
{
    // thông tin phân trang dùng chung cho partial _Pagination.cshtml
    public class PaginationViewModel
    {
        public const int DefaultPageSize = 10;

        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = DefaultPageSize;
        public int TotalItems { get; init; }
        public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalItems / (double)PageSize));
        public string Action { get; init; } = "Index";

        // bộ lọc hiện tại (keyword, trangThai...) được giữ nguyên khi bấm sang trang khác
        public Dictionary<string, string> Filters { get; init; } = new();

        public Dictionary<string, string> RouteFor(int page) =>
            new(Filters) { ["page"] = page.ToString() };

        // đưa số trang về khoảng hợp lệ [1, tổng số trang] (vd: ?page=-3 hoặc ?page=999)
        public static int Normalize(int page, int totalItems, int pageSize = DefaultPageSize)
        {
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
            return Math.Clamp(page, 1, totalPages);
        }

        // chỉ giữ bộ lọc có giá trị để URL gọn
        public static Dictionary<string, string> BuildFilters(params (string Key, string? Value)[] values) =>
            values.Where(v => !string.IsNullOrWhiteSpace(v.Value))
                  .ToDictionary(v => v.Key, v => v.Value!.Trim());
    }
}
