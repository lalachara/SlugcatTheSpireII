using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace demo.Scripts.Powers;

public class OldframeTemp: TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Power<OldframeTemp>();

    protected override bool IsPositive => false;
}