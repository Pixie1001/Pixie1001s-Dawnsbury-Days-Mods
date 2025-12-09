using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.AuraAnimations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Mechanics.Zoning;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks.Monsters.L6;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;
using System;
using System.Xml.Linq;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class WebWarden {
        private const int AURA_SIZE = 5;

        public static Creature Create() {
            Item legAtk = new Item(new SpiderIllustration(Illustrations.StabbingAppendage, IllustrationName.DragonClaws), "stabbing appendage", new Trait[] { Trait.Unarmed, Trait.Finesse, Trait.Agile, Trait.Grab })
                .WithWeaponProperties(new WeaponProperties("2d8", DamageKind.Piercing));

            Creature monster = new Creature(new SpiderIllustration(Illustrations.WebWarden, Illustrations.Bear5), "Web Warden",
                [Trait.Chaotic, Trait.Evil, Trait.Demon, Trait.Fiend, ModTraits.Spider, ModTraits.MeleeMutator],
                5, 12, 8, new Defenses(21, 15, 13, 11), 95,
            new Abilities(5, 3, 6, 2, 2, 0),
            new Skills(athletics: 14, stealth: 12))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature monster = self.Self;

                    //AiFuncs.PositionalGoodness(monster, options, (pos, you, step, them) => them.FriendOf(you) || pos.DistanceTo(them.Occupies) <= AURA_SIZE, 1.5f, false);
                    AiFuncs.PositionalGoodness(monster, options, (pos, you, step, them) => them.EnemyOf(you) && pos.DistanceTo(them.Occupies) <= AURA_SIZE, 2f, false);

                    return null;
                };
            })
            .WithCreatureId(CreatureIds.WebWarden)
            .WithProficiency(Trait.Melee, Proficiency.Master)
            .WithBasicCharacteristics()
            .WithUnarmedStrike(legAtk)
            .AddQEffect(QEffect.DamageWeakness(DamageKind.Good, 5))
            .AddQEffect(QEffect.DamageWeakness(Trait.ColdIron, 5))
            .AddQEffect(new QEffect("Webwalk", "This creature moves through webs unimpeded.") { Id = QEffectId.IgnoresWeb })
            .AddQEffect(QEffect.WebSense())
            .AddQEffect(CommonQEffects.WebAttack(22))
            .AddQEffect(QEffect.SneakAttack("1d6"))
            .AddQEffect(QEffect.MonsterGrab())
            .AddQEffect(new QEffect("Bless Vulnerability", "When the web warden starts its turn inside the radius of a bless spell, it suffers 1d6 mental damage.") {
                StartOfYourPrimaryTurn = async (self, you) => {
                    var blessed = false;

                    foreach (Creature cr in you.Battle.AllCreatures) {
                        var bless = cr.QEffects.FirstOrDefault(qf => qf.Name == "Bless");

                        var tag = bless?.Tag;

                        if (bless != null && you.DistanceTo(cr) <= (bless.Tag as ValueTuple<int, bool>?)?.Item1) {
                            blessed = true;
                            break;
                        }
                    };

                    if (blessed) {
                        Sfxs.Play(SoundEffects.BebilithHiss);
                        await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("1d6", "Bless vulnerability"), self.Owner, CheckResult.Failure, DamageKind.Mental);
                    }
                }
            })
            .AddQEffect(new QEffect() {
                StateCheckWithVisibleChanges = async self => {
                    if (self.Owner.Battle.Encounter.CharacterLevel <= 3 && CampaignState.Instance != null && CampaignState.Instance.AdventurePath?.Name == "Roguelike Mode" && CampaignState.Instance.Tags.TryGetValue("SeenWebWarden", out string val) == false) {
                        CampaignState.Instance.Tags.Add("SeenWebWarden", "true");
                        var advisor1 = R.ChooseAtRandomVisualOnly(self.Owner.Battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null).ToArray());
                        var advisor2 = R.ChooseAtRandomVisualOnly(self.Owner.Battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null && cr != advisor1).ToArray());
                        if (advisor1 == null || advisor2 == null) return;
                        advisor1.Battle.Cinematics.EnterCutscene();
                        Sfxs.Play(SfxName.ZombieAttack, 0.5f);
                        await advisor1.Battle.Cinematics.LineAsync(self.Owner, "Ughhhh...", null);
                        await advisor1.Battle.Cinematics.LineAsync(advisor1, "W-what is that poor creature? Is it some kind of demon?", null);
                        await advisor1.Battle.Cinematics.LineAsync(advisor2, "It looks like a Web Warden demon... I've read it's one of the many punishments the Demon Queen of Spiders inflicts upon the captured souls of her enemies that are drag back to her domain...", null);
                        await advisor1.Battle.Cinematics.LineAsync(advisor2, "Be careful, it's saod tp entangle all that get too near with a potent death curse that will strike down anyone that dares to harm its tormentors. Keep your distance, or try to put it out of its misery first.", null);
                        advisor1.Battle.Cinematics.ExitCutscene();
                    }
                }
            })
            .AddQEffect(new QEffect("Web Layer", "At the end of each turn, the web warden fills all spaces within 5ft of it with webs.") {
                StartOfCombat = async self => await self.StartOfYourPrimaryTurn!(self, self.Owner),
                StartOfYourPrimaryTurn = async (self, you) => {
                    var webTiles = you.Battle.Map.AllTiles.Where(t => t.DistanceTo(you.Occupies) <= 1 && !t.HasEffect(TileQEffectId.Web));
                    var z = Zone.Spawn(you, ZoneAttachment.StableBurst(webTiles.ToList()));
                    DemonWebspinner.CreateWebZone(z, 22);
                    z.Apply();
                }
            })
            .Builder
            .Done();

            var aura = new QEffect("Web of Curses", $"(aura, curse) {AURA_SIZE * 5} feet. Enemies within the emanation that damage an ally of the web warden suffer a stack of Web Warden's Curse. After reaching 2 stacks, they suffer 10d10 negative damage.") {

            };

            aura.AddGrantingOfTechnical(cr => cr.EnemyOf(monster) && cr.DistanceTo(monster) <= AURA_SIZE, qfTech => {
                qfTech.AfterYouDealDamage = async (attacker, action, defender) => {
                    if (attacker.Occupies == null || defender == monster || !defender.FriendOf(monster)) return;

                    QEffect? curse = attacker.FindQEffect(QEffectIds.WebWardensCurse);

                    if (curse == null) {
                        attacker.AddQEffect(new QEffect("Web Warden's Curse", "At 2 stacks, this creature will take 10d6 negative damage.", ExpirationCondition.Never, monster, IllustrationName.BestowCurse) {
                            Key = "Web Warden's Curse",
                            Id = QEffectIds.WebWardensCurse,
                            Value = 1
                        });
                    } else {
                        curse.ExpiresAt = ExpirationCondition.Immediately;
                        Sfxs.Play(SfxName.ChillTouch);
                        attacker.Overhead("*curse triggered*", Color.PaleVioletRed, $"{attacker.Name} triggered the Web Warden's Curse!");
                        await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("10d10", "Web warden's curse"), attacker, CheckResult.Failure, DamageKind.Negative);
                    }
                };
            });

            monster.AddQEffectAtPriority(aura, true);
            var animation = new MagicCircleAuraAnimation(IllustrationName.AngelicHaloCircle, Color.Purple, AURA_SIZE);
            animation.DecreaseOpacityAsSizeIncreases = true;
            monster.AnimationData.AddAuraAnimation(animation);

            return monster;
        }

        private static ValueTuple<int, bool> Test() {
            return (1, true);
        }
    }
}
