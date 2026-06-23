using System.Text.Encodings.Web;
using System.Text.Json;

namespace KyushokuKanriSystem;

public sealed class AppRepository
{
    private readonly string _dataPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public AppRepository()
    {
        var appDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "KyushokuKanriSystem");
        Directory.CreateDirectory(appDir);
        _dataPath = Path.Combine(appDir, "data.json");
    }

    public AppData Load()
    {
        if (!File.Exists(_dataPath))
        {
            return new AppData();
        }

        var json = File.ReadAllText(_dataPath);
        return JsonSerializer.Deserialize<AppData>(json, _jsonOptions) ?? new AppData();
    }

    public void Save(AppData data)
    {
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        File.WriteAllText(_dataPath, json);
    }
}
