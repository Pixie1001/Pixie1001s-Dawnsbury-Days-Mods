using Dawnsbury.Audio;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.IO;
using Dawnsbury.Modding;

// Update the namespace to reflect your mod.
namespace Dawnsbury.Mods.Creatures.RoguelikeMode;

/// <summary>
/// A local library for parry trait logic. Call <see cref="Load(string)"/> to run the parry action granter.
/// </summary>
/// <value>v1.0</value>
public static class ParryLogic
{
    /// The Parry action from the Parry trait.
    public static ActionId ParryAction { set; get; }

    /// The ID of the effect gained from the Parry action.
    public static QEffectId ParryQf { set; get; }

    /// <summary>
    /// The innate effect that grants the parry trait to certain weapons, or increases the bonus to +2 if they already have the trait.
    /// </summary>
    /// <seealso cref="GreaterParry(string?,string?,System.Func{Dawnsbury.Core.Mechanics.QEffect,Dawnsbury.Core.Mechanics.Treasure.Item,bool})"/>
    public static QEffectId GreaterParryQf { set; get; }
        
    /// The Parry trait, as shown on weapons
    public static readonly Trait ParryTrait = ModManager.RegisterTrait("Parry",
        new TraitProperties("Parry", true, "You can Parry {icon:Action} to gain a +1 circumstance bonus to AC for 1 round while you wield it (must be trained).", true) { RelevantForItemBlock = true });
    
    /// A hidden technical trait that indicates a temporary parry trait.
    public static readonly Trait TemporaryParryTrait = ModManager.RegisterTrait("TemporaryParry");

    /// Mod option to show/hide the unique parry icon. Will not be registered if your mod does not provide an <see cref="Illustration"/> to <see cref="Load"/>.
    public const string IconToggle = "ParryLogic.UseSideBySide";

    /// The icon used in a side-by-side for the parry action.
    public static Illustration? ParryActionIcon;
    
    /// The icon used on its own for the parry effect.
    public static Illustration? ParryEffectIcon;

    /// <summary>
    /// <para>Registers the parry action granter and necessary enum data.</para>
    /// <para>Uses a shared "ParryGranter" key; the last mod to load is the final QEffect.</para> 
    /// </summary>
    /// <param name="modName">The name of the mod registering this action. This is added to the name of the hidden QEffect, for debugging purposes.</param>
    /// <param name="parryActionIcon">The <see cref="Illustration"/> used in a side-by-side for the parry action's unique icon. If null, or if IconToggle is disabled, the weapon icon is used directly.</param>
    /// <param name="parryEffectIcon">The <see cref="Illustration"/> used on its own for the parry effect's unique icon. If null, or if IconToggle is disabled, the weapon icon is used directly.</param>
    /// <seealso cref="IconToggle"/>
    public static void Load(
        string modName,
        Illustration? parryActionIcon = null,
        Illustration? parryEffectIcon = null)
    {
        ParryAction = ModManager.TryParse("Parry", out ActionId parryAction)
            ? parryAction
            : ModManager.RegisterEnumMember<ActionId>("Parry");
        ParryQf = ModManager.TryParse("Parry", out QEffectId parryQf)
            ? parryQf
            : ModManager.RegisterEnumMember<QEffectId>("Parry");
        GreaterParryQf = ModManager.TryParse("GreaterParry", out QEffectId greaterParryQf)
            ? greaterParryQf
            : ModManager.RegisterEnumMember<QEffectId>("GreaterParry");
        ParryActionIcon = parryActionIcon;
        ParryEffectIcon = parryEffectIcon;
        
        // If this library wasn't already registered, add a mod option as well.
        // Doesn't add the option to hide the extra icon if your mod doesn't provide one.
        if (greaterParryQf == QEffectId.Unspecified
            && ParryActionIcon is not null)
            ModManager.RegisterBooleanSettingsOption(
                IconToggle,
                "Parry Action: Use side-by-side icon",
                "If enabled, parry actions on the action bar display with the original weapon icon next to a unique icon for parrying. Disabling will use just the weapon itself, the same way Raise Shield works.",
                true);

        ModManager.RegisterActionOnEachCreature(cr =>
        {
            cr.AddQEffect(new QEffect()
            {
                Name = "[PARRY GRANTER: " + modName + "]",
                Key = "ParryGranter",
                ProvideActionsIntoPossibilitySection = (qfThis, section) =>
                {
                    if (section.PossibilitySectionId != PossibilitySectionId.ItemActions)
                        return [];
                    
                    List<Possibility> parries = [];
                    parries.AddRange(cr.HeldItems
                        .Where(item =>
                            item.HasTrait(ParryTrait)
                            && cr.Proficiencies.Get(item.Traits) >= Proficiency.Trained
                            && !cr.QEffects.Any(qf =>
                                qf.Tag == item && qf.Id == ParryQf))
                        .Select(weapon =>
                            CreateParryAction(
                                qfThis.Owner,
                                weapon,
                                PlayerProfile.Instance.IsBooleanOptionEnabled(IconToggle)
                                    ? parryActionIcon
                                    : null))
                        .Select(raiseParryWeapon =>
                            new ActionPossibility(raiseParryWeapon)));

                    return parries;
                },
            });
        });
    }

