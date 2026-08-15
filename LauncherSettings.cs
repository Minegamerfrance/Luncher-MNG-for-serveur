using System.Text.Json;

namespace MNGLauncher;

public sealed class LauncherSettings
{
    public string ServerDirectory { get; set; } = @"C:\Users\Mineg\Desktop\serveur fifa 17\fifa serveur";
    public string FifaExecutable { get; set; } = @"C:\Users\Mineg\Desktop\serveur fifa 17\FIFA 17\FIFA17.exe";
    public string Email { get; set; } = "maxence30evrard@gmail.com";
    public string PersonaName { get; set; } = "maxence30evrard";
    public long Uid { get; set; } = 1000000001;
    public long PersonaId { get; set; } = 1000000001;

    public static string DataDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MNGLauncher");
    public static string SettingsPath => Path.Combine(DataDirectory, "settings.json");

    public static LauncherSettings Load()
    {
        Directory.CreateDirectory(DataDirectory);
        if (!File.Exists(SettingsPath)) return new();
        try { return JsonSerializer.Deserialize<LauncherSettings>(File.ReadAllText(SettingsPath)) ?? new(); }
        catch { return new(); }
    }

    public void Save()
    {
        Directory.CreateDirectory(DataDirectory);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
