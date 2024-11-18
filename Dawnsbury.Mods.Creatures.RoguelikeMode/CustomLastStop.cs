using Dawnsbury.Campaign.Path.CampaignStops;
using Dawnsbury.Core.Mechanics.Treasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class CustomLastStop : DawnsburyStop {

        public CustomLastStop(DawnsburyStop stop, int index) : base(
            (string)typeof(DawnsburyStop).GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(stop),
            (int)typeof(DawnsburyStop).GetField("dawnsburyStopIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(stop),
            (int)typeof(DawnsburyStop).GetField("<ShopLevel>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(stop)
        ) {
            this.Index = index;
        }

        public override string Description {
            get {
                return "Placeholder text for victory\n\n" + Loader.Credits;
            }
            
        }

    }
}
