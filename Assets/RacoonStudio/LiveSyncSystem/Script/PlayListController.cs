
using HoshinoLabs.IwaSync3;
using HoshinoLabs.IwaSync3.Udon;
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// URLの送信とトラックNoの管理用クラス
/// </summary>
/// <remarks>
/// 
/// </remarks>
public class PlayListController : UdonSharpBehaviour
{
    public const string APP_NAME = "PlayListController";
    const uint _MODE_VIDEO = 0x00000010;

    [SerializeField] private VideoCore core = null;
    [SerializeField] private RacoonStudioLiveSyncSystem liveSyncSystem = null;

    [SerializeField] public Button[] _trackButtons = null;
    [SerializeField] public VRCUrl[] _trackURLs = null;

    private int _contentLength = 0;
    [UdonSynced, FieldChangeCallback(nameof(Track))] private int _track = -1;

    public int Track
    {
        set { _track = value; CreateDirection(); }
        get => _track;
    }

    void Start()
    {
        _contentLength = _trackButtons.Length;
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (player == Networking.LocalPlayer)
        {
            SendCustomEventDelayedSeconds(nameof(SyncPerformance), 3);
        }
    }

    public void SyncPerformance()
    {
        Debug.Log($"[<color =#CD853F>{APP_NAME}</color>] TrackNo is:{_track}");
        Debug.Log($"[<color =#CD853F>{APP_NAME}</color>] Player is:{core.isPlaying}");
        if (_track != -1)
        {
            Debug.Log($"[<color =#CD853F>{APP_NAME}</color>] StartSync:{DateTime.Now.ToString()}.{DateTime.Now.Millisecond.ToString()}");
            liveSyncSystem.ChangeTrackIndex(_track);
            liveSyncSystem.StartPlayBack();
            IntractiveButtonControl();
        }
    }

    public void TakeOwnership()
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    public void OnPlayVideo()
    {
        var sender = FindSender();
        if (sender < 0)
            return;

        TakeOwnership();
        PlayTrack(sender);
        RequestSerialization();
    }

    int FindSender()
    {
        for (var i = 0; i < _contentLength; i++)
        {
            if (!_trackButtons[i].enabled)
                return i;
        }
        return -1;
    }

    void PlayTrack(int track)
    {
        if (_track != -1)
        {
            core.Stop();
            _trackButtons[_track].interactable = true;
        }
        core.TakeOwnership();
        core.PlayURL(_MODE_VIDEO, _trackURLs[track]);
        core.RequestSerialization();
        liveSyncSystem.CreateVideoSync();
        Track = track;
    }

    private void CreateDirection()
    {
        liveSyncSystem.ChangeTrackIndex(_track);
        IntractiveButtonControl();
    }

    public void StopPlayOwner()
    {
        TakeOwnership();

        core.TakeOwnership();
        core.Stop();
        core.clockTime = Networking.GetServerTimeInMilliseconds();
        core.time = 0;
        core.RequestSerialization();

        liveSyncSystem.TakeOwnership();

        Track = -1;
        RequestSerialization();
    }

    private void IntractiveButtonControl()
    {
        //最初の6秒間はどのボタンも押せなくする(RateLimited対策)
        foreach(Button button in _trackButtons)
        {
            button.interactable = false;
        }

        //6秒後，活性化するメソッドを起動(オブジェクトマスター以外は動画ロード時間も使用不可)
        if (Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            SendCustomEventDelayedSeconds(nameof(ActiveButtons), 6.0f);
        }
        else
        {
            SendCustomEventDelayedSeconds(nameof(ActiveButtons), liveSyncSystem.bufferTime > 6.0f ? liveSyncSystem.bufferTime : 6.0f);
        }
    }

    public void ActiveButtons()
    {
        foreach (Button button in _trackButtons)
        {
            button.interactable = true;
        }

        if (_track != -1)
        {
            _trackButtons[_track].interactable = false;
        }
    }
}
