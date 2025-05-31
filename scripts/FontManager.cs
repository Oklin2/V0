using Godot;
using System.Linq;
using System.Collections.Generic;

public partial class FontManager : Node
{
	private static FontManager _instance;
	public static FontManager Instance => _instance;

	public override void _Ready()
	{
		_instance = this;
		UpdateFont(); // 초기 실행

		// LocalizationManager에서 언어 변경 감지
		LocalizationManager.Instance.LanguageChanged += OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		GD.Print($"Language changed to {LocalizationManager.Instance.CurrentLanguage}, updating font...");
		UpdateFont();
	}

	public void UpdateFont()
	{
		string locale = LocalizationManager.Instance.CurrentLanguage;
		string folderPath = $"res://locale/{locale}/";

		DirAccess dir = DirAccess.Open(folderPath);
		if (dir == null)
		{
			GD.PrintErr($"Failed to open directory: {folderPath}");
			return;
		}

		var fontFiles = dir.GetFiles()
			.Where(file => file.EndsWith(".ttf") || file.EndsWith(".otf"))
			.ToList();

		if (fontFiles.Count > 0)
		{
			string fontPath = folderPath + fontFiles[0];
			FontFile font = GD.Load<FontFile>(fontPath);
			GD.Print($"Loaded font: {fontPath}");

			Theme theme = new Theme();
			theme.DefaultFont = font;

			GetTree().Root.SetTheme(theme);
			GD.Print("Font applied successfully!");
		}
		else
		{
			GD.PrintErr($"No font file found in {folderPath}");
		}
	}
}
