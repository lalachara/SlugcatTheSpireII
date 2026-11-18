using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Rainworld.relics;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using Rainworld.Scripts;
using Rainworld.Scripts.Skins;
using SlugcatTheSpireII.Scripts.patches.Singleton;

public partial class NRiver : NCreatureVisuals
{
	private string _hatSkin = string.Empty;
	private string _itemSkin = SlugcatSkins.RiverDefaultItemSkin;

	public override void _Ready()
	{
		base._Ready();

		SetProcessInput(true);

		var nc = GetParent() as NCreature;
		var player = nc?.Entity?.IsPlayer == true ? nc.Entity.Player : null;

		// 角色选择界面选的只是帽子那套（normal/0、normal/1），联机时读该玩家同步过来的选择。
		_hatSkin = SlugcatSkins.GetSelectedSkin(player);
		if (string.IsNullOrEmpty(_hatSkin))
			_hatSkin = SlugcatSkins.GetDefaultSkin(typeof(Rainworld_River));

		// 手中道具单独一套：电池遗物优先，否则默认拿水球。
		var battery = player?.Relics.OfType<Battery>().FirstOrDefault();
		_itemSkin = battery == null
			? SlugcatSkins.RiverDefaultItemSkin
			: battery.WasUsed ? SlugcatSkins.RiverBatteryEmptyItemSkin : SlugcatSkins.RiverBatteryItemSkin;

		ApplyCurrentSkin();
	}

	/// <summary>只切换手中道具皮肤（电池用尽时调用），帽子、身体等其它部件保持不变。</summary>
	public void SetItemSkin(string itemSkin)
	{
		_itemSkin = itemSkin;
		ApplyCurrentSkin();
	}

	private void ApplyCurrentSkin()
	{
		// 帽子与手中道具属于不同槽位，必须合并后一起应用，单套 SetSkin 会把另一套隐藏。
		// 联机时队友的帽子皮肤要等同步数据到达，交给 ApplyWithRetry 在数据变化后重套。
		var player = (GetParent() as NCreature)?.Entity?.Player;
		SlugcatSkins.ApplyWithRetry(this, this, player, () =>
		{
			var hat = SlugcatSkins.GetSelectedSkin(player);
			if (string.IsNullOrEmpty(hat))
				hat = SlugcatSkins.GetDefaultSkin(typeof(Rainworld_River));
			_hatSkin = hat;
			return new[] { hat, _itemSkin };
		});
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
		{
			var bounds = GetNodeOrNull<Control>("Bounds");
			if (bounds == null)
			{
				return;
			}

			var mousePos = GetGlobalMousePosition();
			var rect = bounds.GetGlobalRect();

			if (rect.HasPoint(mousePos))
			{
				var creature = GetParent() as NCreature;
				if (creature == null|| creature.Entity.IsDead||creature.Entity.Player?.Character is not Slugcat slugcat||slugcat.SuppressHurtAnim||!RunSingleton.needmeow)
				{
					return;
				}
				if(slugcat.wawawa)
					SfxCmd.Play("event:/Rainworld/sfx/wawawa",0.7f);
				else
					SfxCmd.Play("event:/Rainworld/sfx/MeowRiver");

				creature.SetAnimationTrigger("Meow");
			}
		}
	}
}
