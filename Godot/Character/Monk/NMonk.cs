using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Rainworld.Scripts;
using Rainworld.Scripts.Skins;
using SlugcatTheSpireII.Scripts.patches.Singleton;

namespace Rainworld.Godot.Character;

/// <summary>
/// 僧侣（黄猫）的战斗外观。除应用角色选择界面选中的 Spine 皮肤外，
/// 还负责点击角色时的叫声与 meow 动画，行为与其它三只蛞蝓猫保持一致。
/// </summary>
public partial class NMonk : NCreatureVisuals
{
	public override void _Ready()
	{
		base._Ready();
		SetProcessInput(true);

		var creature = GetParent() as NCreature;
		var player = creature?.Entity?.Player;
		// 联机时队友的皮肤要等同步数据到达，ApplyWithRetry 会在数据变化后重新套用
		SlugcatSkins.ApplyWithRetry(this, this, player, () => new[] { SlugcatSkins.GetSelectedSkin(player) });
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
			return;

		var bounds = GetNodeOrNull<Control>("Bounds");
		if (bounds == null)
			return;

		if (!bounds.GetGlobalRect().HasPoint(GetGlobalMousePosition()))
			return;

		var creature = GetParent() as NCreature;
		if (creature == null || creature.Entity.IsDead ||
			creature.Entity.Player?.Character is not Slugcat slugcat ||
			slugcat.SuppressHurtAnim || !RunSingleton.needmeow)
		{
			return;
		}

		if (slugcat.wawawa)
			SfxCmd.Play("event:/Rainworld/sfx/wawawa", 0.7f);
		else
			SfxCmd.Play("event:/Rainworld/sfx/Meow");

		creature.SetAnimationTrigger("Meow");
	}
}
