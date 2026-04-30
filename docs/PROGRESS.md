# 三十六計 〜天下統一への道〜 実装進捗

## 概要

このドキュメントでは、プロジェクトの実装進捗を記録します。

**最終更新**: 2026-04-30
**現在のフェーズ**: Phase 12 完了

---

## 実装済みフェーズ一覧

| Phase | 名称 | 状態 | PR |
|-------|------|------|-----|
| 1 | プロジェクト基盤 | 完了 | #1-#3 |
| 2 | データ基盤 | 完了 | #4-#7 |
| 3 | コアシステム | 完了 | #8-#12 |
| 4 | 計略システム | 完了 | #13-#19 |
| 5 | 戦闘システム | 完了 | #20-#23 |
| 6 | AIシステム | 完了 | #24-#26 |
| 7 | UI実装 | 完了 | #27-#32 |
| 8 | キャンペーンモード | 完了 | #33-#36 |
| 9 | 仕上げ（基本） | 完了 | #37-#40 |
| 10 | システム統合 | 完了 | #41-#42 |
| 11 | シナリオデータ | 完了 | #43-#44 |
| 12 | シーン/UI統合 | 完了 | #45-#47 |

---

## 詳細実装内容

### Phase 1: プロジェクト基盤

- Unity 2022.3 LTS プロジェクト作成
- ディレクトリ構造とアセンブリ定義
- 基本シーン作成（Title, MainMenu, Game, Battle）

### Phase 2: データ基盤

**モデルクラス** (`Assets/Scripts/Data/Models/`)
- `Territory.cs` - 領地モデル
- `Faction.cs` - 勢力モデル
- `Character.cs` - 武将モデル
- `Army.cs` - 軍隊モデル
- `Enums.cs` - 列挙型定義

**ScriptableObject** (`Assets/Scripts/Data/ScriptableObjects/`)
- `TerritoryData.cs`, `TerritoryDatabase.cs`
- `FactionData.cs`, `FactionDatabase.cs`
- `CharacterData.cs`, `CharacterDatabase.cs`
- `StratagemData.cs`, `StratagemDatabase.cs`
- `MapData.cs`
- `ScenarioDatabase.cs`

**マスターデータ定義**
- `StratagemDefinitions.cs` - 36計略の定義
- `SampleDataDefinitions.cs` - サンプルデータ

### Phase 3: コアシステム

**Core** (`Assets/Scripts/Core/`)
- `GameManager.cs` - ゲーム全体の管理（シングルトン）
- `TurnManager.cs` - ターン・フェーズ管理
- `ResourceManager.cs` - リソース（金・兵糧・計略ポイント）管理
- `EventBus.cs` - イベント駆動システム
- `SaveLoadManager.cs` - セーブ/ロード管理
- `Constants.cs` - 定数定義

### Phase 4: 計略システム

**Stratagem** (`Assets/Scripts/Stratagem/`)
- `StratagemManager.cs` - 計略の実行管理
- `StratagemConditionChecker.cs` - 発動条件チェック
- `StratagemEffectProcessor.cs` - 効果処理

**実装済み計略カテゴリ**:
- 勝戦計（1-6）: 瞞天過海、囲魏救趙、借刀殺人、以逸待労、趁火打劫、声東撃西
- 敵戦計（7-12）: 無中生有、暗渡陳倉、隔岸観火、笑裏蔵刀、李代桃僵、順手牽羊
- 攻戦計（13-18）: 打草驚蛇、借屍還魂、調虎離山、欲擒姑縦、拋磚引玉、擒賊擒王
- 混戦計（19-24）: 釜底抽薪、混水摸魚、金蝉脱殻、関門捉賊、遠交近攻、仮道伐虢
- 併戦計（25-30）: 偷梁換柱、指桑罵槐、仮痴不癲、上屋抽梯、樹上開花、反客為主
- 敗戦計（31-36）: 美人計、空城計、反間計、苦肉計、連環計、走為上

### Phase 5: 戦闘システム

**Battle** (`Assets/Scripts/Battle/`)
- `BattleManager.cs` - 戦闘の開始・進行・終了管理
- `BattleCalculator.cs` - 戦闘力・ダメージ計算
- `ArmyManager.cs` - 軍の移動・管理

**主な機能**:
- ラウンド制戦闘
- 地形効果
- 士気システム
- 武将スキル効果
- 戦闘中計略発動
- 勝敗判定・捕虜処理

### Phase 6: AIシステム

**AI** (`Assets/Scripts/AI/`)
- `AIManager.cs` - AI全体の制御
- `FactionAI.cs` - 勢力の意思決定
- `BattleAI.cs` - 戦闘中のAI判断
- `StratagemAI.cs` - 計略選択AI

