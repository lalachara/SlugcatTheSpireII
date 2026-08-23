using Godot;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib;
using STS2RitsuLib.Data;
using STS2RitsuLib.Settings;
using STS2RitsuLib.Utils.Persistence;
using MegaCrit.Sts2.Core.Localization;

namespace Rainworld.Scripts;

/// <summary>
/// 模组内容（怪物/事件）的出现模式。
/// </summary>
public enum MonsterSpawnMode
{
	/// <summary>始终出现（默认，等同旧的「启用」）。</summary>
	Always = 0,

	/// <summary>仅当对局中包含蛞蝓猫角色时才出现。</summary>
	OnlySlugcat = 1,

	/// <summary>完全禁用，不出现。</summary>
	Disabled = 2,
}

public sealed class RainworldModSettings
{
	public bool EnableCombatCantLossMaxHp { get; set; } = false;
	public bool EnableMaxHpRelicFix { get; set; } = false;
	public bool EnableHpRelicFix { get; set; } = false;

	public bool EnableModAncientFix { get; set; } = true;

	/// <summary>非蛞蝓猫角色使用原版「检索其它角色卡牌」机制时，排除蛞蝓猫角色的卡。</summary>
	public bool EnableExcludeSlugcatFromOtherCharPool { get; set; } = false;

	/// <summary>怪物区三态总控：始终 / 仅蛞蝓猫 / 禁用。为 Always 时跟随各怪物明细。</summary>
	public MonsterSpawnMode MonsterMasterSpawnMode { get; set; } = MonsterSpawnMode.Always;

	public MonsterSpawnMode MushroomMonsterSpawnMode { get; set; } = MonsterSpawnMode.Always;
	public MonsterSpawnMode IronbirdMonsterSpawnMode { get; set; } = MonsterSpawnMode.Always;
	public MonsterSpawnMode FirecabMonsterSpawnMode { get; set; } = MonsterSpawnMode.Always;
	public MonsterSpawnMode FirecentMonsterSpawnMode { get; set; } = MonsterSpawnMode.Always;
	public MonsterSpawnMode WatercentMonsterSpawnMode { get; set; } = MonsterSpawnMode.Always;
	public MonsterSpawnMode GreencabMonsterSpawnMode { get; set; } = MonsterSpawnMode.Always;

	/// <summary>事件区三态总控：始终 / 仅蛞蝓猫 / 禁用。为 Always 时跟随各事件明细。</summary>
	public MonsterSpawnMode EventMasterSpawnMode { get; set; } = MonsterSpawnMode.Always;

	public MonsterSpawnMode RaindeerEventSpawnMode { get; set; } = MonsterSpawnMode.Always;
	public MonsterSpawnMode WatcherEventSpawnMode { get; set; } = MonsterSpawnMode.Always;
}

public static class RainworldModConfig
{
	private const string DataKey = "mod_settings";

	private static readonly ModSettingsValueBinding<RainworldModSettings, bool> EnableCombatCantLossMaxHpBinding = new(
		Entry.ModId, DataKey, SaveScope.Global,
		static s => s.EnableCombatCantLossMaxHp,
		static (s, v) => s.EnableCombatCantLossMaxHp = v);

	private static readonly ModSettingsValueBinding<RainworldModSettings, bool> EnableMaxHpRelicFixBinding = new(
		Entry.ModId, DataKey, SaveScope.Global,
		static s => s.EnableMaxHpRelicFix,
		static (s, v) => s.EnableMaxHpRelicFix = v);

	private static readonly ModSettingsValueBinding<RainworldModSettings, bool> EnableHpRelicFixBinding = new(
		Entry.ModId, DataKey, SaveScope.Global,
		static s => s.EnableHpRelicFix,
		static (s, v) => s.EnableHpRelicFix = v);

	private static readonly ModSettingsValueBinding<RainworldModSettings, bool> EnableModAncientFixBinding = new(
		Entry.ModId, DataKey, SaveScope.Global,
		static s => s.EnableModAncientFix,
		static (s, v) => s.EnableModAncientFix = v);

	/// <summary>非蛞蝓猫角色使用原版「检索其它角色卡」机制时，排除蛞蝓猫角色的卡。</summary>
	private static readonly ModSettingsValueBinding<RainworldModSettings, bool> EnableExcludeSlugcatFromOtherCharPoolBinding = new(
		Entry.ModId, DataKey, SaveScope.Global,
		static s => s.EnableExcludeSlugcatFromOtherCharPool,
		static (s, v) => s.EnableExcludeSlugcatFromOtherCharPool = v);

