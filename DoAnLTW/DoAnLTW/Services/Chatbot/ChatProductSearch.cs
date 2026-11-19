using System.Globalization;
using System.Text.RegularExpressions;
using DoAnLTW.Data;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Services.Chatbot;

public record ChatProduct(
    int MaTui, string TenTui, string TenLoai, string TenThuongHieu, string? MoTa, string? Badge,
    int SoLuong, decimal GiaGoc, decimal DonGia, int PhanTramGiam, string? AnhFile)
{
    public bool DangGiam => DonGia < GiaGoc;
}

/// <param name="Exact">false = không có sản phẩm khớp mọi tiêu chí, danh sách là các mẫu gần nhất (đã nới lỏng tiêu chí)</param>
public record ChatSearchResult(IReadOnlyList<ChatProduct> Products, bool Exact, string FilterSummary);

public interface IChatProductSearch
{
    Task<ChatSearchResult> SearchAsync(string message, int maxItems, CancellationToken cancellationToken = default);
}

/// <summary>
/// truy xuất sản phẩm theo ý định trong câu hỏi, thuần SQL + C# (không vector DB, không gọi AI):
/// loại túi, thương hiệu, khoảng giá, giảm giá, từ khóa, sắp xếp. Kết quả là dữ liệu thật trong CSDL.
/// </summary>
public partial class ChatProductSearch(DbContext_TuiXach db, IProductPricingService pricing) : IChatProductSearch
{
    private enum SortMode { Relevance, PriceAsc, PriceDesc, BestSeller, NewArrival }

    private sealed record Candidate(ChatProduct Product, int Score);

    private static readonly string[] DiscountPhrases = ["giam gia", "khuyen mai", "sale", "uu dai", "dang giam", "dang sale"];

    // từ phổ biến, không mang ý nghĩa tìm sản phẩm (đã bỏ dấu)
    private static readonly HashSet<string> StopWords = new(
    [
        "toi", "minh", "muon", "can", "tim", "cho", "la", "co", "khong", "nao", "mot", "cai", "nhung", "cac", "va", "hay",
        "de", "di", "duoc", "ban", "shop", "gia", "tui", "xin", "chao", "giup", "goi", "tu", "van", "nhe", "oi", "voi",
        "bao", "nhieu", "duoi", "tren", "khoang", "tam", "den", "tr", "trieu", "nghin", "ngan", "dat", "dep", "tot",
        "nhat", "loai", "mau", "san", "pham", "mua", "gi", "nao", "the", "thi", "nay", "kia", "vay", "roi", "con", "hang",
        "tu van", "hoi", "xem", "dang", "giam", "khuyen", "sale", "uu", "dai", "re", "ben", "kieu"
    ]);

    public async Task<ChatSearchResult> SearchAsync(string message, int maxItems, CancellationToken ct = default)
    {
        var norm = VietnameseText.Normalize(message);
        var padded = " " + norm + " ";

        var categories = await db.LoaiTuis.AsNoTracking().ToListAsync(ct);
        var brands = await db.ThuongHieus.AsNoTracking().ToListAsync(ct);

        var matchedCategories = categories.Where(c => MatchesCategory(padded, c.tenLoai)).ToList();
        var matchedBrands = brands.Where(b => MatchesBrand(padded, b.tenThuongHieu)).ToList();
        var (minPrice, maxPrice) = ParsePrice(message);
        var wantDiscount = DiscountPhrases.Any(p => padded.Contains(" " + p + " "));
        var sort = DetectSort(padded);

        // từ khóa còn lại (nam, nữ, tên mẫu...) dùng để chấm điểm; bỏ các từ đã dùng làm bộ lọc loại/thương hiệu
        var consumed = new HashSet<string>(
            matchedCategories.SelectMany(c => VietnameseText.Normalize(c.tenLoai).Split(' '))
                .Concat(matchedBrands.SelectMany(b => VietnameseText.Normalize(b.tenThuongHieu).Split(' '))));
        var keywords = norm.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => !consumed.Contains(w) && !StopWords.Contains(w) && !w.All(char.IsDigit) && (w.Length >= 3 || w is "nam" or "nu"))
            .Distinct()
            .ToList();

        var categoryIds = matchedCategories.Select(c => c.maLoai).ToList();
        var brandIds = matchedBrands.Select(b => b.maThuongHieu).ToList();