**AI性格タイプ**:
- Aggressive（攻撃的）
- Defensive（防御的）
- Balanced（バランス型）
- Strategic（戦略的）

### Phase 7: UI実装

**メニューUI** (`Assets/Scripts/UI/Menu/`)
- `MainMenuUI.cs` - メインメニュー
- `SaveLoadPanel.cs` - セーブ/ロード画面
- `SaveSlotUI.cs` - セーブスロット
- `ScenarioSelectPanel.cs` - シナリオ選択
- `SettingsPanel.cs` - 設定画面

**HUD** (`Assets/Scripts/UI/HUD/`)
- `GameHUD.cs` - ゲーム中HUD
- `FactionInfoPanel.cs` - 勢力情報
- `PauseMenuPanel.cs` - ポーズメニュー

**戦闘UI** (`Assets/Scripts/UI/Battle/`)
- `BattlePanel.cs` - 戦闘メイン画面
- `BattleStratagemPanel.cs` - 戦闘中計略選択
- `BattleUnitDisplay.cs` - ユニット表示

**計略UI** (`Assets/Scripts/UI/Stratagem/`)
- `StratagemListPanel.cs` - 計略一覧
- `StratagemListItem.cs` - 計略アイテム
- `StratagemDetailPanel.cs` - 計略詳細

**その他UI**
- `TerritoryInfoPanel.cs` - 領地情報
- `CharacterInfoPanel.cs` - 武将情報
- `NotificationSystem.cs` - 通知システム
- `EventLogPanel.cs` - イベントログ

### Phase 8: キャンペーンモード

**Campaign** (`Assets/Scripts/Campaign/`)
- `CampaignManager.cs` - キャンペーン管理
- `TutorialSystem.cs` - チュートリアル
- `VictoryConditionSystem.cs` - 勝利条件
- `ScenarioLoader.cs` - シナリオ読み込み
- `SampleScenarioCreator.cs` - サンプルシナリオ生成

**UI**
- `TutorialPanel.cs` - チュートリアル画面
- `GameOverPanel.cs` - ゲームオーバー画面

### Phase 9: 仕上げ（基本）

**Systems** (`Assets/Scripts/Systems/`)
- `AudioManager.cs` - 音声管理
- `SettingsManager.cs` - 設定管理
- `LocalizationSystem.cs` - 多言語対応
- `LocalizedText.cs` - ローカライズテキスト
- `VisualEffectsManager.cs` - 視覚エフェクト
- `UITransitionManager.cs` - UI遷移
- `LoadingScreen.cs` - ローディング画面
- `GameBootstrap.cs` - ゲーム起動処理
- `GameInitializer.cs` - 初期化処理
- `DebugConsole.cs` - デバッグコンソール
- `FPSCounter.cs` - FPS表示
- `SaveLoadSystem.cs` - セーブ/ロード統合

### Phase 10: システム統合

- GameManager拡張（データベース統合）
- EventBus拡張（イベント追加）
- 各システム間の連携強化

### Phase 11: シナリオデータ

- `ScenarioDatabase.cs` - シナリオデータベース
- `ScenarioLoader.cs` - シナリオローダー
- `SampleScenarioCreator.cs` - サンプルシナリオ生成ツール
- GameManagerへのSetGameData/SetCurrentYear追加

### Phase 12: シーン/UI統合

