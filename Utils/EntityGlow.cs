using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace CS2_DecoyXrayScanner.Utils;

/// <summary>
/// Entity-based glow helper (relay + glow CDynamicProp) adapted from GameModifiers Xray logic.
/// Provides creation & removal of glow entities for a player pawn with custom color.
/// </summary>
internal static class EntityGlow
{
    private static readonly HashSet<(int relay,int glow)> Active = new();

    public static bool TryApplyPlayerGlow(CCSPlayerController player, Color color, out int relayIndex, out int glowIndex)
    {
        relayIndex = glowIndex = -1;
        var pawn = player.PlayerPawn?.Value;
        if (pawn == null || !pawn.IsValid) return false;
        if (!ApplyEntityGlowEffect(pawn, out var relay, out var glow)) return false;
        if (relay == null || glow == null) return false;
        try
        {
            // Override color per request
            glow.Glow.GlowColorOverride = color;
            // Keep wide range & outline style similar to original (type 3 for bounds + outline)
            glow.Glow.GlowType = 3;
            glow.Glow.GlowRange = 5000;
            glow.Glow.GlowRangeMin = 20;
        }
        catch { }
        relayIndex = (int)relay.Index;
        glowIndex = (int)glow.Index;
        Active.Add((relayIndex, glowIndex));
        return true;
    }

    public static void RemoveGlow(int relayIndex, int glowIndex)
    {
        try
        {
            var modelRelay = Utilities.GetEntityFromIndex<CDynamicProp>(relayIndex);
            if (modelRelay != null && modelRelay.IsValid) modelRelay.AcceptInput("Kill");
            var modelGlow = Utilities.GetEntityFromIndex<CDynamicProp>(glowIndex);
            if (modelGlow != null && modelGlow.IsValid) modelGlow.AcceptInput("Kill");
        }
        catch { }
        finally { Active.Remove((relayIndex, glowIndex)); }
    }

    public static void RemoveAll()
    {
        foreach (var (relay, glow) in Active) RemoveGlow(relay, glow);
        Active.Clear();
    }

    private static bool ApplyEntityGlowEffect(CBaseEntity? entity, out CDynamicProp? modelRelay, out CDynamicProp? modelGlow)
    {
        modelRelay = null; modelGlow = null;
        if (entity == null) return false;
        try
        {
            modelRelay = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
            modelGlow = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
            if (modelRelay == null || !modelRelay.IsValid || modelGlow == null || !modelGlow.IsValid)
                return false;
            string modelName = entity.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName;
            modelRelay.Spawnflags = 256u;
            modelRelay.RenderMode = RenderMode_t.kRenderNone;
            modelRelay.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);
            modelRelay.SetModel(modelName);
            modelRelay.DispatchSpawn();
            modelRelay.AcceptInput("FollowEntity", entity, modelRelay, "!activator");
            modelGlow.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);
            modelGlow.SetModel(modelName);
            modelGlow.DispatchSpawn();
            modelGlow.AcceptInput("FollowEntity", modelRelay, modelGlow, "!activator");
            modelGlow.Render = Color.FromArgb(1, 255, 255, 255);
            modelGlow.Spawnflags = 256u;
            modelGlow.RenderMode = RenderMode_t.kRenderGlow;
            modelGlow.Glow.GlowRange = 5000;
            modelGlow.Glow.GlowTeam = -1;
            modelGlow.Glow.GlowType = 3;
            modelGlow.Glow.GlowRangeMin = 20;
            return true;
        }
        catch { return false; }
    }
}
