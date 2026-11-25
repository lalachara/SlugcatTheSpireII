using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;


using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
[RegisterPower]
public sealed class SeasoundPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Resource/Powers/River/{GetType().Name}128.png",
        BigIconPath: $"res://Resource/Powers/River/{GetType().Name}128.png"
    );

    public override PowerType Type => PowerType.Buff;

    /// <summary>层数 = 每累计给予 10 层水蚀时获得的伶俐上限数。</summary>
    public override PowerStackType StackType => PowerStackType.Counter;

    private int storedAmount = 0;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<AdroitPower>() ,HoverTipFactory.FromPower<AdroitcapPower>() };


    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (Owner.HasPower<CorrodePower>())
        {
            return;
        }
        await PowerCmd.Apply<AdroitPower>(new BlockingPlayerChoiceContext(), Owner, 1, Owner, null);

    }

    public async Task OnCorrodeApplied(PlayerChoiceContext choiceContext, int corrodeAmount)
    {
        // 每累计给予 10 层水蚀，获得「层数」层伶俐上限（堆叠只影响这里，水蚀下限永远是 1）
        int reward = (int)Math.Max(1m, Amount);

        for (int i = 0; i < corrodeAmount; i++)
        {
            storedAmount++;
            if (storedAmount >= 10)
            {
                storedAmount = 0;
                await PowerCmd.Apply<AdroitcapPower>(choiceContext, Owner, reward, Owner, null);
            }
        }
    }
}