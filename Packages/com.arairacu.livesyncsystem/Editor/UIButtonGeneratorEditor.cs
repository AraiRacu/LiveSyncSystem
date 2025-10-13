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
        DrawDefaultInspector(); // �f�t�H���g�̃C���X�y�N�^�[��\��

        UIButtonGenerator generator = (UIButtonGenerator)target;

        if (GUILayout.Button("�{�^���𐶐�����"))
        {
            GenerateButtons(generator);
        }
    }

    private void GenerateButtons(UIButtonGenerator generator)
    {
        // �`�F�b�N
        if (generator.buttonParent == null)
        {
            Debug.LogError("buttonParent ���ݒ肳��Ă��܂���I");
            return;
        }

        if (generator.buttonPrefab == null)
        {
            Debug.LogError("buttonPrefab ���ݒ肳��Ă��܂���I");
            return;
        }

        // �q����U�S�폜�i���Z�b�g�j
        while(generator.buttonParent.childCount > 0)
        {
            foreach (Transform child in generator.buttonParent)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        // PlayList�̒��g���폜
        generator.playListController._trackButtons = new Button[generator.buttons.Count];
        generator.playListController._trackURLs = new VRC.SDKBase.VRCUrl[generator.buttons.Count];

        //StageCollider�̒��g���폜
        generator.syncSystem.lightDirection = new GameObject[generator.buttons.Count];
        generator.syncSystem.isFadeJack = new bool[generator.buttons.Count];

        // �{�^���𐶐�
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
        Debug.Log("�{�^�����������I");

        generator.syncSystem.defaultLightObject = generator.defaultLightObject;
        generator.syncSystem.bufferTime = generator.bufferTime;
        generator.syncSystem.delayTime = generator.delayTime;
        generator.syncSystem.audioVolume = generator.audioVolume;
        generator.syncSystem.syncFrequency = generator.syncFrequency;
        generator.syncSystem.syncThreshold = generator.syncThreshold;
        generator.syncSystem.fadeTime = generator.fadeTime;
        Debug.Log("�������ݒ芮���I");
    }
}
