﻿﻿using Godot;
  using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.relics;
using STS2RitsuLib.Combat.Ui.ExtraCornerAmountLabels;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;


using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
[RegisterPower]
public sealed class AdroitPower : ModPowerTemplate, IPowerExtraIconAmountLabelSpecsProvider
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Resource/Powers/River/{GetType().Name}128.png",
        BigIconPath: $"res://Resource/Powers/River/{GetType().Name}128.png"
    );

    public override PowerType Type => PowerType.Buff;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("retainAmount", 0)
    ];
    public override PowerStackType StackType => PowerStackType.Counter;

    public int retainAmount = 0;
    public int used = 0;
    
    
    public IReadOnlyList<ExtraIconAmountLabelSpec> GetPowerExtraIconAmountLabelSpecs()
    {
        return
        [
            ExtraIconAmountLabelSpec.Plain(
                ExtraIconAmountLabelCorner.BottomRight,
                $"{retainAmount}/{Amount}")
        ];
        
    }
    

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        retainAmount =0;
        DynamicVars["retainAmount"].BaseValue = retainAmount;
        GD.Print($"初始化获得{retainAmount}");
        await base.AfterApplied(applier, cardSource);
        InvokeDisplayAmountChanged();
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier,
        CardModel? cardSource)
    {
        if (power == this && amount > 0)
        {
            retainAmount += (int)amount;
            DynamicVars["retainAmount"].BaseValue = retainAmount;

            InvokeDisplayAmountChanged();

        }
            //

        await base.AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {

        if (Owner == player.Creature)
        {
            if (Owner.HasPower<SavewaterPower>())
                        retainAmount += Amount; 
            else
                        retainAmount = Amount;
                DynamicVars["retainAmount"].BaseValue = retainAmount;
                InvokeDisplayAmountChanged();
            
        }

        

    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource,CardPlay? cardPlay)
    {
        if(cardSource!=null&&cardSource.Type==CardType.Status)
            return 0M;
        if (props == ValueProp.Unpowered || dealer != Owner ||target == Owner)
            return 0;

        if (Owner.HasPower<AdventurePower>())
            return retainAmount * (Owner.GetPower<AdventurePower>()!.Amount + 1);

        return retainAmount;
    }
    

    public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (cardSource != null)
        {
            if (cardSource.Owner.Creature != this.Owner)
                return 0M;
        }
        else if (this.Owner != target)
            return 0M;
        if (Owner.HasPower<AdventurePower>())
            return !props.IsPoweredCardOrMonsterMoveBlock() ? 0M :(retainAmount * (Owner.GetPower<AdventurePower>()!.Amount + 1));
        return !props.IsPoweredCardOrMonsterMoveBlock() ? 0M : ((Decimal) this.retainAmount);
    }

    public async Task ChangeRetainAmount(int amount)
    {
        int old = retainAmount;
        retainAmount += amount;
        if(retainAmount < 0)
            retainAmount = 0;
        if (amount < 0)
        {
            int consumed = old - retainAmount;
            used += consumed;
            if (Owner.Player?.GetRelic<River_Waterball2>() != null)
            {
                Owner.Player.GetRelic<River_Waterball2>()!.OnAdroitConsumed(consumed);
            }

            if (Owner.HasPower<WetPower>())
            {
              await  Owner.GetPower<WetPower>()!.ReduceAdroit(new BlockingPlayerChoiceContext(),consumed);
            }
        }
        DynamicVars["retainAmount"].BaseValue = retainAmount;
        InvokeDisplayAmountChanged();

    }
    

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner.Player
            && !Owner.HasPower<MoisturizerPower>()
            && retainAmount > 0&&!cardPlay.Card.Keywords.Contains(RainworldKeywords.AdroitNotUsed))
        {
           await ChangeRetainAmount(-1);
        }
        InvokeDisplayAmountChanged();

    }

}