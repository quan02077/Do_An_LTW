using Microsoft.Extensions.Configuration.Json;

namespace DoAnLTW.Services;

/// <summary>
/// đọc file .env (mỗi dòng KEY=VALUE, dòng bắt đầu bằng # là chú thích) vào cấu hình ứng dụng.
/// Dùng cho môi trường phát triển để đặt khóa API cục bộ mà không phải gõ lệnh.
/// - Phân cấp bằng "__" hoặc ":", ví dụ Chatbot__Groq__ApiKey tương đương Chatbot:Groq:ApiKey.
/// - Thứ tự ưu tiên: appsettings*.json < .env < user-secrets/biến môi trường/dòng lệnh (nguồn bên phải luôn thắng).
/// - Không bao giờ ghi giá trị ra log.
/// </summary>
public static class DotEnvConfiguration
{
    public static void AddDotEnvFile(this ConfigurationManager configuration, string path)
    {
        if (!File.Exists(path)) return;

        var values = new Dictionary<string, string?>();
        foreach (var rawLine in File.ReadAllLines(path))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            if (line.StartsWith("export ", StringComparison.Ordinal)) line = line[7..].TrimStart();

            var separator = line.IndexOf('=');
            if (separator <= 0) continue;

            var key = line[..separator].Trim().Replace("__", ":");
            var value = line[(separator + 1)..].Trim();

            // cho phép bọc giá trị trong dấu nháy: KEY="abc" hoặc KEY='abc'
            if (value.Length >= 2 && ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\'')))
            {
                value = value[1..^1];
            }

            // dòng để trống (như trong .env.example) bị bỏ qua, không ghi đè cấu hình mặc định bằng chuỗi rỗng
            if (key.Length == 0 || value.Length == 0) continue;

            if (!HasValueFromHigherPriority(configuration, key))
            {
                values[key] = value;
            }
        }

        if (values.Count > 0)
        {
            configuration.AddInMemoryCollection(values);
        }
    }

    // .env phải thắng appsettings*.json (giá trị mặc định) nhưng thua biến môi trường, user-secrets và dòng lệnh.
    // nên chỉ coi là "đã có giá trị" khi nguồn đó KHÔNG phải file appsettings*.json.
    private static bool HasValueFromHigherPriority(ConfigurationManager configuration, string key)
    {
        foreach (var provider in ((IConfigurationRoot)configuration).Providers)
        {
            if (provider is JsonConfigurationProvider json
                && Path.GetFileName(json.Source.Path)?.StartsWith("appsettings", StringComparison.OrdinalIgnoreCase) == true)
            {
                continue;
            }

            if (provider.TryGet(key, out var existing) && !string.IsNullOrEmpty(existing))
            {
                return true;
            }
        }

        return false;
    }
}
