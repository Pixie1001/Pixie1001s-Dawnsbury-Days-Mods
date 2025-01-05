using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.StatBlocks.Description;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Text;
using Dawnsbury.IO;
using Dawnsbury.Modding;
using Dawnsbury.ThirdParty.SteamApi;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static System.Collections.Specialized.BitVector32;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.Intrinsics.Arm;
using System.Xml;
using Dawnsbury.Core.Mechanics.Damage;
using System.Runtime.CompilerServices;
using System.ComponentModel.Design;
using System.Text;
using static Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.BarbarianFeatsDb.AnimalInstinctFeat;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics.Metrics;
using Microsoft.Xna.Framework.Audio;
using static System.Reflection.Metadata.BlobBuilder;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Core.Animations.Movement;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Ids
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class ModEnums
    {
        internal enum CreatureId
        {
            UNSEEN_GUARDIAN,
            HUNTING_SPIDER,
            DROW_ASSASSIN,
            DROW_FIGHTER,
            DROW_PRIESTESS,
            DROW_ARCANIST,
            DROW_SHADOWCASTER,
            DROW_INQUISITRIX,
            DROW_TEMPLEGUARD,
            DROW_SHOOTIST,
            DROW_SNIPER,
            DRIDER,
            WITCH_CRONE,
            WITCH_MOTHER,
            WITCH_MAIDEN,
            RAVENOUS_RAT,
            TREASURE_DEMON,
            DROW_RENEGADE,
            WITCH_CULTIST,
            ABYSSAL_HANDMAIDEN,
            NUGLUB,
            HOMUNCULUS,
            CRAWLING_HAND,
            ANIMATED_STATUE,
            BEBILITH_SPAWN
        }

        internal enum ObjectId
        {
            CHOKING_MUSHROOM,
            BOOM_SHROOM,
            DEEP_HORNETS,
            SPIDER_QUEEN_SHRINE,
            RESTLESS_SPIRIT,
            ICE_FONT,
            DEMONIC_PUSTULE,
            TEST_PILE,
        }

        internal enum BoonId
        {
            POISON_IMMUNITY,
        }

        internal enum EncounterType
        {
            NORMAL,
            ELITE,
            BOSS,
            EVENT
        }
    }
}
