
using HoshinoLabs.IwaSync3;
using HoshinoLabs.IwaSync3.Udon;
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Video;
using VRC.SDKBase;
using UnityEngine.UI;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class RacoonStudioLiveSyncSystem : UdonSharpBehaviour
{
    public const string APP_NAME = "VideoSyncForLive";

    [SerializeField] VideoCore core;
    [SerializeField] Slider _progressSlider;
    [SerializeField] private Transform directionParent;
    [HideInInspector] public GameObject[] lightDirection;
    [HideInInspector] public Boolean[] isFadeJack;
    [HideInInspector] public GameObject defaultLightObject;
    [SerializeField] private GameObject fadePPSObject;
    [HideInInspector] public float bufferTime = 10;
    [HideInInspector] public int delayTime = 600;
    [HideInInspector] public float audioVolume = 0.2f;
    [HideInInspector] public float syncFrequency = 10f;
    [HideInInspector] public float syncThreshold = 0.1f;
    [HideInInspector] public float fadeTime = 0.5f;

    [UdonSynced, FieldChangeCallback(nameof(SyncStartUTCTime))] private string _syncStartUTCTime = "";
    [UdonSynced, FieldChangeCallback(nameof(ProgressRate))] private float _progressRate = 0;
    private DateTime localStartTime = DateTime.MinValue;

    private int _track = -1;

    private bool isOnStage = false;

    private GameObject directionInstance = null;
    private PlayableDirector _directionInstanceDirection = null;

    private Animator fadeAnimator = null;
    public string SyncStartUTCTime
    {
        set { _syncStartUTCTime = value; SetLocalStartTime(); }
        get => _syncStartUTCTime;
    }

    public float ProgressRate
    {
        set { _progressRate = value; ChangeDirectionProgress(); }
        get => _progressRate;
    }

    private void Start()
    {
        fadeAnimator = fadePPSObject.GetComponent<Animator>();
    }

    void Update()
    {
        if (localStartTime != DateTime.MinValue)
        {
            if ((localStartTime <= DateTime.Now))
            {
                //動画の再生時間を0にする，演出ライトのActive化，客側のAudioVolumeを上げる
                Debug.Log($"[< color =#CD853F>{APP_NAME}</color>] Triggered:{DateTime.Now.ToString()}.{DateTime.Now.Millisecond.ToString()}");
                StartPlayBack();
                localStartTime = DateTime.MinValue;
            }
            else if(localStartTime <= DateTime.Now.AddSeconds(fadeTime))
            {
                if (_track != -1 && isFadeJack[_track] && fadePPSObject.activeSelf == false)
                {
                    FadePPS("FadeStart");
                }
            }
        }
        else if(_directionInstanceDirection != null && directionInstance.activeSelf)
        {
            if(_directionInstanceDirection.duration <= _directionInstanceDirection.time + 0.1)
            {
                StopDirectionInstance();
            }
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player != Networking.LocalPlayer) return;
        isOnStage = true;
        //壇上とのタイムラグの処理
        Debug.Log($"[<color=#CD853F>{APP_NAME}</color>] Player OnStage.");
        CreateTimelagStageORFloor();
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player != Networking.LocalPlayer) return;
        isOnStage = false;
        //壇上とのタイムラグの処理
        Debug.Log($"[<color=#CD853F>{APP_NAME}</color>] Player DownStage.");
        CreateTimelagStageORFloor();
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if(player == Networking.LocalPlayer)
        {
            SendCustomEventDelayedSeconds(nameof(CheckLateJoiner), 3);
        }
    }

    public void PauseOn()
    {
        TakeOwnership();
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(PauseOnAll));
    }

    public void PauseOff()
    {
        TakeOwnership();
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(PauseOffAll));
    }

    public void SeekDirection()
    {
        TakeOwnership();
        ProgressRate = _progressSlider.value;
        RequestSerialization();
    }

    /// <summary>
    ///  再生ボタンを押した際に同期開始のタイミングの時間をUTCで計算をする
    /// </summary>
    public void CreateVideoSync()
    {
        TakeOwnership();

        DateTime targetUTCTime = DateTime.Now.ToUniversalTime().AddSeconds(bufferTime);

        SyncStartUTCTime = targetUTCTime.ToString();

        RequestSerialization();
    }

    public void CheckLateJoiner()
    {
        if (_track != -1 && _directionInstanceDirection != null)
        {
            _directionInstanceDirection.time = core.time;
            SendCustomEventDelayedSeconds(nameof(SyncTimePeriodic), syncFrequency);
        }
    }

    public void TakeOwnership()
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    /// <summary>
    /// マスター以外のプレイヤーが値の同期後，再生開始のタイミングを取得する
    /// </summary>
    private void SetLocalStartTime()
    {
        Debug.Log($"[<color =#CD853F>{APP_NAME}</color>] Recieved:{DateTime.Now.ToString()}.{DateTime.Now.Millisecond.ToString()}");

        //現在再生中の演出を止める
        StopDirectionInstance();

        if (_syncStartUTCTime != "")
        {
            localStartTime = DateTime.Parse(_syncStartUTCTime).ToLocalTime();
            Debug.Log($"[<color =#CD853F>{APP_NAME}</color>] RecievedTime:{localStartTime.ToString()}.{localStartTime.Millisecond.ToString()}");
        }

        //フロアのプレイヤーの音量を開始するまで0にする
        if (!isOnStage)
        {
            OnVolumeChanged(0);
        }
    }

    /// <summary>
    /// PlayListから来るトラック更新の適用
    /// </summary>
    public void ChangeTrackIndex(int track)
    {
        _track = track;
        if (track == -1)
        {
            StopDirectionInstance();
        }
        else
        {
            CreateDrectionInstance(track);
        }
    }

    /// <summary>
    /// 再生する演出のプレハブをインスタンス化
    /// </summary>
    private void CreateDrectionInstance(int track)
    {
        if (directionInstance != null)
        {
            Destroy(directionInstance);
            directionInstance = null;
        }
        directionInstance = Instantiate(lightDirection[track], directionParent);
        directionInstance.SetActive(false);
        _directionInstanceDirection = directionInstance.GetComponent<PlayableDirector>();
        Debug.Log($"[<color=#CD853F>{APP_NAME}</color>] Created Direction");
    }

    private void StopDirectionInstance()
    {
        if(directionInstance == null)
        {
            return;
        }

        Destroy(directionInstance);
        directionInstance = null;
        _directionInstanceDirection = null;
        if (defaultLightObject != null) defaultLightObject.SetActive(true);
        if (fadePPSObject.activeSelf)
        {
            FadePPS("EndStart");
        }
        Debug.Log($"[<color=#CD853F>{APP_NAME}</color>] StopPlay");
    }

    /// <summary>
    /// 再生開始の処理(ローカル処理)
    /// </summary>
    public void StartPlayBack()
    {
        Debug.Log($"[<color=#CD853F>{APP_NAME}</color>] Track:{_track}");
        //videoPlayerの再生を0にする
        if (Networking.IsOwner(gameObject))
        {
            OnProgressChanged();
        }

        //演出のアニメーションスイッチのActive化(lateJoiner用の対策が必要)
        //メモ：止める処理も追加が必要
        //再生する対象のTimelineを探す
        if (_track != -1 && lightDirection[_track] != null && directionInstance != null)
        {
            if (defaultLightObject != null) defaultLightObject.SetActive(false);
            directionInstance.SetActive(true);
            if (isOnStage)
            {
                _directionInstanceDirection.time = delayTime / 1000f;
            }
            _directionInstanceDirection.Play();
            SendCustomEventDelayedSeconds(nameof(SyncTimePeriodic), 0.5f);
        }

        //全員の音量を統一的に指定
        OnVolumeChanged(audioVolume);
    }

    /// <summary>
    /// 壇上に上がる/下がる際のタイムラグを吸収する処理
    /// </summary>
    private void CreateTimelagStageORFloor()
    {
        if (isOnStage)
        {
            //動画のタイムラグ処理
            core.offsetTime = delayTime;

            //演出アニメーションのタイムラグ処理
            if (directionInstance != null)
            {
                _directionInstanceDirection.time += delayTime / 1000f;
            }
        }
        else
        {
            //動画のタイムラグ処理
            core.offsetTime = 0;

            //演出アニメーションのタイムラグ処理
            if (directionInstance != null)
            {
                _directionInstanceDirection.time -= delayTime / 1000f;
            }
        }
    }

    /// <summary>
    /// 動画の再生時間を0にする関数
    /// </summary>
    private void OnProgressChanged()
    {
        Debug.Log($"[<color=#CD853F>{GetType().Name}</color>] Progress changed.");
        core.TakeOwnership();
        core.clockTime = Networking.GetServerTimeInMilliseconds();
        core.time = 0.0001f;
        core.RequestSerialization();
    }

    /// <summary>
    /// 動画の音量を設定する処理
    /// </summary>
    /// <param name="valume"></param>
    private void OnVolumeChanged(float valume)
    {
        Debug.Log($"[<color=#CD853F>{GetType().Name}</color>] Volume changed.");
        core.speakerVolume = valume;
    }

    public void SyncTimePeriodic()
    {
        if (_directionInstanceDirection != null)
        {
            if (Mathf.Abs(core.time - (float)_directionInstanceDirection.time) > syncThreshold)
            {
                _directionInstanceDirection.time = core.time;
                Debug.Log($"[<color=#CD853F>{APP_NAME}</color>] Director ReSynced.");
            }
            SendCustomEventDelayedSeconds(nameof(SyncTimePeriodic), syncFrequency);
        }
        else
        {
            return;
        }
    }

    private void FadePPS(string fadeState)
    {
        fadePPSObject.SetActive(true);
        fadeAnimator.SetTrigger(fadeState);
        fadeAnimator.SetFloat("Speed", 1.0f / fadeTime);
    }

    private void PauseOnAll()
    {
        if (_directionInstanceDirection == null) return;
        _directionInstanceDirection.Pause();
    }

    private void PauseOffAll()
    {
        if (_directionInstanceDirection == null) return;
        _directionInstanceDirection.Play();
    }

    private void ChangeDirectionProgress()
    {
        float progressRate = _progressRate;
        if (_directionInstanceDirection == null) return;
        _directionInstanceDirection.time = core.duration * (double)progressRate + (isOnStage ? 0.6 : 0);
    }
}
