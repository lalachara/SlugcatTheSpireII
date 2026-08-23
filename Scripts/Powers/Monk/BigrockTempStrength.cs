using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts.Card.Monk;

namespace Rainworld.Scripts.Powers;

public class BigrockTempStrength : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Rainworld_Monk_Bigrock>();
    protected override bool IsPositive => false;
}