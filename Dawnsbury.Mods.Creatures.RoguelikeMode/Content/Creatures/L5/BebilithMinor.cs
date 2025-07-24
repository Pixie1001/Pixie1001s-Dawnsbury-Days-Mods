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
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {

    public class BebilithMinor {
        public static Creature Create() {
            return new Creature(new SpiderIllustration(Illustrations.BebilithMinor, Illustrations.Bear2), "Bebilith, Minor",
               [Trait.Chaotic, Trait.Evil, Trait.Demon, Trait.Beast, Trait.Fiend, ModTraits.Spider, ModTraits.MeleeMutator],
               level: 5, perception: 9, speed: 6, new Defenses(21, fort: 11, reflex: 9, will: 12), hp: 95,
            new Abilities(6, 3, 4, 0, 3, 0), new Skills(acrobatics: 10, stealth: 10, athletics: 13))
            .WithCreatureId(CreatureIds.BebilithMinor)
            .WithProficiency(Trait.Melee, Proficiency.Master)
            .WithProficiency(Trait.Ranged, Proficiency.Expert)
            .WithCharacteristics(false, true)
            .WithUnarmedStrike(new Item(IllustrationName.Jaws, "maw", new Trait[] { Trait.Melee, Trait.Unarmed, Trait.Brawling, Trait.Grab }).WithWeaponProperties(new WeaponProperties("2d8", DamageKind.Piercing)))
            .WithAdditionalUnarmedStrike(new Item(new SpiderIllustration(Illustrations.StabbingAppendage, IllustrationName.DragonClaws), "stabbing appendage", new Trait[] { Trait.Agile, Trait.Melee, Trait.Unarmed, Trait.Brawling }).WithWeaponProperties(new WeaponProperties("2d6", DamageKind.Piercing)))
            .AddQEffect(QEffect.DamageWeakness(DamageKind.Good, 5))
            .AddQEffect(QEffect.DamageWeakness(Trait.ColdIron, 5))
            .AddQEffect(new QEffect() {
                StateCheckWithVisibleChanges = async self => {
                    if (self.Owner.Battle.Encounter.CharacterLevel <= 3 && CampaignState.Instance != null && CampaignState.Instance.AdventurePath?.Name == "Roguelike Mode" && CampaignState.Instance.Tags.TryGetValue("SeenBebilithSpawn", out string val) == false) {
                        CampaignState.Instance.Tags.Add("SeenBebilithSpawn", "true");
                        var advisor1 = R.ChooseAtRandom(self.Owner.Battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null).ToArray());
                        var advisor2 = R.ChooseAtRandom(self.Owner.Battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null && cr != advisor1).ToArray());
                        if (advisor1 == null || advisor2 == null) return;
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
            .AddQEffect(QEffect.WebSense())
            .AddQEffect(QEffect.AttackOfOpportunity())
            .AddQEffect(CommonQEffects.AbyssalRotAttack(20, "1d12", "maw"))
            .AddQEffect(CommonQEffects.WebAttack(20))
            .AddQEffect(QEffect.MonsterGrab());
        }
    }
}