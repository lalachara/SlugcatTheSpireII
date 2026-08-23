using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Resource.Card;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Card.Liver.Attack;
[RegisterCard(typeof(Rainworld_Future_CardPool), Inherit = true)]

public class Rainworld_Redcat_Smallcent : ModCardTemplate

{
    
    public override CardAssetProfile AssetProfile => new(
        $"res://Resource/Card/RedCat/{GetType().Name.ToLowerInvariant().Replace("slugcat_the_spire_ii_card_", "")}.png",
        
        FramePath: Type switch
        {
            CardType.Attack => "res://Resource/Card/RedCat/CardAtk1024.png",
            CardType.Skill => "res://Resource/Card/RedCat/CardSkill1024.png",
            CardType.Power => "res://Resource/Card/RedCat/CardPower1024.png",
            _ => ""
        }
        
    );
    // 基础耗能
    private const int energyCost = 0;

    // 卡牌类型
    private const CardType type = CardType.Skill;

    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Token;

    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;

    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    protected override HashSet<CardTag> CanonicalTags => new() { };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust ,RainworldKeywords.Foodcard };
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Food)  };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(1)
    ];
    public override TargetType TargetType
    {
        get => !this.HasSharedFood ? TargetType.Self : TargetType.AllAllies;
    }
    private bool HasSharedFood
    {
        get => this.IsMutable&& this.Owner!=null  && this.Owner.Creature.HasPower<SharefoodPower>();
    }

    public Rainworld_Redcat_Smallcent() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (HasSharedFood)
        {
            foreach (var player in CombatState!.Players)
            {
                if (player.Character is Slugcat)
                {
                    await PowerCmd.Apply<StrengthPower>(choiceContext, player.Creature, DynamicVars.Strength.BaseValue,
                        player.Creature, this);
                    await SlugcatData.addfood(player, 1);
                }
            }
        }
        else
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars.Strength.BaseValue,
                        Owner.Creature, this);
            await SlugcatData.addfood(Owner, 1);
        }
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars.Strength.UpgradeValueBy(1);
    }
}