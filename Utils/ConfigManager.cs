using System.Text.Json;
using System.Text.Json.Serialization;
using CS2_DecoyXrayScanner.Config;

namespace CS2_DecoyXrayScanner.Utils;

public sealed class ConfigManager
{
    private readonly string _path;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public DecoyScannerConfig Current { get; private set; } = new();

    public ConfigManager(string baseDirectory)
    {
        _path = Path.Combine(baseDirectory, "decoy_scanner.json");
    }

    public void Load()
    {
        try
        {
            if (!File.Exists(_path))
            {
                Current = new DecoyScannerConfig();
                Save();
            }
            else
            {
                Current = JsonSerializer.Deserialize<DecoyScannerConfig>(File.ReadAllText(_path), JsonOpts) ?? new DecoyScannerConfig();
            }
        }
        catch
        {
            Current = new DecoyScannerConfig();
        }
    }

    public void Save()
    {
        try
        {
            File.WriteAllText(_path, JsonSerializer.Serialize(Current, JsonOpts));
        }
        catch { }
    }
}
