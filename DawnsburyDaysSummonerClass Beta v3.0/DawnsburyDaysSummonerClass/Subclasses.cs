using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Dawnsbury;
using Dawnsbury.Audio;
using Dawnsbury.Core;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
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
using Dawnsbury.Core.Mechanics.Damage;
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
using Dawnsbury.Core.Animations.Movement;
using Dawnsbury.IO;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Summoner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using static Dawnsbury.Mods.Classes.Summoner.SummonerSpells;
using static Dawnsbury.Mods.Classes.Summoner.SummonerClassLoader;
using static Dawnsbury.Mods.Classes.Summoner.Enums;
using Microsoft.Xna.Framework.Graphics;
using static System.Collections.Specialized.BitVector32;
using System.Reflection.Metadata.Ecma335;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Dawnsbury.Core.Noncombat;
using Microsoft.VisualBasic;
using static System.Net.Mime.MediaTypeNames;
using System.Numerics;
using System.Text.RegularExpressions;
using Dawnsbury.Core.Animations.AuraAnimations;
using System.Runtime.Intrinsics.Arm;

namespace Dawnsbury.Mods.Classes.Summoner {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class Subclasses {

        internal static List<Feat> subclasses = new List<Feat>();

        private static readonly string AngelicEidolonFlavour = "Your eidolon is a celestial messenger, a member of the angelic host with a unique link to you, allowing them to carry a special message to the mortal world at your side. " +
    "Most angel eidolons are roughly humanoid in form, with feathered wings, glowing eyes, halos, or similar angelic features. However, some take the form of smaller angelic servitors like the winged helmet" +
    "cassisian angel instead. The two of you are destined for an important role in the plans of the celestial realms. Though a true angel, your angel eidolon's link to you as a mortal prevents them " +
    "from casting the angelic messenger ritual, even if they somehow learn it.";

        private static readonly string AngelicEidolonCrunch = "\n\n• {b}Tradition{/b} Divine\n• {b}Skills{/b} Diplomacy, Religion\n\n{b}Initial Eidolon Ability (Hallowed Strikes).{/b} Your Eidolon's strikes deal +1 good damage." +
            "\n\n{i}At level 7{/i}\n{b}Symbiosis Eidolon Ability (Angelic Aegis).{/b} Your eidolon's primary natural weapon attack gains the parry trait. While parrying they can use the Angelic Aegis {icon:FreeAction} action, " +
            "which grants an adjacent ally a +2 circumstance bonus to AC until the start of their next turn. In addition, your eidolon can intercept {icon:Reaction} attacks that deal physical damage against the subject of their aegis, reducing the damage by an amount equal to their level.\n\n" +
            "These benefits extend only while the ally is currently adjacent to your eidolon.";

        private static readonly string DraconicEidolonFlavour = "Because dragons have a strong connection to magic, their minds can often leave an echo floating in the Astral Plane. Such an entity is extremely powerful " +
            "but unable to interact with the outside world on its own. Dragon eidolons manifest in the powerful, scaled forms they had in life; most take the form of true dragons (albeit smaller), but some manifest as " +
            "drakes or other draconic beings. You have forged a connection with such a dragon eidolon and together, you seek to grow as powerful as an ancient wyrm.";

        private static readonly string DraconicEidolonCrunch = "\n\n• {b}Tradition{/b} Varies\n• {b}Skills{/b} You gain Intimidation and the knowledge skill associated with your dragon eidolon's magical tradition." +
            "\n\n{b}Initial Eidolon Ability (Breath Weapon) {icon:TwoActions}.{/b} Your eidolon exhales a 60-foot line or 30-foot cone of energy and deal 2d6 of the damage associated with your eidolon's dragon type to each target. " +
            "You can't use breath weapon again for 1d4 rounds. This damage increases by 1d6 at 3rd level and every two levels thereafter.\n\n{b}Special.{/b} " +
            "You must select a specific breed for your dragon. This will determine your spell tradition, one of your bonus skills and the damage type of your eidolon's breath weapon. Your dragon's type also determines the save targeted by its breath weapon." +
            "\n\n{i}At level 7{/i}\n{b}Symbiosis Eidolon Ability (Draconic Frenzy) {icon:TwoActions}.{/b} Your eidolon makes 3 consecutive attacks, one with its primary natural weapon attack and two with its secondary natural weapon attack. If any of these attacks result in a " +
            "critical hit, your eidolon's Breath Weapon is immediately recharged.";

        private static readonly string BeastEidolonFlavour = "Your eidolon is a manifestation of the life force of nature in the form of a powerful magical beast that often has animal features, possibly even several from different species. " +
            "You might have learned the way to connect with the world's life force via a specific philosophy or practice, such as the beliefs of the god callers of Sarkoris, or formed a bond on your own. Regardless, your link to your eidolon " +
            "allows you both to grow in power and influence to keep your home safe from those who would despoil it.";

        private static readonly string BeastEidolonCrunch = "\n\n• {b}Tradition{/b} Primal\n• {b}Skills{/b} Intimidation, Nature\n\n{b}Initial Eidolon Ability (Beast's Charge) {icon:TwoActions}.{/b} Stride twice. " +
            "If you end your movement within melee reach of at least one enemy, you can make a melee Strike against that enemy. If your eidolon moved at least 20ft and ends it's movement in a cardinal diraction, " +
            "it gains a +1 circumstance bonus to this attack roll.\n\n{i}At level 7{/i}\n{b}Symbiosis Eidolon Ability (Primal Roar) {icon:TwoActions}.{/b} Your eidolon unleashes a primal roar or other such terrifying noise that fits your eidolon's form. " +
            "Your eidolon attempts Intimidation checks to Demoralize each enemy that can hear the roar; these Demoralize attempts don't take any penalty for not sharing a language, and gain a +2 bonus.";

        private static readonly string DevoPhantomEidolonFlavour = "Your eidolon is a lost soul, unable to escape the mortal world due to a strong sense of duty, an undying devotion, or a need to complete an important task. " +
            "Most phantom eidolons are humanoid with a spectral or ectoplasmic appearance, though some take far stranger forms. Your link with your eidolon prevents them from succumbing to corruption and undeath, and together, " +
            "you will grow in strength and fulfill your phantom's devotion.";

        private static readonly string DevoPhantomEidolonCrunch = "\n\n• {b}Tradition{/b} Occult\n• {b}Skills{/b} Medicine, Occultism\n\n" +
            "{b}Initial Eidolon Ability (Dutiful Retaliation) {icon:Reaction}.{/b} Your eidolon makes a strike again an enemy that damaged you. Both your eidolon and your attacker must be within 15ft of you." +
            "\n\n{i}At level 7{/i}\n{b}Symbiosis Eidolon Ability (Devotion Stance) {icon:Action}.{/b} Your eidolon takes on a patient defensive stance, steeling their focus with thoughts of their devotion." +
            "\n\nUntil the start of their next turn, they gain a +2 circumstance bonus to AC, and a +4 bonus to damage to attacks made outside their turn.";

        private static readonly string AzataEidolonFlavour = "Your eidolon is an azata, a celestial embodiment of freedom, creativity, whimsy and revelry. They usually take humanoid forms, sometimes incorporating nature motifs. " +
            "Your eidolon is happy to serve as your protector and muse as long as you show the same kindness and respect for freedom and autonomy to others as it does.";

        private static readonly string AzataEidolonCrunch = "\n\n• {b}Tradition{/b} Divine\n• {b}Skills{/b} Divine, Persuasion\n\n" +
            "{b}Initial Eidolon Ability (Celestial Passion) {icon:Action}.{/b} One ally within 30-feet of your eidolon gains temporary HP equal to its level, and a +1 bonus to attack and skill checks for 1 round. Cannot be used on the same ally more than once per encounter." +
            "\n\n{i}At level 7{/i}\n{b}Symbiosis Eidolon Ability (Whimsical Aura).{/b} The wonder and whimsy of Elysium manifests around your eidolon in an aura. Your eidolon and all allies within a 15-foot aura gain a +5-foot status bonus to their speed at the " +
            "start of their turn. In addition, each ally within this aura at the end of your turn, reduces their frightened value by 1." +
            "\n\nThis subclass is designed and contributed by {link:https://www.reddit.com/r/Pathfinder2e/comments/1bqusiu/leorandgers_summoning_circle_new_eidolons/}LeoRandger{/}.";

        private static readonly string DevilEidolonFlavour = "Your eidolon is a devil - a creature born in the depths of Nine Hells,the embodiment of order and tyranny. Whether tricked into being linked to each other through an infernal " +
            "contract or connected through means, your companion represents the interest of his infernal patrons in mortal affairs. He might act authoritative or submit to you, but while your goals align, it shall follow you on your adventures. " +
            "You only have to worry whether your soul is destined for its home...";

        private static readonly string DevilEidolonCrunch = "\n\n• {b}Tradition{/b} Divine\n• {b}Skills{/b} Religion, Intimidation\n\n" +
            "{b}Initial Eidolon Ability (Hellfire Scourage).{/b} Your eidolon gains resistance to fire equal to their level (minimum 1), and an equivalent weakness to good (minimum 1). In addition, the first attack they make against a Frightened creature each turn, deals an additional 1d4 fire damage." +
            "\n\n{i}At level 7{/i}\n{b}Symbiosis Eidolon Ability (Legion Commander) {icon:Action}.{/b} Your eidolon shouts a commands at one ally within 30-feet. The next time that ally attacks or makes a skill check before the start of your next turn, your eidolon can use their reaction {icon:Reaction} " +
            "to make an Intimidation check against an easy DC for their level.\n{b}Critical success{/b} You grant your ally a +2 circumstance bonus to the triggering check. If you're a master with the check you attempted, the bonus is +3, and if you're legendary, it's +4." +
            "\n{b}Success{/b} You grant your ally a +1 circumstance bonus to the triggering check.\n{b}Critical failure{/b} Your ally takes a –1 circumstance penalty to the triggering check.\n\nYour ally also deals extra fire damage equal to half your level, " +
            "if the action your eidolon was assisting them with was an attack, and double if the attack was a critial success." +
            "\n\nThis subclass is designed and contributed by {link:https://www.reddit.com/r/Pathfinder2e/comments/1bqusiu/leorandgers_summoning_circle_new_eidolons/}LeoRandger{/}.";

        private static readonly string FeyEidolonFlavour = "Your eidolon is a fey, a capricious being of the mysterious First World. Many fey appear similar to mortal humanoids with unusual features such as pointed ears, " +
            "wings, or bodies composed of natural elements, but the full variety of fey is endless, and many others appear completely inhuman. Fey from the First World never truly die, instead forming a new creature. " +
            "Fey eidolons usually come about when a summoner helps stabilize a difficult reformation. This means your fey eidolon likely lived a different life just before meeting you and might remember fragments of its old memories. " +
            "Together, you might have to unravel a memory from your eidolon's past life among the fey.";

        private static readonly string FeyEidolonCrunch = "\n\n• {b}Tradition{/b} Primal\n• {b}Skills{/b} Nature, Deception\n\n" +
            "{b}Initial Eidolon Ability (Fey Gifts).{/b} Your eidolon expands your primal magic with enchantment and illusion magic, allowing both of you to wield the power of fey charm and glamour. When you add spells to your repertoire, " +
            "you can choose from the primal list as well as from enchantment and illusion spells that appear on the arcane spell list. As usual for when you add spells of a different tradition to your spell list, you're still a primal spellcaster, " +
            "so all of your spells are primal spells.\n\nYour eidolon gains the Magical Understudy summoner feat, despite not meeting the prerequisite level, and it can choose fey gift cantrips in addition to primal cantrips." +
            "\n\n{i}At level 7{/i}\n{b}Symbiosis Eidolon Ability (Fey Mischief).{/b} Your eidolon's fey magic becomes more powerful and mischievous. Your eidolon gains the Magical Adept feat, despite not meeting the prerequisite level, " +
            "and can choose from fey gift spells in addition to primal spells.";

        private static readonly string AngerPhantomFlavour = "Your eidolon is a lost soul, bound to the mortal world by undying anger or a bitter grudge. Most phantom eidolons are humanoids with a spectral or ectoplasmic appearance, " +
            "though some take far stranger forms. Your link with your eidolon prevents it from succumbing to corruption and undeath. Together, you will need to decide whether to work with your eidolon to control its anger, or channel its wrath into power.";

        private static readonly string AngerPhantomCrunch = "\n\n• {b}Tradition{/b} Occult\n• {b}Skills{/b} Occultism, Intimidation\n\n" +
            "{b}Initial Eidolon Ability (Fenzied Assault) {icon:TwoActions}.{/b} Your eidolon makes two strikes against a single target, one with each of its unarmed attacks, at its current MAP penalty. " +
            "The damage from both attacks are combined for the purposes of damage resistance." +
            "\n\n{i}At level 7{/i}\n{b}Symbiosis Eidolon Ability (Seething Frenzy) {icon:Action}.{/b} Your eidolon enters a seething frenzy, disregarding its own safety to tear your foes apart. " +
            "It gains temporary HP equal to its level, and a +4 damage bonus to its unarmed strike attacks, but it takes a -2 penalty to AC. The rage lasts until the end of the encounter, and leaves your eidolon fatigued if they leave early.";

        private static readonly string PlantEidolonFlavour = "Your eidolon is an intelligent plant, formed from the same disembodied fragments of nature's life energy that become leshys. Plant eidolons tend to be curious and adaptable, " +
            "with temperaments based on the parts of mortal culture they feel affinity toward. Despite coming from the same source, plant eidolons don't always look like leshys. Plant eidolons have forms that vary greatly and can look like " +
            "almost any kind of plant creature in existence. Some even resemble plant creatures so strange they are impossible to identify.";

        private static readonly string PlantEidolonCrunch = "\n\n• {b}Tradition{/b} Primal\n• {b}Skills{/b} Nature, Medicine\n\n" +
            "{b}Initial Eidolon Ability (Tendril Strike) {icon:Action}.{/b} Stretching to extend its body to its limits, your eidolon attacks a foe that would normally be beyond its reach. Your eidolon makes a melee unarmed Strike, " +
            "increasing its reach by 5 feet for that Strike. If the unarmed attack has the disarm, shove, or trip trait, the eidolon can use the corresponding action instead of a Strike." +
            "\n\n{i}At level 7{/i}\n{b}Symbiosis Eidolon Ability (Growing Vines).{/b} Your eidolon's vines and branches lengthen even more. All your eidolon's melee unarmed attacks gain the reach trait.";

        private static readonly string UndeadEidolonFlavour = "Your eidolon is an undead spirit pulled from the Ethereal Plane or Negative Energy Plane, embodied, and bound to your life force in an unusual, " +
            "potentially antithetical way that even other summoners can't quite understand. Undead eidolons take about every imaginable shape and form, as their bodies manifest from their connection to you. " +
            "Their ultimate form can be influenced by an amalgamation of the echoes and memories of their old life before becoming undead, their cause of death, their encounters in the afterlife, and portions " +
            "of your own essences. Together, you and your eidolon need to explore the mysteries of life, death, and undeath to understand what your bond means for both of your futures.";

        private static readonly string UndeadEidolonCrunch = "\n\n• {b}Tradition{/b} Divine\n• {b}Skills{/b} Religion, Intimidation\n\n" +
            "{b}Initial Eidolon Ability (Negative Essence).{/b} Your eidolon is undead, though unlike true undead, your connection grants it a sliver of life. It has negative healing, but instead of the usual immunities it gets a " +
            "+2 circumstance bonus to death, disease and poison effects. Additionally, it gains a +5 bonus to staunch persistent bleed damage." +
            "\n\n{i}At level 7{/i}\n{b}Symbiosis Eidolon Ability (Drain Life) {icon:TwoActions}.{/b} Your eidolon's link with you sustains it, but it still craves the life essence of the living, whether through blood or pure essence. " +
            "It gains the Drain Life activity.\n\n" +
            "Your eidolon attacks a living creature and drains some of the creature's life force to feed your shared link. " +
            "Your eidolon Strikes a living enemy. If the Strike hits and deals damage, the target must attempt a Fortitude save, with the following effects. On a critical hit, the enemy uses the result one degree worse than it rolled.\n\n" +
            "{b}Critical success{/b} No effect.\nSuccess Your eidolon drains a small amount of life force. The enemy takes additional negative damage equal to half your level.\n" +
            "{b}Failure{/b} Your eidolon drains enough life force to satisfy itself. " +
            "The enemy takes additional negative damage equal to half your level and is drained 1. Your eidolon gains temporary Hit Points equal to the enemy's level, which last for 1 minute.\n" +
            "{b}Critical failure{/b} Your eidolon drains an incredible amount of life force and is thoroughly glutted with energy. As failure, but the enemy is drained 2 and the temporary Hit Points are equal to double the enemy's level.";

        private static readonly string PsychopompEidolonFlavour = "Your eidolon is a psychopomp, a creature whose sworn duty is to usher souls safely to the afterlife and maintain the courts of the dead. " +
            "Psychopomp eidolons have a variety of appearances, but since they often traffic with mortals, their form typically includes an elaborate mask. You and your psychopomp eidolon share an important " +
            "fate together, whether it relates directly to your own soul or to a mission that will somehow protect the souls of others.";

        private static readonly string PsychopompEidolonCrunch = "\n\n• {b}Tradition{/b} Divine\n• {b}Skills{/b} Religion, Intimidation\n\n" +
            "{b}Initial Eidolon Ability (Spirit Touch).{/b} Your eidolon's unarmed strikes deals an extra 1 negative damage to living creatures and an extra 1 positive damage to undead, and possess the ghost touch property." +
            "\n\n{i}At level 7{/i}\n{b}Symbiosis Eidolon Ability (Soul Wrench) {icon:TwoActions}.{/b}\n\n" +
            "{b}Range {/b}5 feet\n{b}Target {/b}1 undead, or living creature of non-planar origin.\n{b}Saving throw {/b} Will\n\nYour eidolon attempts to wrench the soul from the body of an adjacent creature, holding it in " +
            "place and stopping it from returning to its origional owner.\n\n" +
            "At stage 1, the target gains enfeebled and clumsy 1 as they experience their soul being partially wrenched from their body. At stage 2, they gain slowed 1 and enfeebled and clumsy 2. " +
            "At stage 3, your eidolon removes their soul completely. Living creatures are paralyzed, their bodies left as soulness husks. Undead are immediately destroyed, unless they're of an equal or higher level than your" +
            " eidolon, in which case they suffer force damage equal to twice your eidolon's level their soul escapes back intoto their body.\n\n" +
            "{b}Failure{/b} The target gains soul wretch 1.\n" +
            "{b}Critical failure{/b} The target gains soul wretch 2.\n\n" +
            "Your eidolon must sustain this ability each turn to maintain their hold on the target's soul. Each time they do so, the target makes another will saving throw to determine how much progress your " +
            "eidolon makes towards wretching out their soul completely.\n\n" +
            "{b}Critical success{/b} The target's soul breaks free from your eidolon's grasp.\n" +
            "{b}Failure{/b} The target's soul wretch increased by 1 stage.\n" +
            "{b}Critical failure{/b} The target's soul wretch increased by 2 stages.";

        private static readonly string ElementalEidolonFlavour = "Your eidolon is a primal chunk of elemental matter infused with sapience, power, and identity, but unable to manifest a true form of their own without the life force you provide" +
               "via your connection. Most elementals in their natural environment already have different sorts of forms, from vaguely humanoid, to animalistic, to simple masses of their component element. As your life force "
               + "provides your eidolon the instincts necessary to adopt a physical form, their appearance varies based on the strength of their own self image and your prior exposure to elementals. Elemental eidolons tend to " +
               "reach their unusual state— powerful but formless—as the result of large scale events or cataclysms, such as the war to seal the benevolent Elemental Lords or their recent unsealing.\n\n" +
               "Whether elemental eidolons possess any memories of a previous life or are a new sapience formed from leftover essence of a mighty servant of the Elemental Lords brought low varies from eidolon to eidolon. " +
               "Together, you might undertake a journey to understand your eidolon's mysterious past or leave the past behind and forge a new destiny of your own.";

