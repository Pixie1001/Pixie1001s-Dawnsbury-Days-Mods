using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Modding;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using Dawnsbury.Core.Creatures.Parts;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Ids {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class CreatureIds {

        internal static CreatureId UnseenGuardian { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_UnseenGuardian");
        internal static CreatureId HuntingSpider { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_HuntingSpider");
        internal static CreatureId DrowAssassin { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_DrowAssassin");
        internal static CreatureId DrowFighter { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_DrowFighter");
        internal static CreatureId DrowPriestess { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_DrowPriestess");
        internal static CreatureId DrowArcanist { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_DrowArcanist");
        internal static CreatureId DrowShadowcaster { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_DrowShadowcaster");
        internal static CreatureId DrowInquisitrix { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_DrowInquisitrix");
        internal static CreatureId DrowTempleGuard { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_DrowTempleGuard");
        internal static CreatureId DrowShootist { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_DrowShootist");
        internal static CreatureId DrowSniper { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_DrowSniper");
        internal static CreatureId Drider { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_Drider");
        internal static CreatureId WitchCrone { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_WitchCrone");
        internal static CreatureId WitchMother { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_WitchMother");
        internal static CreatureId WitchMaiden { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_WitchMaiden");
        internal static CreatureId RavenousRat { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_RavenousRat");
        internal static CreatureId TreasureDemon { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_TreasureDemon");
        internal static CreatureId DrowRenegade { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_DrowRenegade");
        internal static CreatureId DevotedCultist { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_DevotedCultist");
        internal static CreatureId AbyssalHandmaiden { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_AbyssalHandmaiden");
        internal static CreatureId Nuglub { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_Nuglub");
        internal static CreatureId Homunculus { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_Homunculus");
        internal static CreatureId CrawlingHand { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_CrawlingHand");
        internal static CreatureId AnimatedStatue { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_AnimatedStatue");
        internal static CreatureId BebilithSpawn { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_BebilithSpawn");
        internal static CreatureId DrowNecromancer { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_DrowNecromancer");
        internal static CreatureId OwlBear { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_OwlBear");
        internal static CreatureId YoungWhiteDragon { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_YoungWhiteDragon");
        internal static CreatureId MerfolkHarrier { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_MerfolkHarrier");
        internal static CreatureId MerfolkBrineBlade { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_MerfolkBrineBlade");
        internal static CreatureId MerfolkKrakenBorn { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_MerfolkKrakenBorn");
        internal static CreatureId MerfolkSeaWitch { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_MerfolkSeaWitch");
        internal static CreatureId HuntingShark { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_HuntingShark");
        internal static CreatureId UnicornFoal { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_UnicornFoal");
        internal static CreatureId MinnowOfMulnok { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_MinnowOfMulnok");
        internal static CreatureId Bodyguard { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_Bodyguard");
        internal static CreatureId Alchemist { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_Alchemist");
        internal static CreatureId Crocodile { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_Crocodile");
        internal static CreatureId Pikeman { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_Pikeman");
        internal static CreatureId Ardamok { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_Ardamok");
        internal static CreatureId CorruptedTree { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_CorruptedTree");
        internal static CreatureId EchidnaditeMonsterBound { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_EchidnaditeMonsterBound");
        internal static CreatureId EchidnaditeWombCultist { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_EchidnaditeWombCultist");
        internal static CreatureId EchidnaditeBroodGuard { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_EchidnaditeBroodGuard");
        internal static CreatureId EchidnaditeShifter { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_EchidnaditeShifter");
        internal static CreatureId WinterWolf { get; } = ModManager.RegisterEnumMember<CreatureId>("RL_WinterWolf");

    }
}
