using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core;
using Dawnsbury.Display.Illustrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawnsbury.Mods.Classes.Summoner {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class HPShareEffect : QEffect {
        public SummonerClassEnums.InterceptKind Type { get; set; }
        public CombatAction? LoggedAction { get; set; }
        public Creature? LoggedCreature { get; set; }
        public List<CombatAction>? ActionHistory { get; set; }
        public bool LoggedThisTurn { get; set; }
        public int HP { get; set; }
        public int TempHP { get; set; }
        private CombatAction ca;
        public CombatAction CA { get { return ca; } }

        public HPShareEffect(Creature owner) : base() {
            Init(owner);
        }

        public HPShareEffect(Creature owner, string name, string description) : base(name, description) {
            Init(owner);
        }

        private void Init(Creature owner) {
            this.ca = new CombatAction(owner, (Illustration)IllustrationName.ElementWater, "SummonerClass: Share HP", new Trait[0], "", new UncastableTarget("Not a real ability"));
        }

        public void Reset() {
            LoggedAction = null;
            LoggedCreature = null;
            ActionHistory = null;
            LoggedThisTurn = false;
        }

        public void LogAction(Creature self, CombatAction? action, Creature? creature, SummonerClassEnums.InterceptKind type) {
            Type = type;
            HP = self.HP;
            TempHP = self.TemporaryHP;
            LoggedAction = action;
            LoggedCreature = creature;
            if (creature != null) {
                ActionHistory = creature.Actions.ActionHistoryThisTurn;
            } else {
                ActionHistory = null;
            }
            LoggedThisTurn = true;
        }

        public SummonerClassEnums.EffectKind HealOrHarm(Creature owner) {
            if (this.HP + this.TempHP > owner.HP + owner.TemporaryHP) {
                return SummonerClassEnums.EffectKind.HARM;
            }
            if (this.HP < owner.HP) {
                return SummonerClassEnums.EffectKind.HEAL;
            }
            return SummonerClassEnums.EffectKind.NONE;
        }

        public bool CompareEffects(HPShareEffect effectLog) {
            if (this.LoggedAction == effectLog.LoggedAction && this.LoggedCreature == effectLog.LoggedCreature && this.ActionHistory == effectLog.ActionHistory && this.LoggedThisTurn == effectLog.LoggedThisTurn) {
                if (this.HealOrHarm(this.Owner) == effectLog.HealOrHarm(effectLog.Owner)) {
                    return true;
                }
            }
            return false;
        }

        public bool CompareEffects(CombatAction action, Creature attacker) {
            if (this.LoggedAction == action && this.LoggedCreature == attacker && this.ActionHistory == attacker.Actions.ActionHistoryThisTurn && this.LoggedThisTurn == true) {
                return true;
            }
            return false;
        }
    }
}
