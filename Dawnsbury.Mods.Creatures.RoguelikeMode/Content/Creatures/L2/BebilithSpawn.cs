using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class BebilithSpawn {
        public static Creature Create() {
            return new Creature(Illustrations.BebilithSpawn, "Bebilith Spawn", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Demon, ModTraits.Spider }, 2, 5, 6, new Defenses(17, 11, 8, 8), 40,
            new Abilities(4, 3, 3, 0, 2, 0), new Skills(acrobatics: 7, stealth: 7, athletics: 8))
            .WithProficiency(Trait.Melee, Proficiency.Expert)
            .WithProficiency(Trait.Ranged, Proficiency.Trained)
            .WithCharacteristics(false, true)
            .WithUnarmedStrike(new Item(IllustrationName.Jaws, "maw", new Trait[] { Trait.Melee, Trait.Unarmed, Trait.Brawling }).WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Piercing)))
            .WithAdditionalUnarmedStrike(new Item(Illustrations.StabbingAppendage, "stabbing appendage", new Trait[] { Trait.Agile, Trait.Melee, Trait.Unarmed, Trait.Brawling }).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing)))
            .AddQEffect(new QEffect() {
                StateCheck = self => self.Owner.WeaknessAndResistance.AddWeakness(DamageKind.Good, 5)
            })
            .AddQEffect(new QEffect() {
                StateCheckWithVisibleChanges = async self => {
                    if (self.Owner.Battle.Encounter.CharacterLevel <= 3 && CampaignState.Instance != null && CampaignState.Instance.AdventurePath?.Name == "Roguelike Mode" && CampaignState.Instance.Tags.TryGetValue("SeenBebilithSpawn", out string val) == false) {
                        CampaignState.Instance.Tags.Add("SeenBebilithSpawn", "true");
                        var advisor1 = self.Owner.Battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null).ToList().GetRandom();
                        var advisor2 = self.Owner.Battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null && cr != advisor1).ToList().GetRandom();
                        advisor1.Battle.Cinematics.EnterCutscene();
                        Sfxs.Play(SoundEffects.BebilithHiss, 0.5f);
                        await advisor1.Battle.Cinematics.LineAsync(self.Owner, "Krahhh...!", null);
                        await advisor1.Battle.Cinematics.LineAsync(advisor1, "W-what is that!?", null);
                        await advisor1.Battle.Cinematics.LineAsync(advisor2, "It looks like a Bebilith demon...? We should be careful. I've read that their rotting venom can be crippling. If we let it progress, we won't be able to fully recover from its effects until we return to Dawnsbury.", null);
                        advisor1.Battle.Cinematics.ExitCutscene();
                    }
                }
            })
            .AddQEffect(new QEffect("Webwalk", "This creature moves through webs unimpeded.") { Id = QEffectId.IgnoresWeb })
            .AddQEffect(QEffect.AttackOfOpportunity())
            .AddQEffect(CommonQEffects.PreyUpon())
            .AddQEffect(CommonQEffects.AbyssalRotAttack(16, "1d8", "maw"))
            .AddQEffect(CommonQEffects.WebAttack(16));
        }
    }
}