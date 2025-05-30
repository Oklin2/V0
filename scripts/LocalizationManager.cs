using Godot;
using System;
using System.Collections.Generic;

public partial class LocalizationManager : Node
{
	private static LocalizationManager _instance;
	public static LocalizationManager Instance => _instance;

	private Dictionary<string, Dictionary<string, string>> _translations = new();
	public string CurrentLanguage { get; private set; } = "en";
	public event Action LanguageChanged;
	
	// 캐싱 시스템
	private Dictionary<string, string> _nativeNameCache = new();
	private Dictionary<string, string> _languageToCountryMap = new();
	private List<string> _supportedLangsCache;
	private bool _languagesDetected = false;

	public override void _EnterTree()
	{
		if (_instance == null)
		{
			_instance = this;
			Initialize();
		}
		else
		{
			QueueFree();
		}
	}

	private void Initialize()
	{
		// 매핑 파일 로드
		LoadLanguageMapping();
		
		// 저장된 언어 설정 불러오기
		string savedLang = LoadSavedLanguage();
		if (!string.IsNullOrEmpty(savedLang))
		{
			CurrentLanguage = savedLang;
		}
		else
		{
			// 시스템 언어 감지
			string systemLang = OS.GetLocale().Split('-')[0].ToLower();
			var supportedLangs = DetectSupportedLanguages();
			CurrentLanguage = supportedLangs.Contains(systemLang) ? systemLang : "en";
		}
		
		LoadTranslations(CurrentLanguage);
		GD.Print($"LocalizationManager initialized. Language: {CurrentLanguage}");
	}