	// 怪物明细绑定（先声明，供总控绑定引用）。
	private static readonly ModSettingsValueBinding<RainworldModSettings, MonsterSpawnMode> MushroomMonsterSpawnModeBinding = new(
		Entry.ModId, DataKey, SaveScope.Global,
		static s => s.MushroomMonsterSpawnMode,
		static (s, v) => s.MushroomMonsterSpawnMode = v);

	private static readonly ModSettingsValueBinding<RainworldModSettings, MonsterSpawnMode> IronbirdMonsterSpawnModeBinding = new(
		Entry.ModId, DataKey, SaveScope.Global,
		static s => s.IronbirdMonsterSpawnMode,
		static (s, v) => s.IronbirdMonsterSpawnMode = v);

	private static readonly ModSettingsValueBinding<RainworldModSettings, MonsterSpawnMode> FirecabMonsterSpawnModeBinding = new(
		Entry.ModId, DataKey, SaveScope.Global,
		static s => s.FirecabMonsterSpawnMode,
		static (s, v) => s.FirecabMonsterSpawnMode = v);

	private static readonly ModSettingsValueBinding<RainworldModSettings, MonsterSpawnMode> FirecentMonsterSpawnModeBinding = new(
		Entry.ModId, DataKey, SaveScope.Global,
		static s => s.FirecentMonsterSpawnMode,
		static (s, v) => s.FirecentMonsterSpawnMode = v);

	private static readonly ModSettingsValueBinding<RainworldModSettings, MonsterSpawnMode> WatercentMonsterSpawnModeBinding = new(
		Entry.ModId, DataKey, SaveScope.Global,
		static s => s.WatercentMonsterSpawnMode,
		static (s, v) => s.WatercentMonsterSpawnMode = v);

	private static readonly ModSettingsValueBinding<RainworldModSettings, MonsterSpawnMode> GreencabMonsterSpawnModeBinding = new(
		Entry.ModId, DataKey, SaveScope.Global,
		static s => s.GreencabMonsterSpawnMode,
		static (s, v) => s.GreencabMonsterSpawnMode = v);

	/// <summary>
	/// 怪物区总控。写入总控值时把该值一并写入全部怪物明细绑定，RitsuLib 会因每个绑定被写入而自动刷新对应的明细行，
	/// 从而实现「一键覆盖」并同步显示，运行时无需再单独判断总控。
	/// </summary>
	private static readonly IModSettingsValueBinding<MonsterSpawnMode> MonsterMasterSpawnModeBinding =
		new MasterSpawnChoiceBinding(
			new ModSettingsValueBinding<RainworldModSettings, MonsterSpawnMode>(
				Entry.ModId, DataKey, SaveScope.Global,
				static s => s.MonsterMasterSpawnMode,
				static (s, v) => s.MonsterMasterSpawnMode = v),
			new IModSettingsValueBinding<MonsterSpawnMode>[]
			{
				MushroomMonsterSpawnModeBinding, IronbirdMonsterSpawnModeBinding, FirecabMonsterSpawnModeBinding,
				FirecentMonsterSpawnModeBinding, WatercentMonsterSpawnModeBinding, GreencabMonsterSpawnModeBinding,
			});

	// 事件明细绑定（先声明，供总控绑定引用）。
	private static readonly ModSettingsValueBinding<RainworldModSettings, MonsterSpawnMode> RaindeerEventSpawnModeBinding = new(
		Entry.ModId, DataKey, SaveScope.Global,
		static s => s.RaindeerEventSpawnMode,
		static (s, v) => s.RaindeerEventSpawnMode = v);

	private static readonly ModSettingsValueBinding<RainworldModSettings, MonsterSpawnMode> WatcherEventSpawnModeBinding = new(
		Entry.ModId, DataKey, SaveScope.Global,
		static s => s.WatcherEventSpawnMode,
		static (s, v) => s.WatcherEventSpawnMode = v);

