using System.Numerics;
using System.Drawing;
using System.Text.Json;
using CS2_DecoyXrayScanner.Models;
using CS2_DecoyXrayScanner.Config;
using CS2_DecoyXrayScanner.Utils;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CSVector = CounterStrikeSharp.API.Modules.Utils.Vector;
using Microsoft.Extensions.Localization;

namespace CS2_DecoyXrayScanner;

public sealed class DecoyScannerPlugin : BasePlugin
{
    public override string ModuleName => "DecoyScanner";
    public override string ModuleAuthor => "DearCrazyLeaf";
    public override string ModuleDescription => "Decoy grenade triggered scanning X-ray pulses";
    public override string ModuleVersion => "1.0.5";

    private readonly List<ActiveGlow> _activeGlows = new();
    private readonly object _lock = new();
    private DecoyScannerConfig _config = new();
    private Color _enemyColor = Color.Red;
    private Color _allyColor = Color.Red;
    private string ConfigFilePath => Path.Combine(ModuleDirectory, "decoy_scanner_config.json");
    private bool _runtimeEnabled = true;
    private readonly IStringLocalizer<DecoyScannerPlugin>? _loc;

    public DecoyScannerPlugin() { }
    public DecoyScannerPlugin(IStringLocalizer<DecoyScannerPlugin> localizer) { _loc = localizer; }

    private int PulsesPerDecoy => Math.Clamp(_config.PulseCount, 1, 10);
    private float PulseIntervalSeconds => Math.Clamp(_config.PulseIntervalSeconds, 0.2f, 30f);

    public override void Load(bool hotReload)
    {
        LoadConfig();
        _runtimeEnabled = _config.Enabled;
        ApplyColors();
        RegisterListener<Listeners.OnClientDisconnect>(OnClientDisconnect);
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        RegisterEventHandler<EventWeaponFire>(OnWeaponFire);
        RegisterCommands();
    }

    public override void Unload(bool hotReload)
    {
        ClearAllGlows();
        RemoveListener<Listeners.OnClientDisconnect>(OnClientDisconnect);
        RemoveListener<Listeners.OnMapStart>(OnMapStart);
        DeregisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        DeregisterEventHandler<EventRoundStart>(OnRoundStart);
        DeregisterEventHandler<EventRoundEnd>(OnRoundEnd);
        DeregisterEventHandler<EventWeaponFire>(OnWeaponFire);
    }

    private string L(string key, params object[] args)
    {
        if (_loc == null) return args == null || args.Length == 0 ? key : string.Format(key, args);
        var str = _loc[key];
        return (args == null || args.Length == 0) ? str : string.Format(str, args);
    }

    private void RegisterCommands()
    {
        AddCommand("css_decoyxr_reload", "Reload decoy scanner config", CommandReloadConfig);
        AddCommand("css_decoyxr_clear", "Clear all active decoy scanner glows", CommandClearGlows);
        AddCommand("css_decoyxr_info", "Show decoy scanner status", CommandInfo);
        AddCommand("css_decoyxr_enable", "Enable decoy scanner pulses", CommandEnable);
        AddCommand("css_decoyxr_disable", "Disable decoy scanner pulses", CommandDisable);
    }

    #region Permissions
    private bool HasPermission(CCSPlayerController? player) => PermissionUtils.HasAny(player, _config.AdminPermissions);
    private bool HasGlowPermission(CCSPlayerController? player) => PermissionUtils.HasAny(player, _config.UseGlowPermissions);

