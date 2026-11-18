using MegaCrit.Sts2.Core.Combat;
 using MegaCrit.Sts2.Core.Commands;
 using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Resource.Card;
using Rainworld.Scripts;
using Rainworld.Scripts.Card;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;



#nullable enable
[RegisterPower]
public sealed class ShamanFormPower : ModPowerTemplate
{
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/Monk/Form128.png",
    BigIconPath: "res://Resource/Powers/Monk/Form128.png"
  );
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;
  
  private List<CardModel> GetOtherSlugcatColorCards()
  {
    var unlockState = Owner.Player.UnlockState;
    var constraint = Owner.Player.RunState.CardMultiplayerConstraint;
    var currentPoolType = Owner.Player.Character.CardPool.GetType();

    var allCards = new List<CardModel>();
    // Tools.AllSlugcatCardPools 不含 Monk 池（Monk 不是颜色池），这里补上，
    // 让「除自己角色外的所有蛞蝓猫牌」包含 Monk；否则 Liver/River/Redcat 永远刷不出 Monk 牌。
    foreach (var poolType in Tools.AllSlugcatCardPools.Concat(new[] { typeof(Rainworld_Monk_CardPool) }).Distinct())
    {
      if (poolType == currentPoolType)
        continue;
      var cards = Tools.GetCardPool(poolType).GetUnlockedCards(unlockState, constraint);
      allCards.AddRange(cards);
    }

    return allCards
      .Where(c => (c.Type == CardType.Attack || c.Type == CardType.Skill)
                  && (c.Rarity == CardRarity.Common || c.Rarity == CardRarity.Uncommon || c.Rarity == CardRarity.Rare)
                  && !RainworldCardTags.IsGenerateBlocked(c))
      .ToList();
  }
  
  
  public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
  {
    modifiedCost = originalCost;
    if (card.Owner.Creature != base.Owner)
    {
      return false;
    }
    if (!IsOtherSlugcatColor(card))
    {
      return false;
    }
    bool flag;
    switch (card.Pile?.Type)
    {
      case PileType.Hand:
      case PileType.Play:
        flag = true;
        break;
      default:
        flag = false;
        break;
    }
    if (!flag)
    {
      return false;
    }
    modifiedCost = default(decimal);
    return true;
  }
  public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
  {
    if (player == base.Owner.Player && base.AmountOnTurnStart >= 1)
    {
      Flash();
      var otherCards = GetOtherSlugcatColorCards();
      if (otherCards.Count == 0)
        return;
      IEnumerable<CardModel> distinctForCombat = CardFactory.GetDistinctForCombat(
        base.Owner.Player, otherCards, base.AmountOnTurnStart, 
        base.Owner.Player.RunState.Rng.CombatCardGeneration);
      foreach (CardModel c in distinctForCombat)
      {
        //CardCmd.Upgrade(c);
        await CardPileCmd.AddGeneratedCardToCombat(c, PileType.Hand, Owner.Player);
      }
    }
  }
  

  /// <summary>
  /// 是否为「其它蛞蝓猫颜色的牌」：颜色属于蛞蝓猫颜色卡池、且不是当前玩家自己颜色的牌。
  /// 彩虹（彩色）牌视为包含所有蛞蝓猫颜色，与戮力同心 / 云翳的口径一致。
  /// 注意不能按卡牌基类判断：Monk 自己的卡池不在 Tools.SlugcatPoolToCardBase 里，
  /// 旧实现按基类判断会让 Monk 打任何牌都得到 false，减费永远不触发。
  /// </summary>
  private bool IsOtherSlugcatColor(CardModel card)
  {
    if (card.Keywords.Contains(RainworldKeywords.Colorful))
      return true;

    var pool = card.VisualCardPool;
    if (pool == null || pool == Owner.Player.Character.CardPool)
      return false;

    return pool is Rainworld_Liver_CardPool or Rainworld_River_CardPool
      or Rainworld_Redcat_CardPool or Rainworld_Monk_CardPool;
  }


}