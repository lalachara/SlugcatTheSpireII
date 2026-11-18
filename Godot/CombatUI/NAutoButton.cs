using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using Rainworld.Patches;
using Rainworld.Scripts;

namespace Rainworld;

public partial class NAutoButton : NButton
{
	private IHoverTip _hoverTip;
	private LocString workleveltitle = new LocString("settings_ui", "RAINWORLD-Sleep_title");

	// ========== 状态枚举（4种状态） ==========
	public enum ButtonState
	{
		Down,    // 无法点击
		Down1,   // 无法点击
		Down2,   // 无法点击
		Active   // 可以点击
	}

	// ========== 纹理资源（4张图） ==========
	private Texture2D _downTexture = null!;
	private Texture2D _down1Texture = null!;
	private Texture2D _down2Texture = null!;
	private Texture2D _activeTexture = null!;

	// ========== 核心变量 ==========
	private Control _visuals = null!;
	private TextureRect _image = null!;
	private ButtonState _currentState = ButtonState.Active;

	// ========== 不可点击时仍允许悬停，保留提示信息 ==========
	protected override bool AllowFocusWhileDisabled => true;

	// ========== 订阅战斗事件：玩家结束回合后立即变为不可点击 ==========
	public override void _EnterTree()
	{
		base._EnterTree();
		CombatManager.Instance.TurnStarted += OnTurnStarted;
		CombatManager.Instance.PlayerEndedTurn += OnPlayerEndedTurn;
		CombatManager.Instance.PlayerUnendedTurn += OnPlayerUnendedTurn;
	}

	public override void _ExitTree()
	{
		CombatManager.Instance.TurnStarted -= OnTurnStarted;
		CombatManager.Instance.PlayerEndedTurn -= OnPlayerEndedTurn;
		CombatManager.Instance.PlayerUnendedTurn -= OnPlayerUnendedTurn;
		base._ExitTree();
	}

	private void OnTurnStarted(CombatState state)
	{
		if (state.CurrentSide != CombatSide.Player)
			CombatUiPatch.SetSleepUnavailable();
	}

	private void OnPlayerEndedTurn(Player player, bool canBackOut)
	{
		if (LocalContext.IsMe(player))
			CombatUiPatch.SetSleepUnavailable();
	}

	private void OnPlayerUnendedTurn(Player player)
	{
		if (LocalContext.IsMe(player))
			CombatUiPatch.RestoreSleepButton();
	}

	public override void _Ready()
	{
		ConnectSignals();
		
		// 获取节点
		_visuals = GetNode<Control>("Visuals");
		_image = GetNode<TextureRect>("Visuals/Image");

		// 加载4张纹理（替换为你的实际路径）
		_downTexture = GD.Load<Texture2D>("res://Godot/CombatUI/SleepDown.png");
		_down1Texture = GD.Load<Texture2D>("res://Godot/CombatUI/Sleep1.png");
		_down2Texture = GD.Load<Texture2D>("res://Godot/CombatUI/Sleep2.png");
		_activeTexture = GD.Load<Texture2D>("res://Godot/CombatUI/Sleep.png");

		// 初始状态：Active（可点击）
		SetState(ButtonState.Down);
		
		LocString locString = new LocString("settings_ui", "RAINWORLD-Sleep_description");
		_hoverTip = new HoverTip(workleveltitle,locString);
			


	}

	// ========== 外部控制接口：设置按钮状态 ==========
	/// <summary>
	/// 外部调用此方法切换按钮状态
	/// </summary>
	/// <param name="newState">新状态（Down/Down1/Down2/Active）</param>
	public void SetState(ButtonState newState)
	{
		//if (_currentState == newState) return;

		_currentState = newState;
		UpdateButtonTexture();
		UpdateButtonEnabledState();
		
	}

	// ========== 外部控制接口：获取当前状态（可选） ==========
	public ButtonState GetState() => _currentState;

	// ========== 核心逻辑：更新显示图片 ==========
	private void UpdateButtonTexture()
	{
		_image.Texture = _currentState switch
		{
			ButtonState.Down => _downTexture,
			ButtonState.Down1 => _down1Texture,
			ButtonState.Down2 => _down2Texture,
			ButtonState.Active => _activeTexture,
			_ => _activeTexture
		};
	}

	// ========== 核心逻辑：更新按钮可点击状态 ==========
	private void UpdateButtonEnabledState()
	{
		//只有 Active 状态可以点击，其余状态（Down/Down1/Down2）都不可点击
		if (_currentState == ButtonState.Active)
			Enable();
		else
			Disable();
	}

	// ========== 点击逻辑：只有玩家回合内、且尚未结束回合时才能休眠 ==========
	protected override void OnRelease()
	{
		if (_currentState != ButtonState.Active)
			return;
		//玩家已结束回合（或不在玩家回合）时不再响应点击
		if (!CombatUiPatch.IsSleepAvailable())
			return;
		CombatUiPatch.callSleep();
	}



	// ========== 保留：悬停/聚焦框架（无特效，仅保留事件） ==========
	protected override void OnFocus()
	{
		base.OnFocus();

		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, _hoverTip);
		nHoverTipSet.GlobalPosition = base.GlobalPosition + new Vector2(0f, base.Size.Y + 20f);
		// 原特效已删除，如需自定义悬停逻辑可在此添加
	}

	
	protected override void OnUnfocus()
	{
		NHoverTipSet.Remove(this);

	}

	protected override void OnPress()
	{
		base.OnPress();
		// 原特效已删除
	}
}
