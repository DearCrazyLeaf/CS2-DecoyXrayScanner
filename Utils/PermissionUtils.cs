using System;
using System.Collections.Generic;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Core;

namespace CS2_DecoyXrayScanner.Utils;

/// <summary>
/// Centralized permission helper. Supports tokens:
/// @perm  => Admin flag / permission id (passed directly to PlayerHasPermissions)
/// #group => Admin group name
/// steamid64 => direct steam id match
/// single letter (no prefix) => flag
/// other string => group name
/// OR logic across provided collection. Empty / null => unrestricted.
/// </summary>
public static class PermissionUtils
{
    public static bool HasAny(CCSPlayerController? player, List<string>? tokens)
    {
        if (player == null) return false;
        if (tokens == null || tokens.Count == 0) return true;
        foreach (var raw in tokens)
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            if (MatchSingle(player, raw.Trim())) return true;
        }
        return false;
    }

    private static bool MatchSingle(CCSPlayerController player, string token)
    {
        try
        {
            if (token.StartsWith("@"))
                return AdminManager.PlayerHasPermissions(player, token);
            if (token.StartsWith("#"))
                return AdminManager.PlayerInGroup(player, token[1..]);
            if (ulong.TryParse(token, out var sid))
                return player.SteamID == sid;
            if (token.Length == 1)
                return AdminManager.PlayerHasPermissions(player, token);
            return AdminManager.PlayerInGroup(player, token);
        }
        catch { return false; }
    }
}
