using Godot;
using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using Rainworld.Patches;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Combat.SecondaryResources;

// 完全抄你的：继承 NButton
public partial class WorkLevelIndicator : NButton
{
	[Export] private string BaseTexturePath = "res://Godot/CombatUI/resource/WorkLevel.png";
	[Export] private string LockTexturePath = "res://Godot/CombatUI/resource/WorkLevelLock.png";
	[Export] private string MushroomTexturePath = "res://Godot/CombatUI/resource/WorkLevelLock2.png";

	[Export] private string FoodFullPath = "res://Godot/CombatUI/resource/FoodPointFull.png";
	[Export] private string FoodEmptyPath = "res://Godot/CombatUI/resource/FoodPointEmpty.png";
	[Export] private string FoodDividerPath = "res://Godot/CombatUI/resource/FoodPointCut.png";

	// 【完全抄你的】字段命名
	private Sprite2D _workLevelSprite;
	private Control _visuals;

	private const int SQUARE_SIZE = 130;
	private const float ANIMATION_DURATION = 0.5f;

	private Texture2D _baseTexture;
	private Texture2D _lockTexture;
	private Texture2D _mushroomTexture;
	private int _currentWorkLevel = 0;
	private int _maxLevel = 0;
	private Tween? _currentTween;

	private Texture2D _foodFull;
	private Texture2D _foodEmpty;
	private Texture2D _foodDivider;
	private HBoxContainer _foodContainer;
	private List<TextureRect> _foodIcons = new List<TextureRect>();
	private int _maxFood;
	private int _sleepFood;
	private int _currentFood;
	// 【完全抄你的】提示字段
	private IHoverTip _hoverTip,_hoverTipFood;
	private LocString workleveltitle = new LocString("settings_ui", "RAINWORLD-WorkLevel_title");
	private LocString foodtitle = new LocString("settings_ui", "RAINWORLD-Food_title");
	private LocString fooddes ;
	private HoverTip FoodHoverTip;

	public override void _Ready()
	{
			
			ConnectSignals();
			_visuals = GetNodeOrNull<Control>("Visuals");
			_workLevelSprite = GetNodeOrNull<Sprite2D>("Visuals/WorkLevelSprite");
			_baseTexture = GD.Load<Texture2D>(BaseTexturePath);
			_lockTexture = GD.Load<Texture2D>(LockTexturePath);
			_mushroomTexture = GD.Load<Texture2D>(MushroomTexturePath);
			_maxLevel = (int)(_baseTexture.GetHeight() / SQUARE_SIZE) - 1;
			_workLevelSprite.Texture = GetCurrentTexture();
			_workLevelSprite.Modulate = GetSpriteModulate();
			_workLevelSprite.RegionEnabled = true;
			_workLevelSprite.Centered = false;
			_workLevelSprite.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
			UpdateSpriteUV(_currentWorkLevel);

			InitFoodSystem();

			// 动态更新数值


	}

