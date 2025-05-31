using Godot;
using System;
using System.Collections.Generic;

public partial class TitleScreen : Control
{
	// 기존 변수들
	private OptionButton _langOption;
	private bool _isHandlingSelection = false;
	private bool _isEventConnected = false;
	
	// 추가된 버튼 변수들
	private Button _button1;
	private Button _button2;
	private Button _button3;
	private Button _button4;
	private Button _button5;
	private Button _button6;

	public override void _Ready()
	{
		FontManager.Instance.UpdateFont();
		// 기존 초기화 코드
		_langOption = GetNode<OptionButton>("MarginContainer/Locale_Box/LangOption");
		InitializeLanguageOptions(false);
		UITranslator.UpdateTranslations(this);
		LocalizationManager.Instance.LanguageChanged += OnLanguageChanged;
		ConnectItemSelected();

		// 버튼 초기화 및 이벤트 연결
		_button1 = GetNode<Button>("MarginContainer/UI_Box/Button1");
		_button2 = GetNode<Button>("MarginContainer/UI_Box/Button2");
		_button3 = GetNode<Button>("MarginContainer/UI_Box/Button3");
		_button4 = GetNode<Button>("MarginContainer/UI_Box/Button4");
		_button5 = GetNode<Button>("MarginContainer/UI_Box/Button5");
		_button6 = GetNode<Button>("MarginContainer/UI_Box/Button6");

		_button1.Pressed += OnButton1Pressed;
		_button2.Pressed += OnButton2Pressed;
		_button3.Pressed += OnButton3Pressed;
		_button4.Pressed += OnButton4Pressed;
		_button5.Pressed += OnButton5Pressed;
		_button6.Pressed += OnButton6Pressed;
	}
	
	private void InitializeLanguageOptions(bool connectEvent = true)
	{
		// 이벤트 임시 해제
		if (_isEventConnected)
		{
			DisconnectItemSelected();
		}
		
		_langOption.Clear();
		var supportedLangs = LocalizationManager.Instance.DetectSupportedLanguages();
		
		foreach (string langCode in supportedLangs)
		{
			string nativeName = LocalizationManager.Instance.GetNativeLanguageName(langCode);
			string displayName = $"{nativeName} ({langCode.ToUpper()})";
			
			_langOption.AddItem(displayName);
			_langOption.SetItemMetadata(_langOption.ItemCount - 1, langCode);
			
			if (langCode == LocalizationManager.Instance.CurrentLanguage)
				_langOption.Select(_langOption.ItemCount - 1);
		}
		
		// 요청된 경우에만 이벤트 재연결
		if (connectEvent && !_isEventConnected)
		{
			ConnectItemSelected();
		}
	}
	
	// 이벤트 연결 메서드
	private void ConnectItemSelected()
	{
		if (!_isEventConnected)
		{
			_langOption.ItemSelected += OnLanguageSelected;
			_isEventConnected = true;
			GD.Print("ItemSelected event connected");
		}
	}
	
	// 이벤트 해제 메서드
	private void DisconnectItemSelected()
	{
		if (_isEventConnected)
		{
			_langOption.ItemSelected -= OnLanguageSelected;
			_isEventConnected = false;
			GD.Print("ItemSelected event disconnected");
		}
	}
	
	private void OnLanguageSelected(long index)
	{
		if (_isHandlingSelection) return;
		_isHandlingSelection = true;
		
		GD.Print($"Language selected: index={index}");
		
		try
		{
			if (index >= 0)
			{
				string langCode = (string)_langOption.GetItemMetadata((int)index);
				if (!string.IsNullOrEmpty(langCode))
				{
					GD.Print($"Setting language to: {langCode}");
					LocalizationManager.Instance.SetLanguage(langCode);
				}
			}
		}
		catch (Exception e) // System.Exception 사용 가능
		{
			GD.PrintErr($"Error in OnLanguageSelected: {e.Message}");
		}
		finally
		{
			_isHandlingSelection = false;
		}
	}
	
	private void OnLanguageChanged()
	{
		GD.Print("Language changed event received");
		
		// 옵션 버튼 전체 다시 초기화 (이벤트 연결 유지)
		InitializeLanguageOptions(false);
		
		// 기타 UI 업데이트
		UITranslator.UpdateTranslations(this);
		
		// 이벤트 재연결 보장
		ConnectItemSelected();
	}
	
	 private void OnButton1Pressed()
	{
		GD.Print("마지막 저장된 게임 불러오기 실행");
		// 실제 로드 기능 구현시 여기에 코드 추가
	}

	private void OnButton2Pressed()
	{
		GD.Print("새게임 실행");
	}

	private void OnButton3Pressed()
	{
		GD.Print("저장된 게임 불러오기 실행");
		// 실제 로드 기능 구현시 여기에 코드 추가
	}

	private void OnButton4Pressed()
	{
		GD.Print("게임옵션 실행");
	}

	private void OnButton5Pressed()
	{
		GD.Print("게임 모드 선택 실행");
		// 실제 모드 선택 기능 구현시 여기에 코드 추가
	}

	private void OnButton6Pressed()
	{
		GD.Print("게임 종료 실행");
	}

	public override void _ExitTree()
	{
		// 기존 구독 해제 코드
		GD.Print("TitleScreen exiting tree");
		if (LocalizationManager.Instance != null)
		{
			LocalizationManager.Instance.LanguageChanged -= OnLanguageChanged;
		}
		DisconnectItemSelected();

		// 버튼 이벤트 구독 해제
		_button1.Pressed -= OnButton1Pressed;
		_button2.Pressed -= OnButton2Pressed;
		_button3.Pressed -= OnButton3Pressed;
		_button4.Pressed -= OnButton4Pressed;
		_button5.Pressed -= OnButton5Pressed;
		_button6.Pressed -= OnButton6Pressed;
	}
}
