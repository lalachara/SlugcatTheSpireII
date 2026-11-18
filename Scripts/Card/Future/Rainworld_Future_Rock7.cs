﻿using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;

namespace Rainworld.Scripts.Card.Future;

public class Rainworld_Future_Rock7 : FutureCardModel
{
    private HashSet<CardPoolModel> _playedColors = new();

    private const int energyCost = 7;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = false;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(7m, ValueProp.Move)
    ];

    public Rainworld_Future_Rock7() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .TargetingAllOpponents(CombatState)
                .WithHitCount(7)
                .Execute(choiceContext);
            
        
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner && cardPlay.Card != this)
            _playedColors.Add(cardPlay.Card.VisualCardPool);

        await base.AfterCardPlayed(choiceContext, cardPlay);
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        if (card == this)
            modifiedCost = Math.Max(0, originalCost - _playedColors.Count * 2);
        else
            modifiedCost = originalCost;
        return true;
    }

    /// <summary>
    /// 克隆时重建颜色集合：MutableClone 是 MemberwiseClone + DeepCloneFields，
    /// 引用字段默认被克隆体与卡组原件共享，结果战斗内打出的颜色会写回卡组、跨战斗残留
    /// （同名牌的多份拷贝也会互相影响）。这里给每个克隆体一份自己的集合。
    /// </summary>
    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _playedColors = new HashSet<CardPoolModel>(_playedColors);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
        AddKeyword(CardKeyword.Exhaust);
    }
}