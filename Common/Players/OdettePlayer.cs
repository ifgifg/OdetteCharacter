using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;
using OdetteCharacter.Common.Compatibility;
using OdetteCharacter.Common.Config;
using OdetteCharacter.Common.Drawing;

namespace OdetteCharacter.Common.Players;

public sealed class OdettePlayer : ModPlayer
{
    public const int BalletDelayTicks = 300;
    public const int BalletFrameTicks = 40;
    public const int BalletCycleTicks = 360;

    public static bool Enabled => ModContent.GetInstance<OdetteCharacterConfig>().Enabled && !AppearanceCompatibility.HasConflict;
    public int IdleTicks { get; private set; }
    public int PoseIndex => SelectPoseIndex(Player, IdleTicks);

    // This is a visual clock only. No movement/control/equipment fields are
    // written by this ModPlayer, including while showing a ballet sequence.
    public override void PostUpdate()
        => IdleTicks = Enabled && CanBallet(Player) ? Math.Min(IdleTicks + 1, 1_000_000_000) : 0;

    public static bool CanBallet(Player player)
        => !player.dead && !player.ghost && !player.invis && !player.CCed && !player.frozen
        && !player.webbed && !player.stoned && !player.tongued && !player.wet
        && !player.mount.Active && !player.sitting.isSitting && !player.sleeping.isSleeping && !player.isPettingAnimal
        && !player.pulley && player.grapCount == 0
        && Math.Abs(player.velocity.X) < 0.05f && Math.Abs(player.velocity.Y) < 0.05f
        && !player.controlLeft && !player.controlRight && !player.controlUp && !player.controlDown
        && !player.controlJump && !player.controlUseItem && !player.controlUseTile
        && !player.controlHook && !player.controlThrow && !player.controlMount
        && player.itemAnimation == 0 && player.itemTime == 0
        && !HeldItemPose.ShouldPoseForHeldItem(player, player.lastVisualizedSelectedItem)
        && !HeldItemPose.ShouldPoseForHeldItem(player, player.HeldItem);

    public static int SelectPoseIndex(Player player, int idleTicks)
    {
        if (player.mount.Active || player.sitting.isSitting || player.sleeping.isSleeping) return 7;
        float vertical = player.velocity.Y * player.gravDir;
        if (vertical < -0.25f) return 5;
        if (vertical > 0.25f) return 6;
        if (Math.Abs(player.velocity.X) > 0.12f)
            return 1 + Math.Clamp((player.legFrame.Y / Math.Max(1, player.legFrame.Height) - 7) * 4 / 13, 0, 3);
        if (Enabled && idleTicks >= BalletDelayTicks && CanBallet(player)) {
            int phase = (idleTicks - BalletDelayTicks) % BalletCycleTicks;
            if (phase < 4 * BalletFrameTicks) return 8 + phase / BalletFrameTicks;
        }
        return 0;
    }

    public override void UpdateDead() => IdleTicks = 0;
    public override void OnRespawn() => IdleTicks = 0;
    public override void OnEnterWorld()
    {
        IdleTicks = 0;
        if (Player.whoAmI == Main.myPlayer && ModContent.GetInstance<OdetteCharacterConfig>().Enabled && AppearanceCompatibility.HasConflict)
            Main.NewText(Language.GetTextValue("Mods.OdetteCharacter.AppearanceConflict"), new Color(230, 205, 130));
    }

    public override void HideDrawLayers(PlayerDrawSet drawInfo)
    {
        if (!Enabled || CharacterAtlas.Data == null || Player.dead || Player.ghost || drawInfo.hideEntirePlayer) return;
        foreach (PlayerDrawLayer layer in PlayerDrawLayerLoader.Layers) {
            PlayerDrawLayer source = layer is PlayerDrawLayerSlot slot ? slot.Layer : layer;
            if (source.Mod == Mod || KeepNativeLayers.Contains(source)) continue;
            layer.Hide();
        }
    }

    public override void TransformDrawData(ref PlayerDrawSet drawInfo) => CharacterDrawing.AlignUmbrellaHand(ref drawInfo);

    private static readonly HashSet<PlayerDrawLayer> KeepNativeLayers = new() {
        PlayerDrawLayers.HeldItem, PlayerDrawLayers.ProjectileOverArm,
        PlayerDrawLayers.MountBack, PlayerDrawLayers.MountFront, PlayerDrawLayers.Carpet,
        PlayerDrawLayers.PortableStool, PlayerDrawLayers.Pulley,
        PlayerDrawLayers.ElectrifiedDebuffBack, PlayerDrawLayers.ElectrifiedDebuffFront,
        PlayerDrawLayers.WebbedDebuffBack, PlayerDrawLayers.FrozenOrWebbedDebuff,
        PlayerDrawLayers.IceBarrier, PlayerDrawLayers.CaptureTheGem, PlayerDrawLayers.BeetleBuff
    };
}
