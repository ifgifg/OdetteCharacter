using System;
using System.Reflection;
using Terraria.ModLoader;

namespace OdetteCharacter.Common.Compatibility;

public static class AppearanceCompatibility
{
    private static Mod? vodyanitsa;
    private static PropertyInfo? enabledProperty;

    // Vodyanitsa also owns every player appearance and hides other mod layers.
    // Yield to that older mod instead of allowing two independent hide passes
    // to produce an invisible player. Never edit its config or enabled list.
    public static bool HasConflict {
        get {
            if (vodyanitsa == null) return false;
            try {
                object config = vodyanitsa.GetConfig("VodyanitsaConfig");
                return enabledProperty?.GetValue(config) is not false;
            }
            catch { return true; }
        }
    }

    internal static void Initialize(Mod owner)
    {
        if (!ModLoader.TryGetMod("Vodyanitsa", out vodyanitsa)) return;
        try { enabledProperty = vodyanitsa.GetConfig("VodyanitsaConfig").GetType().GetProperty("Enabled", BindingFlags.Instance | BindingFlags.Public); }
        catch (Exception exception) { owner.Logger.Warn("Could not read the optional Vodyanitsa appearance setting; Odette will yield while it is loaded.", exception); }
        if (HasConflict) owner.Logger.Warn("Odette appearance is paused because Vodyanitsa is enabled. Disable Vodyanitsa or its Enabled setting to use OdetteCharacter. No settings were changed.");
    }

    internal static void Unload() { vodyanitsa = null; enabledProperty = null; }
}