        private static readonly string ElementalEidolonCrunch = "\n\n• {b}Tradition{/b} Primal\n• {b}Skills{/b} Nature, Athletics\n\n" +
            "{b}Initial Eidolon Ability (Elemental Core).{/b} Your eidolon's elemental nature grants it a +2 circumstance bonus to saves against being poisoned, and against the paralyze spell. " +
            "Additionally, it gains a +5 bonus to staunch persistent bleed damage. You can choose to form a bond with an {i}air, earth, fire, metal{/i} or {/i}water{/i} elemental. Your eidolon and all their unarmed attacks " +
            "gain the trait of the chosen element, as well as additional effects based on your choice." +
            "\n\n{i}At level 7{/i}\n{b}Symbiosis Eidolon Ability (Elemental Burst) {icon:TwoActions}.{/b}\n\n" +
            "{b}Range{/b} 60 feet\n{b}Area{/b} 20 foot burst\n{b}Saving throw{/b} basic Reflex\n{b}Frequency{/b} 1/ encounter\n\n" +
            "Your eidolon rips off a chunk of elemental matter from their own form and hurls it into a group of foes. Your eidolon loses a number of Hit Points equal to your level, dealing 6d6 damage to all creatures inside the burst. " +
            "The damage increases by 1d6 for each level you have beyond 7th. The damage's type is either fire damage if your eidolon is a fire elemental, or the same physical damage type as your eidolon's primary unarmed attack if your " +
            "eidolon isn't a fire elemental. Elemental Burst gains any traits that your eidolon's unarmed attacks gain from elemental core.";

        private static readonly string DemonEidolonFlavour = "Your eidolon is a demon, a chaotic evil invader from the stars, using its link to you to spread chaos at your side. Choose an associated sin for your " +
            "demon. Demon eidolons have appearances as varied as the infinite Abyss. While demons are inherently untrustworthy, your demon eidolon has reached an accord with you and generally keeps it, though that doesn't mean your eidolon isn't " +
            "actively working to bring your life deeper into its associated sin.";

        private static readonly string DemonEidolonCrunch = "{b}Special.{/b} This eidolon option expands upon the controversial options presented in Secrets of Magic, with several new alternatives. To use the vanilla demon, you can select the 'Classic' sin.\n\n• {b}Tradition{/b} Divine\n• {b}Skills{/b} You gain Religion and a bonus skill associated with your eidolon's demonic nature." +
            "\n\n{b}Initial Eidolon Ability (Lesser Demonic Nature).{/b} Your demon gains unique powers and weaknesses associated with its sin." +
            "\n\n{i}At level 7{/i}\n{b}Symbiosis Eidolon Ability (Greater Demonic Nature).{/b} Your demon gains a unique once per encounter ability associated with its sin.";

