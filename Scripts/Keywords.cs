using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace Rainworld.Scripts;

[RegisterOwnedCardKeyword(nameof(Spear), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(nameof(Treasure), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(nameof(Treasurespear), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(nameof(Corrupt), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(nameof(AdroitNotUsed), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(nameof(Immunity), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(nameof(Simulated), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(nameof(Foodcard), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(nameof(Colorful), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(nameof(Strange), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(nameof(Lightning))]
[RegisterOwnedCardKeyword(nameof(Food))]
[RegisterOwnedCardKeyword(nameof(Worklevel))]
[RegisterOwnedCardKeyword(nameof(CatStrike))]
[RegisterOwnedCardKeyword(nameof(Workreport))]
[RegisterOwnedCardKeyword(nameof(Backtohand))]
[RegisterOwnedCardKeyword(nameof(Last))]
[RegisterOwnedCardKeyword(nameof(Cantcorrupt))]
[RegisterOwnedCardKeyword(nameof(Companion))]
[RegisterOwnedCardKeyword(nameof(Companioncab))]
[RegisterOwnedCardKeyword(nameof(Greencab))]
[RegisterOwnedCardKeyword(nameof(Bluecab))]
[RegisterOwnedCardKeyword(nameof(Goldcab))]
[RegisterOwnedCardKeyword(nameof(Firecab))]
[RegisterOwnedCardKeyword(nameof(Sugarcab))]
[RegisterOwnedCardKeyword(nameof(Kitty))]
[RegisterOwnedCardKeyword(nameof(Monkey))]
[RegisterOwnedCardKeyword(nameof(Rotcab))]
[RegisterOwnedCardKeyword(nameof(First))]



public class RainworldKeywords
{
	public static readonly CardKeyword Spear = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Spear)).GetModCardKeyword();
	public static readonly CardKeyword Treasure = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Treasure)).GetModCardKeyword();
	public static readonly CardKeyword Treasurespear = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Treasurespear)).GetModCardKeyword();
	public static readonly CardKeyword Corrupt = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Corrupt)).GetModCardKeyword();
	public static readonly CardKeyword Food = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Food)).GetModCardKeyword();
	public static readonly CardKeyword Worklevel = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Worklevel)).GetModCardKeyword();
	public static readonly CardKeyword CatStrike = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(CatStrike)).GetModCardKeyword();
	public static readonly CardKeyword AdroitNotUsed = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(AdroitNotUsed)).GetModCardKeyword();
	public static readonly CardKeyword Lightning = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Lightning)).GetModCardKeyword();
	public static readonly CardKeyword Immunity = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Immunity)).GetModCardKeyword();
	public static readonly CardKeyword Workreport = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Workreport)).GetModCardKeyword();
	public static readonly CardKeyword Backtohand = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Backtohand)).GetModCardKeyword();
	public static readonly CardKeyword Simulated = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Simulated)).GetModCardKeyword();
	public static readonly CardKeyword Last = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Last)).GetModCardKeyword();
	public static readonly CardKeyword Cantcorrupt = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Cantcorrupt)).GetModCardKeyword();
	public static readonly CardKeyword Foodcard = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Foodcard)).GetModCardKeyword();
	public static readonly CardKeyword Colorful = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Colorful)).GetModCardKeyword();
	public static readonly CardKeyword Strange = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Strange)).GetModCardKeyword();
	public static readonly CardKeyword Companion = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Companion)).GetModCardKeyword();
	public static readonly CardKeyword Companioncab = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Companioncab)).GetModCardKeyword();
	public static readonly CardKeyword Greencab = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Greencab)).GetModCardKeyword();
	public static readonly CardKeyword Bluecab = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Bluecab)).GetModCardKeyword();
	public static readonly CardKeyword Goldcab = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Goldcab)).GetModCardKeyword();
	public static readonly CardKeyword Firecab = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Firecab)).GetModCardKeyword();
	public static readonly CardKeyword Sugarcab = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Sugarcab)).GetModCardKeyword();
	public static readonly CardKeyword Kitty = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Kitty)).GetModCardKeyword();
	public static readonly CardKeyword Monkey = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Monkey)).GetModCardKeyword();
	public static readonly CardKeyword Rotcab = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Rotcab)).GetModCardKeyword();
	public static readonly CardKeyword First = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(First)).GetModCardKeyword();
}