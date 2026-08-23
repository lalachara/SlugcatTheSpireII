using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable
[RegisterPower]
public sealed class RandomslimePower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Randomslime128.png",
        BigIconPath: "res://Resource/Powers/Randomslime128.png"
    );

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[]
        { HoverTipFactory.FromCard<Slimed>() };

    public bool IsUpgraded = false;
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == base.Owner.Player && cardPlay.Card is Slimed)
        {
            Flash();
            var card = CardFactory.GetDistinctForCombat(base.Owner.Player,
                from c in base.Owner.Player.Character.CardPool.GetUnlockedCards(
                    base.Owner.Player.UnlockState,
                    base.Owner.Player.RunState.CardMultiplayerConstraint)
                where (c.Type == CardType.Attack || c.Type == CardType.Skill || c.Type == CardType.Status || c.Type == CardType.Power)
                      && (c.Rarity == CardRarity.Common || c.Rarity == CardRarity.Uncommon || c.Rarity == CardRarity.Rare || c.Rarity == CardRarity.Status || c.Rarity == CardRarity.Ancient)
                select c, 1, base.Owner.Player.RunState.Rng.CombatCardGeneration).First();
            if (IsUpgraded&&card.IsUpgradable)
                CardCmd.Upgrade(card);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, base.Owner.Player);
        }
    }
}