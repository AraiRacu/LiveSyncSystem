# LiveSyncSystem

バージョン: 1.0.0  
作成者: アライラク

## 概要
iwaSync3をベースとした、ダンスライブ向け同期システムです。

## 主な機能
- 演者の動画再生タイミングの調整によって、客席側で動画とダンスが一致して見える
- 独自プレイリストによって、ワンボタンで再生開始
- iwaSyncのプログレスバーの操作により、演出も同期
- 暗転機能
- 各種設定を1つのインスペクターで設定可能

## 導入方法
1. 事前にVRCWorld向けのプロジェクトを作成し、iwaSync3をインポートする
2. Project Settings>Tags and LayersでUser Layer 22に"PostProcessing"を追加する
3. 本unitypackageをD&Dでプロジェクトに適用する
4. Assets/RacoonStudio/LiveSyncSystem以下のSceneの展開または、シーンにPrefabを配置する
5. RacoonStudioLiveSystem>StageColliderのPosition・Scaleをライブステージ上になるように調整する
6. RacoonStudioLiveSystem>iwasyncSet>iwaSync3-Speakerをライブステージ上の音源の位置に設置する
7. RacoonStudioLiveSystem>iwasyncSet>PlayList及びRacoonStudioLiveSystem>iwasyncSet>iwaSync3-Controllerをスタッフルーム等、操作する場所に設置する

## 使い方
1. 演出オブジェクトの作成と設置
   1. 演出オブジェクトは一番親のGameObjectにPlayableDirectorコンポーネントを追加し、Timelineをセットする
   2. RacoonStudioLiveSystem>LightDirections以下に演出オブジェクトを非アクティブ状態で設置する
2. 演出と映像のアタッチ
   1. RacoonStudioLiveSystem>SetListSettingのUI Button GeneratorのButtonsを追加し、起動ボタンの表示テキスト、動画のURL、1.2で設置した演出オブジェクトをアタッチする\
    再生前後に暗転が必要な場合はIs Fadeにチェックを入れる
   2. "ボタンを生成する"を押下すると再生ボタンの作成及び再生の設定がされる\
    ※本コンポーネントのプロパティを変更する度にボタンを押さないと適用がされない
3. 同期システムの詳細設定
   1. 同期の設定はRacoonStudioLiveSystem>SetListSettingで設定する\
   　※変更する際は"ボタンを生成する"を押さないと適用がされない
      - DefaultLightObject : 演出再生をしていない際にアクティブになるオブジェクト\
      DirectionalLightなど、演出中に非表示にしておきたい際に利用
      - Buffer Time : 動画を事前にロードするための時間[s]
      - Delay Time : 演者と観客との再生時間をずらす値[ms]
      - Audio Volume : iwaSyncのボリューム[0~1]
      - Sync Frequency : 同期処理チェックをする頻度[s]
      - Sync Threshold : マスターの再生時間の差がこの値以上になった場合に同期処理を実施する[s]
      - Fade Time : 暗転の時間[s]


## 前提条件
- iwaSync3 v3.7.1

## 依存関係
- Unity バージョン: 2022.3.22f1
- VRCSDK3: 3.8.2

## 更新履歴
- 1.0.0: 初回リリース