    /// <summary>
    /// Create the action that raises a parry weapon.
    /// </summary>
    /// <param name="owner">The creature raising the weapon. This effect expires at the start of their next turn.</param>
    /// <param name="weapon">The weapon being raised. Does not need to have the parry trait.</param>
    /// <param name="parryIcon">(optional) A custom parry icon used in the action's side-by-side illustration.</param>
    public static CombatAction CreateParryAction(
        Creature owner,
        Item weapon,
        Illustration? parryIcon = null)
    {
        CombatAction parry = new CombatAction(
                owner,
                parryIcon is not null
                    ? new SideBySideIllustration(weapon.Illustration, parryIcon)
                    : weapon.Illustration,
                $"Parry ({weapon.Name})",
                [Trait.Basic, Trait.DoNotShowOverheadOfActionName],
                "",
                Target.Self((cr, ai) =>
                    ai.GainBonusToAC(cr.HasEffect(GreaterParryQf) ? 2 : 1)))
            .WithDescription(
                "You position your weapon defensively.",
                "{b}Requirements{/b} You are wielding this weapon, and your proficiency with it is trained or better.\n\nYou gain a +1 circumstance bonus to AC until the start of your next turn.")
            .WithShortDescription("Gain a +1 circumstance bonus to AC until the start of your next turn.")
            .WithActionCost(1)
            .WithItem(weapon)
            .WithActionId(ParryAction)
            .WithSoundEffect(SfxName.RaiseShield)
            .WithEffectOnEachTarget(async (action, _, self, _) =>
            {
                self.AddQEffect(Parrying(action, self, weapon));
            });
        return parry;
    }

    /// <summary>
    /// The effect to be applied by <see cref="CreateParryAction"/>.
    /// </summary>
    /// <param name="sourceAction">The action which raised this weapon.</param>
    /// <param name="source">The creature raising the weapon (in most cases).</param>
    /// <param name="weapon">The weapon being raised.</param>
    /// <param name="bonus">(Default: 1) The circumstance bonus to AC.</param>
    public static QEffect Parrying(
        CombatAction? sourceAction,
        Creature source,
        Item weapon,
        int bonus = 1)
    {
        return new QEffect(
                $"Parrying ({weapon.Name})",
                "You have a +1 circumstance bonus to AC while wielding this weapon.",
                ExpirationCondition.ExpiresAtStartOfYourTurn,
                source,
                ParryEffectIcon ?? weapon.Illustration)
            {
                SourceAction = sourceAction,
                Id = ParryQf,
                Tag = weapon,
                StateCheck = qfThis2 =>
                {
                    if (!qfThis2.Owner.HeldItems.Contains(weapon))
                        qfThis2.ExpiresAt = ExpirationCondition.Immediately;
                },
                BonusToDefenses = (_, _, def) =>
                    def == Defense.AC
                        ? new Bonus(
                            bonus,
                            BonusType.Circumstance,
                            "Parry")
                        : null,
            }
            .WithExpirationAtStartOfSourcesTurn(source, 1);
    }

    /// <summary>
    /// <para>An innate effect for abilities which allow you to parry with certain items.</para>
    /// <para>Increase the value of the Parry bonus to 2 if that certain item also has the Parry trait.</para>
    /// <para>To upgrade the parry bonus, it must be caused by a <see cref="CombatAction"/> with the Parry <see cref="ActionId"/>.</para>
    /// </summary>
    /// <param name="name">The name of this effect on the stat block.</param>
    /// <param name="description">The description of this effect on the stat block.</param>
    /// <param name="weaponTest">(thisQEffect, itemToTest) A function which returns whether the item can gain parry or increase its existing parry bonus.</param>
    /// <returns></returns>
    public static QEffect GreaterParry(
        string? name,
        string? description,
        Func<QEffect, Item, bool> weaponTest)
    {
        return new QEffect(name!, description!)
        {
            Id = GreaterParryQf,
            Tag = new List<Item>(), // Tracked items
            // Manage temporary parry for wielded items
            StateCheck = qfThis =>
            {
                List<Item> items = (qfThis.Tag as List<Item>)!;
                
                // Remove parry to dropped items.
                foreach (Item item in items
                             .Where(item =>
                                 !qfThis.Owner.HeldItems.Contains(item)))
                {
                    item.Traits.Remove(ParryTrait);
                    item.Traits.Remove(TemporaryParryTrait);
                }
                items = items
                    .Where(qfThis.Owner.HeldItems.Contains)
                    .ToList();
                
                // Add parry stuff to wielded items.
                foreach (Item item in qfThis.Owner.HeldItems
                             .Where(item =>
                                 weaponTest.Invoke(qfThis, item)
                                 && !item.HasTrait(ParryTrait)
                                 && !item.HasTrait(TemporaryParryTrait)))
                {
                    item.Traits.AddRange([ParryTrait, TemporaryParryTrait]);
                    items.Add(item);
                }
            },
            // Increase the bonus to 2 for certain items.
            AfterYouAcquireEffect = async (qfThis, qfAcquired) =>
            {
                if (qfAcquired.Id == ParryQf
                    && qfAcquired.SourceAction?.ActionId == ParryAction
                    && qfAcquired.Tag is Item weapon
                    && weapon.HasTrait(ParryTrait) // If some other effect parries this weapon, it must properly be from the Parry trait's action.
                    && !weapon.HasTrait(TemporaryParryTrait) // Only upgrade already-parry weapons
                    && weaponTest.Invoke(qfThis, weapon))
                {
                    qfAcquired.Description = qfAcquired.Description!.Replace("+1", "{Blue}+2{/Blue}");
                    qfAcquired.BonusToDefenses = (_, _, def) =>
                        def == Defense.AC
                            ? new Bonus(
                                2,
                                BonusType.Circumstance,
                                "Parry (Greater)")
                            : null;
                }
            },
        };
    }
}
