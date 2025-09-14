using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

public class UIButtonGenerator : MonoBehaviour
{
    public GameObject defaultLightObject;
    public float bufferTime = 10;
    public int delayTime = 600;
    public float audioVolume = 0.2f;
    public float syncFrequency = 10f;
    public float syncThreshold = 0.1f;
    public float fadeTime = 0.5f;

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
