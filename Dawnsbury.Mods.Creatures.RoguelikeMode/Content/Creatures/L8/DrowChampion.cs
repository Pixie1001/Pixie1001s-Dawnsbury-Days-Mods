using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.AuraAnimations;
using Dawnsbury.Core.Animations.Movement;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Mechanics.Zoning;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    public class DrowChampion {
        public static Creature Create() {

            Creature monster = new Creature(Illustrations.DrowChampion, "Drow Champion", [Trait.Chaotic, Trait.Evil, Trait.Elf, Trait.Humanoid, Trait.Female, Trait.MetalArmor, ModTraits.Drow, ModTraits.MeleeMutator],
               level: 8, perception: 16, speed: 4, new Defenses(27, fort: 19, reflex: 16, will: 16), hp: 150,
            new Abilities(6, 3, 4, 2, 3, 4), new Skills(religion: 16, athletics: 19, intimidation: 16))
            .WithCreatureId(CreatureIds.DrowChampion)
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(CommonQEffects.DrowClergy())
            .AddQEffect(QEffect.AttackOfOpportunity())
            .AddQEffect(new QEffect("Critical Specialisation (Flail)", "") {
                YouHaveCriticalSpecialization = (self, weapon, combatAction, defender) => weapon.HasTrait(Trait.Flail)
            })
            .WithProficiency(Trait.Weapon, Proficiency.Master)
            .WithProficiency(Trait.Unarmed, Proficiency.Master)
            .WithBasicCharacteristics()
            .AddQEffect(new QEffect("Contemptious Retaliation {icon:Reaction}", "{b}Trigger{/b} An enemy within 5 feet attacks you. {b}Effect{/b} You may make a strike against the attacker.") {
                YouAreTargeted = async (self, action) => {
                    if (action.Owner?.Occupies == null || !action.HasTrait(Trait.Attack) || action.Owner.DistanceTo(self.Owner) > 1) {
                        return;
                    }

                    CombatAction strike = self.Owner.CreateStrike(self.Owner.PrimaryWeapon!, 0).WithActionCost(0);
                    strike.ChosenTargets = ChosenTargets.CreateSingleTarget(action.Owner);

                    int map = self.Owner.Actions.AttackedThisManyTimesThisTurn;

                    if ((bool)strike.CanBeginToUse(self.Owner) && (strike.Target as CreatureTarget)!.IsLegalTarget(self.Owner, action.Owner).CanBeUsed && await self.Owner.AskToUseReaction($"{action.Owner.Name} is attempting to attack you in melee. Would you like to retaliate?")) {
                        if (strike.CanBeginToUse(action.Owner)) {
                            await strike.AllExecute();
                            self.Owner.Actions.AttackedThisManyTimesThisTurn = map;
                        }

                    }
                }
            })
            .AddQEffect(new QEffect("Demonic Aegis", "While raising its shield, this creature gains a +2 circumstance bonus against ranged attacks and reflex saves.") {
                BonusToDefenses = (self, action, def) => self.Owner.HasEffect(QEffectId.RaisingAShield) && action != null && ((action.HasTrait(Trait.Attack) && action.HasTrait(Trait.Ranged) && def == Defense.AC) || def == Defense.Reflex) ? new Bonus(2, BonusType.Circumstance, "Demonic aegis", true) : null
            })
            .Builder
            .AddManufacturedWeapon(ItemName.Flail, 20, [Trait.Evil, Trait.Magical, Trait.Chaotic], "3d6+6", wp => {
                wp.AdditionalDamage.Add(("1d6", DamageKind.Negative));
            })
            .AddMainAction(you => {
                return new CombatAction(you, IllustrationName.Bane, "Dark Chant", [Trait.Divine, Trait.Concentrate, Trait.Flourish],
                    "Increase your Demonic Power by 1.",
                    Target.Self((user, ai) => user.GetQEffectValue(QEffectIds.DemonicPower) < 6 ? 20f : int.MinValue))
                .WithActionCost(1)
                .WithSoundEffect(SfxName.Necromancy)
                .WithEffectOnSelf(async (action, caster) => {
                    var stacks = caster.GetQEffectValue(QEffectIds.DemonicPower);

                    if (stacks > 0) {
                        stacks += 1;
                        caster.FindQEffect(QEffectIds.DemonicPower)!.Value = stacks;
                    }
                    else
                        caster.AddQEffect(new QEffect("Demonic Power", caster.Name + " is channeling the demon power of the Demon Queen of Spiders, growing stronger the longer she chants." +
                            "\n • 1. +2 status bonus all saves." +
                            "\n • 2. Gains a 5-foot demonic aura that deals 3d8 negative damage (Will save mitigates) to creatures that start their turn inside it." +
                            "\n • 3. +1 status bonus to attack." +
                            "\n • 4. Demonic aura grows to a 10-foot radius." +
                            "\n • 5. +10 status bonus to speed, and assume a powerful demonic form.", ExpirationCondition.Never, you, you.Illustration) {
                            Id = QEffectIds.DemonicPower,
                            Value = 1,
                            BonusToAllSpeeds = (self) => self.Value >= 5 ? new Bonus(2, BonusType.Status, "Demonic power", true) : null,
                            BonusToDefenses = (self, action, def) => def != Defense.AC ? new Bonus(2, BonusType.Status, "Demonic power", true) : null,
                            BonusToAttackRolls = (self, action, target) => self.Value >= 3 ? new Bonus(1, BonusType.Status, "Demonic power", true) : null,
                            // BonusToDamage = (self, action, target) => action.HasTrait(Trait.Strike) && self.Value >= 6 ? new Bonus(4, BonusType.Status, "Demonic power", true) : null,
                            StateCheck = self => {
                                if (self.Value < 2) return;

                                var effect = new QEffect("Demonic Aura",
                                    $"(aura, divine) {(self.Value >= 5 ? 10 : 5)} feet. Enemy creatures that end their turn within the aura suffer 3d8 negative damage, mitigated by a basic DC {23 - 8 + caster.Level} Will save.",
                                    ExpirationCondition.Ephemeral, caster, IllustrationName.Bane);
                                self.Owner.AddQEffect(effect);

                                var z = Zone.Spawn(effect, ZoneAttachment.Aura(self.Value >= 5 ? 2 : 1));
                                z.AfterCreatureEndsItsTurnHere = async cr => {
                                    if (cr.FriendOf(caster)) return;

                                    var ca = CombatAction.CreateSimple(caster, "Demonic Aura", Trait.Evil, Trait.Divine, Trait.Demon);
                                    var result = await CommonSpellEffects.RollSavingThrowAsync(cr, ca, Defense.Fortitude, 23 - 8 + caster.Level);
                                    await CommonSpellEffects.DealBasicDamage(ca, caster, cr, result, "3d8", DamageKind.Negative);
                                };
                            }
                        });

                    if (stacks == 2) {
                        var aura = new MagicCircleAuraAnimation(Illustrations.BaneCircleWhite, Color.Black, 1) { OwnerQEffect = caster.FindQEffect(QEffectIds.DemonicPower) };
                        caster.AnimationData.AddAuraAnimation(aura);
                        aura.MoveTo(aura.InitialSize);
                    } else if (stacks == 4) {
                        caster.AnimationData.AuraAnimations.FirstOrDefault(aura => aura.OwnerQEffect == caster.FindQEffect(QEffectIds.DemonicPower))?.MoveTo(2);
                    }

                    if (stacks >= 5) {
                        Sfxs.Play(SoundEffects.BebilithHiss);
                        caster.Overhead("*stance change*", Color.Crimson, "The Drow Champion's hands grow into spindly claws, as she flies into a demonic frenzy.");

                        caster.UnarmedStrike = NaturalWeapons.Create(NaturalWeaponKind.Claw, "3d12", DamageKind.Slashing, [Trait.Brawling, Trait.Grab]);

                        caster.AddQEffect(new QEffect("Critical Specialisation (Brawling)", "") {
                            YouHaveCriticalSpecialization = (self, weapon, combatAction, defender) => weapon.HasTrait(Trait.Brawling),
                            BonusToAttackRolls = (qfSelf, action, _) => new Bonus(2, BonusType.Item, "claw", false)
                        });

                        while (caster.HeldItems.Count() > 0)
                            await CreateDrop(caster, caster.HeldItems[0]).AllExecute();

                        var mb = CommonQEffects.MiniBoss();
                        caster.AddQEffect(mb);
                        await mb.StartOfCombat.InvokeIfNotNull(mb);

                        caster.AddQEffect(QEffect.MonsterGrab(false));

                        static CombatAction CreateDrop(Creature user, Item item) {
                            return new CombatAction(user, IllustrationName.DropItem, "Drop " + item.Name, [Trait.Manipulate, Trait.Basic, Trait.DoesNotProvoke], "Drop this item on the ground as a free action. You will be able to pick it up later.\n\nDropping an item doesn't provoke attacks of opportunity.", Target.Self())
                                    .WithEffectOnSelf(cr => {
                                        item.Traits.Add(Trait.HandEphemeral);
                                        cr.DropItem(item);
                                    })
                                    .WithItem(item)
                                    .WithActionCost(0)
                                    .WithActionId(ActionId.DropItem)
                                    .WithSoundEffect(SfxName.DropItem);
                        }
                    }

                })
                ;
            })
            .AddMainAction(you => {
                return new CombatAction(you, IllustrationName.ShieldingStrike, "Lunging Advance", [Trait.Move], "The Drow Champion Steps towards an enemy and Strikes them.",
                    Target.Ranged(2)
                    //.WithAdditionalConditionOnTargetCreature((a, d) => d.HeldItems.Any(itm => itm.HasTrait(Trait.Shield)) ? Usability.Usable : Usability.NotUsable("You must be wielding a shield"))
                    .WithAdditionalConditionOnTargetCreature((a, d) => d.Space.GetNeighbours().Where(t => t.IsTrulyGenuinelyFreeTo(a) && t.IsAdjacentTo(a)).Count() > 0 ? Usability.Usable : Usability.NotUsableOnThisCreature("no free space")))
                .WithActionCost(1)
                .WithGoodnessAgainstEnemy((targeting, a, d) => {
                    return (a.CreateStrike(a.PrimaryWeapon!).Target as CreatureTarget)?.CreatureGoodness(targeting, a, d) ?? int.MinValue;
                })
                .WithEffectOnEachTarget(async (spell, caster, target, _) => {
                    var strike = caster.CreateStrike(caster.PrimaryWeapon!).WithActionCost(0);

                    Tile bestTile = target.Space.GetNeighbours().Where(t => t.IsTrulyGenuinelyFreeTo(caster) && t.IsAdjacentTo(caster)).FirstOrDefault();
                    if (bestTile == null) {
                        spell.RevertRequested = true;
                        return;
                    }
                    await caster.SingleTileMove(bestTile, null);
                    if ((strike.Target as CreatureTarget)?.IsLegalTarget(caster, target) ?? false) {
                        strike.ChosenTargets = new ChosenTargets() {
                            ChosenCreature = target,
                            ChosenCreatures = { target }
                        };
                        await strike.AllExecute();
                    }
                })
                ;
            })
            .Done()
            .AddHeldItem(Items.CreateNew(ItemName.SteelShield))
            ;

            return monster;
        }
    }

}