	/// <summary>
	/// 事件区总控。写入总控值时把该值一并写入全部事件明细绑定，RitsuLib 会因每个绑定被写入而自动刷新对应的明细行，
	/// 从而实现「一键覆盖」并同步显示，运行时无需再单独判断总控。
	/// </summary>
	private static readonly IModSettingsValueBinding<MonsterSpawnMode> EventMasterSpawnModeBinding =
		new MasterSpawnChoiceBinding(
			new ModSettingsValueBinding<RainworldModSettings, MonsterSpawnMode>(
				Entry.ModId, DataKey, SaveScope.Global,
				static s => s.EventMasterSpawnMode,
				static (s, v) => s.EventMasterSpawnMode = v),
			new IModSettingsValueBinding<MonsterSpawnMode>[] { RaindeerEventSpawnModeBinding, WatcherEventSpawnModeBinding });

	public static bool EnableCombatCantLossMaxHp
	{
		get => EnableCombatCantLossMaxHpBinding.Read();
		set { EnableCombatCantLossMaxHpBinding.Write(value); EnableCombatCantLossMaxHpBinding.Save(); }
	}

	public static bool EnableMaxHpRelicFix
	{
		get => EnableMaxHpRelicFixBinding.Read();
		set { EnableMaxHpRelicFixBinding.Write(value); EnableMaxHpRelicFixBinding.Save(); }
	}

	public static bool EnableHpRelicFix
	{
		get => EnableHpRelicFixBinding.Read();
		set { EnableHpRelicFixBinding.Write(value); EnableHpRelicFixBinding.Save(); }
	}

	public static bool EnableModAncientFix
	{
		get => EnableModAncientFixBinding.Read();
		set { EnableModAncientFixBinding.Write(value); EnableModAncientFixBinding.Save(); }
	}

	/// <summary>非蛞蝓猫角色使用原版「检索其它角色卡」机制时，排除蛞蝓猫角色的卡。</summary>
	public static bool EnableExcludeSlugcatFromOtherCharPool
	{
		get => EnableExcludeSlugcatFromOtherCharPoolBinding.Read();
		set { EnableExcludeSlugcatFromOtherCharPoolBinding.Write(value); EnableExcludeSlugcatFromOtherCharPoolBinding.Save(); }
	}

	public static MonsterSpawnMode MonsterMasterSpawnMode
	{
		get => MonsterMasterSpawnModeBinding.Read();
		set { MonsterMasterSpawnModeBinding.Write(value); MonsterMasterSpawnModeBinding.Save(); }
	}

	public static MonsterSpawnMode MushroomMonsterSpawnMode
	{
		get => MushroomMonsterSpawnModeBinding.Read();
		set { MushroomMonsterSpawnModeBinding.Write(value); MushroomMonsterSpawnModeBinding.Save(); }
	}

	public static MonsterSpawnMode IronbirdMonsterSpawnMode
	{
		get => IronbirdMonsterSpawnModeBinding.Read();
		set { IronbirdMonsterSpawnModeBinding.Write(value); IronbirdMonsterSpawnModeBinding.Save(); }
	}

	public static MonsterSpawnMode FirecabMonsterSpawnMode
	{
		get => FirecabMonsterSpawnModeBinding.Read();
		set { FirecabMonsterSpawnModeBinding.Write(value); FirecabMonsterSpawnModeBinding.Save(); }
	}

	public static MonsterSpawnMode FirecentMonsterSpawnMode
	{
		get => FirecentMonsterSpawnModeBinding.Read();
		set { FirecentMonsterSpawnModeBinding.Write(value); FirecentMonsterSpawnModeBinding.Save(); }
	}

	public static MonsterSpawnMode WatercentMonsterSpawnMode
	{
		get => WatercentMonsterSpawnModeBinding.Read();
		set { WatercentMonsterSpawnModeBinding.Write(value); WatercentMonsterSpawnModeBinding.Save(); }
	}

	public static MonsterSpawnMode GreencabMonsterSpawnMode
	{
		get => GreencabMonsterSpawnModeBinding.Read();
		set { GreencabMonsterSpawnModeBinding.Write(value); GreencabMonsterSpawnModeBinding.Save(); }
	}

	public static MonsterSpawnMode EventMasterSpawnMode
	{
		get => EventMasterSpawnModeBinding.Read();
		set { EventMasterSpawnModeBinding.Write(value); EventMasterSpawnModeBinding.Save(); }
	}

	public static MonsterSpawnMode RaindeerEventSpawnMode
	{
		get => RaindeerEventSpawnModeBinding.Read();
		set { RaindeerEventSpawnModeBinding.Write(value); RaindeerEventSpawnModeBinding.Save(); }
	}

