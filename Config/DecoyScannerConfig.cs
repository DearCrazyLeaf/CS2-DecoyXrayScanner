
namespace CS2_DecoyXrayScanner.Config;

public sealed class DecoyScannerConfig
{
    // Master enable switch
    public bool Enabled { get; set; } = true;

    // Command permissions (OR logic). Empty list => unrestricted
    public List<string> AdminPermissions { get; set; } = [];

    // Trigger permissions (who can activate scanning). OR logic. Empty list => everyone can trigger
    public List<string> UseGlowPermissions { get; set; } = [];

    // How many pulses per decoy (default 2)
    public int PulseCount { get; set; } = 2;
    // Radius to scan each pulse
    public float PulseRadius { get; set; } = 800f;
    // Interval between pulses (seconds)
    public float PulseIntervalSeconds { get; set; } = 3f;
    // Glow lifetime per pulse (seconds)
    public float GlowDurationSeconds { get; set; } = 1f;
    // Delay after decoy throw before first pulse (seconds)
    public float FirstPulseDelaySeconds { get; set; } = 1.5f;
    // Include teammates in glows (false = only enemies)
    public bool IncludeTeamMates { get; set; } = false;
    // Enemy glow color (HTML hex)
    public string EnemyGlowColor { get; set; } = "#FF0000";
    // Ally glow color (used only if IncludeTeamMates = true)
    public string AllyGlowColor { get; set; } = "#FF0000";
    // Sound (engine relative path) played once per pulse; leave empty to disable.
    public string PulseSound { get; set; } = "ui/competitive_accept_beep.vsnd";
}