	protected override void OnFocus()
	{
		// 先给默认值：HoverTip(title, description) 会直接读 description.LocTable，
		// 传 null 会抛 NullReferenceException 并中断整个 OnFocus（连业力提示都不会显示）。
		// 未单独列出的蛞蝓猫统一用通用食物说明兜底。
		fooddes = new LocString("settings_ui", "RAINWORLD-Food_description");
		if (CombatUiPatch.creature != null && CombatUiPatch.creature.Player?.Character is Slugcat slugcat)
		{
			if(slugcat is Rainworld_Liver)
				fooddes = new LocString("settings_ui", "RAINWORLD-Food_Liver_description");
			else if(slugcat is Rainworld_Redcat)
			{
				fooddes = new LocString("settings_ui", "RAINWORLD-Food_RedCat_description");
			}
			else if(slugcat is Rainworld_River)
			{
				fooddes = new LocString("settings_ui", "RAINWORLD-Food_River_description");
			}
			else if(slugcat is Rainworld_Monk)
			{
				fooddes = new LocString("settings_ui", "RAINWORLD-Food_Monk_description");
			}
		}
		FoodHoverTip =  new HoverTip(foodtitle,fooddes);

		LocString locString = new LocString("settings_ui", "RAINWORLD-WorkLevel_description");
		locString.Add("work_level", SlugcatData.GetWorkLevel(CombatUiPatch.creature.Player)+1);
		locString.Add("max_work_level", SlugcatData.GetMaxWorkLevel(CombatUiPatch.creature.Player)+1);
		_hoverTip = new HoverTip(workleveltitle, locString);
		
		IEnumerable<IHoverTip> hoverTips = new []{_hoverTip,FoodHoverTip};
		
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, hoverTips);
		nHoverTipSet.GlobalPosition = base.GlobalPosition + new Vector2(base.Size.X+60f, base.Size.Y -160f);
	}
	private void UpdateButtonEnabledState()
	{
		Enable();
		
	}
	protected override void OnUnfocus()
	{
		// 【完全抄你的】移除提示
		NHoverTipSet.Remove(this);
	}

	// 【完全抄你的】点击事件
	protected override void OnPress()
	{
		base.OnPress();
	}

	protected override void OnRelease()
	{
		// 空实现，不可点击
	}

	// ====================== 以下所有功能代码 完全保留 ======================
	private void InitFoodSystem()
	{
		_foodFull = GD.Load<Texture2D>(FoodFullPath);
		_foodEmpty = GD.Load<Texture2D>(FoodEmptyPath);
		_foodDivider = GD.Load<Texture2D>(FoodDividerPath);

		_foodContainer = new HBoxContainer();
		_foodContainer.Name = "FoodContainer";
		_foodContainer.SetAnchorsPreset(LayoutPreset.CenterBottom);
		_foodContainer.OffsetTop = 20;
		_foodContainer.OffsetLeft = -50; 
		_foodContainer.Alignment = BoxContainer.AlignmentMode.Center;
		
		AddChild(_foodContainer);
	}

	public void SetFoodData(int maxFood, int sleepFood, int currentFood)
	{
		_maxFood = maxFood;
		_sleepFood = sleepFood;
		_currentFood = currentFood;
		RebuildFoodIcons();
	}

	public void SetCurrentFood(int value)
	{
		_currentFood = value;
		RefreshFoodVisuals();
	}

	public void SetSleepFood(int value)
	{
		_sleepFood = value;
		RefreshFoodVisuals();
	}
	public override void _Process(double delta)
	{
		_workLevelSprite.Texture = GetCurrentTexture();
		_workLevelSprite.Modulate = GetSpriteModulate();
	}

	private Color GetSpriteModulate()
	{
		return HasRedMushroomBuff() ? new Color(1, 0, 0, 1) : Colors.White;
	}
	
	private void RebuildFoodIcons()
	{
		foreach (var c in _foodIcons) c.QueueFree();
		_foodIcons.Clear();

		for (int i = 0; i < _maxFood; i++)
		{
			var tr = new TextureRect();
			tr.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
			tr.Size = new Vector2(24, 24);
			_foodContainer.AddChild(tr);
			_foodIcons.Add(tr);

			var divider = new TextureRect();
			divider.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
			divider.Size = new Vector2(16, 24);
			divider.Visible = false;
			_foodContainer.AddChild(divider);
			_foodIcons.Add(divider);
		}

		RefreshFoodVisuals();
	}

	private void RefreshFoodVisuals()
	{
		for (int i = 0; i < _maxFood; i++)
		{
			int iconIdx = i * 2;
			int divIdx = i * 2 + 1;

			var icon = _foodIcons[iconIdx];
			icon.Texture = i < _currentFood ? _foodFull : _foodEmpty;

			var div = _foodIcons[divIdx];
			div.Visible = (i == _sleepFood - 1);
			div.Texture = _foodDivider;
		}
	}

	private float CalculateIconY(int level)
	{
		return (_maxLevel - level) * SQUARE_SIZE;
	}

	private void UpdateSpriteUV(int level)
	{
		float y = CalculateIconY(level);
		_workLevelSprite.RegionRect = new Rect2(0, y, SQUARE_SIZE, SQUARE_SIZE);
	}

	private void SetIconY(float y)
	{
		_workLevelSprite.RegionRect = new Rect2(0, y, SQUARE_SIZE, SQUARE_SIZE);
	}

	private Texture2D GetCurrentTexture()
	{
		if (HasRedMushroomBuff() && _mushroomTexture != null) return _mushroomTexture;
		if (HasWorkLevelLockBuff() && _lockTexture != null) return _lockTexture;
		return _baseTexture;
	}

	private bool HasRedMushroomBuff() => (CombatUiPatch.creature?.HasPower<WorkerrorPower>()??false)||((CombatUiPatch.creature?.HasPower<RedmushroomPower>()??false)&&!CombatUiPatch.creature.HasPower<RefusePower>());
	private bool HasWorkLevelLockBuff() => CombatUiPatch.creature?.HasPower<WorklockPower>() ?? false;

	public void UpdateWorkLevel(int newLevel)
	{
		if (_workLevelSprite == null || _workLevelSprite.Texture == null) return;

		int targetLevel = Math.Clamp(newLevel, 0, _maxLevel);
		if (targetLevel == _currentWorkLevel) return;

		_currentTween?.Kill();
		float startY = CalculateIconY(_currentWorkLevel);
		float targetY = CalculateIconY(targetLevel);

		_currentTween = CreateTween();
		_currentTween.SetEase(Tween.EaseType.Out);
		_currentTween.SetTrans(Tween.TransitionType.Expo);
		_currentTween.TweenMethod(Callable.From<float>(SetIconY), startY, targetY, ANIMATION_DURATION);

		_currentWorkLevel = targetLevel;
	}

	public override void _ExitTree()
	{
		_baseTexture?.Dispose();
		_lockTexture?.Dispose();
		_mushroomTexture?.Dispose();
		_foodFull?.Dispose();
		_foodEmpty?.Dispose();
		_foodDivider?.Dispose();
		_currentTween?.Kill();
		base._ExitTree();
	}
}