        var scoped = await LoadCandidatesAsync(categoryIds, brandIds, keywords, ct);

        var categoryNames = matchedCategories.Select(c => c.tenLoai).ToList();
        var brandNames = matchedBrands.Select(b => b.tenThuongHieu).ToList();
        var hasPrice = minPrice is not null || maxPrice is not null;

        // các bước nới lỏng, theo thứ tự: đủ tiêu chí -> bỏ bớt giảm giá/khoảng giá (loại túi và thương hiệu luôn giữ)
        var steps = new List<(bool Price, bool Discount, string? Dropped)> { (hasPrice, wantDiscount, null) };
        if (hasPrice && wantDiscount)
        {
            steps.Add((true, false, "giảm giá"));
            steps.Add((false, true, "khoảng giá"));
            steps.Add((false, false, "khoảng giá và giảm giá"));
        }
        else if (hasPrice) steps.Add((false, false, "khoảng giá"));
        else if (wantDiscount) steps.Add((false, false, "giảm giá"));

        foreach (var (usePrice, useDiscount, dropped) in steps)
        {
            var min = usePrice ? minPrice : null;
            var max = usePrice ? maxPrice : null;
            var filtered = Filter(scoped, min, max, useDiscount);
            if (filtered.Count == 0) continue;

            return new ChatSearchResult(
                Take(filtered, sort, maxItems),
                dropped is null,
                BuildSummary(categoryNames, brandNames, min, max, useDiscount, dropped));
        }

