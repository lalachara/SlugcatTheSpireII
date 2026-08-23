using Godot;
using System;

public partial class Ncorrupt : Sprite2D
{
	// 硬编码图片路径列表（按需修改）
	private string[] _imagePaths = new string[]
	{
		"res://Godot/CombatUI/resource/RedCatVar1.png",
		"res://Godot/CombatUI/resource/RedCatVar2.png",
		"res://Godot/CombatUI/resource/RedCatVar3.png",
		"res://Godot/CombatUI/resource/RedCatVar4.png"
		// 想加几张就加几行，注意末尾不要逗号
	};

	// 缓存的纹理数组
	private Texture2D[] _textures;

	// 当前显示索引
	private int _currentIndex = 0;

	public override void _Ready()
	{
		// 加载所有图片
		int count = _imagePaths.Length;
		_textures = new Texture2D[count];
		for (int i = 0; i < count; i++)
		{
			// 加载纹理，如果路径错误会返回 null
			_textures[i] = GD.Load<Texture2D>(_imagePaths[i]);

		}

		// 默认显示第一张（如果有）
		if (count > 0 && _textures[0] != null)
		{
			Texture = _textures[0];
		}
	}

	/// <summary>
	/// 切换图片（外部调用此方法）
	/// </summary>
	/// <param name="index">从 0 开始</param>
	public void ShowImage(int index)
	{
		
		Texture = _textures[index];
		_currentIndex = index;
	}

	/// <summary>
	/// 获取当前显示索引
	/// </summary>
	public int GetCurrentIndex() => _currentIndex;

	/// <summary>
	/// 获取图片总数
	/// </summary>
	public void MakeTransparent()
	{
		Modulate = Colors.Transparent;
	}
	public int GetImageCount() => _textures.Length;
		public void RestoreDisplay()
	{
		// 恢复可见
		Visible = true;
		// 恢复不透明
		Modulate = Colors.White;
		// 如果之前清空了纹理，则重新设置当前图片
		if (Texture == null && _textures != null && _currentIndex >= 0 && _currentIndex < _textures.Length)
		{
			Texture = _textures[_currentIndex];
		}
	}
}
