
namespace CS2_DecoyXrayScanner.Models;

public sealed class ActiveGlow
{
    public int TargetSlot { get; init; }
    public double ExpireGameTime { get; set; }
    public int RelayIndex { get; set; }
    public int GlowIndex { get; set; }
    public bool IsAlly { get; init; }
}