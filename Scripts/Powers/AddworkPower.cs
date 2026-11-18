using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 加班：每回合开始时获得 <see cref="Amount"/> 点能量，并失去1点力量。
/// </summary>
[RegisterPower]
public sealed class AddworkPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/friendship128.png",
        BigIconPath: "res://Resource/Powers/Monk/friendship128.png"
    );

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
            return;

        Flash();
        await PlayerCmd.GainEnergy(Amount, Owner.Player);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, -1, Owner, null);
    }
}
