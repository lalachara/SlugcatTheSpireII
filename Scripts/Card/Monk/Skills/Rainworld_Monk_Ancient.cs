using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Ancient : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public Rainworld_Monk_Ancient() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 从「所有角色」的卡池里收集先古卡（不再只看自己角色的池子）
        var ancientCards = ModelDb.AllCardPools
            .SelectMany(pool => pool.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint))
            .Where(c => c.Rarity == CardRarity.Ancient && !RainworldCardTags.IsGenerateBlocked(c))
            .ToList();
        if (ancientCards.Count > 0)
        {
            var picked = ancientCards[Owner.RunState.Rng.CombatCardGeneration.NextInt(0, ancientCards.Count)];

            // 优先走「去重」工厂；当前卡池没有可生成结果时（例如候选被过滤光）会返回空序列，
            // 以前这里直接 .First() 会抛 InvalidOperationException，卡牌动作异常后整局卡死。
            var card = CardFactory
                .GetDistinctForCombat(Owner, new[] { picked }, 1, Owner.RunState.Rng.CombatCardGeneration)
                .FirstOrDefault()
                ?? CombatState!.CreateCard(picked, Owner);
            if (IsUpgraded)
                CardCmd.Upgrade(card);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade()
    {
    }
}