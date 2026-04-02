using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts.Card.Liver.Attack;

namespace Rainworld.Scripts.Powers;

public class OldframeTemp: TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Rainworld_Liver_Oldfram>();

    protected override bool IsPositive => false;
}