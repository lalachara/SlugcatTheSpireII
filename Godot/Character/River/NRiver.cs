using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Rainworld.relics;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using Rainworld.Scripts;

public partial class NRiver : NCreatureVisuals
{
	private MegaSkeleton _megaSkeleton;
	private string _pendingSkin;
	private int _retryCount;

	public override void _Ready()
	{
		base._Ready();

		SetProcessInput(true);

		string skinName = "shuiguo";
		var nc = GetParent() as NCreature;
		if (nc?.Entity?.IsPlayer == true)
		{
			var battery = nc.Entity.Player.Relics.OfType<Battery>().FirstOrDefault();
			if (battery != null)
			{
				skinName = battery.WasUsed ? "dianchi2" : "dianchi";
			}
		}

		SetSkin(skinName);
	}

	public void SetSkin(string skinName)
	{
		if (_megaSkeleton != null)
		{
			ApplySkin(skinName);
			return;
		}
		_pendingSkin = skinName;
		_retryCount = 0;
		CallDeferred("TryApplySkin");
	}

	private void TryApplySkin()
	{
		if (_pendingSkin == null)
			return;

		_retryCount++;
		if (_retryCount > 60)
		{
			_pendingSkin = null;
			return;
		}

		Node visuals = GetNode("Visuals");
		if (visuals == null)
		{
			_pendingSkin = null;
			return;
		}

		Variant skeletonVariant = visuals.Call("get_skeleton");
		if (skeletonVariant.VariantType == Variant.Type.Nil || skeletonVariant.AsGodotObject() == null)
		{
			CallDeferred("TryApplySkin");
			return;
		}

		_megaSkeleton = new MegaSkeleton(skeletonVariant);
		ApplySkin(_pendingSkin);
		_pendingSkin = null;
	}

	private void ApplySkin(string skinName)
	{
		_megaSkeleton.SetSkinByName(skinName);
		_megaSkeleton.SetSlotsToSetupPose();
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
				if (creature == null||creature.Entity.Player?.Character is not Slugcat slugcat||slugcat.SuppressHurtAnim)
				{
					return;
				}
				if(slugcat.wawawa)
					SfxCmd.Play("event:/Rainworld/sfx/wawawa");
				else
					SfxCmd.Play("event:/Rainworld/sfx/MeowRiver");

				creature.SetAnimationTrigger("Meow");
			}
		}
	}
}