        // loại túi/thương hiệu khách nêu hiện không còn hàng: gợi ý các mẫu nổi bật khác của cửa hàng
        var everything = categoryIds.Count > 0 || brandIds.Count > 0
            ? await LoadCandidatesAsync([], [], keywords, ct)
            : scoped;
        return new ChatSearchResult(
            Take(everything, sort, maxItems),
            false,
            "không có sản phẩm còn hàng khớp loại túi/thương hiệu khách nêu; đây là các mẫu nổi bật khác của cửa hàng");
    }

    private async Task<List<Candidate>> LoadCandidatesAsync(List<int> categoryIds, List<int> brandIds, List<string> keywords, CancellationToken ct)
    {
        var query = db.TuiXachs.AsNoTracking()
            .Include(p => p.LoaiTui)
            .Include(p => p.ThuongHieu)
            .Include(p => p.KhuyenMai)
            .Include(p => p.HinhAnhTuis)
            .Where(p => p.soLuong > 0);              // chỉ gợi ý sản phẩm còn hàng

        if (categoryIds.Count > 0) query = query.Where(p => categoryIds.Contains(p.maLoai));
        if (brandIds.Count > 0) query = query.Where(p => brandIds.Contains(p.maThuongHieu));

        var products = await query.OrderBy(p => p.maTui).Take(300).ToListAsync(ct);
        var now = DateTime.Now;

        return products.Select(p =>
        {
            var price = pricing.GetPrice(p, now);
            var percent = price.DonGiaGoc > 0 ? (int)Math.Round(price.SoTienGiam / price.DonGiaGoc * 100m) : 0;
            var image = p.HinhAnhTuis.OrderByDescending(h => h.laAnhChinh).ThenBy(h => h.thuTu).FirstOrDefault()?.tenHinhAnh;

            var haystack = " " + VietnameseText.Normalize($"{p.tenTui} {p.moTa} {p.badge} {p.LoaiTui?.tenLoai} {p.ThuongHieu?.tenThuongHieu}") + " ";
            var score = keywords.Count(k => haystack.Contains(" " + k + " "));

            return new Candidate(
                new ChatProduct(p.maTui, p.tenTui, p.LoaiTui?.tenLoai ?? "", p.ThuongHieu?.tenThuongHieu ?? "", p.moTa, p.badge,
                    p.soLuong, price.DonGiaGoc, price.DonGiaApDung, percent, image),
                score);
        }).ToList();
    }

    private static List<Candidate> Filter(List<Candidate> source, decimal? min, decimal? max, bool onlyDiscounted) =>
        source.Where(c =>
                (min is null || c.Product.DonGia >= min) &&
                (max is null || c.Product.DonGia <= max) &&
                (!onlyDiscounted || c.Product.DangGiam))
            .ToList();

    private static List<ChatProduct> Take(List<Candidate> source, SortMode sort, int maxItems)
    {
        IEnumerable<Candidate> ordered = sort switch
        {
            SortMode.PriceAsc => source.OrderBy(c => c.Product.DonGia).ThenByDescending(c => c.Score),
            SortMode.PriceDesc => source.OrderByDescending(c => c.Product.DonGia).ThenByDescending(c => c.Score),
            SortMode.BestSeller => source.OrderByDescending(c => IsBadge(c, "best seller")).ThenByDescending(c => c.Score).ThenBy(c => c.Product.DonGia),
            SortMode.NewArrival => source.OrderByDescending(c => IsBadge(c, "new arrival")).ThenByDescending(c => c.Score).ThenBy(c => c.Product.DonGia),
            _ => source.OrderByDescending(c => c.Score).ThenByDescending(c => c.Product.PhanTramGiam).ThenBy(c => c.Product.DonGia)
        };
        return ordered.Take(maxItems).Select(c => c.Product).ToList();
    }

    private static bool IsBadge(Candidate c, string badge) =>
        c.Product.Badge is not null && VietnameseText.Normalize(c.Product.Badge) == badge;

    private static SortMode DetectSort(string padded)
    {
        if (ContainsAny(padded, "re nhat", "gia thap", "tiet kiem", "binh dan", "gia re")) return SortMode.PriceAsc;
        if (ContainsAny(padded, "dat nhat", "cao cap", "sang trong", "gia cao")) return SortMode.PriceDesc;
        if (ContainsAny(padded, "ban chay", "best seller", "hot")) return SortMode.BestSeller;
        if (ContainsAny(padded, "moi", "new arrival", "moi ve")) return SortMode.NewArrival;
        return SortMode.Relevance;
    }

    private static bool ContainsAny(string padded, params string[] phrases) =>
        phrases.Any(p => padded.Contains(" " + p + " "));

    // "Túi đeo chéo" khớp khi khách gõ "túi đeo chéo" hoặc "đeo chéo"; "Balo" khớp "balo"
    private static bool MatchesCategory(string padded, string categoryName)
    {
        var n = VietnameseText.Normalize(categoryName);
        var alt = n.StartsWith("tui ") ? n[4..] : n;
        return new[] { n, alt }.Any(c => c.Length >= 4 && padded.Contains(" " + c + " "));
    }

    // "CHARLES & KEITH" khớp khi câu hỏi có cả hai từ "charles" và "keith"
    private static bool MatchesBrand(string padded, string brandName)
    {
        var words = VietnameseText.Normalize(brandName).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length > 0 && words.Sum(w => w.Length) >= 3 && words.All(w => padded.Contains(" " + w + " "));
    }

    private static string BuildSummary(IEnumerable<string> categories, IEnumerable<string> brands,
        decimal? min, decimal? max, bool discounted, string? droppedCriteria)
    {
        var parts = new List<string>();
        var cats = categories.ToList();
        var brs = brands.ToList();
        if (cats.Count > 0) parts.Add("loại túi: " + string.Join(", ", cats));
        if (brs.Count > 0) parts.Add("thương hiệu: " + string.Join(", ", brs));
        if (min is not null && max is not null) parts.Add($"giá từ {min:N0}đ đến {max:N0}đ");
        else if (max is not null) parts.Add($"giá tối đa {max:N0}đ");
        else if (min is not null) parts.Add($"giá từ {min:N0}đ trở lên");
        if (discounted) parts.Add("chỉ sản phẩm đang giảm giá");

        var summary = parts.Count > 0 ? string.Join("; ", parts) : "không có tiêu chí cụ thể";
        return droppedCriteria is null
            ? summary
            : $"{summary}. Lưu ý: không có mẫu nào khớp cả {droppedCriteria}, nên đã bỏ tiêu chí đó và liệt kê các mẫu gần nhất";
    }

    // ---------- đọc khoảng giá từ câu hỏi tiếng Việt ----------

    private sealed record Amount(decimal Raw, decimal? Multiplier, int Start, int End);

    [GeneratedRegex(@"(\d{1,3}(?:[.,]\d{3})+|\d+(?:[.,]\d+)?)\s*(k|nghin|ngan|trieu|tr|m|vnd|dong)?(?![a-z0-9])")]
    private static partial Regex AmountRegex();

    [GeneratedRegex(@"^\s*(den|toi|-|~|->)\s*$")]
    private static partial Regex RangeJoinRegex();

    // "dưới 500k", "từ 500k đến 1 triệu", "500-800k", "1tr5", "khoảng 700k", "trên 1 triệu"
    // trả về (min, max) bằng đồng; (null, null) nếu không có khoảng giá.
    internal static (decimal? Min, decimal? Max) ParsePrice(string message)
    {
        var t = VietnameseText.RemoveDiacritics(message);
        t = Regex.Replace(t, @"(\d+)\s*(?:tr|trieu|m)\s*(\d)(?!\d)", "$1.$2tr");   // "1tr5" -> "1.5tr"

        var amounts = new List<Amount>();
        foreach (Match m in AmountRegex().Matches(t))
        {
            var numText = m.Groups[1].Value;
            var raw = Regex.IsMatch(numText, @"^\d{1,3}([.,]\d{3})+$")
                ? decimal.Parse(Regex.Replace(numText, "[.,]", ""), CultureInfo.InvariantCulture)
                : decimal.Parse(numText.Replace(',', '.'), CultureInfo.InvariantCulture);

            decimal? multiplier = m.Groups[2].Value switch
            {
                "k" or "nghin" or "ngan" => 1_000m,
                "tr" or "trieu" or "m" => 1_000_000m,
                "vnd" or "dong" => 1m,
                _ => null
            };
            amounts.Add(new Amount(raw, multiplier, m.Index, m.Index + m.Length));
        }

        if (amounts.Count == 0) return (null, null);

        // khoảng "từ X đến Y" / "X - Y": đơn vị của một vế áp dụng cho cả hai ("2 đến 3 triệu")
        for (var i = 0; i + 1 < amounts.Count; i++)
        {
            var a = amounts[i];
            var b = amounts[i + 1];
            if (!RangeJoinRegex().IsMatch(t[a.End..b.Start])) continue;

            var priceLike = a.Multiplier is not null || b.Multiplier is not null || a.Raw >= 10_000 || HasPriceWordBefore(t, a.Start);
            if (!priceLike) continue;

            var v1 = ToVnd(a, b.Multiplier);
            var v2 = ToVnd(b, a.Multiplier);
            return (Math.Min(v1, v2), Math.Max(v1, v2));
        }

        // một mức giá: dưới/tối đa -> max, trên/ít nhất -> min, còn lại hiểu là "khoảng" (±20%)
        foreach (var a in amounts)
        {
            if (!(a.Multiplier is not null || a.Raw >= 10_000 || HasPriceWordBefore(t, a.Start))) continue;

            var value = ToVnd(a, null);
            var before = t[Math.Max(0, a.Start - 20)..a.Start];
            var after = t[a.End..Math.Min(t.Length, a.End + 12)];

            if (Regex.IsMatch(before, @"(duoi|toi da|khong qua|khong vuot|nho hon|re hon|max|<)\s*(khoang\s*)?$")
                || Regex.IsMatch(after, @"^\s*(tro xuong|do lai)"))
            {
                return (null, value);
            }

            if (Regex.IsMatch(before, @"(tren|tu|it nhat|lon hon|min|>)\s*$")
                || Regex.IsMatch(after, @"^\s*(tro len|do len)"))
            {
                return (value, null);
            }

            return (Math.Round(value * 0.8m), Math.Round(value * 1.2m));
        }

        return (null, null);
    }

    // số trần dưới 10.000 mà không có đơn vị được hiểu là nghìn đồng ("dưới 500" = 500k) khi đứng trong ngữ cảnh giá
    private static decimal ToVnd(Amount a, decimal? inheritedMultiplier)
    {
        var multiplier = a.Multiplier ?? inheritedMultiplier;
        if (multiplier is not null) return a.Raw * multiplier.Value;
        return a.Raw < 10_000 ? a.Raw * 1_000m : a.Raw;
    }

    private static bool HasPriceWordBefore(string text, int index) =>
        Regex.IsMatch(text[Math.Max(0, index - 15)..index], @"(duoi|tren|tu|den|toi|khoang|tam|gia|max)\s*$");
}
