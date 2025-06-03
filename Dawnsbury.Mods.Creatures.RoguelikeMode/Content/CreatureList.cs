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
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModTraits;
using Dawnsbury.Campaign.Encounters.A_Crisis_in_Dawnsbury;
using System.Buffers;
using System.Xml.Schema;
using Microsoft.Xna.Framework.Input;
using FMOD;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class CreatureList {
        internal static Dictionary<CreatureId, Func<Encounter?, Creature>> Creatures = new Dictionary<CreatureId, Func<Encounter?, Creature>>();
        internal static Dictionary<ObjectId, Func<Encounter?, Creature>> Objects = new Dictionary<ObjectId, Func<Encounter?, Creature>>();


        /// <summary>
        /// Gets the Creature object based off the id
        /// </summary>
        internal static Creature GetCreature<TEnum>(TEnum id) where TEnum : Enum {
            switch (id) {
                case var v when v.Equals(CreatureIds.UnseenGuardian):
                    return UnseenGuardian.Create();
                case var v when v.Equals(CreatureIds.DrowAssassin):
                    return DrowAssassin.Create();
                case var v when v.Equals(CreatureIds.DrowFighter):
                    return DrowFighter.Create();
                case var v when v.Equals(CreatureIds.DrowShootist):
                    return DrowShootist.Create();
                case var v when v.Equals(CreatureIds.DrowSniper):
                    return DrowSniper.Create();
                case var v when v.Equals(CreatureIds.DrowPriestess):
                    return DrowPriestess.Create();
                case var v when v.Equals(CreatureIds.DrowTempleGuard):
                    return DrowTempleGuard.Create();
                case var v when v.Equals(CreatureIds.HuntingSpider):
                    return HuntingSpider.Create();
                case var v when v.Equals(CreatureIds.Drider):
                    return Drider.Create();
                case var v when v.Equals(CreatureIds.DrowArcanist):
                    return DrowArcanist.Create();
                case var v when v.Equals(CreatureIds.DrowShadowcaster):
                    return DrowShadowcaster.Create();
                case var v when v.Equals(CreatureIds.DrowInquisitrix):
                    return DrowInquisitrix.Create();
                case var v when v.Equals(CreatureIds.TreasureDemon):
                    return TreasureDemon.Create();
                case var v when v.Equals(CreatureIds.DrowRenegade):
                    return DrowRenegade.Create();
                case var v when v.Equals(CreatureIds.WitchCrone):
                    return WitchCrone.Create();
                case var v when v.Equals(CreatureIds.WitchMother):
                    return WitchMother.Create();
                case var v when v.Equals(CreatureIds.WitchMaiden):
                    return WitchMaiden.Create();
                case var v when v.Equals(CreatureIds.RavenousRat):
                    return RavenousRat.Create();
                case var v when v.Equals(CreatureIds.DevotedCultist):
                    return DevotedCultist.Create();
                case var v when v.Equals(CreatureIds.Nuglub):
                    return Nuglub.Create();
                case var v when v.Equals(CreatureIds.Homunculus):
                    return Homunculus.Create();
                case var v when v.Equals(CreatureIds.AbyssalHandmaiden):
                    return AbyssalHandmaiden.Create();
                case var v when v.Equals(CreatureIds.CrawlingHand):
                    return CrawlingHand.Create();
                case var v when v.Equals(CreatureIds.AnimatedStatue):
                    return AnimatedStatue.Create();
                case var v when v.Equals(CreatureIds.BebilithSpawn):
                    return BebilithSpawn.Create();
                case var v when v.Equals(CreatureIds.DrowNecromancer):
                    return DrowNecromancer.Create();
                case var v when v.Equals(CreatureIds.OwlBear):
                    return OwlBear.Create();
                case var v when v.Equals(CreatureIds.YoungWhiteDragon):
                    return YoungWhiteDragon.Create();
                case var v when v.Equals(CreatureIds.MerfolkHarrier):
                    return MerfolkHarrier.Create();
                case var v when v.Equals(CreatureIds.HuntingShark):
                    return HuntingShark.Create();
                case var v when v.Equals(CreatureIds.MerfolkBrineBlade):
                    return MerfolkBrineblade.Create();
                case var v when v.Equals(CreatureIds.MerfolkKrakenBorn):
                    return MerfolkKrakenborn.Create();
                case var v when v.Equals(CreatureIds.UnicornFoal):
                    return UnicornFoal.Create();
                case var v when v.Equals(CreatureIds.MinnowOfMulnok):
                    return MinnowOfMulnok.Create();
                case var v when v.Equals(CreatureIds.Alchemist):
                    return Alchemist.Create();
                case var v when v.Equals(CreatureIds.Bodyguard):
                    return Bodyguard.Create();
                case var v when v.Equals(CreatureIds.MerfolkSeaWitch):
                    return MerfolkSeaWitch.Create();
                case var v when v.Equals(CreatureIds.Crocodile):
                    return Crocodile.Create();
                case var v when v.Equals(CreatureIds.Pikeman):
                    return Pikeman.Create();
                case var v when v.Equals(CreatureIds.Ardamok):
                    return Ardamok.Create();
                case var v when v.Equals(CreatureIds.CorruptedTree):
                    return CorruptedTree.Create();
                case var v when v.Equals(CreatureIds.EchidnaditeWombCultist):
                    return EchidnaditeWombCultist.Create();
                case var v when v.Equals(CreatureIds.EchidnaditeMonsterBound):
                    return EchidnaditeMonsterBound.Create();
                case var v when v.Equals(CreatureIds.EchidnaditeBroodGuard):
                    return EchidnaditeBroodguard.Create();
                case var v when v.Equals(CreatureIds.EchidnaditeBroodNurse):
                    return EchidnaditeBroodNurse.Create();
                case var v when v.Equals(CreatureIds.WinterWolf):
                    return WinterWolf.Create();
                case var v when v.Equals(CreatureIds.Sigbin):
                    return Sigbin.Create();
                case var v when v.Equals(CreatureIds.Basilisk):
                    return Basilisk.Create();
                case var v when v.Equals(CreatureIds.Medusa):
                    return Medusa.Create();
                case var v when v.Equals(CreatureIds.PetrifiedGuardian):
                    return PetrifiedGuardian.Create();
                case var v when v.Equals(CreatureIds.DragonWitch):
                    return DragonWitch.Create();
                case var v when v.Equals(CreatureIds.YoungRedDragon):
                    return YoungRedDragon.Create();
                case var v when v.Equals(CreatureIds.Chimera):
                    return Chimera.Create();
                case var v when v.Equals(CreatureIds.YoungChimera):
                    return YoungChimera.Create();
                case var v when v.Equals(CreatureIds.FlailSnail):
                    return FlailSnail.Create();
                case var v when v.Equals(CreatureIds.DuplicityDemon):
                    return DuplicityDemon.Create();
                case var v when v.Equals(CreatureIds.DrowHuntress):
                    return DrowHuntress.Create();
                case var v when v.Equals(CreatureIds.ShadowWebStalker):
                    return ShadowWebStalker.Create();
                case var v when v.Equals(CreatureIds.NightmareWeaver):
                    return NightmareWeaver.Create();
                case var v when v.Equals(CreatureIds.WebWarden):
                    return WebWarden.Create();
                case var v when v.Equals(CreatureIds.DrowHighPriestess):
                    return DrowHighPriestess.Create();
                case var v when v.Equals(CreatureIds.PrincessOfPandemonium):
                    return PrincessOfPandemonium.Create();
                default:
                    throw new NotSupportedException($"The creature id '{id}' is not supported");
            }
        }

        /// <summary>
        /// Gets the Object object based off the id
        /// </summary>
        internal static Creature GetObject<TEnum>(TEnum id) where TEnum : Enum {
            switch (id)
            {
                case ModEnums.ObjectId.CHOKING_MUSHROOM:
                    return ChokingMushroom.Create();
                case ModEnums.ObjectId.BOOM_SHROOM:
                    return ExplosiveMushroom.Create();
                case ModEnums.ObjectId.ICE_FONT:
                    return IceFont.Create();
                case ModEnums.ObjectId.SPIDER_FONT:
                    return SpiderFont.Create();
                case ModEnums.ObjectId.SPIDER_QUEEN_SHRINE:
                    return SpiderQueenShrine.Create();
                case ModEnums.ObjectId.RESTLESS_SPIRIT:
                    return RestlessSpirit.Create();
                case ModEnums.ObjectId.DEMONIC_PUSTULE:
                    return DemonicPustule.Create();
                case ModEnums.ObjectId.TEST_PILE:
                    return TestPile.Create();
                default:
                    throw new NotSupportedException($"The object id '{id}' is not supported");
            }
        }

        /// <summary>
        /// Registers the creatures through the ModManager then adds the creature to the running creature list
        /// </summary>
        internal static void RegisterAndAddCreatureToDictonary<TEnum>(Dictionary<TEnum, Func<Encounter?, Creature>> creatures, TEnum id, string? overridenCreatureName = null) where TEnum : Enum {
            Func<Encounter?, Creature> creatureFunction = encounter => (typeof(TEnum) == typeof(CreatureId)) ? GetCreature(id) : GetObject(id);
            ModManager.RegisterNewCreature(overridenCreatureName ?? creatureFunction(null).Name, creatureFunction);
            creatures.Add(id, creatureFunction);
        }

        internal static void RegisterTile((string, Func<Tile, Encounter?, TileQEffect>) effect) {
            ModManager.RegisterCustomTile(effect.Item1, (tile, map, encounter) => {
                var qf = effect.Item2.Invoke(tile, encounter);
                tile.AddQEffect(qf);
            });
        }

        internal static void LoadCreatures() {
            // Level -1 Creatures
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.RavenousRat);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.CrawlingHand);

            // Level 0 Creatures
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.Bodyguard);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.Homunculus);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.MinnowOfMulnok);

            // Level 1 Creatures - Drow
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.DrowArcanist);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.DrowAssassin);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.DrowFighter);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.DrowShootist);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.DrowSniper);

            // Level 1 Creatures - Allies
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.DrowRenegade);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.UnicornFoal);

            // Level 1 Creatures - Other
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.DevotedCultist);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.MerfolkHarrier);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.HuntingSpider);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.Pikeman);

            // Level 2 Creatures - Drow
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.DrowInquisitrix);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.DrowNecromancer);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.DrowTempleGuard);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.Alchemist, "Alchemist");

            // Level 2 Creatures - Other

            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.BebilithSpawn);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.HuntingShark);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.MerfolkBrineBlade);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.MerfolkKrakenBorn);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.Nuglub);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.TreasureDemon);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.UnseenGuardian);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.Ardamok);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.Crocodile);

            // Level 3 Creatures - Drow
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.Drider);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.DrowPriestess);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.DrowShadowcaster);

            // Level 3 Creatures - Other
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.AnimatedStatue);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.WitchCrone, "Witch Crone");
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.WitchMother, "Witch Mother");
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.WitchMaiden, "Witch Maiden");

            // Level 4 Creatures
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.OwlBear);

            // Level 5 Creatures - Echidnadite
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.EchidnaditeWombCultist);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.EchidnaditeMonsterBound);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.EchidnaditeBroodNurse);

            // Level 5 creatures - Spider Demons
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.DuplicityDemon);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.ShadowWebStalker);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.WebWarden);

            // Level 5 Creatures - Other
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.CorruptedTree);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.MerfolkSeaWitch);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.Sigbin);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.WinterWolf);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.Basilisk);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.FlailSnail);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.PetrifiedGuardian);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.DrowHuntress);

            // Level 6 Creatures
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.AbyssalHandmaiden);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.YoungWhiteDragon);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.EchidnaditeBroodGuard);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.NightmareWeaver);

            // Level 7 Creatures
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.Medusa);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.DrowHighPriestess);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.PrincessOfPandemonium);

            // Level 8 Creatures
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.Chimera);

            // Level 10 creatures
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.DragonWitch);
            RegisterAndAddCreatureToDictonary(Creatures, CreatureIds.YoungRedDragon);
        }

        internal static void LoadObjects() {
            // Level 1 Hazards
            RegisterAndAddCreatureToDictonary(Objects, ModEnums.ObjectId.ICE_FONT, "Scaling Font of Ice");
            RegisterAndAddCreatureToDictonary(Objects, ModEnums.ObjectId.SPIDER_FONT, "Spider Font");

            // Level 2 Hazards
            RegisterAndAddCreatureToDictonary(Objects, ModEnums.ObjectId.CHOKING_MUSHROOM);
            RegisterAndAddCreatureToDictonary(Objects, ModEnums.ObjectId.BOOM_SHROOM);
            RegisterAndAddCreatureToDictonary(Objects, ModEnums.ObjectId.SPIDER_QUEEN_SHRINE);
            RegisterAndAddCreatureToDictonary(Objects, ModEnums.ObjectId.RESTLESS_SPIRIT);
            RegisterAndAddCreatureToDictonary(Objects, ModEnums.ObjectId.DEMONIC_PUSTULE);
            RegisterAndAddCreatureToDictonary(Objects, ModEnums.ObjectId.TEST_PILE, "TestPile");
        }

        internal static void LoadTiles() {
            RegisterTile(GrantMonsterMutator.Create());
            RegisterTile(TripWire.Create());
            RegisterTile(TripWireSpawnLocation.Create());
        }
    }
}
