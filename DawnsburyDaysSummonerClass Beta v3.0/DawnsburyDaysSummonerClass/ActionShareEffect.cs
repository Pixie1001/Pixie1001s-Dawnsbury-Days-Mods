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
    public class ActionShareEffect : QEffect {
        public int ActionTally { get; private set;  }
        public bool UsedQuickenedAction { get; private set; }
        public bool ResetRequired { get; private set; }


        public ActionShareEffect(string name, string description) : base(name, description) {
        }

        public ActionShareEffect() : base() {
        }

        public void Init() {
            ResetRequired = false;
        }

        public void LogTurnEnd(Actions actions) {
            ActionTally = actions.ActionsLeft;
            UsedQuickenedAction = actions.UsedQuickenedAction;
            ResetRequired = true;
        }

        public void Clear() {
            ResetRequired = false;
        }
    }
}