        internal static IEnumerable<Feat> LoadSubclasses() {

            //yield return new Feat(ftKeyEidolonAbilityStr, "", "", new List<Trait>() { }, null);
            //yield return new Feat(ftKeyEidolonAbilityDex, "", "", new List<Trait>() { }, null);

            // Init subclasses
            subclasses.Add(new EidolonBond(Enums.scAngelicEidolon, AngelicEidolonFlavour, AngelicEidolonCrunch, Trait.Divine, new List<FeatName>() { FeatName.Religion, FeatName.Diplomacy },
                new Func<Feat, bool>(ft => new FeatName[] { ftALawfulGood, ftAGood, ftAChaoticGood }.Contains(ft.FeatName)), new List<Trait> { Trait.Celestial })
            .WithClassFeatures((eidolon, summoner) => AngelEidolonLogic(eidolon))
            .WithAbilityText("\n{b}Hallowed Strikes.{/b} Your eidolon's unarmed strikes deal +1 extra good damage.\n")
            .WithOnSheet((Action<CalculatedCharacterSheetValues>)(values => {
                values.AddSelectionOptionRightNow((SelectionOption)new SingleFeatSelectionOption("AngelicEidolonArray", "Eidolon Ability Scores", 1, (Func<Feat, bool>)(ft => new FeatName[] { Enums.scAngelicEidolonAvenger, Enums.scAngelicEidolonEmmissary }.Contains(ft.FeatName))));
            })));

            yield return CreateEidolonFeat(Enums.scAngelicEidolonAvenger, "Your eidolon is a fierce warrior of the heavens.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText, new int[6] { 4, 2, 3, -1, 1, 0 }, 2, 3).WithTag(Ability.Strength);
            yield return CreateEidolonFeat(Enums.scAngelicEidolonEmmissary, "Your eidolon is a regal emmisary of the heavens.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText, new int[6] { 1, 4, 1, 0, 1, 2 }, 1, 4).WithTag(Ability.Dexterity);


            subclasses.Add(new EidolonBond(scAngerPhantom, AngerPhantomFlavour, AngerPhantomCrunch, Trait.Occult, new List<FeatName>() { FeatName.Occultism, FeatName.Intimidation }, new Func<Feat, bool>(ft => ft.HasTrait(tAlignment)), new List<Trait> { })
            .WithClassFeatures((eidolon, summoner) => AngerPhantomEidolonLogic(eidolon, summoner))
            .WithActionText("{b}Frenzied Assault{/b} {{icon:TwoActions} Your eidolon makes two strikes against a single target, one with each of its unarmed attacks, at its current MAP penalty. " +
                "The damage from both attacks are combined for the purposes of damage resistance.\n")
            .WithOnSheet((Action<CalculatedCharacterSheetValues>)(values => {
                values.AddSelectionOptionRightNow((SelectionOption)new SingleFeatSelectionOption("AngerPhantomEidolonArray", "Eidolon Ability Scores", 1, (Func<Feat, bool>)(ft => new FeatName[] { scAngerPhantomBerserker, scAngerPhantomAssassin }.Contains(ft.FeatName))));
            })));

            yield return CreateEidolonFeat(scAngerPhantomBerserker, "Your eidolon is an unyielding guardian.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText, new int[6] { 4, 2, 3, -1, 0, 1 }, 2, 3).WithTag(Ability.Strength);
            yield return CreateEidolonFeat(scAngerPhantomAssassin, "Your eidolon is a vigilant protector.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText, new int[6] { 2, 4, 3, 0, -1, 1 }, 1, 4).WithTag(Ability.Dexterity);


            subclasses.Add(new EidolonBond(Enums.scAzataEidolon, AzataEidolonFlavour, AzataEidolonCrunch, Trait.Divine, new List<FeatName>() { FeatName.Religion, FeatName.Diplomacy },
                new Func<Feat, bool>(ft => new FeatName[] { Enums.ftAChaoticGood }.Contains(ft.FeatName)), new List<Trait>() { Trait.Homebrew }, new List<Trait> { Trait.Celestial }, null)
            .WithClassFeatures((eidolon, summoner) => AzataEidolonLogic(eidolon, summoner))
            .WithActionText("{b}Celestial Passion{/b} {{icon:Action} One ally within 30-feet of your eidolon gains temporary HP equal to its level, and a +1 bonus to attack and skill checks for 1 round. Cannot be used on the same ally more than once per encounter.\n")
            .WithOnSheet((Action<CalculatedCharacterSheetValues>)(values => {
                values.AddSelectionOptionRightNow((SelectionOption)new SingleFeatSelectionOption("AzataEidolonArray", "Eidolon Ability Scores", 1, (Func<Feat, bool>)(ft => new FeatName[] { scAzataEidolonCrusader, scAzataEidolonPoet }.Contains(ft.FeatName))));
            })));

            yield return CreateEidolonFeat(scAzataEidolonCrusader, "Your eidolon is a benevolant crusader of Elysium.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText, new int[6] { 4, 2, 3, -1, 0, 1 }, 2, 3).WithTag(Ability.Strength);
            yield return CreateEidolonFeat(scAzataEidolonPoet, "Your eidolon is an inspiring muse of Elysium.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText, new int[6] { 1, 4, 1, -1, 1, 3 }, 1, 4).WithTag(Ability.Dexterity);



            subclasses.Add(new EidolonBond(scBeastEidolon, BeastEidolonFlavour, BeastEidolonCrunch, Trait.Primal, new List<FeatName>() { FeatName.Nature, FeatName.Intimidation }, new Func<Feat, bool>(ft => ft.HasTrait(tAlignment)), new List<Trait> { Trait.Beast, Trait.Animal })
            .WithClassFeatures((eidolon, summoner) => BeastEidolonLogic(eidolon, summoner))
            .WithActionText("{b}Beast's Charge{/b} {{icon:TwoActions} Stride twice. If you end your movement within melee reach of at least one enemy, you can make a melee Strike against that enemy. If your eidolon moved at least 20ft and ends it's movement in a cardinal diraction, it gains a +1 circumstance bonus to this attack roll.\n")
            .WithOnSheet((Action<CalculatedCharacterSheetValues>)(values => {
                values.AddSelectionOptionRightNow((SelectionOption)new SingleFeatSelectionOption("AngelicEidolonArray", "Eidolon Ability Scores", 1, (Func<Feat, bool>)(ft => new FeatName[] { scBeastEidolonBrutal, scBeastEidolonFleet }.Contains(ft.FeatName))));
            })));

            yield return CreateEidolonFeat(scBeastEidolonBrutal, "Your eidolon is a powerful and brutally strong beast.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText, new int[6] { 4, 2, 3, -1, 1, 0 }, 2, 3).WithTag(Ability.Strength);
            yield return CreateEidolonFeat(scBeastEidolonFleet, "Your eidolon is a fleet and agile beast.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText, new int[6] { 2, 4, 3, -1, 1, 0 }, 1, 4).WithTag(Ability.Dexterity);



            subclasses.Add(new EidolonBond(scDevoPhantomEidolon, DevoPhantomEidolonFlavour, DevoPhantomEidolonCrunch, Trait.Occult, new List<FeatName>() { FeatName.Occultism, FeatName.Medicine }, new Func<Feat, bool>(ft => ft.HasTrait(tAlignment)), new List<Trait> { })
            .WithClassFeatures((eidolon, summoner) => DevoPhantomEidolonLogic(eidolon, summoner))
            .WithAbilityText("\n{b}Dutiful Retaliation {icon:Reaction}.{/b} Your eidolon makes a strike again an enemy that damaged you. Both your eidolon and your attacker must be within 15ft of you.\n")
            .WithOnSheet((Action<CalculatedCharacterSheetValues>)(values => {
                values.AddSelectionOptionRightNow((SelectionOption)new SingleFeatSelectionOption("DevoPhantomEidolonArray", "Eidolon Ability Scores", 1, (Func<Feat, bool>)(ft => new FeatName[] { scDevoPhantomEidolonStalwart, scDevoPhantomEidolonSwift }.Contains(ft.FeatName))));
            })));

            yield return CreateEidolonFeat(scDevoPhantomEidolonStalwart, "Your eidolon is an unyielding guardian.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText,new int[6] { 4, 2, 3, 0, 0, 0 }, 2, 3).WithTag(Ability.Strength);
            yield return CreateEidolonFeat(scDevoPhantomEidolonSwift, "Your eidolon is a vigilant protector.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText, new int[6] { 2, 4, 3, 0, 0, 0 }, 1, 4).WithTag(Ability.Dexterity);


            subclasses.Add(new Feat(Enums.scDraconicEidolon, DraconicEidolonFlavour, DraconicEidolonCrunch, new List<Trait>() { }, null)
            .WithTag("{b}Breath Weapon{/b} {{icon:TwoActions} Your eidolon exhales a line or cone of energy and deal 2d6 of the damage associated with your eidolon's dragon type to each target. You can't use breath weapon again for 1d4 rounds.\n")
            .WithOnSheet((Action<CalculatedCharacterSheetValues>)(values => {
                values.AddSelectionOptionRightNow((SelectionOption)new SingleFeatSelectionOption("DragonType", "Dragon Type", 1, (Func<Feat, bool>)(ft => ft.HasTrait(Enums.tDragonType))));
                values.AddSelectionOptionRightNow((SelectionOption)new SingleFeatSelectionOption("DraconicEidolonArray", "Eidolon Ability Scores", 1, (Func<Feat, bool>)(ft => new FeatName[] { Enums.scDraconicEidolonCunning, Enums.scDraconicEidolonMarauding }.Contains(ft.FeatName))));
            })));

            yield return CreateEidolonFeat(scDraconicEidolonCunning, "Your eidolon is a cunning wyrm.", null, (subclasses.Last()).Tag as string, new int[6] { 1, 4, 1, 2, 1, 1 }, 1, 4).WithTag(Ability.Dexterity);
            yield return CreateEidolonFeat(scDraconicEidolonMarauding, "Your eidolon is a fierce marauding drake.", null, (subclasses.Last()).Tag as string, new int[6] { 4, 2, 3, 0, 0, 0 }, 2, 3).WithTag(Ability.Strength);

            // Dragon breath feats
            Feat dragonLineBreath = new Feat(Enums.ftBreathWeaponLine, "Your dragon eidolon emits a sharp, destructive line of energy.", "Your eidolon's breath weapon hits each creature in a 60-foot line.", new List<Trait>() { Enums.tBreathWeaponArea }, null);
            Feat dragonConeBreath = new Feat(Enums.ftBreathWeaponCone, "Your dragon eidolon spaws worth a torrent of destructive energy.", "Your eidolon's breath weapon hits each creature in a 30-foot cone.", new List<Trait>() { Enums.tBreathWeaponArea }, null);


            subclasses.Add(new EidolonBond(scDevilEidolon, DevilEidolonFlavour, DevilEidolonCrunch, Trait.Divine, new List<FeatName>() { FeatName.Religion, FeatName.Intimidation },
                new Func<Feat, bool>(ft => new FeatName[] { ftALawfulEvil }.Contains(ft.FeatName)), new List<Trait>() { Trait.Homebrew }, new List<Trait> { Trait.Fiend }, null)
            .WithClassFeatures((eidolon, summoner) => DevilEidolonLogic(eidolon, summoner))
            .WithAbilityText("\n{b}Hellfire Scourge{/b} Your eidolon deals +1d4 fire damage to the first frightened creature it strikes each round.\n")
            .WithOnSheet((Action<CalculatedCharacterSheetValues>)(values => {
                values.AddSelectionOptionRightNow((SelectionOption)new SingleFeatSelectionOption("DevilEidolonArray", "Eidolon Ability Scores", 1, (Func<Feat, bool>)(ft => new FeatName[] { scDevilEidolonBarrister, scDevilEidolonLegionnaire }.Contains(ft.FeatName))));
            })));

            yield return CreateEidolonFeat(scDevilEidolonLegionnaire, "Your eidolon is a ruthlessly professional legionnaire of hell.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText, new int[6] { 4, 2, 2, 0, -1, 2 }, 2, 3).WithTag(Ability.Strength);
            yield return CreateEidolonFeat(scDevilEidolonBarrister, "Your eidolon is a cunning and corruptive barrister of hell.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText, new int[6] { 1, 4, 1, 0, 1, 2 }, 1, 4).WithTag(Ability.Dexterity);



            subclasses.Add(new EidolonBond(scElementalEidolon, ElementalEidolonFlavour, ElementalEidolonCrunch, Trait.Primal, new List<FeatName>() { FeatName.Nature, FeatName.Athletics },
                new Func<Feat, bool>(ft => ft.HasTrait(tAlignment)), new List<Trait> { Trait.Elemental })
            .WithClassFeatures(ElementalEidolonLogic)
            .WithAbilityText("{b}Elemental Core.{/b} Your eidolon's elemental nature grants it a +2 circumstance bonus to saves against being poisoned, and against the paralyze spell. Additionally, it gains a +5 bonus to staunch persistent bleed damage.\n")
            .WithOnSheet((Action<CalculatedCharacterSheetValues>)(values => {
                values.AddSelectionOptionRightNow(new SingleFeatSelectionOption("ElementalType", "Eidolon Element", 1, ft => ft.HasTrait(tElementalType)));
                values.AddSelectionOptionRightNow(new SingleFeatSelectionOption("ElementalEidolonArray", "Eidolon Ability Scores", 1, (ft => new FeatName[] { scElementalEidolonAdaptable, scElementalEidolonPrimordial }.Contains(ft.FeatName))));
            })));

            yield return CreateEidolonFeat(scElementalEidolonPrimordial, "Your eidolon is a hulking undead abomination.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText, new int[6] { 4, 2, 3, -1, 1, 0 }, 2, 3).WithTag(Ability.Strength);
            yield return CreateEidolonFeat(scElementalEidolonAdaptable, "Your eidolon is a ghoulish stalker of the living.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText, new int[6] { 1, 4, 3, 0, 1, 0 }, 1, 4).WithTag(Ability.Dexterity);



            subclasses.Add(new EidolonBond(scFeyEidolon, FeyEidolonFlavour, FeyEidolonCrunch, Trait.Primal, new List<FeatName>() { FeatName.Nature, FeatName.Deception },
                new Func<Feat, bool>(ft => ft.HasTrait(tAlignment)), new List<Trait> { Trait.Fey })
            .WithOnSheet((Action<CalculatedCharacterSheetValues>)(values => {
                values.AddSelectionOptionRightNow((SelectionOption)new SingleFeatSelectionOption("FeyEidolonArray", "Eidolon Ability Scores", 1, (Func<Feat, bool>)(ft => new FeatName[] { scFeyEidolonSkirmisher, scFeyEidolonTrickster }.Contains(ft.FeatName))));
            })));

            yield return CreateEidolonFeat(scFeyEidolonSkirmisher, "Your eidolon is an illusive predator of the first world.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText, new int[6] { 2, 4, 2, 0, 0, 1 }, 1, 4).WithTag(Ability.Dexterity);
            yield return CreateEidolonFeat(scFeyEidolonTrickster, "Your eidolon is a mercurial trickster of the first world.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText, new int[6] { 1, 4, 1, 1, -1, 3 }, 1, 4).WithTag(Ability.Dexterity);



            subclasses.Add(new EidolonBond(scPsychopompEidolon, PsychopompEidolonFlavour, PsychopompEidolonCrunch, Trait.Divine, new List<FeatName>() { FeatName.Religion, FeatName.Intimidation },
                ft => new FeatName[] { ftANeutral }.Contains(ft.FeatName), new List<Trait> { Trait.Monitor })
            .WithClassFeatures(PsychopompEidolonLogic)
            .WithAbilityText("\n{b}Spirit Touch.{/b} Your eidolon's unarmed strikes deals an extra 1 negative damage to living creatures and an extra 1 positive damage to undead, and possess the ghost touch property.\n")
            .WithOnSheet((Action<CalculatedCharacterSheetValues>)(values => {
                values.AddSelectionOptionRightNow((SelectionOption)new SingleFeatSelectionOption("PsychopompEidolonArray", "Eidolon Ability Scores", 1, (Func<Feat, bool>)(ft => new FeatName[] { scPsychopompEidolonGuardian, scPsychopompEidolonScribe }.Contains(ft.FeatName))));
            })));

            yield return CreateEidolonFeat(scPsychopompEidolonGuardian, "Your eidolon is a vigilant protector of lost souls destined for the Boneyard.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText, new int[6] { 4, 2, 3, 0, 1, -1 }, 2, 3).WithTag(Ability.Strength);
            yield return CreateEidolonFeat(scPsychopompEidolonScribe, "Your eidolon is diligant archiver of prophecy and mortal transgression.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText, new int[6] { 1, 4, 1, 2, 1, 0 }, 1, 4).WithTag(Ability.Dexterity);



            subclasses.Add(new EidolonBond(scPlantEidolon, PlantEidolonFlavour, PlantEidolonCrunch, Trait.Primal, new List<FeatName>() { FeatName.Nature, FeatName.Medicine },
                new Func<Feat, bool>(ft => ft.HasTrait(tAlignment)), new List<Trait> { Trait.Plant })
            .WithClassFeatures(PlantEidolonLogic)
            .WithAbilityText("\n{b}Tendril Strike {icon:Action}.{/b} Your eidolon makes a melee unarmed Strike, increasing its reach by 5 feet for that Strike. If the unarmed attack has the disarm, shove, or trip trait, the eidolon can use the corresponding action instead of a Strike.\n")
            .WithOnSheet((Action<CalculatedCharacterSheetValues>)(values => {
                values.AddSelectionOptionRightNow((SelectionOption)new SingleFeatSelectionOption("PlantEidolonArray", "Eidolon Ability Scores", 1, (Func<Feat, bool>)(ft => new FeatName[] { scPlantEidolonGuardian, scPlantEidolonCreeping }.Contains(ft.FeatName))));
            })));

            yield return CreateEidolonFeat(scPlantEidolonGuardian, "Your eidolon is a stalward guardian of nature.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText, new int[6] { 4, 2, 3, -1, 1, 0 }, 2, 3).WithTag(Ability.Strength);
            yield return CreateEidolonFeat(scPlantEidolonCreeping, "Your eidolon is patient, predatory plant, such as a carnivorous vine or flytrap.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText, new int[6] { 1, 4, 3, -1, 2, 0 }, 1, 4).WithTag(Ability.Dexterity);



            subclasses.Add(new EidolonBond(scUndeadEidolon, UndeadEidolonFlavour, UndeadEidolonCrunch, Trait.Divine, new List<FeatName>() { FeatName.Religion, FeatName.Intimidation },
                new Func<Feat, bool>(ft => ft.HasTrait(tAlignment)), new List<Trait> { Trait.Undead })
            .WithClassFeatures(UndeadEidolonLogic)
            .WithAbilityText("{b}Negative Essence.{/b} Your eidolon is undead, though unlike true undead, your connection grants it a sliver of life. It has negative healing, but instead of the usual immunities it gets a +2 circumstance bonus to death, disease and poison effects. Additionally, it gains a +5 bonus to staunch persistent bleed damage.\n")
            .WithOnSheet((Action<CalculatedCharacterSheetValues>)(values => {
                values.AddSelectionOptionRightNow((SelectionOption)new SingleFeatSelectionOption("UndeadEidolonArray", "Eidolon Ability Scores", 1, (Func<Feat, bool>)(ft => new FeatName[] { scUndeadEidolonBrute, scUndeadEidolonStalker }.Contains(ft.FeatName))));
            })));

            yield return CreateEidolonFeat(scUndeadEidolonBrute, "Your eidolon is a hulking undead abomination.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText, new int[6] { 4, 2, 3, -1, 0, 1 }, 2, 3).WithTag(Ability.Strength);
            yield return CreateEidolonFeat(scUndeadEidolonStalker, "Your eidolon is a ghoulish stalker of the living.", (subclasses.Last() as EidolonBond).AbilityText, (subclasses.Last() as EidolonBond).ActionText, new int[6] { 2, 4, 3, -1, 1, 0 }, 1, 4).WithTag(Ability.Dexterity);



            // Elemental type subfeats
            yield return new EvolutionFeat(ModManager.RegisterFeatName("Summoner_AirElemental", "Air Elemental"), 1, "Your eidolon is formed from elemental air and is light as a breeze.", "Your eidolon gains the airborn form evolution feat at 1st level.",
                new Trait[] { Trait.Air, Enums.tElementalType }, cr => { }, null)
            .WithOnSheet(sheet => {
                sheet.GrantFeat(ftAirbornForm);
                EidolonBond? bond = (EidolonBond?) sheet.AllFeats.FirstOrDefault(ft => ft is EidolonBond);
                if (bond != null) {
                    bond.eidolonTraits = new List<Trait>() { Trait.Elemental, Trait.Air };
                }
            });

            yield return new EvolutionFeat(ModManager.RegisterFeatName("Summoner_EarthElemental", "Earth Elemental"), 1, "Your eidolon is formed from elemental earth, and is incredibly hard to move by force.",
                "Your eidolon gains a +2 circumstance bonus to their save DCs against attempts to Shove or Trip them, and are immune to forced movement.",
                new Trait[] { Trait.Earth, Enums.tElementalType }, cr => { }, null)
            .WithOnSheet(sheet => {
                EidolonBond? bond = (EidolonBond?)sheet.AllFeats.FirstOrDefault(ft => ft is EidolonBond);
                if (bond != null) {
                    bond.eidolonTraits = new List<Trait>() { Trait.Elemental, Trait.Earth };
                }
            });

            yield return new EvolutionFeat(ModManager.RegisterFeatName("Summoner_FireElemental", "Fire Elemental"), 1, "Your eidolon is formed from elemental fire and burns with embers of flame.",
                "Your eidolon gains resistance equal to half your level (minimum 1) to fire and an equal amount of weakness to cold and water. Their unarmed attacks deal 1 additional fire damage.",
                new Trait[] { Trait.Fire, Enums.tElementalType }, cr => { }, null)
            .WithOnSheet(sheet => {
                EidolonBond? bond = (EidolonBond?)sheet.AllFeats.FirstOrDefault(ft => ft is EidolonBond);
                if (bond != null) {
                    bond.eidolonTraits = new List<Trait>() { Trait.Elemental, Trait.Fire };
                    //bond.AbilityText += "\n{b}Fire Elemental.{/b} Your eidolon's unarmed attacks deal 1 additional fire damage.\n";
                }
            });

            string traitTags = "\n\n{b}" + Trait.VersatileB.HumanizeTitleCase2() + "{/b} " + Trait.VersatileB.GetTraitProperties().RulesText +
                "\n{b}" + Trait.VersatileP.HumanizeTitleCase2() + "{/b} " + Trait.VersatileP.GetTraitProperties().RulesText +
                "\n{b}" + Trait.VersatileS.HumanizeTitleCase2() + "{/b} " + Trait.VersatileS.GetTraitProperties().RulesText;

            yield return new EvolutionFeat(ModManager.RegisterFeatName("Summoner_MetalElemental", "Metal Elemental"), 1, "Your eidolon is formed from elemental metal and can adapt their metallic form to battle.",
                "One of your eidolon's starting melee unarmed attacks gains the versatile bludgeoning, piercing, or slashing trait, as your eidolon learns how to shift the metal into various weaponlike forms.",
                new Trait[] { Trait.Metal, Enums.tElementalType }, cr => {}, new List<Feat>() {
                    new Feat(ModManager.RegisterFeatName("MetalElemental_PrimaryUnarmedAttack", "Primary Unarmed Attack"), "",
                    "Your eidolon's primary natural weapon attack gains the Versatile S, P and B traits." + traitTags, new List<Trait>() { tMetalElementalAtkType }, null),
                    new Feat(ModManager.RegisterFeatName("MetalElemental_SecondaryUnarmedAttack", "Secondary Unarmed Attack"), "",
                    "Your eidolon's secondary natural weapon attack gains the Versatile S, P and B traits." + traitTags, new List<Trait>() { tMetalElementalAtkType }, null)
                })
            .WithOnSheet(sheet => {
                EidolonBond? bond = (EidolonBond?)sheet.AllFeats.FirstOrDefault(ft => ft is EidolonBond);
                if (bond != null) {
                    bond.eidolonTraits = new List<Trait>() { Trait.Elemental, Trait.Metal };
                }
            });

            yield return new EvolutionFeat(ModManager.RegisterFeatName("Summoner_WaterElemental", "Water Elemental"), 1, "Your eidolon is formed from elemental water and swims with ease.",
                "Your eidolon has a swim speed, they are not flat-footed while in water, and you don't take the usual penalties for making bludgeoning or slashing melee attacks in water.",
                new Trait[] { Trait.Water, Enums.tElementalType }, cr => { }, null)
            .WithOnSheet(sheet => {
                EidolonBond? bond = (EidolonBond?)sheet.AllFeats.FirstOrDefault(ft => ft is EidolonBond);
                if (bond != null) {
                    bond.eidolonTraits = new List<Trait>() { Trait.Elemental, Trait.Aquatic, Trait.Water };
                    //bond.AbilityText += "\n{b}Water Elemental.{/b} Your eidolon has a swim speed, they are not flat-footed while in water, and you don't take the usual penalties for making bludgeoning or slashing melee attacks in water.\n";
                }
            });

            yield return new EvolutionFeat(ModManager.RegisterFeatName("Summoner_WoodElemental", "Wood Elemental"), 1, "Your eidolon is formed from elemental wood, and its living wooden form twists and regrows as you focus your elemental energies.",
                "Your eidolon cans the regrowth ability.\n\n{b}Regrowth {icon:Action}.{/b} Your eidolon regains a number of hits points equal to three times its level. Usable once per day.",
                new Trait[] { Trait.Wood, Enums.tElementalType }, cr => { }, null)
            .WithOnSheet(sheet => {
                EidolonBond? bond = (EidolonBond?)sheet.AllFeats.FirstOrDefault(ft => ft is EidolonBond);
                if (bond != null) {
                    bond.eidolonTraits = new List<Trait>() { Trait.Elemental, Trait.Plant, Trait.Wood };
                }
            });

            subclasses.Add(new Feat(Enums.scDemonEidolon, DemonEidolonFlavour, DemonEidolonCrunch, new List<Trait>() { }, null)
            .WithOnSheet((Action<CalculatedCharacterSheetValues>)(values => {
                values.AddSelectionOptionRightNow((SelectionOption)new SingleFeatSelectionOption("SummonerDemonSin", "Embodied Sin", 1, ft => ft.HasTrait(Enums.tSinType)));
                values.AddSelectionOptionRightNow((SelectionOption)new SingleFeatSelectionOption("DemonEidolonArray", "Eidolon Ability Scores", 1, ft => new FeatName[] { Enums.scDemonEidolonTempter, Enums.scDemonEidolonWrecker }.Contains(ft.FeatName)));
            })));

            yield return CreateEidolonFeat(scDemonEidolonTempter, "Your eidolon is a manipulative demon, that craves the corruption of mortals.", null, null, new int[6] { 1, 4, 1, 0, 0, 3 }, 1, 4).WithTag(Ability.Dexterity);
            yield return CreateEidolonFeat(scDemonEidolonWrecker, "Your eidolon is a demon born of violence and loathing, that revels in war and destruction.", null, null, new int[6] { 4, 2, 3, 0, -1, 1 }, 2, 3).WithTag(Ability.Strength);

            // Demon Sin Subfeats
            yield return new EidolonBond(ModManager.RegisterFeatName("DemonSinClassic", "Classic Demon"), "Your bound demon embodies sin itself.",
                "{b}Initial Eidolon Ability (Demonic Strikes).{/b} Your eidolon's unarmed strikes deal +1 extra evil damage. Additionally, choose one of your eidolon's unarmed attacks; it gains versatile B, versatile P and versatile S." +
                "\n\n{i}At level 7{/i}\n{b}Symbiosis Eidolon Ability (Visions of Sin) {icon:TwoActions}.{/b}\n{b}Frequency{/b} Once per encounter.\n{b}Range{/b} 30 feet\n{b}Saving Throw{/b} Will" +
                "\n\nYour eidolon summons images of its sin into the mind of a target creature, tormenting and confusing them. If the target is evil, they suffer a -2 circumstance penalty to their Will save."
                + S.FourDegreesOfSuccess(null, "The target can't use reactions.", "The target is slowed 1 and can't use reactions.", "As failure, and the target is also confused for 1 round. The confusion can't be extended, but the other effects can."),
                Trait.Divine, new List<FeatName>() { FeatName.Religion, FeatName.Intimidation }, new Func<Feat, bool>(ft => ft.FeatName == ftAChaoticEvil),
                [Trait.Demon, Trait.Fiend, Enums.tSinType], [Trait.Demon, Trait.Fiend, Trait.Starborn], [
                    new Feat(ftPDemonicStrikes, "", "Your eidolon's primary natural weapon attack gains versatile B, versatile P and versatile S.", [], null),
                    new Feat(ftSDemonicStrikes, "", "Your eidolon's secondary natural weapon attack gains versatile B, versatile P and versatile S.", [], null)
                ]).WithClassFeatures((eidolon, summoner) => ClassicDemonEidolonLogic(eidolon, summoner));

            yield return new EidolonBond(ModManager.RegisterFeatName("DemonSinLust", "Lust Demon"), "Your bound eidolon is a demon of destructive lust, sent from the stars to corrupt the souls of mortals.",
                "{b}Demonic Weakness (Rejection Vulnerability).{/b} When your eidolon fails a Diplomacy check to Embrace, or when a creature succeeds at its save against your eidolon's Bewitch or Passionate Kiss, " +
                "it takes 1d6 mental damage for every 3 levels it has (minimum 1d6) and when that creature next successfully Demoralizes your eidolon this encounter, it takes that damage again." +
                "\nIn addition, they gain weakness to Cold Iron and Good damage equal to half their level." +
                "\n\n{b}Initial Eidolon Ability (Power of Lust).{/b} Your eidolon gains the following abilities, as a demon embodieying the sin of lust:" +
                "\n• {b}Embrace {icon:Action}.{/b} Your eidolon can use the embrace action to attempt to grapple an enemy creature, using their diplomacy in place of athletics." +
                "\n• {b}Seductive Presence.{/b} Creatures susceptible to earthly desires within 10 feet of you take a -1 circumstance penalty to saving throws and DCs against effects with the Mental trait." +
                "\n• {b}Passionate Kiss {icon:Action}.{/b} Your eidolon can passionately kiss a grappled enemy, dealing 1d6+level negative damage opposed by a basic Will save. If the target fails the save, you gain temporary hit points equal to your level and they can't attempt to Escape or take actions against your eidolon until your next turn." +
                "\n\n{i}At level 7{/i}\n{b}Symbiosis Eidolon Ability (Bewitch) {icon:ThreeActions}.{/b}\n{b}Frequency{/b} Once per encounter.\n{b}Range{/b} 30 feet\n{b}Saving Throw{/b} Will\n\n{i}You take command of the target, forcing it to obey your orders.{/i}" +
                    S.FourDegreesOfSuccess("The target is unaffected.", "The target is stunned 1.", "You gain control of the target until the end of their next turn. They are Slowed 2 while controlled in this way.", "As failure, but they are not slowed."),
                Trait.Divine, new List<FeatName>() { FeatName.Religion, FeatName.Diplomacy }, new Func<Feat, bool>(ft => ft.FeatName == ftAChaoticEvil),
                [Trait.Demon, Trait.Fiend, Enums.tSinType, Trait.Homebrew], [Trait.Demon, Trait.Fiend, Trait.Starborn], null).WithClassFeatures((eidolon, summoner) => LustDemonEidolonLogic(eidolon, summoner));

            yield return new EidolonBond(ModManager.RegisterFeatName("DemonSinWrath", "Wrath Demon"), "Your bound eidolon is a demon of wrath, born from the hatred of mortals souls. " +
                "They often infect their victims with burrowing spores, and herald oncoming ivasions with their primal screeching",
                "{b}Demonic Weakness (Peace Vulnerability).{/b} If the your eidolon fails a saving throw against an emotion effect, it takes takes 1d6 mental damage for every 3 levels it has (minimum 1d6)." +
                "\nIn addition, they gain weakness to Cold Iron and Good damage equal to half their level." +
                "\n\n{b}Initial Eidolon Ability (Spore Cloud) {icon:Action}.{/b}" +
                "Your eidolon deals poison damage equal to its level to all adjacent creatures {i}(no save){/i}. Each adjacent creature also makes a Fortitude save." +
                " On a failure, it takes persistent piercing damage equal to your eidolon's level. This persistent damage is removed if the creature is affected by a good" +
                " effect. After your eidolon uses Spore Cloud, the ability can't be used again for 1d6 rounds." +
                "\n\n{i}At level 7{/i}\n{b}Symbiosis Eidolon Ability (Stunning Screech) {icon:TwoActions} [Incapacitation].{/b}\nYour eidolon emits a shrill screech. Each enemy creature within a 15 foot burst must attempt a Fortitude save. On a failure, the creature is stunned 1, " +
                "and on a critical failure, it's stunned 2. This ability can only be used once per encounter.",
                Trait.Divine, new List<FeatName>() { FeatName.Religion, FeatName.Diplomacy }, new Func<Feat, bool>(ft => ft.FeatName == ftAChaoticEvil),
                [Trait.Demon, Trait.Fiend, Enums.tSinType, Trait.Homebrew], [Trait.Demon, Trait.Fiend, Trait.Starborn], null).WithClassFeatures((eidolon, summoner) => WrathDemonEidolonLogic(eidolon, summoner));

            // Dragon Type Subfeats

            // Primal
            yield return new EidolonBond(ModManager.RegisterFeatName("BrineDragon", "Brine Dragon"), "Your eidolon is an orderly brine dragon from the elemental plane of water.", "Your eidolon's breath weapon deals acid damage vs. Reflex.\n\nBrine dragons are associated with the {b}Primal{/b} spellcasting tradition.",
                Trait.Primal, new List<FeatName>() { FeatName.Nature, FeatName.Intimidation }, new Func<Feat, bool>(ft => ft.HasTrait(Enums.tAlignment) && ft.HasTrait(Trait.Lawful)),
                new List<Trait>() { Trait.Acid, Enums.tDragonType }, new List<Trait> { Trait.Dragon }, new List<Feat>() { dragonConeBreath, dragonLineBreath }).WithClassFeatures((eidolon, summoner) => DragonEidolonLogic(eidolon, summoner));
            yield return new EidolonBond(ModManager.RegisterFeatName("Cloud Dragon", "Cloud Dragon"), "Your eidolon is an adventurous cloud dragon from the elemental plane of air.", "Your eidolon's breath weapon deals electricity damage vs. Reflex.\n\nCloud dragons are associated with the {b}Primal{/b} spellcasting tradition.",
                Trait.Primal, new List<FeatName>() { FeatName.Nature, FeatName.Intimidation }, new Func<Feat, bool>(ft => ft.HasTrait(Enums.tAlignment)),
                new List<Trait>() { Trait.Electricity, Enums.tDragonType }, new List<Trait> { Trait.Dragon }, new List<Feat>() { dragonConeBreath, dragonLineBreath }).WithClassFeatures((eidolon, summoner) => DragonEidolonLogic(eidolon, summoner));
            yield return new EidolonBond(ModManager.RegisterFeatName("CrystalDragon", "Crystal Dragon"), "Your eidolon is a beautiful crystal dragon from the elemental plane of earth.", "Your eidolon's breath weapon deals piercing damage vs. Reflex.\n\nCrystal dragons are associated with the {b}Primal{/b} spellcasting tradition.",
                Trait.Primal, new List<FeatName>() { FeatName.Nature, FeatName.Intimidation }, new Func<Feat, bool>(ft => ft.HasTrait(Enums.tAlignment)),
                new List<Trait>() { Trait.VersatileP, Enums.tDragonType }, new List<Trait> { Trait.Dragon }, new List<Feat>() { dragonConeBreath, dragonLineBreath }).WithClassFeatures((eidolon, summoner) => DragonEidolonLogic(eidolon, summoner));
            yield return new EidolonBond(ModManager.RegisterFeatName("MagmaDragon", "Magma Dragon"), "Your eidolon is a volatile magma dragon from the elemental plane of fire.", "Your eidolon's breath weapon deals fire damage vs. Reflex.\n\nMagma dragons are associated with the {b}Primal{/b} spellcasting tradition.",
                Trait.Primal, new List<FeatName>() { FeatName.Arcana, FeatName.Intimidation }, new Func<Feat, bool>(ft => ft.HasTrait(Enums.tAlignment)),
                new List<Trait>() { Trait.Fire, Enums.tDragonType }, new List<Trait> { Trait.Dragon }, new List<Feat>() { dragonConeBreath, dragonLineBreath }).WithClassFeatures((eidolon, summoner) => DragonEidolonLogic(eidolon, summoner));
            yield return new EidolonBond(ModManager.RegisterFeatName("UmbralDragon", "Umbral Dragon"), "Your eidolon is a shadowy umbral dragon from the Shadowfell.",
                "Your eidolon's breath weapon deals negative damage vs. Reflex.\n\nUmbral dragons are associated with the {b}Occult{/b} spellcasting tradition.\n\n{b}Special{/b} Your dragon's ghostkilling breath deals force damage to undead.",
                Trait.Occult, new List<FeatName>() { FeatName.Occultism, FeatName.Intimidation }, new Func<Feat, bool>(ft => ft.HasTrait(Enums.tAlignment) && ft.HasTrait(Trait.Evil)),
                new List<Trait>() { Trait.Negative, Enums.tDragonType }, new List<Trait> { Trait.Dragon }, new List<Feat>() { dragonConeBreath, dragonLineBreath }).WithClassFeatures((eidolon, summoner) => DragonEidolonLogic(eidolon, summoner));

            // Chromatic
            yield return new EidolonBond(ModManager.RegisterFeatName("BlackDragon", "Black Dragon"), "Your eidolon is a vile black dragon.", "Your eidolon's breath weapon deals acid damage vs. Reflex.\n\nBlack dragons are associated with the {b}Arcane{/b} spellcasting tradition.",
                Trait.Arcane, new List<FeatName>() { FeatName.Arcana, FeatName.Intimidation }, new Func<Feat, bool>(ft => ft.HasTrait(Enums.tAlignment) && ft.HasTrait(Trait.Evil)),
                new List<Trait>() { Trait.Acid, Enums.tDragonType }, new List<Trait> { Trait.Dragon }, new List<Feat>() { dragonConeBreath, dragonLineBreath }).WithClassFeatures((eidolon, summoner) => DragonEidolonLogic(eidolon, summoner));
            yield return new EidolonBond(ModManager.RegisterFeatName("BlueDragon", "Blue Dragon"), "Your eidolon is a sophisticated blue dragon.", "Your eidolon's breath weapon deals electricity damage vs. Reflex.\n\nBlue dragons are associated with the {b}Arcane{/b} spellcasting tradition.",
                Trait.Arcane, new List<FeatName>() { FeatName.Arcana, FeatName.Intimidation }, new Func<Feat, bool>(ft => ft.HasTrait(Enums.tAlignment) && ft.HasTrait(Trait.Evil)),
                new List<Trait>() { Trait.Electricity, Enums.tDragonType }, new List<Trait> { Trait.Dragon }, new List<Feat>() { dragonConeBreath, dragonLineBreath }).WithClassFeatures((eidolon, summoner) => DragonEidolonLogic(eidolon, summoner));
            yield return new EidolonBond(ModManager.RegisterFeatName("GreenDragon", "Green Dragon"), "Your eidolon is a cunning green dragon.", "Your eidolon's breath weapon deals poison damage vs. Fortitude.\n\nGreen dragons are associated with the {b}Arcane{/b} spellcasting tradition.",
                Trait.Arcane, new List<FeatName>() { FeatName.Arcana, FeatName.Intimidation }, new Func<Feat, bool>(ft => ft.HasTrait(Enums.tAlignment) && ft.HasTrait(Trait.Evil)),
                new List<Trait>() { Trait.Poison, Enums.tDragonType }, new List<Trait> { Trait.Dragon }, new List<Feat>() { dragonConeBreath, dragonLineBreath }).WithClassFeatures((eidolon, summoner) => DragonEidolonLogic(eidolon, summoner));
            yield return new EidolonBond(ModManager.RegisterFeatName("RedDragon", "Red Dragon"), "Your eidolon is a tyranical red dragon.", "Your eidolon's breath weapon deals fire damage vs. Reflex.\n\nRed dragons are associated with the {b}Arcane{/b} spellcasting tradition.",
                Trait.Arcane, new List<FeatName>() { FeatName.Arcana, FeatName.Intimidation }, new Func<Feat, bool>(ft => ft.HasTrait(Enums.tAlignment) && ft.HasTrait(Trait.Evil)),
                new List<Trait>() { Trait.Fire, Enums.tDragonType }, new List<Trait> { Trait.Dragon }, new List<Feat>() { dragonConeBreath, dragonLineBreath }).WithClassFeatures((eidolon, summoner) => DragonEidolonLogic(eidolon, summoner));
            yield return new EidolonBond(ModManager.RegisterFeatName("WhiteDragon", "White Dragon"), "Your eidolon is a feral white dragon.", "Your eidolon's breath weapon deals cold damage vs. Reflex.\n\nWhite dragons are associated with the {b}Arcane{/b} spellcasting tradition.",
                Trait.Arcane, new List<FeatName>() { FeatName.Arcana, FeatName.Intimidation }, new Func<Feat, bool>(ft => ft.HasTrait(Enums.tAlignment) && ft.HasTrait(Trait.Evil)),
                new List<Trait>() { Trait.Cold, Enums.tDragonType }, new List<Trait> { Trait.Dragon }, new List<Feat>() { dragonConeBreath, dragonLineBreath }).WithClassFeatures((eidolon, summoner) => DragonEidolonLogic(eidolon, summoner));

            // Metallic
            yield return new EidolonBond(ModManager.RegisterFeatName("CopperDragon", "Copper Dragon"), "Your eidolon is a wily copper dragon.", "Your eidolon's breath weapon deals acid damage vs. Reflex.\n\nCopper dragons are associated with the {b}Arcane{/b} spellcasting tradition.",
                Trait.Arcane, new List<FeatName>() { FeatName.Arcana, FeatName.Intimidation }, new Func<Feat, bool>(ft => ft.FeatName == Enums.ftAChaoticGood),
                new List<Trait>() { Trait.Acid, Enums.tDragonType }, new List<Trait> { Trait.Dragon }, new List<Feat>() { dragonConeBreath, dragonLineBreath }).WithClassFeatures((eidolon, summoner) => DragonEidolonLogic(eidolon, summoner));
            yield return new EidolonBond(ModManager.RegisterFeatName("BronzeDragon", "Bronze Dragon"), "Your eidolon is a scholarly bronze dragon.", "Your eidolon's breath weapon deals electricity damage vs. Reflex.\n\nBronze dragons are associated with the {b}Arcane{/b} spellcasting tradition.",
                Trait.Arcane, new List<FeatName>() { FeatName.Arcana, FeatName.Intimidation }, new Func<Feat, bool>(ft => ft.HasTrait(Enums.tAlignment) && ft.HasTrait(Trait.Good)),
                new List<Trait>() { Trait.Electricity, Enums.tDragonType }, new List<Trait> { Trait.Dragon }, new List<Feat>() { dragonConeBreath, dragonLineBreath }).WithClassFeatures((eidolon, summoner) => DragonEidolonLogic(eidolon, summoner));
            yield return new EidolonBond(ModManager.RegisterFeatName("BrassDragon", "Brass Dragon"), "Your eidolon is a whimsical brass dragon.", "Your eidolon's breath weapon deals fire damage vs. Reflex.\n\nBrass dragons are associated with the {b}Arcane{/b} spellcasting tradition.",
                Trait.Arcane, new List<FeatName>() { FeatName.Arcana, FeatName.Intimidation }, new Func<Feat, bool>(ft => ft.HasTrait(Enums.tAlignment) && ft.HasTrait(Trait.Good)),
                new List<Trait>() { Trait.Fire, Enums.tDragonType }, new List<Trait> { Trait.Dragon }, new List<Feat>() { dragonConeBreath, dragonLineBreath }).WithClassFeatures((eidolon, summoner) => DragonEidolonLogic(eidolon, summoner));
            yield return new EidolonBond(ModManager.RegisterFeatName("GoldDragon", "Gold Dragon"), "Your eidolon is an honourable gold dragon.", "Your eidolon's breath weapon deals fire damage vs. Reflex.\n\nGold dragons are associated with the {b}Divine{/b} spellcasting tradition.",
                Trait.Divine, new List<FeatName>() { FeatName.Arcana, FeatName.Intimidation }, new Func<Feat, bool>(ft => ft.FeatName == Enums.ftALawfulGood),
                new List<Trait>() { Trait.Fire, Enums.tDragonType }, new List<Trait> { Trait.Dragon }, new List<Feat>() { dragonConeBreath, dragonLineBreath }).WithClassFeatures((eidolon, summoner) => DragonEidolonLogic(eidolon, summoner));
            yield return new EidolonBond(ModManager.RegisterFeatName("SilverDragon", "Silver Dragon"), "Your eidolon is a silver white dragon.", "Your eidolon's breath weapon deals cold damage vs. Reflex.\n\nSilver dragons are associated with the {b}Arcane{/b} spellcasting tradition.",
                Trait.Arcane, new List<FeatName>() { FeatName.Religion, FeatName.Intimidation }, new Func<Feat, bool>(ft => ft.HasTrait(Enums.tAlignment) && ft.HasTrait(Trait.Good)),
                new List<Trait>() { Trait.Cold, Enums.tDragonType }, new List<Trait> { Trait.Dragon }, new List<Feat>() { dragonConeBreath, dragonLineBreath }).WithClassFeatures((eidolon, summoner) => DragonEidolonLogic(eidolon, summoner));
        }

        private static void AngelEidolonLogic(Creature eidolon) {
            eidolon.AddQEffect(new QEffect("Hallowed Strikes", "Your eidolon's unarmed strikes deal +1 extra good damage.") {
                AddExtraKindedDamageOnStrike = (action, target) => {
                    return new KindedDamage(DiceFormula.FromText("1", "Hallowed Strikes"), DamageKind.Good);
                },
            });
            if (eidolon.Level >= 7) {
                eidolon.UnarmedStrike.Traits.Add(tParry);
                eidolon.AddQEffect(new QEffect() {
                    ProvideMainAction = (Func<QEffect, Possibility?>)(qfEidolon => {
                        return (Possibility)(ActionPossibility)new CombatAction(eidolon, IllustrationName.GenericCombatManeuver, "Parry", new Trait[] { }, "Your eidolon gains a +1 bonus to AC until the start of your next turn, and can use the Angelic Aegis action.", Target.Self()
                            .WithAdditionalRestriction(self => self.HasEffect(qfParrying) == true ? "Already parrying" : null))
                        .WithActionCost(1)
                        .WithSoundEffect(SfxName.RaiseShield)
                        .WithEffectOnSelf(self => {
                            self.AddQEffect(new QEffect("Parrying", "You have a +1 circumstance bonus to AC.") {
                                Id = qfParrying,
                                Illustration = IllustrationName.GenericCombatManeuver,
                                Source = eidolon,
                                ExpiresAt = ExpirationCondition.ExpiresAtStartOfSourcesTurn,
                                BonusToDefenses = (qf, action, defence) => {
                                    if (defence != Defense.AC) {
                                        return (Bonus)null;
                                    }
                                    return new Bonus(1, BonusType.Circumstance, "Parrying");
                                },
                            });
                        });
                    })
                });
                eidolon.AddQEffect(new QEffect() {
                    ProvideMainAction = (Func<QEffect, Possibility?>)(qfEidolon => {
                        if (eidolon.HasEffect(qfParrying) == false) {
                            return null;
                        }
                        return (Possibility)(ActionPossibility)new CombatAction(eidolon, illAngelicAegis, "Angelic Aegis", new Trait[] { },
                            "{b}Frequency{/b} Once per round\n{b}Requirements{/b} Your eidolon is parrying.\n\n" +
                            "Adjacent ally gains a +2 circumstance bonus to their AC and, as a reaction {icon:Reaction}, your eidolon " +
                            "may intercept any attack that deals physical damage to them, reducing the damage taken by an amount equal to your level." +
                            "\n\nThese benefits only apply whilst the target ally is adjacent to your eidolon.", Target.AdjacentFriend()) {
                            ShortDescription = "Adjacent ally gains a +2 circumstance bonus to their AC and, as a reaction {icon:Reaction}, your eidolon " +
                            "may intercept any attack that deals physical damage to them, reducing the damage taken by an amount equal to your level."
                        }
                        .WithActionCost(0)
                        .WithProjectileCone(illAngelicAegis, 5, ProjectileKind.Ray)
                        .WithSoundEffect(SfxName.Abjuration)
                        .WithEffectOnSelf(self => {
                            self.AddQEffect(new QEffect() {
                                ExpiresAt = ExpirationCondition.ExpiresAtStartOfYourTurn,
                                PreventTakingAction = (action => action.Name == "Angelic Aegis" ? "Once per round limit." : null)
                            });
                        })
                        .WithEffectOnEachTarget(async (action, self, target, checkResult) => {
                            target.AddQEffect(new QEffect("Angelic Aegis", $"You have a +2 circumstance bonus to AC and can be protected by {eidolon}, so long as you're adjacent to them.") {
                                Innate = false,
                                Illustration = illAngelicAegis,
                                Source = eidolon,
                                ExpiresAt = ExpirationCondition.ExpiresAtStartOfSourcesTurn,
                                BonusToDefenses = (qf, action, defence) => {
                                    if (defence != Defense.AC || qf.Owner.DistanceTo(qf.Source) > 1) {
                                        return (Bonus)null;
                                    }
                                    return new Bonus(2, BonusType.Circumstance, "Angelic Aegis");
                                },
                                YouAreDealtDamage = async (qf, attacker, damage, defender) => {
                                    if (qf.Owner.DistanceTo(qf.Source) > 1) {
                                        return null;
                                    }
                                    if (new DamageKind[] { DamageKind.Bludgeoning, DamageKind.Slashing, DamageKind.Piercing }.Contains(damage.Kind) == false) {
                                        return null;
                                    }
                                    if (await eidolon.Battle.AskToUseReaction(eidolon, (damage.Power != null ? "{b}" + attacker.Name + "{/b} uses {b}" + damage.Power.Name + "{/b} on " + "{b}" + qf.Owner.Name + "{/b}" : "{b}" + qf.Owner.Name + "{/b} has been hit") + " for " + damage.Amount + $" damage, which provokes Angelic Aegis Interception.\nUse your reaction " + "{icon:Reaction}" + $" to reduce the damage by {qf.Source.Level}?")) {
                                        return (DamageModification)new ReduceDamageModification(qf.Source.Level, "Angelic Aegis Interception");
                                    }
                                    return null;
                                }
                            });
                        });
                    }),
                });
            }
        }

        private static void DragonEidolonLogic(Creature eidolon, Creature summoner) {
            SpellRepertoire repertoire = summoner.PersistentCharacterSheet.Calculated.SpellRepertoires[tSummoner];
            int saveDC = summoner.GetOrCreateSpellcastingSource(SpellcastingKind.Spontaneous, tSummoner, Ability.Charisma, repertoire.SpellList).GetSpellSaveDC();

            Trait damageTrait = summoner.PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault((Func<Feat, bool>)(ft => ft.HasTrait(tDragonType))).Traits[0];
            FeatName targetFeat = summoner.PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault((Func<Feat, bool>)(ft => ft.HasTrait(tBreathWeaponArea))).FeatName;
            Target target;
            if (targetFeat == ftBreathWeaponLine) {
                target = Target.Line(12);
            } else {
                target = Target.Cone(6);
            }

            DamageKind damageKind = TraitToDamage(damageTrait);
            string damageName = damageKind.HumanizeTitleCase2().ToLower();
            Defense save = Defense.Reflex;

            if (damageKind == DamageKind.Mental) {
                save = Defense.Will;
            } else if (damageKind == DamageKind.Poison) {
                save = Defense.Fortitude;
            }

            Trait[] traits = new Trait[] { };
            if (damageTrait != Trait.VersatileP) {
                traits = new Trait[] { damageTrait };
            }

            eidolon.AddQEffect(new QEffect() {
                ProvideMainAction = qfEidolon => {
                    Creature summoner = GetSummoner(qfEidolon.Owner);
                    int dice = 1 + (summoner.Level + 1) / 2;
                    CombatAction breathWeapon = new CombatAction(qfEidolon.Owner, IllustrationName.BreathWeapon, "Breath Weapon", traits, "{b}Range{/b} " +
                        (targetFeat == ftBreathWeaponCone ? "30-foot cone" : "60-foot line") + "\n{b}Saving Throw{/b} " +
                        (save == Defense.Reflex ? "Reflex" : save == Defense.Fortitude ? "Fortitude" : "Will") +
                        "\n\nYour eidolon exhales a breath of destructive energy. Deal " + dice + "d6 " + damageName +
                        " damage to each creature in the area.\n\n{b}Special.{/b}Your eidolon can't use breath weapon again for 1d4 rounds.\n\nAt 3rd level, and every 2 levels thereafter, damage increases by 1d6." +
                        (damageKind == DamageKind.Negative ? "\n\nTheir ghost killing breath weapon deals force damage to undead creatures." : ""), target) {
                        ShortDescription = "Your eidolon exhales a breath of destructive energy. Deal " + dice + "d6 " + damageName + " damage to each creature in the area."
                    }
                    .WithSavingThrow(new SavingThrow(save, (_ => saveDC)))
                    .WithEffectOnEachTarget(async (action, self, target, checkResult) => {
                        if (target.HasTrait(Trait.Undead) && damageKind == DamageKind.Negative) {
                            await CommonSpellEffects.DealBasicDamage(action, self, target, checkResult, $"{dice}d6", DamageKind.Force);
                        } else {
                            await CommonSpellEffects.DealBasicDamage(action, self, target, checkResult, $"{dice}d6", damageKind);
                        }
                    })
                    .WithEffectOnChosenTargets(async (self, defenders) => {
                        int num = R.Next(1, 5);

                        self.AddQEffect(new QEffect("Recharging Fire Breath", "This creature can't use Fire Breath until the value counts down to zero.", ExpirationCondition.CountsDownAtEndOfYourTurn, self, (Illustration)IllustrationName.Recharging) {
                            Id = QEffectId.Recharging,
                            CountsAsADebuff = true,
                            PreventTakingAction = ca => !(ca.Name == "Breath Weapon") ? null : "This ability is recharging.",
                            Value = num
                        });
                    })
                    .WithSoundEffect(SfxName.Fireball)
                    .WithActionCost(2);

                    if (targetFeat == ftBreathWeaponLine) {
                        breathWeapon.WithProjectileCone(IllustrationName.BreathWeapon, 30, ProjectileKind.Cone);
                    } else {
                        breathWeapon.WithProjectileCone(IllustrationName.BreathWeapon, 20, ProjectileKind.Ray);
                    }

                    Possibility output = (Possibility)(ActionPossibility)breathWeapon;
                    if (eidolon.Level >= 7) {
                        output.PossibilitySize = PossibilitySize.Half;
                    }

                    return output;
                },
            });

            if (eidolon.Level >= 7) {
                eidolon.AddQEffect(new QEffect() {
                    ProvideMainAction = (Func<QEffect, Possibility>)(qfEidolon => {
                        Possibility output = (Possibility)(ActionPossibility)new CombatAction(eidolon, illDraconicFrenzy, "Draconic Frenzy", new Trait[] { },
                            "Your eidolon makes one Strike with their primary unarmed attack and two Strikes with their secondary unarmed attack (in any order). " +
                            "If any of these attacks critically hits an enemy, your eidolon instantly recovers the use of their Breath Weapon.",
                            Target.Self().WithAdditionalRestriction((Func<Creature, string>)(self => {
                                if (!self.CanMakeBasicUnarmedAttack && self.QEffects.All<QEffect>(qf => qf.AdditionalUnarmedStrike == null))
                                    return "You must be able to attack to use Draconic Frenzy.";
                                foreach (Item obj in self.MeleeWeapons.Where(weapon => weapon.HasTrait(Trait.Unarmed))) {
                                    if (self.CreateStrike(obj).CanBeginToUse(self).CanBeUsed)
                                        return null;
                                }
                                return "There is no nearby enemy or you can't make attacks.";
                            })))
                        .WithActionCost(2)
                        .WithSoundEffect(SfxName.BeastRoar)
                        .WithEffectOnEachTarget(async (action, self, target, result) => {
                            self.AddQEffect(new QEffect() {
                                Value = 0,
                                Key = "Draconic Frenzy",
                                AfterYouDealDamage = async (self2, action2, target) => {
                                    if (action2 == null || !action2.HasTrait(Trait.Strike)) {
                                        return;
                                    }
                                    if (action2.CheckResult == CheckResult.CriticalSuccess) {
                                        if (self2.RemoveAllQEffects(qf => qf.Name == "Recharging Fire Breath") > 0) {
                                            self.Overhead("{b}{i}breath weapon recharged{/i}{/b}", Color.White, self?.ToString() + "'s breath weapon has recharged.");
                                        }
                                    }
                                }
                            });
                            for (int i = 0; i < 3; ++i) {
                                await self.Battle.GameLoop.StateCheck();
                                List<Option> options = new List<Option>();
                                CombatAction strike = self.CreateStrike((i == 0 ? self.UnarmedStrike : self.MeleeWeapons.ToArray()[1]));
                                strike.WithActionCost(0);
                                GameLoop.AddDirectUsageOnCreatureOptions(strike, options, true);
                                if (options.Count > 0) {
                                    Option chosenOption;
                                    if (options.Count >= 2) {
                                        options.Add((Option)new CancelOption(true));
                                        chosenOption = (await self.Battle.SendRequest(new AdvancedRequest(self, "Choose a creature to Strike.", options) {
                                            TopBarText = $"Choose a creature to Strike or right-click to cancel. ({i + 1}/3)",
                                            TopBarIcon = illDraconicFrenzy
                                        })).ChosenOption;
                                    } else
                                        chosenOption = options[0];

                                    if (chosenOption is CancelOption) {
                                        action.RevertRequested = true;
                                        return;
                                    }
                                    int num = await chosenOption.Action() ? 1 : 0;
                                }
                            }
                            self.RemoveAllQEffects(qf => qf.Key == "Draconic Frenzy");
                        });
                        output.PossibilitySize = PossibilitySize.Half;
                        return output;
                    }),
                });
            }
        }

        private static void BeastEidolonLogic(Creature eidolon, Creature summoner) {
            eidolon.AddQEffect(new QEffect() {
                ProvideMainAction = qfSelf => {
                    SubmenuPossibility output = new SubmenuPossibility(illBeastsCharge, "Beast's Charge");
                    output.Subsections.Add(new PossibilitySection("Charge Option"));

                    output.Subsections[0].AddPossibility((ActionPossibility)new CombatAction(qfSelf.Owner, illBeastsCharge, "Beast's Charge (Line)", new Trait[] { Trait.Move },
                        "Stride up to twice your speed in a direct line, then strike. If you moved at least 20-feet, the strike gains a +1 circumstance bonus." +
                        "\n\nThis movement will not path around hazards or attacks of opportunity.",
                        Target.Self()) {
                        ShortDescription = "Stride up to twice your speed, then strike. If you travelled at least 20-feet and only in a straight line, the strike gains a +1 circumstance bonus."
                    }
                    .WithActionCost(2)
                    .WithSoundEffect(SfxName.Footsteps)
                    .WithEffectOnSelf(async (action, self) => {
                        MovementStyle movementStyle = new MovementStyle() {
                            //MaximumSquares = self.Speed * 2 * 5,
                            MaximumSquares = self.Speed * 2,
                            ShortestPath = false,
                            PermitsStep = false,
                            IgnoresUnevenTerrain = false,
                        };

                        Tile startingTile = self.Occupies;
                        Tile? destTile = await GetChargeTiles(self, movementStyle, 4, "Choose where to Stride with Beast's Charge or right-click to cancel", illBeastsCharge);

                        if (destTile == null) {
                            action.RevertRequested = true;
                        } else {
                            movementStyle.Shifting = self.HasEffect(QEffectId.Mobility) && destTile.InIteration.RequiresProvokingAttackOfOpportunity;
                            await self.MoveTo(destTile, action, movementStyle);
                            QEffect? chargeBonus = null;
                            if (self.DistanceTo(startingTile) >= 4) {
                                self.AddQEffect(chargeBonus = new QEffect("Charge Bonus", "+1 circumstance bonus to your next strike action.") {
                                    BonusToAttackRolls = (qf, action, target) => {
                                        return new Bonus(1, BonusType.Circumstance, "Beast's Charge");
                                    },
                                    Illustration = illBeastsCharge,
                                });
                            }
                            await CommonCombatActions.StrikeAdjacentCreature(self);
                            if (chargeBonus != null) {
                                chargeBonus.ExpiresAt = ExpirationCondition.Immediately;
                            }
                        }
                    })
                    );

                    output.Subsections[0].AddPossibility((ActionPossibility)new CombatAction(qfSelf.Owner, illBeastsCharge, "Beast's Charge (Mobile)", new Trait[] { Trait.Move, Trait.Basic },
                    "Stride twice. If you end your movement within melee reach of at least one enemy, you can make a melee Strike against that enemy, but do not gain a charge bonus to the attack roll.", (Target)Target.Self())
                    .WithActionCost(2)
                    .WithSoundEffect(SfxName.Footsteps)
                    .WithEffectOnSelf(async (action, self) => {
                        if (!await self.StrideAsync("Choose where to Stride with Beast's Charge. (1/2)", allowCancel: true)) {
                            action.RevertRequested = true;
                        } else {
                            int num = await self.StrideAsync("Choose where to Stride with Beast's Charge. You should end your movement within melee reach of an enemy. (2/2)", allowPass: true) ? 1 : 0;
                            await CommonCombatActions.StrikeAdjacentCreature(self);
                        }
                    }));

                    if (eidolon.Level >= 7) {
                        output.PossibilitySize = PossibilitySize.Half;
                    }
                    return output;
                },
            });
            if (eidolon.Level >= 7) {
                eidolon.AddQEffect(new QEffect() {
                    ProvideMainAction = (Func<QEffect, Possibility?>)(qfEidolon => {
                        Possibility output = (Possibility)(ActionPossibility)new CombatAction(eidolon, illPrimalRoar, "Primal Roar", new Trait[] { },
                            "Your eidolon unleashes a primal roar or other such terrifying noise that fits your eidolon's form. Your eidolon attempts Intimidation " +
                            "checks with a +2 bonus to Demoralize each enemy that can hear the roar; these Demoralize attempts don't take any penalty for not sharing a language.",
                            Target.Emanation(30))
                        .WithActionCost(2)
                        .WithSoundEffect(SfxName.BeastRoar)
                        .WithActiveRollSpecification(new ActiveRollSpecification(Checks.SkillCheck(Skill.Intimidation), Checks.DefenseDC(Defense.Will)))
                        .WithNoSaveFor((action, target) => target.OwningFaction == action.Owner.OwningFaction || target.QEffects.FirstOrDefault(qf => qf.Name == "Immunity to " + ActionId.Demoralize.HumanizeTitleCase2() + " by " + action.Owner.Name) != null)
                        .WithEffectOnEachTarget(async (action, self, target, checkResult) => {
                            if (target.OwningFaction == action.Owner.OwningFaction) {
                                target.Overhead("{b}{i}unaffected{/i}{/b}", Color.White, target?.ToString() + " is an ally.");
                                return;
                            }

                            if (target.QEffects.FirstOrDefault(qf => qf.Name == "Immunity to " + ActionId.Demoralize.HumanizeTitleCase2() + " by " + self.Name) != null) {
                                target.Overhead("{b}{i}unaffected{/i}{/b}", Color.White, target?.ToString() + " has already been demoralized this combat.");
                                return;
                            }

                            if (checkResult == CheckResult.CriticalSuccess) {
                                target.AddQEffect(QEffect.Frightened(2));
                            } else if (checkResult == CheckResult.Success) {
                                target.AddQEffect(QEffect.Frightened(1));
                            }
                            target.AddQEffect(QEffect.ImmunityToTargeting(ActionId.Demoralize, self));
                        });
                        output.PossibilitySize = PossibilitySize.Half;
                        return output;
                    }),
                    BonusToSkillChecks = (skill, action, target) => {
                        return new Bonus(2, BonusType.Untyped, "Primal Roar");
                    }
                });
            }
        }

        private static void DevoPhantomEidolonLogic(Creature eidolon, Creature summoner) {
            eidolon.AddQEffect(new QEffect("Dutiful Retaliation {icon:Reaction}", "Your eidolon makes a strike again an enemy that damaged you. Both your eidolon and your attacker must be within 15ft of you."));
            summoner.AddQEffect(new QEffect() {
                AfterYouTakeDamage = (async (qf, amount, damagekind, action, critical) => {
                    if (!action.HasTrait(Trait.Strike) || action.Owner == null) {
                        return;
                    }

                    Creature eidolon = GetEidolon(qf.Owner);

                    if (eidolon == null || eidolon.Destroyed || !eidolon.Actions.CanTakeActions()) {
                        return;
                    }

                    if (qf.Owner.DistanceTo(action.Owner) <= 3 && qf.Owner.DistanceTo(eidolon) <= 3) {
                        CombatAction combatAction = eidolon.CreateStrike(eidolon.UnarmedStrike, 0).WithActionCost(0);

                        // Check if eidolon cannot make strikes
                        foreach (QEffect restriction in eidolon.QEffects) {
                            if (restriction.PreventTakingAction != null && restriction.PreventTakingAction(combatAction) != null) {
                                return;
                            }
                        }

                        // Check if triggering creature cannot be targetted
                        foreach (QEffect condition in action.Owner.QEffects) {
                            if (condition.PreventTargetingBy != null && condition.PreventTargetingBy(combatAction) != null) {
                                return;
                            }
                        }

                        if (await eidolon.Battle.AskToUseReaction(eidolon, "{b}" + action.Owner.Name + "{/b} uses {b}" + action.Name + "{/b} which provokes Dutiful Retaliation.\nUse your reaction {icon:Reaction} to make an attack of opportunity?")) {
                            int map = eidolon.Actions.AttackedThisManyTimesThisTurn;
                            eidolon.Overhead("*dutiful devotion*", Color.White);
                            await eidolon.MakeStrike(action.Owner, eidolon.UnarmedStrike, 0);
                            eidolon.Actions.AttackedThisManyTimesThisTurn = map;
                        }
                    }
                })
            });
            if (eidolon.Level >= 7) {
                eidolon.AddQEffect(new QEffect() {
                    ProvideMainAction = (Func<QEffect, Possibility?>)(qfEidolon => {
                        return (Possibility)(ActionPossibility)new CombatAction(eidolon, illDevoStance, "Devotion Stance", new Trait[] { },
                            "Your eidolon takes on a patient defensive stance, steeling their focus with thoughts of their devotion." +
                            "\n\nUntil the start of their next turn, they gain a +2 circumstance bonus to AC, and a +4 bonus to damage to attacks made outside their turn.",
                            Target.Self()) {
                            ShortDescription = "Your eidolon assumes a stance that grants a +2 circumstance bonus to AC, and a +4 bonus to damage to attacks made outside their turn."
                        }
                        .WithActionCost(1)
                        .WithSoundEffect(SfxName.RaiseShield)
                        .WithEffectOnSelf(self => {
                            self.AddQEffect(new QEffect() {
                                ExpiresAt = ExpirationCondition.ExpiresAtStartOfYourTurn,
                                PreventTakingAction = (action => action.Name == "Devotion Stance" ? "Once per round limit." : null)
                            });
                            self.AddQEffect(new QEffect("Devotion Stance", "You have a +2 circumstance bonus to AC, and deal +4 damage with attacks made outside your turn.") {
                                Illustration = illDevoStance,
                                Source = eidolon,
                                ExpiresAt = ExpirationCondition.ExpiresAtStartOfSourcesTurn,
                                BonusToDefenses = (qf, action, defence) => {
                                    if (defence != Defense.AC) {
                                        return (Bonus)null;
                                    }
                                    return new Bonus(2, BonusType.Circumstance, "Devotion Stance");
                                },
                                BonusToDamage = (qf, action, defender) => {
                                    if (qf.Owner.Battle.ActiveCreature == qf.Owner || qf.Owner.Battle.ActiveCreature == GetSummoner(qf.Owner)) {
                                        return null;
                                    }
                                    return new Bonus(4, BonusType.Untyped, "Devotion Stance");
                                }
                            });
                        });
                    })
                });
            }
        }

        private static void AngerPhantomEidolonLogic(Creature eidolon, Creature summoner) {
            eidolon.AddQEffect(new QEffect("Frenzied Assault", "Your eidolon can attack with both of its natural weapon attacks at the same time.") {
                ProvideMainAction = effect => {
                    Possibility output = new ActionPossibility(new CombatAction(effect.Owner, illFrenziedAssault, "Frenzied Assault", new Trait[4] {
                                tSummoner, Trait.Basic, Trait.AlwaysHits, Trait.IsHostile },
                      "Make two Strikes against the same target, one with each of your melee natural weapon attacks, each using your current multiple attack penalty." +
                      "\n\nCombine the damage for the purposes of weakness and resistance. This counts as two attacks when calculating your multiple attack penalty.", (Target)Target.Reach(effect.Owner.UnarmedStrike))
                        .WithActionCost(2)
                        .WithEffectOnChosenTargets((Func<Creature, ChosenTargets, Task>)(async (self, targets) => {
                            int map = self.Actions.AttackedThisManyTimesThisTurn;

                            Creature enemy = targets.ChosenCreature;

                            await self.MakeStrike(enemy, self.UnarmedStrike, map);
                            await self.MakeStrike(enemy, self.MeleeWeapons.ToArray()[1], map);

                            GetSummoner(eidolon).Actions.AttackedThisManyTimesThisTurn = self.Actions.AttackedThisManyTimesThisTurn;
                        }))
                    .WithTargetingTooltip((Func<CombatAction, Creature, int, string>)((power, target, index) => power.Description)));
                    if (eidolon.Level >= 7) {
                        output.PossibilitySize = PossibilitySize.Half;
                    }
                    return output;
                }
            });
            if (eidolon.Level >= 7) {
                eidolon.AddQEffect(new QEffect() {
                    ProvideMainAction = effect => {
                        if (effect.Owner.HasEffect(qfSeethingFrenzy)) {
                            return null;
                        }
                        Possibility output = new ActionPossibility(new CombatAction(effect.Owner, illSeethingFrenzy, "Seething Frenzy", new Trait[] {
                                tSummoner, Trait.Emotion, Trait.Mental, Trait.Concentrate },
                            "Your eidolon enters a seething frenzy, disregarding its own safety to tear your foes apart. It gains temporary HP equal to its level, and a +4 damage bonus to its unarmed strike attacks, " +
                            "but it takes a -2 penalty to AC. The rage lasts until the end of the encounter, and leaves your eidolon fatigued if they leave early.", (Target)Target.Self())
                            .WithActionCost(1)
                            .WithSoundEffect(SfxName.BeastRoar)
                            .WithEffectOnSelf((async (self) => {
                                self.GainTemporaryHP(self.Level);
                                self.AddQEffect(new QEffect("Seething Frenzy", "You take a -2 penalty to AC, but your strikes deal +4 bonus damage.") {
                                    Illustration = illSeethingFrenzy,
                                    Id = qfSeethingFrenzy,
                                    BonusToDefenses = (effect, action, defence) => {
                                        if (defence == Defense.AC) {
                                            return new Bonus(-2, BonusType.Untyped, "Seething Frenzy");
                                        }
                                        return null;
                                    },
                                    BonusToDamage = (effect, action, target) => {
                                        if (action.HasTrait(Trait.Strike) && action.HasTrait(Trait.Unarmed)) {
                                            return new Bonus(4, BonusType.Untyped, "Seething Frenzy");
                                        }
                                        return null;
                                    },
                                    WhenExpires = effect => {
                                        effect.Owner.AddQEffect(QEffect.Fatigued());
                                    }
                                });
                            }))
                        );
                        output.PossibilitySize = PossibilitySize.Half;
                        return output;
                    }
                });
            }
        }

        private static void AzataEidolonLogic(Creature eidolon, Creature summoner) {
            eidolon.AddQEffect(new QEffect() {
                ProvideMainAction = qfCelestialPassion => {
                    return (Possibility)(ActionPossibility)new CombatAction(qfCelestialPassion.Owner, IllustrationName.AngelicHalo, "Celestial Passion",
                        new Trait[] { Trait.Divine, Trait.Concentrate, Trait.Emotion, Trait.Mental, Trait.Auditory },
                        "{b}Range{/b} 30 feet\n\nYour eidolon inspires an ally with a display of heavenly artistry, or heart warming encouragment.\n\n" +
                        "Target ally gains temporary HP equal to your level, and a +1 status bonus to attack rolls and skill checks until the start of your next turn.\n\n" +
                        "The target is then immune to this ability for the rest of the encounter.", Target.RangedFriend(6)) {
                        ShortDescription = "[Range 30] Grant target ally temp HP equal to your level, and a +1 bonus to attack rolls and skill checks unil the end of your next turn. Can only be used on each ally once per encounter."
                    }
                    .WithProjectileCone(IllustrationName.AngelicHalo, 10, ProjectileKind.Cone)
                    .WithActionId(acCelestialPassion)
                    .WithActionCost(1)
                    .WithSoundEffect(SfxName.Bless)
                    .WithEffectOnEachTarget(async (action, caster, target, result) => {
                        target.GainTemporaryHP(caster.Level);
                        target.AddQEffect(new QEffect("Celestial Passion", "+1 status bonus to all attack rolls and skill checks.") {
                            Illustration = IllustrationName.AngelicHalo,
                            Source = caster,
                            ExpiresAt = ExpirationCondition.ExpiresAtStartOfSourcesTurn,
                            BonusToAttackRolls = (qf, action, defender) => new Bonus(1, BonusType.Status, "Celestial Passion"),
                            BonusToSkillChecks = (skill, action, target) => new Bonus(1, BonusType.Status, "Celestial Passion"),
                        });
                        QEffect.ImmunityToTargeting(acCelestialPassion, caster);
                    });
                }
            });
            if (eidolon.Level >= 7) {
                eidolon.AnimationData.AddAuraAnimation(new MagicCircleAuraAnimation(illAngelicAura, Color.LightPink, 3));
                //auraAnimation.Color = Color.LightSeaGreen;
                //auraAnimation.Color = Color.LawnGreen;
                //auraAnimation.Color = Color.HotPink;

                eidolon.AddQEffect(new QEffect("Whimsical Aura", "Your eidolon has a +5ft status bonus to its speed, and grants this benefit to all allies that start their turn within 15ft of it. At the end of your eidolon's turn, all allies within the aura reduce their frightened condition by 1.") {
                    BonusToAllSpeeds = qf => new Bonus(1, BonusType.Status, "Whimsical Aura"),
                    StateCheck = effect => {
                        //qf.Tag = qf.Owner.Battle.AllCreatures.Where(creature => creature.OwningFaction == eidolon.OwningFaction);
                        foreach (Creature creature in effect.Owner.Battle.AllCreatures) {
                            if (creature.HasEffect(qfWhimsicalAura) || creature.QEffects.FirstOrDefault(qf => qf.Name == "Whimsical Aura") != null) {
                                continue;
                            }
                            creature.AddQEffect(new QEffect() {
                                Id = qfWhimsicalAura,
                                Tag = false,
                                StartOfYourPrimaryTurn = async (effect, self) => {
                                    effect.Tag = false;
                                    List<Creature> auraHavers = self.Battle.AllCreatures.Where(c => c.OwningFaction == self.OwningFaction && c.QEffects.FirstOrDefault(qf => qf.Name == "Whimsical Aura") != null && c.DistanceTo(self) <= 3).ToList();
                                    if (auraHavers.Count > 0) {
                                        effect.Tag = true;
                                        self.Overhead("*+5ft status bonus to speed*", Color.White);
                                    }
                                },
                                BonusToAllSpeeds = qf => (bool)qf.Tag == true ? new Bonus(1, BonusType.Status, "Whimsical Aura") : null,
                                StateCheck = self => {
                                    if ((bool)self.Tag) {
                                        self.Name = "Blessed by Whimsical Aura";
                                        self.Description = "You gain a +5 status bonus to your speed.";
                                        self.Illustration = Enums.illWhimsicalAura;
                                    } else {
                                        self.Name = null;
                                        self.Description = null;
                                        self.Illustration = null;
                                    }
                                }
                            });
                        }
                    },
                    EndOfYourTurnBeneficialEffect = async (qf, self) => {
                        foreach (Creature ally in qf.Owner.Battle.AllCreatures.Where(creature => creature.OwningFaction == eidolon.OwningFaction)) {
                            if (ally.DistanceTo(self) > 3) {
                                continue;
                            }
                            QEffect? frightened = ally.QEffects.FirstOrDefault(effect => effect.Id == QEffectId.Frightened);
                            if (frightened != null) {
                                frightened.Value -= 1;
                                if (frightened.Value == 0) {
                                    frightened.ExpiresAt = ExpirationCondition.Immediately;
                                }
                                ally.Occupies.Overhead("*frightened reduced by 1*", Color.White);
                            }
                        }
                    }
                });
            }
        }

        private static void DevilEidolonLogic(Creature eidolon, Creature summoner) {
            eidolon.AddQEffect(QEffect.DamageResistance(DamageKind.Fire, Math.Max(1, (eidolon.Level / 2))));
            eidolon.AddQEffect(QEffect.DamageWeakness(DamageKind.Good, Math.Max(1, (eidolon.Level / 2))));
            eidolon.AddQEffect(new QEffect("Hellfire Scourge", "Your eidolon deals +1d4 fire damage to the first frightened creature it strikes each round.") {
                Tag = false,
                StartOfYourPrimaryTurn = async (effect, self) => {
                    effect.Tag = false;
                },
                AddExtraKindedDamageOnStrike = (action, target) => {
                    if (target.HasEffect(QEffectId.Frightened) && (bool)action.Owner.QEffects.FirstOrDefault(qf => qf.Name == "Hellfire Scourge").Tag == false) {
                        action.Owner.QEffects.FirstOrDefault(qf => qf.Name == "Hellfire Scourge").Tag = true;
                        return new KindedDamage(DiceFormula.FromText("1d4"), DamageKind.Fire);
                    }
                    return null;
                }
            });
            if (eidolon.Level >= 7) {
                eidolon.AddQEffect(new QEffect() {
                    ProvideMainAction = effect => {
                        return new ActionPossibility(new CombatAction(effect.Owner, illDisciplineTheLegion, "Discipline the Legion", new Trait[] {
                                tSummoner, Trait.Linguistic, Trait.Concentrate, Trait.Mental, Trait.Emotion },
                            "Your eidolon shouts a commands at one ally within 30-feet. The next time that ally attacks or makes a skill check before the start of your next turn, your eidolon can " +
                            "use their reaction {icon:Reaction} to make an Intimidation check against an easy DC for their level.\n\n{b}Critical success{/b} You grant your ally a +2 circumstance bonus to the triggering " +
                            "check. If you're a master with the check you attempted, the bonus is +3, and if you're legendary, it's +4.\n{b}Success{/b} You grant your ally a +1 circumstance bonus to " +
                            "the triggering check.\n{b}Critical failure{/b} Your ally takes a –1 circumstance penalty to the triggering check.\n\nif the action your eidolon was assisting them with was an " +
                            "attack, it deals extra fire damage equal to half your level.",
                            (Target)Target.RangedFriend(6).WithAdditionalConditionOnTargetCreature((source, target) => {
                                if (target.QEffects.FirstOrDefault(qf => qf.Key == "Infernal Command") == null) {
                                    return Usability.Usable;
                                }
                                return Usability.NotUsableOnThisCreature("Your eidolon has already disciplined this creature.");
                            })) {
                                ShortDescription = "Target creature can be assisted via the Aid reaction {icon:Reaction} during the next attack roll they make before the start of your next turn. In addition, the aided attack deals extra fire damage equal to half your level."
                        }
                            .WithActionCost(1)
                            .WithEffectOnChosenTargets((Func<Creature, ChosenTargets, Task>)(async (self, targets) => {
                                targets.ChosenCreature.AddQEffect(new QEffect("Infernal Command", $"This creature has the attention of {self.Name}, and is ready to be spurred into decisive action.") {
                                    Illustration = illDisciplineTheLegion,
                                    Source = self,
                                    Key = "Infernal Command",
                                    ExpiresAt = ExpirationCondition.ExpiresAtStartOfSourcesTurn,
                                    BeforeYourActiveRoll = async (effect, action, innerTarget) => {
                                        if (await eidolon.Battle.AskToUseReaction(eidolon, "{b}" + effect.Owner + "{/b} is about to use {b}" + action.Name + "{/b} against " + innerTarget?.ToString() + ". \nRoll for Discipline the Legion?")) {
                                            CheckResult result = CommonSpellEffects.RollCheck("Discipline the Legion", new ActiveRollSpecification(TaggedChecks.SkillCheck(new Skill[] { Skill.Intimidation, Skill.Deception }), Checks.FlatDC(Checks.LevelBasedDC(self.Level, SimpleDCAdjustment.Easy))), self, effect.Owner);
                                            int bonus = 0;

                                            if (result == CheckResult.CriticalSuccess) {
                                                bonus = Math.Max((int)self.Proficiencies.Get(Trait.Intimidation), (int)self.Proficiencies.Get(Trait.Deception));
                                                bonus = Math.Max(2, bonus / 2);
                                            } else if (result == CheckResult.Success) {
                                                bonus = 1;
                                            } else if (result == CheckResult.Failure) {
                                                bonus = 1;
                                            } else if (result == CheckResult.CriticalFailure) {
                                                bonus = -1;
                                            }

                                            effect.Owner.AddQEffect(new QEffect() {
                                                BonusToSkillChecks = (skill, action, target) => {
                                                    return new Bonus(bonus, BonusType.Circumstance, "Infernal Command");
                                                },
                                                BonusToAttackRolls = (effect, action, target) => {
                                                    return new Bonus(bonus, BonusType.Circumstance, "Infernal Command");
                                                },
                                                YouDealDamageEvent = async (self, dmgEvent) => {
                                                    if (dmgEvent.TargetCreature.OwningFaction != self.Owner.OwningFaction && dmgEvent.TargetCreature == innerTarget) {
                                                        int damage = self.Owner.Level / 2;
                                                        dmgEvent.KindedDamages.Add(new KindedDamage(DiceFormula.FromText(damage.ToString(), "Infernal Command"), DamageKind.Fire));
                                                    }
                                                },
                                                AfterYouTakeAction = async (effect, action) => {
                                                    effect.ExpiresAt = ExpirationCondition.Immediately;
                                                }
                                            });
                                        }
                                    },
                                });
                            }))
                        );
                    }
                });
            }
        }

        private static void PlantEidolonLogic(Creature eidolon, Creature summoner) {
            eidolon.AddQEffect(new QEffect("Tendril Strike {icon:Action}", "Your eidolon makes a melee unarmed Strike, increasing its reach by 5 feet for that Strike. If the unarmed attack has the disarm, shove, or trip trait, the eidolon can use the corresponding action instead of a Strike.") {
                ProvideStrikeModifierAsPossibility = item => {
                    if (item != eidolon.UnarmedStrike && item != eidolon.QEffects.FirstOrDefault(qf => qf.AdditionalUnarmedStrike != null && qf.AdditionalUnarmedStrike.WeaponProperties.Melee).AdditionalUnarmedStrike) {
                        return null;
                    }

                    int range = item.HasTrait(Trait.Reach) ? 3 : 2;

                    CombatAction action = eidolon.CreateStrike(item, -1, null);
                    action.Traits.Add(Trait.DoesNotProvoke);
                    action.Name = "Tendril Strike (" + item.Name + ")";

                    var split = action.Description.Split("\n");
                    action.Description = "";
                    split[0] = "{b}Reach{/b} " + range * 5 + " feet";
                    foreach (string line in split) {
                        action.Description += line + "\n";
                    }

                    action.Target = (Target)Target.Ranged(range);
                    action.Traits.Add(Trait.Basic);
                    action.WithProjectileCone(IllustrationName.None, 0, ProjectileKind.None);
                    action.Illustration = new SideBySideIllustration(illTendrilStrike, item.Illustration);
                    return (ActionPossibility)action;
                }
            });
            eidolon.AddQEffect(new QEffect() {
                ProvideStrikeModifierAsPossibility = item => {
                    if (item != eidolon.UnarmedStrike && item != eidolon.QEffects.FirstOrDefault(qf => qf.AdditionalUnarmedStrike != null && qf.AdditionalUnarmedStrike.WeaponProperties.Melee).AdditionalUnarmedStrike) {
                        return null;
                    }

                    if (!item.HasTrait(Trait.Disarm)) {
                        return null;
                    }

                    int range = item.HasTrait(Trait.Reach) ? 3 : 2;

                    CombatAction action = CombatManeuverPossibilities.CreateDisarmAction(eidolon, item);
                    action.Name = "Tendril Strike (disarm)";
                    action.Target = (Target)Target.Ranged(range).WithAdditionalConditionOnTargetCreature((a, d) => !d.HeldItems.Any(hi => !hi.HasTrait(Trait.Grapplee)) ? Usability.CommonReasons.TargetHasNoWeapon : Usability.Usable);
                    action.WithProjectileCone(IllustrationName.None, 0, ProjectileKind.None);
                    action.Illustration = new SideBySideIllustration(illTendrilStrike, IllustrationName.Disarm);
                    return (ActionPossibility)action;
                }
            });
            eidolon.AddQEffect(new QEffect() {
                ProvideStrikeModifierAsPossibility = item => {
                    if (item != eidolon.UnarmedStrike && item != eidolon.QEffects.FirstOrDefault(qf => qf.AdditionalUnarmedStrike != null && qf.AdditionalUnarmedStrike.WeaponProperties.Melee).AdditionalUnarmedStrike) {
                        return null;
                    }

                    if (!item.HasTrait(Trait.Shove)) {
                        return null;
                    }

                    int range = item.HasTrait(Trait.Reach) ? 3 : 2;

                    CombatAction action = CombatManeuverPossibilities.CreateShoveAction(eidolon, item);
                    action.Name = "Tendril Strike (shove)";
                    action.Target = (Target)Target.Ranged(range);
                    action.WithProjectileCone(IllustrationName.None, 0, ProjectileKind.None);
                    action.Illustration = new SideBySideIllustration(illTendrilStrike, IllustrationName.Shove);
                    return (ActionPossibility)action;
                }
            });
            eidolon.AddQEffect(new QEffect() {
                ProvideStrikeModifierAsPossibility = item => {
                    if (item != eidolon.UnarmedStrike && item != eidolon.QEffects.FirstOrDefault(qf => qf.AdditionalUnarmedStrike != null && qf.AdditionalUnarmedStrike.WeaponProperties.Melee).AdditionalUnarmedStrike) {
                        return null;
                    }

                    if (!item.HasTrait(Trait.Trip)) {
                        return null;
                    }

                    int range = item.HasTrait(Trait.Reach) ? 3 : 2;

                    CombatAction action = CombatManeuverPossibilities.CreateTripAction(eidolon, item);
                    action.Name = "Tendril Strike (trip)";
                    action.Target = (Target)Target.Ranged(range).WithAdditionalConditionOnTargetCreature((a, d) => d.HasEffect(QEffectId.Prone) ? Usability.CommonReasons.TargetIsAlreadyProne : Usability.Usable);
                    action.WithProjectileCone(IllustrationName.None, 0, ProjectileKind.None);
                    action.Illustration = new SideBySideIllustration(illTendrilStrike, IllustrationName.Trip);
                    return (ActionPossibility)action;
                }
            });
            if (eidolon.Level >= 7) {
                Item[] attacks = new Item[] { eidolon.UnarmedStrike, eidolon.QEffects.FirstOrDefault(qf => qf.AdditionalUnarmedStrike != null && qf.AdditionalUnarmedStrike.WeaponProperties.Melee).AdditionalUnarmedStrike };

                foreach (Item attack in attacks) {
                    if (!attack.HasTrait(Trait.Reach)) {
                        attack.Traits.Add(Trait.Reach);
                    }
                }
            }
        }

        private static void UndeadEidolonLogic(Creature eidolon, Creature summoner) {
            eidolon.AddQEffect(new QEffect("Negative Essence", "Your eidolon is undead, though unlike true undead, your connection grants it a sliver of life. It has negative healing, but instead of the usual immunities it gets a +2 circumstance bonus to death, disease and poison effects. Additionally, it gains a +5 bonus to staunch persistent bleed damage.") {
                StartOfCombat = async self => {
                    string[] qfNames = new string[] { QEffect.DamageImmunity(DamageKind.Bleed).Name, QEffect.DamageImmunity(DamageKind.Poison).Name, QEffect.ImmunityToCondition(QEffectId.Paralyzed).Name };

                    eidolon.RemoveAllQEffects(qf => qfNames.Contains(qf.Name) && qf.Name != null || qf.ImmuneToTrait == Trait.Death);
                },
                BonusToDefenses = (self, action, defence) => {
                    if (defence != Defense.AC && action != null && action.Traits.ContainsOneOf(new Trait[] { Trait.Death, Trait.Disease, Trait.Poison })) {
                        return new Bonus(2, BonusType.Circumstance, "Negative Essence");
                    }
                    return null;
                },
                ReducesPersistentDamageRecoveryCheckDc = (self, pd, kind) => kind == DamageKind.Bleed
            });
            if (eidolon.Level >= 7) {
                eidolon.AddQEffect(new QEffect() {
                    ProvideStrikeModifierAsPossibility = item => {
                        if (item != eidolon.UnarmedStrike && eidolon.QEffects.Where(qf => qf.AdditionalUnarmedStrike != null).ToList().FirstOrDefault(qf => qf.AdditionalUnarmedStrike == item) == null) {
                            return null;
                        }

                        CombatAction action = eidolon.CreateStrike(item, -1, null);

                        CombatAction drainLife = new CombatAction(eidolon, IllustrationName.VampiricTouch2, "Drain Life", new Trait[] { Trait.Divine, tEidolon, Trait.Necromancy, Trait.Negative }, "", Target.Ranged(1));

                        StrikeModifiers modifiers = new StrikeModifiers() {
                            OnEachTarget = async (a, d, strikeResult) => {
                                if (strikeResult < CheckResult.Success)
                                    return;

                                CheckResult saveResult = CommonSpellEffects.RollSavingThrow(d, action, Defense.Fortitude, summoner.Spellcasting.PrimarySpellcastingSource.GetSpellSaveDC());
                                if (strikeResult == CheckResult.CriticalSuccess && saveResult != CheckResult.CriticalFailure) {
                                    saveResult -= 1;
                                }

                                if (saveResult <= CheckResult.Success) {
                                    await CommonSpellEffects.DealDirectDamage(drainLife, DiceFormula.FromText($"{eidolon.Level / 2}"), d, saveResult, DamageKind.Negative);
                                }
                                if (saveResult == CheckResult.Failure) {
                                    eidolon.GainTemporaryHP(d.Level);
                                    d.AddQEffect(QEffect.Drained(1));
                                }
                                if (saveResult == CheckResult.CriticalFailure) {
                                    eidolon.GainTemporaryHP(d.Level * 2);
                                    d.AddQEffect(QEffect.Drained(2));
                                }
                            }
                        };

                        action.StrikeModifiers = modifiers;

                        action.Name = $"Drain Life ({item.Name})";
                        action.ActionCost = 2;
                        action.Traits.AddRange(new Trait[] { Trait.Divine, tEidolon, Trait.Necromancy, Trait.Negative });
                        action.Description = "Your eidolon attacks a living creature and drains some of the creature's life force to feed your shared link.\nYour eidolon Strikes a living enemy. " +
                        "If the Strike hits and deals damage, the target must attempt a Fortitude save, with the following effects. On a critical hit, the enemy uses the result one degree worse " +
                        "than it rolled.\n\n{b}Critcal Success.{/b} No effect.\r\n{b}Success.{/b} Your eidolon drains a small amount of life force. The enemy takes additional negative damage equal to " +
                        "half your level.\n{b}Failure.{/b} Your eidolon drains enough life force to satisfy itself. The enemy takes additional negative damage equal to half your level and is drained 1. " +
                        "Your eidolon gains temporary Hit Points equal to the enemy's level, which last for 1 minute.\n{b}Critical failure.{/b} Your eidolon drains an incredible amount of life force " +
                        "and is thoroughly glutted with energy. As failure, but the enemy is drained 2 and the temporary Hit Points are equal to double the enemy's level.";
                        action.ShortDescription += ". On hit; The target must make a fort save to prevent its life force being drained.";
                        action.Target = (action.Target as CreatureTarget).WithAdditionalConditionOnTargetCreature((a, d) => !d.IsLivingCreature ? Usability.CommonReasons.TargetIsNotAlive : Usability.Usable);
                        action.Illustration = new SideBySideIllustration(IllustrationName.VampiricTouch2, item.Illustration);
                        return (ActionPossibility)action;
                    }
                });
            }
        }

        private static void PsychopompEidolonLogic(Creature eidolon, Creature summoner) {
            eidolon.AddQEffect(new QEffect("Spirit Touch", "Your eidolon's unarmed strikes deals an extra 1 negative damage to living creatures and an extra 1 positive damage to undead, and possess the ghost touch property.") {
                AddExtraKindedDamageOnStrike = (action, target) => {
                    if (target.HasTrait(Trait.Undead)) {
                        return new KindedDamage(DiceFormula.FromText("1", "Spirit Touch"), DamageKind.Positive);
                    }
                    return new KindedDamage(DiceFormula.FromText("1", "Spirit Touch"), DamageKind.Negative);
                },
            });
            if (eidolon.Level >= 7) {
                eidolon.AddQEffect(new QEffect() {
                    ProvideMainAction = self => {
                        return (ActionPossibility)new CombatAction(eidolon, illSoulWrench, "Soul Wrench", new Trait[] { Trait.Necromancy, Trait.Magical, Trait.Incapacitation },
                            "{b}Range {/b}5 feet\n{b}Target {/b}1 undead, or living creature of non-planar origin.\n{b}Saving throw {/b} Will\n\nYour eidolon attempts to wrench the soul from the body of an adjacent creature, holding it in " +
                            "place and stopping it from returning to its origional owner.\n\n" +
                            "At stage 1, the target gains enfeebled and clumsy 1 as they experience their soul being partially wrenched from their body. At stage 2, they gain slowed 1 and enfeebled and clumsy 2. " +
                            "At stage 3, your eidolon removes their soul completely. Living creatures are paralyzed, their bodies left as soulness husks. Undead are immediately destroyed, unless they're of an equal or higher level than your" +
                            " eidolon, in which case they suffer force damage equal to twice your eidolon's level their soul escapes back intoto their body.\n\n" +
                            "{b}Failure{/b} The target gains soul wretch 1.\n" +
                            "{b}Critical failure{/b} The target gains soul wretch 2.\n\n" +
                            "Your eidolon must sustain this ability each turn to maintain their hold on the target's soul. Each time they do so, the target makes another will saving throw to determine how much progress your " +
                            "eidolon makes towards wretching out their soul completely.\n\n" +
                            "{b}Critical success{/b} The target's soul breaks free from your eidolon's grasp.\n" +
                            "{b}Failure{/b} The target's soul wretch increased by 1 stage.\n" +
                            "{b}Critical failure{/b} The target's soul wretch increased by 2 stages.",
                            Target.Ranged(1)
                            .WithAdditionalConditionOnTargetCreature((a, d) => new Trait[] {
                                Trait.Incorporeal, Trait.Celestial, Trait.Fiend, Trait.Monitor, Trait.Archon, Trait.Angel, Trait.Elemental, Trait.Construct }
                                .ContainsOneOf(d.Traits) ? Usability.NotUsableOnThisCreature("no external soul") : Usability.Usable)
                            .WithAdditionalConditionOnTargetCreature((a, d) => d.HasEffect(qfSoulSiphon) ? Usability.NotUsableOnThisCreature("soul already being siphoned") : Usability.Usable)) {
                            ShortDescription = "Your eidolon attempts to wrench the soul from the body of an adjacent creature, holding it in place and stopping it from returning to its origional owner."
                        }
                        .WithActionCost(2)
                        .WithSoundEffect(SfxName.Necromancy)
                        .WithSpellInformation(eidolon.Level / 2 + 1, "", null)
                        .WithProjectileCone(illSoulWrench, 5, ProjectileKind.Cone)
                        .WithSavingThrow(new SavingThrow(Defense.Will, summoner.Spellcasting.PrimarySpellcastingSource.GetSpellSaveDC()))
                        .WithEffectOnEachTarget(async (action, a, d, checkResult) => {
                            // Remove soul siphon from previous target
                            self.UsedThisTurn = true;
                            Creature? prevTarget = a.Battle.AllCreatures.FirstOrDefault(cr => cr.QEffects.FirstOrDefault(qf => qf.Id == qfSoulSiphon && qf.Source == eidolon) != null);
                            if (prevTarget != null) {
                                prevTarget.RemoveAllQEffects(qf => qf.Id == qfSoulSiphon && qf.Source == eidolon);
                            }

                            QEffect soulSiphon = new QEffect("Soul Wrench", "Your soul is being wrenched away from your body by a psychopomp.\n" +
                                "{b}Stage 1{/b} You enfeebled and clumsy 1\n{b}Stage 2{/b} You gain slowed 1 and enfeebled and clumsy 2.\n" +
                                "{b}Stage 3{/b} Your soul is removed completely. If you are a living creatures are are paralyzed. Undead are " +
                                "immediately destroyed, unless they're of an equal or higher level than the psychopomp, in which case they suffer force damage equal to twice the psychomp's level, and the condition ends.") {
                                Id = qfSoulSiphon,
                                Innate = false,
                                CountsAsADebuff = true,
                                Illustration = illSoulWrench,
                                Source = eidolon,
                                ExpiresAt = ExpirationCondition.ExpiresAtEndOfSourcesTurn,
                                CannotExpireThisTurn = true,
                                StateCheck = self => {
                                    switch (self.Value) {
                                        case 1:
                                            self.Owner.AddQEffect(QEffect.Enfeebled(1).WithExpirationEphemeral());
                                            self.Owner.AddQEffect(QEffect.Clumsy(1).WithExpirationEphemeral());
                                            break;
                                        case 2:
                                            self.Owner.AddQEffect(QEffect.Enfeebled(2).WithExpirationEphemeral());
                                            self.Owner.AddQEffect(QEffect.Clumsy(2).WithExpirationEphemeral());
                                            self.Owner.AddQEffect(QEffect.Slowed(1).WithExpirationEphemeral());
                                            break;
                                        case 3:
                                            if (self.Owner.HasTrait(Trait.Undead)) {
                                                if (self.Owner.Level < eidolon.Level) {
                                                    self.Owner.Overhead("*vaporised*", Color.AliceBlue, $"{self.Owner.Name} was vaporised by Soul Wrench, destroying it instantly.");
                                                    self.Owner.Battle.RemoveCreatureFromGame(self.Owner);
                                                } else {
                                                    self.Owner.Overhead("*soul tearing*", Color.AliceBlue, $"{self.Owner.Name}'s essence was too powerful for {eidolon.Name} to destroy.");
                                                    CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText($"{eidolon.Level * 2}"), self.Owner, CheckResult.Success, DamageKind.Force);
                                                }
                                                self.ExpiresAt = ExpirationCondition.Immediately;
                                            }
                                            self.Owner.AddQEffect(QEffect.Paralyzed().WithExpirationEphemeral());
                                            break;
                                        default:
                                            break;
                                    }
                                },
                            };

                            if (checkResult == CheckResult.Failure) {
                                soulSiphon.Value = 1;
                                d.AddQEffect(soulSiphon);
                            } else if (checkResult == CheckResult.CriticalFailure) {
                                soulSiphon.Value = 2;
                                d.AddQEffect(soulSiphon);
                            }
                        })
                        ;
                    },
                    ProvideContextualAction = self => {
                        if (self.UsedThisTurn) {
                            return null;
                        }
                        
                        if (self.Owner.Battle.AllCreatures.Where(cr => cr.QEffects.FirstOrDefault(qf => qf.Id == qfSoulSiphon && qf.Source == eidolon) != null).Count() <= 0) {
                            return null;
                        }

                        return (ActionPossibility)new CombatAction(eidolon, illSoulWrench, "Sustain Soul Wrench", new Trait[] { Trait.Necromancy, Trait.Magical, Trait.Incapacitation },
                            "Your eidolon continues the battle of tug of war for their victim's soul.\n\n" +
                            "{b}Critical success{/b} The target's soul breaks free from your eidolon's grasp.\n" +
                            "{b}Failure{/b} The target's soul wretch increased by 1 stage.\n" +
                            "{b}Critical failure{/b} The target's soul wretch increased by 2 stages.\n\n",
                            Target.Self()) {
                                ShortDescription = "Your eidolon continues the battle of tug of war for their victim's soul."
                        }
                        .WithActionCost(1)
                        .WithSpellInformation(eidolon.Level / 2 + 1, "", null)
                        .WithProjectileCone(illSoulWrench, 5, ProjectileKind.Cone)
                        .WithEffectOnSelf(async (action, user) => {
                            self.UsedThisTurn = true;
                            Creature target = user.Battle.AllCreatures.FirstOrDefault(cr => cr.QEffects.FirstOrDefault(qf => qf.Id == qfSoulSiphon && qf.Source == eidolon) != null);
                            QEffect soulSiphon = target.QEffects.FirstOrDefault(qf => qf.Id == qfSoulSiphon && qf.Source == eidolon);
                            CheckResult result = CommonSpellEffects.RollSavingThrow(target, action, Defense.Will, summoner.Spellcasting.PrimarySpellcastingSource.GetSpellSaveDC());
                            
                            soulSiphon.CannotExpireThisTurn = true;
                            if (result == CheckResult.CriticalSuccess) {
                                target.RemoveAllQEffects(qf => qf.Id == qfSoulSiphon && qf.Source == eidolon);
                                target.Overhead("*escaped soul wrench*", Color.White, $"{target.Name} tugged their soul free of {eidolon.Name}'s grasp.");
                                await target.Battle.GameLoop.StateCheck();
                            } else if (result == CheckResult.Failure) {
                                soulSiphon.Value += 1;
                                if (soulSiphon.Value > 3) {
                                    soulSiphon.Value = 3;
                                }
                                target.Overhead("*soul wrench increased by 1*", Color.White, $"{target.Name}'s soul wrench has increased to stage {soulSiphon.Value}.");
                            } else if (result == CheckResult.CriticalFailure) {
                                soulSiphon.Value += 2;
                                if (soulSiphon.Value > 3) {
                                    soulSiphon.Value = 3;
                                }
                                target.Overhead("*soul wrench increased by 2*", Color.White, $"{target.Name}'s soul wrench has increased to stage {soulSiphon.Value}.");
                            }
                        });
                    }
                });
                    
            }
        }

        private static void ElementalEidolonLogic(Creature eidolon, Creature summoner) {
            eidolon.AddQEffect(new QEffect("Elemental Core", "Your eidolon's elemental nature grants it a +2 circumstance bonus to saves against being poisoned, and against the paralyze spell. Additionally, it gains a +5 bonus to staunch persistent bleed damage.") {
                StartOfCombat = async self => {
                    string[] qfNames = new string[] { QEffect.DamageImmunity(DamageKind.Bleed).Name, QEffect.DamageImmunity(DamageKind.Poison).Name, QEffect.ImmunityToCondition(QEffectId.Paralyzed).Name };

                    eidolon.RemoveAllQEffects(qf => qfNames.Contains(qf.Name) && qf.Name != null);
                },
                BonusToDefenses = (self, action, defence) => {
                    if (defence != Defense.AC && action != null && (action.Traits.ContainsOneOf(new Trait[] { Trait.Poison }) || action.SpellId == SpellId.Paralyze)) {
                        return new Bonus(2, BonusType.Circumstance, "Elemental Core");
                    }
                    return null;
                },
                ReducesPersistentDamageRecoveryCheckDc = (self, pd, kind) => kind == DamageKind.Bleed
            });

            Trait element = summoner.PersistentCharacterSheet.Calculated.AllFeats.First(ft => ft.HasTrait(tElementalType)).Traits[1];

            switch (element) {
                case Trait.Earth:
                    eidolon.AddQEffect(new QEffect("Earth Elemental", "Your eidolon gains a +2 circumstance bonus to their save DCs against attempts to Shove or Trip them, and is immune to forced movement.") {
                        BonusToDefenses = (self, action, defence) => {
                            List<SpellId> shoveSpells = new List<SpellId>() { SpellId.PummelingRubble, SpellId.HydraulicPush, SpellId.KineticRam, SpellId.TelekineticManeuver, SpellId.Grease };
                            if (action != null && defence != Defense.AC && (action.ActionId == ActionId.Trip || action.ActionId == ActionId.Shove || shoveSpells.Contains(action.SpellId))) {
                                return new Bonus(2, BonusType.Circumstance, "Earth Elemental");
                            }
                            return null;
                        },
                        StateCheck = self => {
                            self.Owner.WeaknessAndResistance.ImmunityToForcedMovement = true;
                        }
                    });
                    break;
                case Trait.Fire:
                    eidolon.AddQEffect(new QEffect("Fire Elemental", "Your eidolon's unarmed attacks deal 1 additional fire damage, and they have weakness equal to half their level attacks with the water trait. While underwater, your attacks lose the fire trait.") {
                        AddExtraKindedDamageOnStrike = (action, target) => {
                            return new KindedDamage(DiceFormula.FromText("1", "Fire Elemental"), DamageKind.Fire);
                        },
                        StateCheck = self => {
                            self.Owner.WeaknessAndResistance.AddResistance(DamageKind.Fire, int.Max(1, eidolon.Level / 2));
                            self.Owner.WeaknessAndResistance.AddWeakness(DamageKind.Cold, int.Max(1, eidolon.Level / 2));
                            self.Owner.WeaknessAndResistance.Weaknesses.Add(new SpecialResistance("water", (action, kind) => action != null && action.HasTrait(Trait.Water), 5, null));
                        }
                    });
                    break;
                case Trait.Metal:
                    Item chosenAttack = summoner.PersistentCharacterSheet.Calculated.AllFeats.First(ft => ft.HasTrait(tMetalElementalAtkType)).FeatName.HumanizeTitleCase2().ToLower().Contains("primary") ?
                        eidolon.UnarmedStrike :
                        eidolon.QEffects.FirstOrDefault(qf => qf.AdditionalUnarmedStrike != null && qf.AdditionalUnarmedStrike.WeaponProperties.Melee).AdditionalUnarmedStrike;

                    Trait[] nTraits = new Trait[3] { Trait.VersatileB, Trait.VersatileP, Trait.VersatileS };
                    nTraits.ForEach(t => {
                        if (!chosenAttack.Traits.Contains(t)) {
                            chosenAttack.Traits.Add(t);
                        }
                    });
                    break;
                case Trait.Water:
                    eidolon.AddQEffect(new QEffect("Water Elemental", "Your eidolon has a swim speed, they are not flat-footed while in water, and you don't take the usual penalties for making bludgeoning or slashing melee attacks in water.") {
                        YouAcquireQEffect = (self, newEffect) => {
                            if (newEffect.Id == QEffectId.AquaticCombat && newEffect.Name != "Aquatic Combat (water elemental)") {
                                return new QEffect("Aquatic Combat (water elemental)", "You can't cast fire spells (but fire impulses still work).\nYou can't use slashing or bludgeoning ranged attacks.\nWeapon ranged attacks have their range increments halved.\nYou have resistance 5 to acid and fire.") {
                                    Innate = false,
                                    Id = QEffectId.AquaticCombat,
                                    DoNotShowUpOverhead = self.Owner.HasTrait(Trait.Aquatic),
                                    Illustration = IllustrationName.ElementWater,
                                    StateCheck = (Action<QEffect>)(qfAquaticCombat => {
                                        qfAquaticCombat.Owner.AddQEffect(QEffect.DamageResistance(DamageKind.Acid, 5).WithExpirationEphemeral());
                                        qfAquaticCombat.Owner.AddQEffect(QEffect.DamageResistance(DamageKind.Fire, 5).WithExpirationEphemeral());
                                        if (qfAquaticCombat.Owner.HasTrait(Trait.Aquatic) || qfAquaticCombat.Owner.HasEffect(QEffectId.Swimming))
                                            return;
                                        qfAquaticCombat.Owner.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                                            Id = QEffectId.CountsAllTerrainAsDifficultTerrain
                                        });
                                    }),
                                    PreventTakingAction = (Func<CombatAction, string>)(action => {
                                        if (action.HasTrait(Trait.Impulse))
                                            return (string)null;
                                        if (action.HasTrait(Trait.Fire))
                                            return "You can't use fire actions underwater.";
                                        return action.HasTrait(Trait.Ranged) && action.HasTrait(Trait.Attack) && IsSlashingOrBludgeoning(action) ? "You can't use slashing or bludgeoning ranged attacks underwater." : (string)null;
                                    })
                                };
                            }
                            return newEffect;
                        }
                    });
                    break;
                case Trait.Wood:
                    eidolon.AddQEffect(new QEffect() {
                        ProvideMainAction = self => {
                            if (summoner.PersistentUsedUpResources.UsedUpActions.Contains("Regrowth")) {
                                return null;
                            }

                            return (ActionPossibility)new CombatAction(self.Owner, IllustrationName.FlourishingFlora, "Regrowth", new Trait[] { Trait.Healing, Trait.Magical, tEidolon, Trait.Abjuration },
                                $"Your eidolon regains a number of hits points equal to three times its level ({self.Owner.Level * 3}). Usable once per day.",
                                Target.Self().WithAdditionalRestriction(caster => caster.HP >= caster.MaxHP ? "full HP" : null)) {
                                ShortDescription = $"Regain {self.Owner.Level * 3} HP."
                            }
                            .WithActionCost(1)
                            .WithSoundEffect(SfxName.NaturalHealing)
                            .WithProjectileCone(IllustrationName.FlourishingFlora, 5, ProjectileKind.Cone)
                            .WithEffectOnSelf(async (action, user) => {
                                await user.HealAsync(DiceFormula.FromText($"{user.Level * 3}", "Regrowth"), action);
                                summoner.PersistentUsedUpResources.UsedUpActions.Add("Regrowth");
                            });
                        }
                    });
                    break;
                default:
                    break;
            }
            if (eidolon.Level >= 7) {
                eidolon.AddQEffect(new QEffect() {
                    ProvideMainAction = self => {
                        if (self.Owner.HasEffect(qfElementalBurst)) {
                            return null;
                        }

                        string damage = $"{self.Owner.Level - 1}d6";
                        DamageKind kind = element == Trait.Fire ? DamageKind.Fire : self.Owner.UnarmedStrike.WeaponProperties.DamageKind;

                        return (ActionPossibility)new CombatAction(self.Owner, illElementalBurst, "Elemental Burst", new Trait[] { Trait.Evocation, Trait.Primal },
                            "{b}Range{/b} 60 feet\n{b}Area{/b} 20 foot burst\n{b}Saving throw{/b} basic Reflex\n{b}Frequency{/b} Once per encounter\n\n" +
                            $"Your eidolon rips off a chunk of elemental matter from their own form and hurls it into a group of foes. Your eidolon loses a number of Hit Points equal to your level, dealing {damage} {kind.HumanizeTitleCase2()} damage to all creatures inside the burst. ",
                            Target.Burst(12, 4)) {
                            ShortDescription = $"20 foot burst, basic Reflex; {damage} {kind.HumanizeTitleCase2()} damage"
                        }
                        .WithActionCost(2)
                        .WithSoundEffect(SfxName.Fireball)
                        .WithProjectileCone(illElementalBurst, 20, ProjectileKind.Cone)
                        .WithSavingThrow(new SavingThrow(Defense.Reflex, summoner.ClassOrSpellDC()))
                        .WithEffectOnEachTarget(async (spell, caster, target, result) => await CommonSpellEffects.DealBasicDamage(spell, caster, target, result, damage, kind))
                        .WithEffectOnSelf(async (action, user) => {
                            await CommonSpellEffects.DealDirectDamage(action, DiceFormula.FromText($"{user.Level}", "Elemental Burst Cost"), user, CheckResult.Success, DamageKind.Force);
                            user.AddQEffect(new QEffect() { Id = qfElementalBurst });
                        });
                    }
                });
            }
        }

        private static void ClassicDemonEidolonLogic(Creature eidolon, Creature summoner) {
            eidolon.AddQEffect(new QEffect("Demonic Strikes", "Your eidolon's unarmed strikes deal +1 extra evil damage.") {
                AddExtraKindedDamageOnStrike = (action, target) => {
                    return new KindedDamage(DiceFormula.FromText("1", "Demonic Strikes"), DamageKind.Evil);
                },
            });
            if (eidolon.Level >= 7) {
                QEffect lvl7 = new QEffect() {
                    ProvideMainAction = self => {
                        if (self.Owner.HasEffect(Enums.qfVisionsOfSin))
                            return null;

                        return (ActionPossibility)new CombatAction(self.Owner, IllustrationName.HideousLaughter, "Visions of Sin", [Trait.Divine, Trait.Emotion, Enums.tEidolon, Trait.Mental, Trait.InflictsSlow],
                            "{b}Frequency{/b} Once per encounter.\n{b}Range{/b} 30 feet\n{b}Saving Throw{/b} Will\n\nYour eidolon summons images of its sin into the mind of a target creature, tormenting and confusing them. If the target is evil, they suffer a -2 circumstance penalty to their Will save.\n\n"
                            + S.FourDegreesOfSuccess(null, "The target can't use reactions.", "The target is slowed 1 and can't use reactions.", "As failure, and the target is also confused for 1 round. The confusion can't be extended, but the other effects can."),
                            Target.Ranged(6)) {
                            ShortDescription = "Slow or confuse an enemy with disturbing visions."
                        }
                        .WithSavingThrow(new SavingThrow(Defense.Will, summoner.ClassOrSpellDC()))
                        .WithProjectileCone(IllustrationName.HideousLaughter, 7, ProjectileKind.Cone)
                        .WithSoundEffect(SfxName.HideousLaughterVoiceMaleB02)
                        .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                            if (result <= CheckResult.Success) {

                                var effect = new QEffect("Visions of Sin", "You can't take reactions.", ExpirationCondition.ExpiresAtEndOfSourcesTurn, caster, IllustrationName.HideousLaughter) {
                                    CountsAsBeneficialToSource = true,
                                    CannotExpireThisTurn = true,
                                    StateCheck = (qfVoS) => {
                                        qfVoS.Owner.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                                            Id = QEffectId.CannotTakeReactions
                                        });
                                        if (result <= CheckResult.Failure) {
                                            qfVoS.Owner.AddQEffect(QEffect.Slowed(1).WithExpirationEphemeral());
                                        }
                                    }
                                };
                                if (result == CheckResult.CriticalFailure) {
                                    target.AddQEffect(QEffect.Confused(false, spell).WithExpirationAtStartOfSourcesTurn(caster, 1));
                                }

                                target.AddQEffect(effect);
                                var qSustaining = QEffect.Sustaining(spell, effect);
                                caster.AddQEffect(qSustaining);
                            }
                        })
                        .WithEffectOnSelf(caster => {
                            caster.AddQEffect(new QEffect() { Id = Enums.qfVisionsOfSin });
                        })
                        ;
                    }
                };

                lvl7.AddGrantingOfTechnical(cr => !eidolon.HasEffect(Enums.qfVisionsOfSin) && cr.HasTrait(Trait.Evil), qfTech => {
                    qfTech.BonusToDefenses = (self, action, defence) => action?.Name == "Visions of Sin" ? new Bonus(-2, BonusType.Circumstance, "Evil", false) : null;
                });

                eidolon.AddQEffect(lvl7);
            }
        }

        private static void LustDemonEidolonLogic(Creature eidolon, Creature summoner) {
            string rejectionDmg = Math.Max(1, eidolon.Level / 3) + "d6";

            eidolon.AddQEffect(QEffect.DamageWeakness(DamageKind.Good, Math.Max(1, eidolon.Level / 2)));
            eidolon.AddQEffect(QEffect.DamageWeakness(Trait.ColdIron, Math.Max(1, eidolon.Level / 2)));
            eidolon.AddQEffect(new QEffect("Seductive Presence", "Creatures susceptible to earthly desires within 10 feet of you take a -1 circumstance penalty to saving throws and DCs against effects with the Mental trait.") {
                 StateCheck = (sc) => {
                     foreach (var inAura in sc.Owner.Battle.AllCreatures.Where(cr => cr != sc.Owner && cr.EnemyOf(sc.Owner) && cr.DistanceTo(sc.Owner) <= 2)) {
                         if (!inAura.HasTrait(Trait.Mindless) && !inAura.HasTrait(Trait.Object) && !inAura.HasTrait(Trait.Undead) && !inAura.HasTrait(Trait.Animal) && !inAura.HasTrait(Trait.Elemental) && !inAura.HasTrait(Trait.Beast) && !inAura.HasTrait(Trait.Aberration)) {
                             inAura.AddQEffect(new QEffect("Seductive Presence", $"You have -1 to saves and DCs against {sc.Owner.Name}'s abilities with the mental trait.", ExpirationCondition.Ephemeral, sc.Owner, IllustrationName.Love) {
                                 BonusToDefenses = (effect, action, defense) => {
                                     if (action?.Owner == sc.Owner && action.HasTrait(Trait.Mental)) {
                                         return new Bonus(-1, BonusType.Circumstance, "Seductive Presence");
                                     }

                                     return null;
                                 }
                             });
                         }
                     }
                 }
            });
            eidolon.AddQEffect(new QEffect("Rejection vulnerability", $"When your eidolon fails a Diplomacy check to Embrace, or when a creature succeeds at its save against your eidolon's Bewitch or Passionate Kiss, " +
                $"it takes {rejectionDmg} mental damage and when that creature next successfully Demoralizes the your eidolon this encounter, they take the damage again.") {
                AfterYouTakeActionAgainstTarget = async (effect, action, defender, checkResult) => {
                    var mentalActionOwnerSuccubus = action.Owner;
                    if ((action.Name == "Embrace" || action.Name == "Passionate Kiss" || action.Name == "Bewitch") && defender != mentalActionOwnerSuccubus) {
                        if ((action.ActiveRollSpecification != null && checkResult <= CheckResult.Failure) ||
                            (action.SavingThrow != null && checkResult >= CheckResult.Success)) {
                            await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText(rejectionDmg, "Rejection vulnerability"), mentalActionOwnerSuccubus, CheckResult.Failure, DamageKind.Mental);
                            defender.AddQEffect(new QEffect("Rejected " + mentalActionOwnerSuccubus, $"You rejected a lust demon and you can incorporate that rejection into an attempt to Demoralize it. If that Demoralize succeeds, it also deals {rejectionDmg} additional mental damage.", ExpirationCondition.Never, defender, IllustrationName.Demoralize) {
                                Key = "Lust Eidolon Rejection",
                                StateCheck = (sc) => {
                                    if (!mentalActionOwnerSuccubus.Alive) {
                                        sc.ExpiresAt = ExpirationCondition.Immediately;
                                    }
                                },
                                AfterYouTakeActionAgainstTarget = async (rejection, demoralize, succubus, demoralizeResult) => {
                                    if (demoralize.ActionId == ActionId.Demoralize && succubus == action.Owner) {
                                        if (demoralizeResult >= CheckResult.Success) {
                                            await CommonSpellEffects.DealDirectSplashDamage(demoralize, DiceFormula.FromText(rejectionDmg, "Rejection vulnerability"), succubus, DamageKind.Mental);
                                        }

                                        rejection.ExpiresAt = ExpirationCondition.Immediately;
                                    }
                                },
                                AdditionalGoodness = (self, action, target) => {
                                    if (target == eidolon && action.ActionId == ActionId.Demoralize)
                                        return Math.Max(1, eidolon.Level / 3) * 3.5f;
                                    return 0f;
                                }
                            });
                        }
                    }
                }
            });
            eidolon.AddQEffect(new QEffect() { Id = QEffectId.GrappleWithDiplomacy });
            eidolon.AddQEffect(new QEffect() {
                ProvideMainAction = qfSelf => new ActionPossibility(new CombatAction(qfSelf.Owner, new SideBySideIllustration(IllustrationName.LoversGloves, IllustrationName.Grapple), "Embrace", [Trait.Attack, Trait.Mental, Trait.Emotion, Trait.AttackDoesNotTargetAC, Trait.Restraining], "Make a Diplomacy check against the target's Will DC." + CommonAbilityEffects.GrappleDescription, Target.Touch().WithAdditionalConditionOnTargetCreature(new GrappleCreatureTargetingRequirement()))
                    .WithActionId(ActionId.Grapple)
                    .WithSoundEffect(SfxName.Grapple)
                    .WithShortDescription("Grapple the target, but it's Diplomacy vs. Will DC instead of Athletics vs. Fortitude DC.")
                    .WithActiveRollSpecification(new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Diplomacy), TaggedChecks.DefenseDC(Defense.Will)))
                    .WithGoodnessAgainstEnemy((t, a, d) => {
                        if (!d.HasTrait(Trait.Humanoid)) return AIConstants.NEVER;
                        return d.HasEffect(QEffectId.Grappled) ? AIConstants.NEVER : 25;
                    })
                    .WithEffectOnEachTarget((async (action, grappler, target, checkResult) => { await Possibilities.Grapple(grappler, target, checkResult); }))
                )
            });
            string kissDmg = "1d6+" + eidolon.Level;
            eidolon.AddQEffect(new QEffect() {
                ProvideMainAction = qfSelf => new ActionPossibility(new CombatAction(qfSelf.Owner, IllustrationName.HealersBlessing, "Passionate Kiss", [Trait.Divine, Trait.Emotion, Trait.Enchantment, Trait.Mental],
                "{b}Frequency{b} Once per round\n{b}Saving throw{b} basic Will\n{b}Target{b} 1 creature you have grappled{i}\n\nYou passionately kiss your embraced victim.{/i}\n\n" +
                "You deal " + kissDmg + " negative damage to the target and gain temporary hit points equal to your level.\n\nIf the target fails or critical fails their saving throw," +
                " they cannot attempt to Escape or take actions against you until your next turn.",
                Target.Touch().WithAdditionalConditionOnTargetCreature(new GrappledCreatureOnlyCreatureTargetingRequirement()).WithAdditionalConditionOnTargetCreature(new NotUsedThisTurnCreatureTargetingRequirement(ActionId.Kiss)))
                    .WithSoundEffect(SfxName.Kiss)
                    .WithActionId(ActionId.Kiss)
                    .WithShortDescription("Kiss a grappled creature, inflicting " + kissDmg + " negative damage against a basic Will save. On a failure, you gain " + eidolon.Level + " temporary HP and they are unable to escape or attack you.")
                    .WithSavingThrow(new SavingThrow(Defense.Will, summoner.ClassOrSpellDC()))
                    .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                        // Deal damage
                        await CommonSpellEffects.DealBasicDamage(spell, caster, target, result, DiceFormula.FromText(kissDmg, "Kiss"), DamageKind.Negative);

                        // Suggest
                        if (result <= CheckResult.Failure) {
                            caster.GainTemporaryHP(caster.Level);
                            summoner.GainTemporaryHP(caster.Level);
                            target.AddQEffect(new QEffect("Passionate Kiss", "You're under a magical compulsion by " + caster + " to submit to acts of passion rather than fight. You can't attempt to Escape them or take actions against them.", ExpirationCondition.Never, caster, IllustrationName.DarkHeart) {
                                PreventTakingAction = ca => ca.ActionId == ActionId.Escape && ca.Name.Contains(caster.Name) ? $"You're forced to stay in the grapple by {caster.Name}." : null,
                                StateCheck = qfCharmed => {
                                    caster.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                                        PreventTargetingBy = attack => attack.Owner == qfCharmed.Owner ? "passionate-kiss" : null,
                                        PreventAreaAttacksAgainstMe = (_, incomingSpell) => incomingSpell.Owner == qfCharmed.Owner && (incomingSpell.WillBecomeHostileAction || (incomingSpell.SpellId == SpellId.Heal && incomingSpell.Owner.HasEffect(QEffectId.HolyCastigation)))
                                    });
                                    if (!caster.Alive) {
                                        qfCharmed.ExpiresAt = ExpirationCondition.Immediately;
                                    }
                                }
                            }.WithExpirationAtStartOfSourcesTurn(caster, 1));
                        }
                    })
                )
            });
            if (eidolon.Level >= 7) {
                eidolon.AddQEffect(new QEffect() {
                    ProvideMainAction = self => {
                        if (self.Owner.HasEffect(Enums.qfVisionsOfSin))
                            return null;

                        return (ActionPossibility) new CombatAction(self.Owner, IllustrationName.Dominate, "Bewitch", [Trait.Enchantment, Trait.Incapacitation, Trait.Mental, Trait.AssumesDirectControl],
                            "{b}Frequency{/b} Once per encounter.\n{b}Range{/b} 30 feet\n{b}Saving Throw{/b} Will\n\n{i}You take command of the target, forcing it to obey your orders.{/i}" +
                            S.FourDegreesOfSuccess("The target is unaffected.", "The target is stunned 1.", "You gain control of the target until the end of their next turn. They are Slowed 2 while controlled in this way.", "As failure, but they are not slowed."),
                        Target.Ranged(6).WithAdditionalConditionOnTargetCreature((a, d) => !d.HasTrait(Trait.Minion) && !d.Traits.Any(tr => tr.HumanizeLowerCase2() == "eidolon") ? Usability.Usable : Usability.NotUsableOnThisCreature("minion"))) {
                            ShortDescription = "Dominate target creature's mind."
                        }
                        .WithSoundEffect(SfxName.Mental)
                        .WithActionCost(3)
                        .WithSavingThrow(new SavingThrow(Defense.Will, summoner.ClassOrSpellDC()))
                        .WithGoodnessAgainstEnemy((Func<Target, Creature, Creature, float>)((t, a, d) => (float)d.HP))
                        .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                            if (result == CheckResult.Success)
                                target.AddQEffect(QEffect.Stunned(1));
                            if (result > CheckResult.Failure)
                                return;
                            Faction originalFaction = target.OwningFaction;
                            target.OwningFaction = caster.OwningFaction;
                            QEffect slowed = QEffect.Slowed(2).WithExpirationAtStartOfSourcesTurn(caster, 1);
                            if (result == CheckResult.Failure)
                                target.AddQEffect(slowed);
                            target.AddQEffect(new QEffect("Controlled", "You're controlled by " + caster?.ToString() + ".", ExpirationCondition.ExpiresAtEndOfYourTurn, caster, (Illustration)IllustrationName.Dominate) {
                                Value = 0,
                                StateCheck = qf => {
                                    if (caster.Alive)
                                        return;
                                    qf.Owner.Occupies.Overhead("end of control", Color.Lime, caster?.ToString() + " was banished and so can no longer bewitch " + target?.ToString() + ".");
                                    if (qf.Owner.OwningFaction != caster.OwningFaction)
                                        return;
                                    qf.Owner.OwningFaction = originalFaction;
                                    qf.ExpiresAt = ExpirationCondition.Immediately;
                                },
                                WhenExpires = self => {
                                    self.Owner.Occupies.Overhead("end of control", Color.Lime, target?.ToString() + " shook off the bewitchment.");
                                    if (self.Owner.OwningFaction != caster.OwningFaction)
                                        return;
                                    self.Owner.OwningFaction = originalFaction;
                                    self.Owner.RemoveAllQEffects(qf => qf == slowed);
                                }
                            });
                        })
                        .WithEffectOnSelf(caster => {
                            caster.AddQEffect(new QEffect() { Id = Enums.qfVisionsOfSin });
                        });
                    }
                });
            }
        }

        private static void WrathDemonEidolonLogic(Creature eidolon, Creature summoner) {
            string peaceDmg = Math.Max(1, eidolon.Level / 3) + "d6";

            eidolon.AddQEffect(QEffect.DamageWeakness(DamageKind.Good, Math.Max(1, eidolon.Level / 2)));
            eidolon.AddQEffect(QEffect.DamageWeakness(Trait.ColdIron, Math.Max(1, eidolon.Level / 2)));

            eidolon.AddQEffect(new QEffect("Peace Vulnerability", $"If your eidolon fails a saving throw against an emotion effect, it takes {peaceDmg} mental damage.") {
                AfterYouAreTargeted = async (qfPeaceVulnerability, action) => {
                    if (action.HasTrait(Trait.Emotion)
                        && action.SavingThrow != null
                        && action.ChosenTargets.CheckResults.TryGetValue(qfPeaceVulnerability.Owner, out var checkResult)
                        && checkResult <= CheckResult.Failure) {
                        qfPeaceVulnerability.Owner.Occupies.Overhead("Peace Vulnerability", Color.White, $"An emotional effect hurts {eidolon.Name} (peace vulnerability).");
                        int prevDmg = eidolon.Damage;
                        await CommonSpellEffects.DealDirectSplashDamage(action, DiceFormula.FromText(peaceDmg, "Peace Vulnerability"), qfPeaceVulnerability.Owner, DamageKind.Mental);
                        await CommonSpellEffects.DealDirectSplashDamage(action, DiceFormula.FromText((eidolon.Damage - prevDmg).ToString(), "Health Share (Peace Vulnerability)"), summoner, DamageKind.Mental);
                    }
                }
            });

            eidolon.AddQEffect(new QEffect() {
                ProvideMainAction = self => {
                    return (ActionPossibility) new CombatAction(self.Owner, IllustrationName.StinkingCloud, "Spore Cloud", [Trait.Disease, Trait.Poison],
                        $"Deal {eidolon.Level.ToString()} poison damage to all adjacent creatures {{i}}(no save){{/i}}. Each adjacent creature also makes a Fortitude save." +
                        $" On a failure, it takes {eidolon.Level.ToString()} persistent piercing damage. This persistent damage is removed if the creature is affected by a good" +
                        " effect. After you use Spore Cloud, the ability can't be used again for 1d6 rounds.", Target.SelfExcludingEmanation(1))
                    .WithActionCost(1)
                    .WithSavingThrow(new SavingThrow(Defense.Fortitude, summoner.ClassOrSpellDC()))
                    .WithProjectileCone(VfxStyle.BasicProjectileCone(IllustrationName.StinkingCloud))
                    .WithSoundEffect(SfxName.GaleBlast)
                    .WithEffectOnSelf(qf => {
                        qf.AddQEffect(QEffect.Recharging("Spore Cloud", Dice.D6));
                    })
                    .WithEffectOnEachTarget(async (action, attacker, defender, result) => {
                        await CommonSpellEffects.DealDirectDamage(action, DiceFormula.FromText(eidolon.Level.ToString(), "Spore Cloud"), defender, result, DamageKind.Poison);
                        if (result <= CheckResult.Failure) {
                            QEffect persistentDamage = QEffect.PersistentDamage(eidolon.Level.ToString(), DamageKind.Piercing);
                            persistentDamage.Description += "\nThe damage goes away automatically if you are targeted by a good effect.";
                            QEffect persistentDamageRemover = new QEffect() {
                                AfterYouAreTargeted = async (qf, goodAction) => {
                                    if (goodAction.HasTrait(Trait.Good)) {
                                        persistentDamage.ExpiresAt = ExpirationCondition.Immediately;
                                        qf.ExpiresAt = ExpirationCondition.Immediately;
                                    }
                                }
                            };
                            persistentDamage.WhenExpires = (expires) => { persistentDamageRemover.ExpiresAt = ExpirationCondition.Immediately; };
                            defender.AddQEffect(persistentDamage);
                            defender.AddQEffect(persistentDamageRemover);
                        }
                    });
                }
            });

            if (eidolon.Level >= 7) {
                eidolon.AddQEffect(new QEffect() {
                    ProvideMainAction = self => {
                        if (self.Owner.HasEffect(QEffectId.UsedMonsterOncePerEncounterAbility)) return null;
                        return (ActionPossibility)new CombatAction(self.Owner, IllustrationName.Deafness, "Stunning Screech", [Trait.Auditory, Trait.Divine, Trait.Incapacitation, Trait.Sonic],
                            "You emit a shrill screech. Each enemy creature within a 15-foot burst must attempt a Fortitude save. On a failure, the creature is stunned 1, " +
                            "and on a critical failure, it's stunned 2. This ability can only be used once per encounter.", Target.SelfExcludingEmanation(3))
                        .WithActionCost(2)
                        .WithSavingThrow(new SavingThrow(Defense.Fortitude, summoner.ClassOrSpellDC()))
                        .WithProjectileCone(VfxStyle.BasicProjectileCone(IllustrationName.Deafness))
                        .WithNoSaveFor((ca, df) => df.FriendOf(ca.Owner))
                        .WithGoodnessAgainstEnemy((target, attacker, defender) => {
                            if (defender.FriendOf(attacker)) return 0;
                            return attacker.AI.Stun(defender, 1);
                        })
                        .WithSoundEffect(SfxName.HauntingHymn)
                        .WithEffectOnEachTarget(async (action, attacker, defender, result) => {
                            if (defender.FriendOf(attacker)) return;
                            if (result <= CheckResult.Failure) {
                                defender.AddQEffect(QEffect.Stunned(result == CheckResult.CriticalFailure ? 2 : 1));
                            }
                        })
                        .WithEffectOnChosenTargets(async (caster, targets) => {
                            caster.AddQEffect(new QEffect() { Id = QEffectId.UsedMonsterOncePerEncounterAbility });
                        });
                    }
                });
            }
        }


        private static bool IsSlashingOrBludgeoning(CombatAction action) {
            Item obj1 = action.Item;
            DamageKind? damageKind1;
            int num1;
            if (obj1 == null) {
                num1 = 0;
            } else {
                damageKind1 = obj1.WeaponProperties?.DamageKind;
                DamageKind damageKind2 = DamageKind.Slashing;
                num1 = damageKind1.GetValueOrDefault() == damageKind2 & damageKind1.HasValue ? 1 : 0;
            }
            if (num1 == 0) {
                Item obj2 = action.Item;
                int num2;
                if (obj2 == null) {
                    num2 = 0;
                } else {
                    damageKind1 = obj2.WeaponProperties?.DamageKind;
                    DamageKind damageKind3 = DamageKind.Bludgeoning;
                    num2 = damageKind1.GetValueOrDefault() == damageKind3 & damageKind1.HasValue ? 1 : 0;
                }
                if (num2 == 0)
                    return false;
            }
            Item obj3 = action.Item;
            return (obj3 != null ? (obj3.HasTrait(Trait.VersatileP) ? 1 : 0) : 0) == 0;
        }

    }
}



            
            
            //} else if (summoner.HasFeat(scBeastEidolon)) {
            //    
            //    }
            //} else if (summoner.HasFeat(scDevoPhantomEidolon)) {
            //    
            //    }
            //} else if (summoner.HasFeat(scAzataEidolon)) {
            //    
            //} else if (summoner.HasFeat(scDevilEidolon)) {
            //    
            //} else if (summoner.HasFeat(scAngerPhantom)) {
            //    
            //}
