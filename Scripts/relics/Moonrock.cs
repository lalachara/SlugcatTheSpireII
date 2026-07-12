using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class Moonrock : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Relics/MoonRock.png",
        IconOutlinePath: "res://Resource/Relics/outline/MoonRock.png",
        BigIconPath: "res://Resource/Relics/MoonRock.png"
    );
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1)
    ];
	
    public override Decimal ModifyMaxEnergy(Player player, Decimal amount)
    {
        return player != this.Owner ? amount : amount + (Decimal) this.DynamicVars.Energy.IntValue;
    }
    public override async Task AfterObtained()
    {
        await CreatureCmd.LoseMaxHp(new BlockingPlayerChoiceContext(),Owner.Creature,Owner.Creature.MaxHp * 0.3m,false);
    }


}