	public static MonsterSpawnMode WatcherEventSpawnMode
	{
		get => WatcherEventSpawnModeBinding.Read();
		set { WatcherEventSpawnModeBinding.Write(value); WatcherEventSpawnModeBinding.Save(); }
	}

	/// <summary>
	/// 生成一份当前本地配置的 spawn 快照，供宿主机下发给全体客户端（多人以宿主机为准）。
	/// 离线或单人时则直接使用该快照作为生效配置。
	/// </summary>
	public static SpawnConfigRunData CreateSpawnSnapshot() => new()
	{
		MonsterMasterSpawnMode = MonsterMasterSpawnMode,
		MushroomMonsterSpawnMode = MushroomMonsterSpawnMode,
		IronbirdMonsterSpawnMode = IronbirdMonsterSpawnMode,
		FirecabMonsterSpawnMode = FirecabMonsterSpawnMode,
		FirecentMonsterSpawnMode = FirecentMonsterSpawnMode,
		WatercentMonsterSpawnMode = WatercentMonsterSpawnMode,
		GreencabMonsterSpawnMode = GreencabMonsterSpawnMode,
		EventMasterSpawnMode = EventMasterSpawnMode,
		RaindeerEventSpawnMode = RaindeerEventSpawnMode,
		WatcherEventSpawnMode = WatcherEventSpawnMode,
	};

	private static ModSettingsText T(string key) =>
		ModSettingsText.LocString(new LocString("settings_ui", key));

	/// <summary>返回该出现模式的本地化文案（用于枚举选项标签）。</summary>
	private static ModSettingsText SpawnModeLabelText(MonsterSpawnMode mode) => mode switch
	{
		MonsterSpawnMode.Always => T("RAINWORLD-config.monsterSpawnMode.always"),
		MonsterSpawnMode.OnlySlugcat => T("RAINWORLD-config.monsterSpawnMode.onlySlugcat"),
		MonsterSpawnMode.Disabled => T("RAINWORLD-config.monsterSpawnMode.disabled"),
		_ => ModSettingsText.Literal(mode.ToString()),
	};

	/// <summary>
	/// 总控绑定包装器：写入总控值时把同一值一并写入所有明细绑定。
	/// 每个 <see cref="IModSettingsValueBinding{TValue}.Write" /> 都会触发 RitsuLib 的
	/// <see cref="ModSettingsBindingWriteEvents.ValueWritten" />，从而自动刷新对应行，实现总控一键联动明细并同步显示。
	/// 保存（<see cref="Save" />）时同样一并落盘所有明细。
	/// </summary>
	private sealed class MasterSpawnChoiceBinding : IModSettingsValueBinding<MonsterSpawnMode>
	{
		private readonly IModSettingsValueBinding<MonsterSpawnMode> _master;
		private readonly IModSettingsValueBinding<MonsterSpawnMode>[] _details;

		public MasterSpawnChoiceBinding(
			IModSettingsValueBinding<MonsterSpawnMode> master,
			IModSettingsValueBinding<MonsterSpawnMode>[] details)
		{
			_master = master;
			_details = details;
		}

		public string ModId => _master.ModId;
		public string DataKey => _master.DataKey;
		public SaveScope Scope => _master.Scope;

		public MonsterSpawnMode Read() => _master.Read();

		public void Write(MonsterSpawnMode value)
		{
			_master.Write(value);
			foreach (var detail in _details)
				detail.Write(value);
		}

		public void Save()
		{
			_master.Save();
			foreach (var detail in _details)
				detail.Save();
		}
	}

