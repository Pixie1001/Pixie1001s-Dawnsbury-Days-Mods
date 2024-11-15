// Decompiled with JetBrains decompiler
// Type: Dawnsbury.Core.Mechanics.ReduceDamageModification
// Assembly: Dawnsbury Days, Version=3.5.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 17BDFECB-A1D5-4257-9B12-0A0D71D62757
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Dawnsbury Days\Data\Dawnsbury Days.dll
// XML documentation location: C:\Program Files (x86)\Steam\steamapps\common\Dawnsbury Days\Data\Dawnsbury Days.xml

using Dawnsbury.Core.Mechanics.Damage;

#nullable enable
namespace Dawnsbury.Core.Mechanics {
    public class IncreaseDamageModification : DamageModification {
        private readonly int increaseAmount;
        private readonly string explanation;

        public IncreaseDamageModification(int amount, string explanation) {
            this.increaseAmount = amount;
            this.explanation = explanation;
        }

        public override void Apply(DamageEvent damageEvent) {
            damageEvent.KindedDamages[0].ResolvedDamage += increaseAmount;
            damageEvent.DamageEventDescription.AppendLine("{b}+" + increaseAmount.ToString() + "{/b} " + explanation);
        }
    }
}
