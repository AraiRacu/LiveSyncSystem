using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

public class UIButtonGenerator : MonoBehaviour
{
    [System.Serializable]
    public class ButtonData
    {
        public string buttonText;
        public VRCUrl url;
        public GameObject directionPrefab;
        public bool isFade = false;
    }

    public List<ButtonData> buttons = new List<ButtonData>();

    public Transform buttonParent; // 生成したボタンを配置する親（Canvasの子など）
    public GameObject buttonPrefab; // uGUIのボタンのプレハブ

    public RacoonStudioLiveSyncSystem syncSystem;
    public PlayListController playListController;

}