    private bool CheckPermissions(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null) return true; // console always allowed
        if (HasPermission(player)) return true;
        info.ReplyToCommand(L("DecoyXR.NoPermission"));
        return false;
    }
    #endregion

    #region Command Handlers
    private void CommandReloadConfig(CCSPlayerController? player, CommandInfo info)
    { if (!CheckPermissions(player, info)) return; LoadConfig(); _runtimeEnabled = _config.Enabled; ApplyColors(); info.ReplyToCommand(L("DecoyXR.ConfigReloaded")); }
    private void CommandClearGlows(CCSPlayerController? player, CommandInfo info)
    { if (!CheckPermissions(player, info)) return; ClearAllGlows(); info.ReplyToCommand(L("DecoyXR.Cleared")); }
    private void CommandInfo(CCSPlayerController? player, CommandInfo info)
    { if (!CheckPermissions(player, info)) return; info.ReplyToCommand(L("DecoyXR.Info", _runtimeEnabled && _config.Enabled, PulsesPerDecoy, PulseIntervalSeconds, _config.PulseRadius, _config.GlowDurationSeconds, _config.IncludeTeamMates, _activeGlows.Count)); }
    private void CommandEnable(CCSPlayerController? player, CommandInfo info)
    { if (!CheckPermissions(player, info)) return; _runtimeEnabled = true; _config.Enabled = true; SaveConfig(); info.ReplyToCommand(L("DecoyXR.Enabled")); }
    private void CommandDisable(CCSPlayerController? player, CommandInfo info)
    { if (!CheckPermissions(player, info)) return; _runtimeEnabled = false; _config.Enabled = false; SaveConfig(); ClearAllGlows(); info.ReplyToCommand(L("DecoyXR.Disabled")); }
    #endregion

    #region Config
    private void LoadConfig()
    {
        try
        {
            if (!File.Exists(ConfigFilePath)) { _config = new DecoyScannerConfig(); SaveConfig(); }
            else _config = JsonSerializer.Deserialize<DecoyScannerConfig>(File.ReadAllText(ConfigFilePath)) ?? new DecoyScannerConfig();
        }
        catch { _config = new DecoyScannerConfig(); }
        _config.PulseRadius = Math.Clamp(_config.PulseRadius, 50f, 5000f);
        _config.GlowDurationSeconds = Math.Clamp(_config.GlowDurationSeconds, 0.05f, 10f);
    }
    private void SaveConfig()
    { try { File.WriteAllText(ConfigFilePath, JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true })); } catch { }
    }
    private void ApplyColors()
    { _enemyColor = ColorUtils.Parse(_config.EnemyGlowColor, Color.Red); _allyColor = ColorUtils.Parse(_config.AllyGlowColor, Color.Red); }
    #endregion

    private static Vector3 ToVec3(CSVector v) => new(v.X, v.Y, v.Z);
    private bool IsActive() => _runtimeEnabled && _config.Enabled;

    private sealed class DecoySequence
    {
        public int OwnerSlot { get; init; }
        public Vector3 InitialPos { get; init; }
        public int PulsesDone { get; set; }
        public int PulseLimit { get; init; }
        public bool Completed => PulsesDone >= PulseLimit;
    }

    private void StartDecoySequence(CCSPlayerController thrower, Vector3 approxPos)
    {
        var seq = new DecoySequence { OwnerSlot = thrower.Slot, InitialPos = approxPos, PulsesDone = 0, PulseLimit = PulsesPerDecoy };
        for (int i = 0; i < PulsesPerDecoy; i++)
        {
            float delay = _config.FirstPulseDelaySeconds + i * PulseIntervalSeconds;
            AddTimer(delay, () => ExecuteDecoyPulse(seq, thrower.Team));
        }
    }

    private void ExecuteDecoyPulse(DecoySequence seq, CsTeam team)
    {
        if (!IsActive()) return;
        if (seq.Completed) return;
        seq.PulsesDone++;
        Vector3 center = LocateDecoyCurrentPosition(seq) ?? seq.InitialPos;
        ScanAndMark(center, team);
        PlayPulseSound(team);
    }

    private void PlayPulseSound(CsTeam throwerTeam)
    {
        if (string.IsNullOrWhiteSpace(_config.PulseSound)) return;
        var targets = Utilities.GetPlayers().Where(p => p != null && p.IsValid && (p.Team == throwerTeam || p.Team == CsTeam.Spectator)).ToList();
        if (targets.Count == 0) return;
        Server.NextFrame(() =>
        {
            foreach (var p in targets)
            {
                if (p == null || !p.IsValid) continue;
                try { p.ExecuteClientCommand($"play {_config.PulseSound}"); } catch { }
            }
        });
    }

    private Vector3? LocateDecoyCurrentPosition(DecoySequence seq)
    {
        string[] candidateNames = { "decoy_projectile", "weapon_decoy", "decoygrenade" };
        foreach (var name in candidateNames)
        {
            try
            {
                var list = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>(name);
                if (list == null) continue;
                CBaseEntity? closest = null;
                float best = float.MaxValue;
                foreach (var ent in list)
                {
                    if (ent == null || !ent.IsValid) continue;
                    var pos = ent.AbsOrigin;
                    float dist = Vector3.Distance(seq.InitialPos, ToVec3(pos));
                    if (dist < best) { best = dist; closest = ent; }
                }
                if (closest != null) return ToVec3(closest.AbsOrigin);
            }
            catch { }
        }
        return null;
    }

    private void ScanAndMark(Vector3 center, CsTeam throwerTeam)
    {
        bool includeTeam = _config.IncludeTeamMates || TeammatesAreEnemies();
        var players = Utilities.GetPlayers();
        foreach (var target in players)
        {
            if (target == null || !target.IsValid || !target.PawnIsAlive) continue;
            var pawn = target.PlayerPawn?.Value; if (pawn == null || !pawn.IsValid) continue;
            if (!includeTeam && target.Team == throwerTeam) continue;
            var origin = pawn.AbsOrigin;
            if (Vector3.Distance(ToVec3(origin), center) > _config.PulseRadius) continue;
            EnsureGlow(target, target.Team == throwerTeam);
        }
    }

    private void EnsureGlow(CCSPlayerController targetPlayer, bool isAlly)
    {
        if (!IsActive()) return;
        lock (_lock)
        {
            if (!EntityGlow.TryApplyPlayerGlow(targetPlayer, isAlly ? _allyColor : _enemyColor, out var relayIdx, out var glowIdx)) return;
            var record = new ActiveGlow { TargetSlot = targetPlayer.Slot, ExpireGameTime = Server.CurrentTime + _config.GlowDurationSeconds, RelayIndex = relayIdx, GlowIndex = glowIdx, IsAlly = isAlly };
            _activeGlows.Add(record);
            AddTimer(_config.GlowDurationSeconds, () => RemoveGlowIfStill(record));
        }
    }

    private void RemoveGlowIfStill(ActiveGlow record)
    {
        lock (_lock)
        {
            var idx = _activeGlows.FindIndex(g => g.TargetSlot == record.TargetSlot && g.RelayIndex == record.RelayIndex && g.GlowIndex == record.GlowIndex);
            if (idx >= 0)
            {
                EntityGlow.RemoveGlow(record.RelayIndex, record.GlowIndex);
                _activeGlows.RemoveAt(idx);
            }
        }
    }

    private void RemoveTargetGlows(int slot)
    {
        lock (_lock)
        {
            for (int i = _activeGlows.Count - 1; i >= 0; i--)
            {
                if (_activeGlows[i].TargetSlot == slot)
                {
                    EntityGlow.RemoveGlow(_activeGlows[i].RelayIndex, _activeGlows[i].GlowIndex);
                    _activeGlows.RemoveAt(i);
                }
            }
        }
    }

    private void ClearAllGlows()
    {
        lock (_lock)
        {
            foreach (var g in _activeGlows) EntityGlow.RemoveGlow(g.RelayIndex, g.GlowIndex);
            _activeGlows.Clear();
        }
    }

    private bool TeammatesAreEnemies()
    {
        try
        {
            var cv = ConVar.Find("mp_teammates_are_enemies");
            if (cv == null) return false;
            try { return cv.GetPrimitiveValue<bool>(); } catch { }
            var raw = cv.StringValue?.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(raw)) return false;
            return raw == "1" || raw == "true" || raw == "yes" || raw == "on";
        }
        catch { return false; }
    }

    private HookResult OnWeaponFire(EventWeaponFire e, GameEventInfo info)
    {
        if (!IsActive()) return HookResult.Continue;
        var player = e.Userid; if (player == null || !player.IsValid || !player.PawnIsAlive) return HookResult.Continue;
        if (!string.Equals(e.Weapon, "weapon_decoy", StringComparison.OrdinalIgnoreCase)) return HookResult.Continue;
        if (!HasGlowPermission(player)) return HookResult.Continue;
        var pawn = player.PlayerPawn?.Value; if (pawn == null || !pawn.IsValid) return HookResult.Continue;
        Vector3 startPos = ToVec3(pawn.AbsOrigin);
        AddTimer(0.05f, () => { if (IsActive() && player.IsValid) StartDecoySequence(player, startPos); });
        return HookResult.Continue;
    }

    private HookResult OnPlayerDeath(EventPlayerDeath e, GameEventInfo info)
    { var p = e.Userid; if (p != null && p.IsValid) RemoveTargetGlows(p.Slot); return HookResult.Continue; }
    private void OnClientDisconnect(int slot) => RemoveTargetGlows(slot);
    private void OnMapStart(string map) => ClearAllGlows();
    private HookResult OnRoundStart(EventRoundStart e, GameEventInfo info) { ClearAllGlows(); return HookResult.Continue; }
    private HookResult OnRoundEnd(EventRoundEnd e, GameEventInfo info) { ClearAllGlows(); return HookResult.Continue; }
}
