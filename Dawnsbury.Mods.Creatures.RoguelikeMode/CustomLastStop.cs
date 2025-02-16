using Dawnsbury.Campaign.Path;
using Dawnsbury.Campaign.Path.CampaignStops;
using Dawnsbury.Core;
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
        private CampaignState campaign;

        public CustomLastStop(DawnsburyStop stop, int index, CampaignState campaign) : base(
            (string?)typeof(DawnsburyStop).GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(stop),
            false,
            //(int)typeof(DawnsburyStop).GetField("dawnsburyStopIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(stop),
            (int)typeof(DawnsburyStop).GetField("<ShopLevel>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(stop)
        ) {
            this.campaign = campaign;
            this.Index = index;
        }

        public override string Description {
            get {
                return "{b}Congratulations!{/b} You survived the Below and saved Dawnsbury from the Machinations of the Spider Queen! But it won't be long before she tries again, and another brave group of adventurers will need to once again brave the Below...\n\n" +
                    "{b}Stats{/b}\n" +
                    "{b}Deaths:{/b} " + campaign.Tags["deaths"] + "\n" +
                    "{b}Restarts:{/b} " + campaign.Tags["restarts"] +
                    "\n\n" + Loader.Credits;
            }

        }

    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class CustomMidStop : DawnsburyStop {
        private CampaignState campaign;

        public CustomMidStop(DawnsburyStop stop, int index, CampaignState campaign) : base(
            (string?)typeof(DawnsburyStop).GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(stop),
            false,
            //(int)typeof(DawnsburyStop).GetField("dawnsburyStopIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(stop),
            (int)typeof(DawnsburyStop).GetField("<ShopLevel>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(stop)
        ) {
            this.campaign = campaign;
            this.Index = index;
        }

        //public override string Description {
        //    get {
        //        return flavourText "{b}Congratulations!{/b} You survived the Below and saved Dawnsbury from the Machinations of the Spider Queen! But it won't be long before she tries again, and another brave group of adventurers will need to once again brave the Below...\n\n" +
        //            "{b}Stats{/b}\n" +
        //            "{b}Deaths:{/b} " + campaign.Tags["deaths"] + "\n" +
        //            "{b}Restarts:{/b} " + campaign.Tags["restarts"] +
        //            "\n\n" + Loader.Credits;
        //    }

        //}

    }
}
