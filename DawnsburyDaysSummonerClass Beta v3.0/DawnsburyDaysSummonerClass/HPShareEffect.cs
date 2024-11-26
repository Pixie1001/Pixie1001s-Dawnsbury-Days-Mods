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
using Microsoft.Xna.Framework.Graphics;

namespace Dawnsbury.Mods.Classes.Summoner {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class HPShareEffect : QEffect {
        public List<HPShareLogEntry> Logs { get; private set; }
        private CombatAction ca;
        public CombatAction CA { get { return ca; } }

        public HPShareEffect(Creature owner) : base() {
            Init(owner);
        }

        public HPShareEffect(Creature owner, string name, string description) : base(name, description) {
            Init(owner);
        }

        private void Init(Creature owner) {
            Logs = new List<HPShareLogEntry>();
            this.ca = new CombatAction(owner, (Illustration)IllustrationName.ElementWater, "SummonerClass: Share HP", new Trait[0], "", new UncastableTarget("Not a real ability"));
        }

        public void Reset() {
            Logs.Clear();
        }

        public void Clean() {
            Logs.RemoveAll(log => log.Processed && log.Type != SummonerClassEnums.InterceptKind.TARGET);
        }

        //public void SoftReset() {
        //    HP = Owner.HP;
        //    TempHP = Owner.TemporaryHP;
        //}

        public void LogAction(Creature self, CombatAction? action, Creature? attacker, SummonerClassEnums.InterceptKind type) {
            Logs.Add(new HPShareLogEntry(self, action, attacker, type));
        }

        public void UpdateLogs(int damage, HPShareLogEntry triggeringLog) {
            foreach (HPShareLogEntry log in Logs.Where(l => l != triggeringLog)) {
                log.HP -= damage;
            }
        }

        public bool CheckForTargetLog(CombatAction action, Creature attacker) {
            foreach (HPShareLogEntry log in Logs) {
                if (log.Type == SummonerClassEnums.InterceptKind.TARGET && log.LoggedAction == action && log.LoggedCreature == attacker && log.ActionHistory == attacker.Actions.ActionHistoryThisTurn && log.LoggedThisTurn) {
                    return true;
                }
            }
            return false;
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class HPShareLogEntry {
        public SummonerClassEnums.InterceptKind Type { get; set; }
        public CombatAction? LoggedAction { get; set; }
        public Creature? LoggedCreature { get; set; }
        public List<CombatAction>? ActionHistory { get; set; }
        public bool LoggedThisTurn { get; set; }
        public int HP { get; set; }
        public int TempHP { get; set; }
        public Creature Owner { get; set; }
        public bool Processed { get; set; }

        public HPShareLogEntry(Creature self, CombatAction? action, Creature? attacker, SummonerClassEnums.InterceptKind type) {
            Processed = false;
            Owner = self;
            Type = type;
            HP = self.HP;
            TempHP = self.TemporaryHP;
            LoggedAction = action;
            LoggedCreature = attacker;
            if (attacker != null) {
                ActionHistory = attacker.Actions.ActionHistoryThisTurn;
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

        //public bool CompareEffects(HPShareEffect partnerLogs) {
        //    foreach (HPShareLogEntry log in partnerLogs.Logs) {
        //        if (this.LoggedAction == log.LoggedAction && this.LoggedCreature == log.LoggedCreature && this.ActionHistory == log.ActionHistory && this.LoggedThisTurn == log.LoggedThisTurn) {
        //            if (this.HealOrHarm(this.Owner) == log.HealOrHarm(log.Owner)) {
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}

        public SummonerClassEnums.EffectKind CompareEffects(HPShareEffect partnerLogs, out HPShareLogEntry? matchingLog) {
            foreach (HPShareLogEntry log in partnerLogs.Logs) {
                if (this.LoggedAction == log.LoggedAction && this.LoggedCreature == log.LoggedCreature && this.ActionHistory == log.ActionHistory && this.LoggedThisTurn == log.LoggedThisTurn) {
                    matchingLog = log;
                    if (this.HealOrHarm(this.Owner) == log.HealOrHarm(log.Owner)) {
                        return this.HealOrHarm(this.Owner);
                    }
                    if (this.HealOrHarm(this.Owner) == SummonerClassEnums.EffectKind.HEAL && log.HealOrHarm(log.Owner) == SummonerClassEnums.EffectKind.HARM) {
                        return SummonerClassEnums.EffectKind.HEAL_HARM;
                    }
                    if (this.HealOrHarm(this.Owner) == SummonerClassEnums.EffectKind.HARM && log.HealOrHarm(log.Owner) == SummonerClassEnums.EffectKind.HEAL) {
                        return SummonerClassEnums.EffectKind.HARM_HEAL;
                    }
                    return SummonerClassEnums.EffectKind.NONE;
                }
            }
            matchingLog = null;
            return SummonerClassEnums.EffectKind.NONE;
        }

        //public bool CompareEffects(CombatAction action, Creature attacker) {
        //    if (this.LoggedAction == action && this.LoggedCreature == attacker && this.ActionHistory == attacker.Actions.ActionHistoryThisTurn && this.LoggedThisTurn == true) {
        //        return true;
        //    }
        //    return false;
        //}
    }
}