**Phase 12-1: シーンセットアップツール** (PR #45)
- `SceneSetupHelper.cs` - シーン初期化ヘルパー
- `PrefabGenerator.cs` - プレハブ自動生成

**Phase 12-2: GameSceneコンポーネント** (PR #46)
- `GameSceneController.cs` - ゲームシーン制御
  - カメラ操作（パン、ズーム、キーボード移動）
  - 領地選択とハイライト
  - EventBus連携
- `MapDisplay.cs` - マップ描画
  - 領地の動的生成
  - 接続線の描画
  - 勢力色による表示
- `TerritoryVisual.cs` - 領地ビジュアル
  - 選択/ホバー状態
  - パルスアニメーション
  - ハイライト表示

**Phase 12-3: BattleSceneコンポーネント** (PR #47)
- `BattleSceneController.cs` - 戦闘シーン制御
  - シーン初期化
  - 計略パネル統合
  - BattleManager連携
- `BattleDisplay.cs` - 戦闘ビジュアル
  - 攻撃/防御アニメーション
  - ダメージテキスト表示
  - 体力バー更新
  - 勝敗アニメーション
- `BattleStratagemPanel.cs` - 戦闘中計略選択
  - 戦闘用計略フィルタリング
  - 成功率計算（知力ボーナス込み）
  - 計略ポイント確認

---

## ファイル構成

```
Assets/Scripts/
├── AI/                          # AIシステム
│   ├── AIManager.cs
│   ├── BattleAI.cs
│   ├── FactionAI.cs
│   └── StratagemAI.cs
├── Battle/                      # 戦闘システム
│   ├── ArmyManager.cs
│   ├── BattleCalculator.cs
│   └── BattleManager.cs
├── Campaign/                    # キャンペーン
│   ├── CampaignManager.cs
│   ├── SampleScenarioCreator.cs
│   ├── ScenarioLoader.cs
│   ├── TutorialSystem.cs
│   └── VictoryConditionSystem.cs
├── Core/                        # コアシステム
│   ├── Constants.cs
│   ├── EventBus.cs
│   ├── GameManager.cs
│   ├── ResourceManager.cs
│   ├── SaveLoadManager.cs
│   └── TurnManager.cs
├── Data/                        # データ層
│   ├── Models/
│   │   ├── Army.cs
│   │   ├── Character.cs
│   │   ├── Enums.cs
│   │   ├── Faction.cs
│   │   └── Territory.cs
│   ├── ScriptableObjects/
│   │   ├── CharacterData.cs
│   │   ├── CharacterDatabase.cs
│   │   ├── FactionData.cs
│   │   ├── FactionDatabase.cs
│   │   ├── MapData.cs
│   │   ├── ScenarioDatabase.cs
│   │   ├── StratagemData.cs
│   │   ├── StratagemDatabase.cs
│   │   ├── TerritoryData.cs
│   │   └── TerritoryDatabase.cs
│   ├── SampleDataDefinitions.cs
│   └── StratagemDefinitions.cs
├── Editor/                      # エディタツール
│   ├── MasterDataGenerator.cs
│   ├── PrefabGenerator.cs
│   └── SceneSetupHelper.cs
├── Scene/                       # シーン制御
│   ├── BattleDisplay.cs
│   ├── BattleSceneController.cs
│   ├── GameSceneController.cs
│   ├── MapDisplay.cs
│   └── TerritoryVisual.cs
├── Stratagem/                   # 計略システム
│   ├── StratagemConditionChecker.cs
│   ├── StratagemEffectProcessor.cs
│   └── StratagemManager.cs
├── Systems/                     # 汎用システム
│   ├── AudioManager.cs
│   ├── DebugConsole.cs
│   ├── FPSCounter.cs
│   ├── GameBootstrap.cs
│   ├── GameInitializer.cs
│   ├── LoadingScreen.cs
│   ├── LocalizationSystem.cs
│   ├── LocalizedText.cs
│   ├── SaveLoadSystem.cs
│   ├── SettingsManager.cs
│   ├── UITransitionManager.cs
│   └── VisualEffectsManager.cs
└── UI/                          # UI
    ├── Battle/
    │   ├── BattlePanel.cs
    │   ├── BattleStratagemPanel.cs
    │   └── BattleUnitDisplay.cs
    ├── Campaign/
    │   ├── GameOverPanel.cs
    │   └── TutorialPanel.cs
    ├── Character/
    │   └── CharacterInfoPanel.cs
    ├── HUD/
    │   ├── FactionInfoPanel.cs
    │   ├── GameHUD.cs
    │   └── PauseMenuPanel.cs
    ├── Menu/
    │   ├── MainMenuUI.cs
    │   ├── SaveLoadPanel.cs
    │   ├── SaveSlotUI.cs
    │   ├── ScenarioSelectPanel.cs
    │   └── SettingsPanel.cs
    ├── Notification/
    │   ├── EventLogPanel.cs
    │   ├── NotificationItem.cs
    │   └── NotificationSystem.cs
    ├── Stratagem/
    │   ├── StratagemDetailPanel.cs
    │   ├── StratagemListItem.cs
    │   └── StratagemListPanel.cs
    └── Territory/
        └── TerritoryInfoPanel.cs
```

---

## マイルストーン状況

| マイルストーン | 対象Phase | 状態 |
|---------------|-----------|------|
| Alpha版 | Phase 1-5 | 完了 |
| Beta版 | Phase 6-7 | 完了 |
| RC版 | Phase 8 | 完了 |
| Release準備 | Phase 9-12 | 完了 |

---

## 技術的な特徴

### アーキテクチャ
- **シングルトンパターン**: GameManager, BattleManager, AIManager等
- **イベント駆動**: EventBusによる疎結合な通信
- **ScriptableObject**: マスターデータの管理
- **コンポーネント指向**: Unity標準のMonoBehaviour

### デザインパターン
- **Strategy**: AI性格による行動変更
- **Observer**: EventBusによるイベント購読
- **Factory**: シナリオ・データ生成

### 主要な依存関係
- Unity 2022.3 LTS
- TextMeshPro
- Unity UI (uGUI)
