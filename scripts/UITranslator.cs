using Godot;
using System.Collections.Generic;

public static class UITranslator
{
	// 모든 번역 가능한 컨트롤 업데이트 (재귀적 트리 탐색)
	public static void UpdateTranslations(Node rootNode)
	{
		// 재귀적으로 모든 노드 탐색
		TraverseNodes(rootNode);
	}

	// 재귀적으로 노드 트리 탐색
	private static void TraverseNodes(Node node)
	{
		// 현재 노드가 Control이고 메타데이터가 있으면 번역 적용
		if (node is Control control && control.HasMeta("translation_key"))
		{
			ApplyTranslation(control);
		}

		// 자식 노드들에 대해 재귀 호출
		foreach (Node child in node.GetChildren())
		{
			TraverseNodes(child);
		}
	}

	// 개별 컨트롤에 번역 적용
	private static void ApplyTranslation(Control control)
	{
		// 메타데이터에서 번역 정보 추출
		string key = (string)control.GetMeta("translation_key");
		string category = control.HasMeta("translation_category") 
			? (string)control.GetMeta("translation_category") 
			: "main_menu"; // 기본 카테고리

		// 번역 가져오기
		string translation = LocalizationManager.Instance.GetString(category, key);
		
		// 컨트롤 유형에 따라 적절히 적용
		if (control is Button button)
		{
			button.Text = translation;
		}
		else if (control is Label label)
		{
			label.Text = translation;
		}
		else if (control is CheckButton checkButton)
		{
			checkButton.Text = translation;
		}
		// 필요시 다른 컨트롤 유형 추가
	}

	// 특정 컨트롤만 업데이트
	public static void ApplyTranslationTo(Control control)
	{
		if (control.HasMeta("translation_key"))
		{
			ApplyTranslation(control);
		}
	}
}
