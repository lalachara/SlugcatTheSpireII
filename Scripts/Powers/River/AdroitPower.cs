﻿﻿using Godot;
  using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.relics;
using Rainworld.Scripts.Card.River.Attack;
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
public sealed class AdroitPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Resource/Powers/River/{GetType().Name}128.png",
        BigIconPath: $"res://Resource/Powers/River/{GetType().Name}128.png"
    );

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource,CardPlay? cardPlay)
    {
        if(cardSource!=null&&cardSource.Type==CardType.Status)
            return 0M;
        if (props != ValueProp.Move || dealer != Owner ||target == Owner)
            return 0;
        

        if (Owner.HasPower<AdventurePower>())
            return Amount * (Owner.GetPower<AdventurePower>()!.Amount + 1);

        return Amount;
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
            return !props.IsPoweredCardOrMonsterMoveBlock() ? 0M :(Amount * (Owner.GetPower<AdventurePower>()!.Amount + 1));
        return !props.IsPoweredCardOrMonsterMoveBlock() ? 0M : ((Decimal) this.Amount);
    }

    public async Task ChangeRetainAmount(int amount)
    {
        
            if (Owner.Player?.GetRelic<River_Waterball2>() != null)
            {
                Owner.Player.GetRelic<River_Waterball2>()!.OnAdroitConsumed(amount);
            }

            if (Owner.HasPower<WetPower>())
            {
              await  Owner.GetPower<WetPower>()!.ReduceAdroit(new BlockingPlayerChoiceContext(),amount);
            }
            foreach (CardModel card in Owner.Player!.PlayerCombatState!.AllCards)
            {
                if (card is Rainworld_River_Backtosea backtosea)
                    backtosea.DynamicVars.Damage.UpgradeValueBy(backtosea.DynamicVars["Special"].BaseValue);
                
            }

            if (Owner.HasPower<UtubePower>())
            {
                await Owner.GetPower<UtubePower>()!.UsedAdroit(amount);
            }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player)
        {
            if(!Owner.HasPower<SavewaterPower>())
                await PowerCmd.Remove(this);
        }
    }


    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner.Player
            && !Owner.HasPower<MoisturizerPower>()
            && Amount > 0&&!cardPlay.Card.Keywords.Contains(RainworldKeywords.AdroitNotUsed))
        {
           await PowerCmd.Decrement(this);
           await ChangeRetainAmount(1);
        }
        InvokeDisplayAmountChanged();

    }

}