	public static void Register()
	{
		ModDataStore.For(Entry.ModId).Register<RainworldModSettings>(
			key: DataKey,
			fileName: "mod_settings.json",
			scope: SaveScope.Global,
			defaultFactory: () => new RainworldModSettings(),
			autoCreateIfMissing: true);

		RitsuLibFramework.RegisterModSettings(Entry.ModId, page => page
			.WithTitle(T("RAINWORLD-config.title"))
			.WithModDisplayName(T("RAINWORLD-config.displayName"))
			.AddSection("general", section => section
				.WithTitle(T("RAINWORLD-config.section.hpFix"))
				.AddToggle("enableExcludeSlugcatFromOtherCharPool",
					T("RAINWORLD-config.enableExcludeSlugcatFromOtherCharPool"),
					EnableExcludeSlugcatFromOtherCharPoolBinding,
					T("RAINWORLD-config.enableExcludeSlugcatFromOtherCharPool.desc"))
				.AddToggle("enableCombatCantLossMaxHp",
					T("RAINWORLD-config.enableCombatCantLossMaxHp"),
					EnableCombatCantLossMaxHpBinding,
					T("RAINWORLD-config.enableCombatCantLossMaxHp.desc"))
				.AddToggle("enableMaxHpRelicFix",
					T("RAINWORLD-config.enableMaxHpRelicFix"),
					EnableMaxHpRelicFixBinding,
					T("RAINWORLD-config.enableMaxHpRelicFix.desc"))
				.AddToggle("enableHpRelicFix",
					T("RAINWORLD-config.enableHpRelicFix"),
					EnableHpRelicFixBinding,
					T("RAINWORLD-config.enableHpRelicFix.desc"))

			)
			.AddSection("monster", section => section
				.WithTitle(T("RAINWORLD-config.section.monster"))
				.AddEnumChoice<MonsterSpawnMode>("monsterMasterSpawnMode",
					T("RAINWORLD-config.monsterMasterSpawnMode"),
					MonsterMasterSpawnModeBinding,
					SpawnModeLabelText,
					T("RAINWORLD-config.monsterMasterSpawnMode.desc"))
				.AddEnumChoice<MonsterSpawnMode>("mushroomMonsterSpawnMode",
					T("RAINWORLD-config.mushroomMonsterSpawnMode"),
					MushroomMonsterSpawnModeBinding,
					SpawnModeLabelText,
					T("RAINWORLD-config.mushroomMonsterSpawnMode.desc"))
				.AddEnumChoice<MonsterSpawnMode>("ironbirdMonsterSpawnMode",
					T("RAINWORLD-config.ironbirdMonsterSpawnMode"),
					IronbirdMonsterSpawnModeBinding,
					SpawnModeLabelText,
					T("RAINWORLD-config.ironbirdMonsterSpawnMode.desc"))
				.AddEnumChoice<MonsterSpawnMode>("firecabMonsterSpawnMode",
					T("RAINWORLD-config.firecabMonsterSpawnMode"),
					FirecabMonsterSpawnModeBinding,
					SpawnModeLabelText,
					T("RAINWORLD-config.firecabMonsterSpawnMode.desc"))
				.AddEnumChoice<MonsterSpawnMode>("firecentMonsterSpawnMode",
					T("RAINWORLD-config.firecentMonsterSpawnMode"),
					FirecentMonsterSpawnModeBinding,
					SpawnModeLabelText,
					T("RAINWORLD-config.firecentMonsterSpawnMode.desc"))
				.AddEnumChoice<MonsterSpawnMode>("watercentMonsterSpawnMode",
					T("RAINWORLD-config.watercentMonsterSpawnMode"),
					WatercentMonsterSpawnModeBinding,
					SpawnModeLabelText,
					T("RAINWORLD-config.watercentMonsterSpawnMode.desc"))
				.AddEnumChoice<MonsterSpawnMode>("greencabMonsterSpawnMode",
					T("RAINWORLD-config.greencabMonsterSpawnMode"),
					GreencabMonsterSpawnModeBinding,
					SpawnModeLabelText,
					T("RAINWORLD-config.greencabMonsterSpawnMode.desc"))
			).AddSection("event", section => section
				.WithTitle(T("RAINWORLD-config.section.event"))
				.AddEnumChoice<MonsterSpawnMode>("eventMasterSpawnMode",
					T("RAINWORLD-config.eventMasterSpawnMode"),
					EventMasterSpawnModeBinding,
					SpawnModeLabelText,
					T("RAINWORLD-config.eventMasterSpawnMode.desc"))
				.AddEnumChoice<MonsterSpawnMode>("raindeerEventSpawnMode",
					T("RAINWORLD-config.raindeerEventSpawnMode"),
					RaindeerEventSpawnModeBinding,
					SpawnModeLabelText,
					T("RAINWORLD-config.raindeerEventSpawnMode.desc"))
				.AddEnumChoice<MonsterSpawnMode>("watcherEventSpawnMode",
					T("RAINWORLD-config.watcherEventSpawnMode"),
					WatcherEventSpawnModeBinding,
					SpawnModeLabelText,
					T("RAINWORLD-config.watcherEventSpawnMode.desc"))

			)

		);
	}
}
