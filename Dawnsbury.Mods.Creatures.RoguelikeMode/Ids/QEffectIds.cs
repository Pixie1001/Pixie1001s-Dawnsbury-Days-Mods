using System;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Modding;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Ids {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class QEffectIds {

        internal static QEffectId LethargyPoison { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_Drow Lethargy Poison");
        internal static QEffectId AbyssalRot { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_Abyssal Rot");
        internal static QEffectId Lurking { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_Lurking");
        internal static QEffectId Stalked { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_Stalked");
        internal static QEffectId ExtraTurn { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_ExtraTurnMiniBoss");
        internal static QEffectId DrowClergy { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_DrowClergy");
        internal static QEffectId BloodBond { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_BloodBond");
        internal static QEffectId Harvested { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_Harvested");
        internal static QEffectId Hazard { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_Hazard");
        internal static QEffectId MushroomInoculation { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_MushroomInoculation");
        internal static QEffectId ChokingMushroomSporeCloud { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_ChokingMushroomSporeCloud");
        internal static QEffectId ShifterFurs { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_ShifterFurs");
        internal static QEffectId HomunculusMaster { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_HomunculusMaster");
        internal static QEffectId HomunculusPoison { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_HomunculusPoison");
        internal static QEffectId MarkQuarry { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_MarkQuarry");
        internal static QEffectId LoosenedGrip { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_LoosenedGrip");
        internal static QEffectId BloodcurdlingScreechImmunity { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_BloodcurdlingScreechImmunity");
        internal static QEffectId CommandSwarm { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_CommandSwarm");
        internal static QEffectId RatFamiliar { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_RatFamiliar");
        internal static QEffectId RatPlague { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_RatPlague");
        internal static QEffectId Exhausted { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_Exhausted");
        internal static QEffectId SigbinStenchImmunity { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_SigbinStenchImmunity");
        internal static QEffectId Parry { get; private set; }
        internal static QEffectId SerpentVenom { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_SerpentVenom");
        internal static QEffectId ShootingStarStance { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_ShootingStarStance");
        internal static QEffectId MonasticArcherStance { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_MonasticArcherStance");
        internal static QEffectId ShadowSlip { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_ShadowSlip");
        internal static QEffectId ShadowWebSickness { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_ShadowWebSickness");
        internal static QEffectId WebWardensCurse { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_WebWardensCurse");
        internal static QEffectId ImpendingPandemonium { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_ImpendingPandemonium");
        internal static QEffectId BlessedOfEchidna { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_BlessedOfEchidna");
        internal static QEffectId MonstrousGift { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_MonstrousGift");
        internal static QEffectId MaenadForm { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_MaenadForm");
        internal static QEffectId HydraStumps { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_HydraStumps");
        internal static QEffectId HydraHeads { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_HydraHeads");
        internal static QEffectId DemonicPower { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_DemonicPower");
        internal static QEffectId DrowTerrorTactics { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_DrowTerrorTactics");
        internal static QEffectId ThrowAngelfireUsedUp { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_ThrowAngelfireUsedUp");
        internal static QEffectId CannotChargeThisTurn { get; } = ModManager.RegisterEnumMember<QEffectId>("RL_CannotChargeThisTurn");

        // Tile Effects IDs
        internal static TileQEffectId ChokingSpores { get; } = ModManager.RegisterEnumMember<TileQEffectId>("RL_Choking Spores");
        internal static TileQEffectId Maelstrom { get; } = ModManager.RegisterEnumMember<TileQEffectId>("RL_Maelstrom");

        public static void RegisterConflictedIds() {
            Parry = ModManager.TryParse("Parry", out QEffectId parry) ? parry : ModManager.RegisterEnumMember<QEffectId>("Parry");
        }
    }
}
