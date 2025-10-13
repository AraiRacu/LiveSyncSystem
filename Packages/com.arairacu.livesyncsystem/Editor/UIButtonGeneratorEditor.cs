using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(UIButtonGenerator))]
public class UIButtonGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // デフォルトのインスペクターを表示

        UIButtonGenerator generator = (UIButtonGenerator)target;

        if (GUILayout.Button("ボタンを生成する"))
        {
            GenerateButtons(generator);
        }
    }

    private void GenerateButtons(UIButtonGenerator generator)
    {
        // チェック
        if (generator.buttonParent == null)
        {
            Debug.LogError("buttonParent が設定されていません！");
            return;
        }

        if (generator.buttonPrefab == null)
        {
            Debug.LogError("buttonPrefab が設定されていません！");
            return;
        }

        // 子を一旦全削除（リセット）
        while(generator.buttonParent.childCount > 0)
        {
            foreach (Transform child in generator.buttonParent)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        // PlayListの中身を削除
        generator.playListController._trackButtons = new Button[generator.buttons.Count];
        generator.playListController._trackURLs = new VRC.SDKBase.VRCUrl[generator.buttons.Count];

        //StageColliderの中身を削除
        generator.syncSystem.lightDirection = new GameObject[generator.buttons.Count];
        generator.syncSystem.isFadeJack = new bool[generator.buttons.Count];

        // ボタンを生成
        for (int i = 0; i < generator.buttons.Count; i++)
        {
            var buttonData = generator.buttons[i];
            GameObject newButtonObj = (GameObject)Instantiate(generator.buttonPrefab, generator.buttonParent);
            newButtonObj.SetActive(true);

            Button button = newButtonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = newButtonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null)
            {
                buttonText.text = buttonData.buttonText;
            }

            generator.playListController._trackButtons[i] = button;
            generator.playListController._trackURLs[i] = buttonData.url;
            generator.syncSystem.lightDirection[i] = buttonData.directionPrefab;
            generator.syncSystem.isFadeJack[i] = buttonData.isFade;

            PrefabUtility.RecordPrefabInstancePropertyModifications(generator.playListController);
            PrefabUtility.RecordPrefabInstancePropertyModifications(generator.syncSystem);
        }
        Debug.Log("ボタン生成完了！");

        generator.syncSystem.defaultLightObject = generator.defaultLightObject;
        generator.syncSystem.bufferTime = generator.bufferTime;
        generator.syncSystem.delayTime = generator.delayTime;
        generator.syncSystem.audioVolume = generator.audioVolume;
        generator.syncSystem.syncFrequency = generator.syncFrequency;
        generator.syncSystem.syncThreshold = generator.syncThreshold;
        generator.syncSystem.fadeTime = generator.fadeTime;
        Debug.Log("同期情報設定完了！");
    }
}