	private void LoadLanguageMapping()
	{
		string path = "res://locale/language_mapping.cfg";
		
		if (!Godot.FileAccess.FileExists(path))
		{
			GD.Print("Language mapping file not found. Using default mapping.");
			return;
		}

		try
		{
			using (var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read))
			{
				while (!file.EofReached())
				{
					string line = file.GetLine().StripEdges();
					
					if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) 
						continue;

					int separatorIndex = line.IndexOf('=');
					if (separatorIndex > 0)
					{
						string langCode = line.Substring(0, separatorIndex).StripEdges();
						string countryCode = line.Substring(separatorIndex + 1).StripEdges();
						
						_languageToCountryMap[langCode] = countryCode;
						GD.Print($"Language mapping: {langCode} -> {countryCode}");
					}
				}
			}
		}
		catch (Exception e)
		{
			GD.PrintErr($"Error loading language mapping: {e.Message}");
		}
	}

	// 저장된 언어 불러오기
	private string LoadSavedLanguage()
	{
		if (Godot.FileAccess.FileExists("user://language.save"))
		{
			try
			{
				using (var file = Godot.FileAccess.Open("user://language.save", Godot.FileAccess.ModeFlags.Read))
				{
					return file.GetLine();
				}
			}
			catch (Exception e)
			{
				GD.PrintErr($"Error loading saved language: {e.Message}");
				return null;
			}
		}
		return null;
	}

	// 언어 설정 저장
	private void SaveLanguage(string langCode)
	{
		try
		{
			using (var file = Godot.FileAccess.Open("user://language.save", Godot.FileAccess.ModeFlags.Write))
			{
				file.StoreLine(langCode);
			}
		}
		catch (Exception e)
		{
			GD.PrintErr($"Error saving language: {e.Message}");
		}
	}

	// 지원 가능한 언어 목록 동적 검색 (캐싱 적용)
	public List<string> DetectSupportedLanguages()
	{
		if (_languagesDetected && _supportedLangsCache != null)
		{
			return _supportedLangsCache;
		}
		
		var langs = new List<string>();
		using (var dir = DirAccess.Open("res://locale"))
		{
			if (dir != null)
			{
				dir.ListDirBegin();
				string folderName = dir.GetNext();
				while (folderName != "")
				{
					if (dir.CurrentIsDir() && !folderName.StartsWith("."))
					{
						langs.Add(folderName);
					}
					folderName = dir.GetNext();
				}
				dir.ListDirEnd();
			}
		}
		
		_supportedLangsCache = langs;
		_languagesDetected = true;
		GD.Print($"Detected supported languages: {string.Join(", ", langs)}");
		return langs;
	}

	// 언어 파일 로드 및 파싱
	private void LoadTranslations(string langCode)
	{
		_translations.Clear();
		string path = $"res://locale/{langCode}/base.ini";

		if (!Godot.FileAccess.FileExists(path))
		{
			GD.PrintErr($"Translation file not found: {path}");
			return;
		}

		try
		{
			using (var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read))
			{
				string currentSection = "";
				while (!file.EofReached())
				{
					string line = file.GetLine().StripEdges();
					
					if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) 
						continue;

					if (line.StartsWith("[") && line.EndsWith("]"))
					{
						currentSection = line.Substring(1, line.Length - 2);
						if (!_translations.ContainsKey(currentSection))
						{
							_translations[currentSection] = new Dictionary<string, string>();
						}
						continue;
					}

					int separatorIndex = line.IndexOf('=');
					if (separatorIndex > 0 && !string.IsNullOrEmpty(currentSection))
					{
						string key = line.Substring(0, separatorIndex).StripEdges();
						string value = line.Substring(separatorIndex + 1).StripEdges();
						_translations[currentSection][key] = value;
					}
				}
			}
			
			GD.Print($"Translations loaded for {langCode}");
		}
		catch (Exception e)
		{
			GD.PrintErr($"Error loading translations for {langCode}: {e.Message}");
		}
		
		// 언어 변경 이벤트 발생
		LanguageChanged?.Invoke();
	}

	// 번역 문자열 가져오기
	public string GetString(string category, string key)
	{
		if (_translations.TryGetValue(category, out var categoryDict) && 
			categoryDict.TryGetValue(key, out var value))
		{
			return value;
		}
		
		GD.PrintErr($"Translation missing: [{category}] {key}");
		return $"{category}.{key}";
	}

	// 언어 변경 기능
	public void SetLanguage(string langCode)
	{
		var supportedLangs = DetectSupportedLanguages();
		if (supportedLangs.Contains(langCode))
		{
			CurrentLanguage = langCode;
			LoadTranslations(langCode);
			SaveLanguage(langCode);
			GD.Print($"Language changed to: {langCode}");
		}
		else
		{
			GD.PrintErr($"Unsupported language: {langCode}");
		}
	}
	
	// 원어민 언어 이름 가져오기 (캐싱 적용)
	public string GetNativeLanguageName(string langCode)
	{
		// 캐시 확인
		if (_nativeNameCache.TryGetValue(langCode, out string name))
		{
			return name;
		}
		
		// 기본값: 폴더명 대문자
		string result = langCode.ToUpper();
		string path = $"res://locale/{langCode}/base.ini";
		
		if (!Godot.FileAccess.FileExists(path))
		{
			_nativeNameCache[langCode] = result;
			return result;
		}
		
		try
		{
			using (var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read))
			{
				string currentSection = "";
				while (!file.EofReached())
				{
					string line = file.GetLine().StripEdges();
					
					if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) 
						continue;

					if (line.StartsWith("[") && line.EndsWith("]"))
					{
						currentSection = line.Substring(1, line.Length - 2);
						continue;
					}

					if (currentSection == "system")
					{
						int separatorIndex = line.IndexOf('=');
						if (separatorIndex > 0)
						{
							string key = line.Substring(0, separatorIndex).StripEdges();
							string value = line.Substring(separatorIndex + 1).StripEdges();
							
							if (key == "language_name")
							{
								result = value;
								break;
							}
						}
					}
				}
			}
		}
		catch (Exception e)
		{
			GD.PrintErr($"Error reading language name for {langCode}: {e.Message}");
		}
		
		// 캐시에 저장
		_nativeNameCache[langCode] = result;
		return result;
	}
	
	// 언어 코드를 국가 코드로 변환
	public string GetCountryCode(string langCode)
	{
		if (_languageToCountryMap.TryGetValue(langCode, out string countryCode))
		{
			return countryCode;
		}
		return langCode;
	}
	
	// 캐시 초기화 (새 언어 추가 시 호출)
	public void ClearCache()
	{
		_nativeNameCache.Clear();
		_supportedLangsCache = null;
		_languagesDetected = false;
		GD.Print("Localization cache cleared");
	}
}
