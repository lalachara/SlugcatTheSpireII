﻿using MegaCrit.Sts2.Core.Entities.Cards;
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
}