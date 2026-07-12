using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Card.Colorless;

[RegisterCard(typeof(TokenCardPool))]
public class Rainworld_Turtal : ModCardTemplate
{
    private const int energyCost = -2;
    private const CardType type = CardType.Status;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        $"res://Resource/Card/other/{GetType().Name.ToLowerInvariant().Replace("slugcat_the_spire_ii_card_", "")}.png"
        

        // 卡框等，有需求自己添加。需要自行判断卡牌类型（攻击、技能、能力等）设置，建议写在基类里。
        // 如果使用自定义卡池，需要改下material，看添加人物章节的添加卡池部分
        // FramePath: "", // 卡牌背景
        // PortraitBorderPath: "", // 边框（状态牌感染使用的）
        // BannerTexturePath: "" // 横幅（不同类型）
    );
    
    public Rainworld_Turtal() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 抽到时自动打出
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card == this)
        {
            await Cmd.Wait(0.25f);
            await CardCmd.AutoPlay(choiceContext, card, null);
            SfxCmd.Play("event:/Rainworld/sfx/turtle");
        }
    }

    // 随机丢弃1张手牌
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = PileType.Hand.GetPile(Owner);
        var cardModel = Owner.RunState.Rng.Shuffle.NextItem(hand.Cards);
        if (cardModel != null)
            await CardCmd.Discard(choiceContext, cardModel);
    }

    public override int MaxUpgradeLevel => 0;

    protected override void OnUpgrade()
    {
    }
}