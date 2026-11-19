using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DoAnLTW.Services.Chatbot;

/// <summary>xử lý chuỗi tiếng Việt: bỏ dấu để so khớp, làm sạch văn bản trước khi đưa vào prompt.</summary>
public static class VietnameseText
{
    // hạ chữ thường và bỏ dấu (đ -> d), giữ nguyên chữ số và ký hiệu (để đọc giá: < > - ~ . ,)
    public static string RemoveDiacritics(string text)
    {
        var lower = text.ToLowerInvariant().Replace('đ', 'd');
        var decomposed = lower.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch);
            }
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    // chỉ còn chữ + số, cách nhau đúng một dấu cách (dùng để so khớp từ khóa)
    public static string Normalize(string text) =>
        Regex.Replace(RemoveDiacritics(text), "[^a-z0-9]+", " ").Trim();

    // làm sạch dữ liệu trước khi đưa vào prompt: bỏ ký tự điều khiển, bỏ < > (không cho phá dấu phân cách khối dữ liệu),
    // gộp khoảng trắng, cắt độ dài.
    public static string CleanForPrompt(string? text, int maxChars)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var sb = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            if (ch == '<' || ch == '>') sb.Append(' ');
            else if (char.IsControl(ch)) sb.Append(' ');
            else sb.Append(ch);
        }

        var cleaned = Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
        return cleaned.Length <= maxChars ? cleaned : cleaned[..maxChars].TrimEnd() + "…";
    }
}
