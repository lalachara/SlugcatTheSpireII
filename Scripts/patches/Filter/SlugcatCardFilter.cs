using System;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using Rainworld.Scripts.Card;

namespace Rainworld.Scripts.patches.Filter;

/// <summary>
/// 「非蛞蝓猫角色检索其它角色卡牌时排除蛞蝓猫卡」的共享判断逻辑，供飞溅 / 棱镜宝石两个补丁复用。
/// </summary>
internal static class SlugcatCardFilter
{
	/// <summary>该卡牌是否属于某个蛞蝓猫角色主池（Liver / River / RedCat）。</summary>
	public static bool IsSlugcatCard(CardModel card) =>
		card is LiverCardModel or RiverCardModel or RedCatCardModel;

	/// <summary>该卡池是否属于某个蛞蝓猫角色（仅角色主池，不含 Future 共享池）。</summary>
	public static bool IsSlugcatPool(CardPoolModel pool) =>
		ModelDb.AllCharacters.Any(c => c is Slugcat && c.CardPool == pool);

	/// <summary>
	/// 读取「检索其它角色卡排除蛞蝓猫卡」开关：多人以宿主机写入的局内数据为准，单机读本地配置。
	/// </summary>
	public static bool IsConfigEnabled(Player player)
	{
		bool isMultiplayer = RunManager.Instance?.NetService?.Type.IsMultiplayer() ?? false;
		if (isMultiplayer)
		{
			if (player.RunState is RunState rs)
			{
				try
				{
					return SlugcatResources.OtherCharPoolFilterData.Get(rs).EnableExcludeSlugcatFromOtherCharPool;
				}
				catch (Exception ex)
				{
					Entry.Logger.Warn($"[角色卡池过滤] 读取局内保存数据失败: {ex.Message}");
				}
			}
		}
		return RainworldModConfig.EnableExcludeSlugcatFromOtherCharPool;
	}
}
