using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Card.River.Attack;

public class Rainworld_River_Bomb : RiverCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    public override CardAssetProfile AssetProfile => new(
        GetLocalizedCardImage(),
        FramePath: Type switch
        {
            CardType.Attack => "res://Resource/Card/River/CardAtk1024.png",
            CardType.Skill => "res://Resource/Card/River/CardSkill1024.png",
            CardType.Power => "res://Resource/Card/River/CardPower1024.png",
            _ => ""
        }
    );

    private string GetLocalizedCardImage()
    {
        var cardName = GetType().Name.ToLowerInvariant()
            .Replace("slugcat_the_spire_ii_card_", "");
        var locale = TranslationServer.GetLocale(); // 如 "zh", "en"
        string localizedPath = $"res://Resource/Card/River/{cardName}.png";
        if (locale == "ko")
        {
            localizedPath = $"res://Resource/Card/diff/KR/rainworld_liver_bomb.png";
        }

        return ResourceLoader.Exists(localizedPath) 
            ? localizedPath 
            : $"res://Resource/Card/River/{cardName}.png"; // 回退到默认
    }
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9m, ValueProp.Move)
    ];

    public Rainworld_River_Bomb() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Vfx.BombBurstVfx.Play(CombatState!.HittableEnemies);
        await DamageCmd
            .Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this,cardPlay)
            .TargetingAllOpponents(CombatState)
            .WithHitFx(sfx: "event:/Rainworld/sfx/bomb")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}