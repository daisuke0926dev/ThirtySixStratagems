# 三十六計 〜天下統一への道〜 実装計画

## 概要

このドキュメントでは、ゲーム開発をフェーズごとに分割し、各フェーズで作成するPRを定義します。
各PRは独立してレビュー・マージ可能な単位で設計されています。

---

## フェーズ一覧

| Phase | 名称 | 目標 | PR数 |
|-------|------|------|------|
| 1 | プロジェクト基盤 | Unity設定、基本構造 | 3 |
| 2 | データ基盤 | モデル、ScriptableObject | 4 |
| 3 | コアシステム | ターン制、基本ロジック | 5 |
| 4 | 計略システム | 36計略の実装 | 7 |
| 5 | 戦闘システム | 戦闘ロジック、演出 | 4 |
| 6 | AIシステム | CPU思考ロジック | 3 |
| 7 | UI実装 | 全画面のUI | 6 |
| 8 | キャンペーンモード | ストーリー、チュートリアル | 4 |
| 9 | 仕上げ | バランス調整、演出、音声 | 4 |

---

## Phase 1: プロジェクト基盤

### PR 1-1: Unityプロジェクト初期設定
**ブランチ名**: `feature/phase1-unity-setup`

**作業内容**:
- [ ] Unityプロジェクト作成 (Unity 2022.3 LTS)
- [ ] .gitignore設定 (Unity用)
- [ ] エディタ設定 (EditorSettings.asset)
- [ ] 品質設定 (QualitySettings)
- [ ] 入力設定 (InputManager)

**成果物**:
```
Assets/
├── Scenes/
│   └── _Empty.unity
├── Settings/
ProjectSettings/
```

---

### PR 1-2: ディレクトリ構造とアセンブリ定義
**ブランチ名**: `feature/phase1-directory-structure`

**作業内容**:
- [ ] Scriptsディレクトリ構造作成
- [ ] Assembly Definition設定
  - Core.asmdef
  - Data.asmdef
  - Systems.asmdef
  - UI.asmdef
  - Utils.asmdef
- [ ] 各ディレクトリにREADME配置

**成果物**:
```
Assets/Scripts/
├── Core/Core.asmdef
├── Data/Data.asmdef
├── Systems/Systems.asmdef
├── UI/UI.asmdef
└── Utils/Utils.asmdef
```

---

### PR 1-3: 基本シーン作成
**ブランチ名**: `feature/phase1-scenes`

**作業内容**:
- [ ] TitleScene.unity作成
- [ ] MainMenuScene.unity作成
- [ ] GameScene.unity作成
- [ ] BattleScene.unity作成 (空)
- [ ] Build Settings登録

---

## Phase 2: データ基盤

### PR 2-1: 基本モデルクラス
**ブランチ名**: `feature/phase2-models`

**作業内容**:
- [ ] Territory.cs (領地モデル)
- [ ] Faction.cs (勢力モデル)
- [ ] Character.cs (武将モデル)
- [ ] Army.cs (軍隊モデル)
- [ ] 列挙型定義 (Enums.cs)

**成果物**:
```
Assets/Scripts/Data/Models/
├── Territory.cs
├── Faction.cs
├── Character.cs
├── Army.cs
└── Enums.cs
```

---

### PR 2-2: ScriptableObject定義
**ブランチ名**: `feature/phase2-scriptable-objects`

**作業内容**:
- [ ] TerritoryData.cs
- [ ] FactionData.cs
- [ ] CharacterData.cs
- [ ] MapData.cs
- [ ] カスタムエディタ (基本)

---

### PR 2-3: 計略データ構造
**ブランチ名**: `feature/phase2-stratagem-data`

**作業内容**:
- [ ] StratagemData.cs (ScriptableObject)
- [ ] StratagemCategory列挙型
- [ ] StratagemCondition構造体
- [ ] StratagemEffect構造体

---

### PR 2-4: マスターデータ作成
**ブランチ名**: `feature/phase2-master-data`

**作業内容**:
- [ ] 36計略のScriptableObjectインスタンス作成
- [ ] サンプル領地データ (8領地)
- [ ] サンプル武将データ (10人)
- [ ] サンプル勢力データ (3勢力)

**成果物**:
```
Assets/Data/
├── Stratagems/
│   ├── 01_ManTianGuoHai.asset
│   ├── 02_WeiWeiJiuZhao.asset
│   └── ... (36個)
├── Territories/
├── Characters/
└── Factions/
```

---

## Phase 3: コアシステム

### PR 3-1: GameManager実装
**ブランチ名**: `feature/phase3-game-manager`

**作業内容**:
- [ ] GameManager.cs (シングルトン)
- [ ] GameState列挙型
- [ ] ゲーム初期化処理
- [ ] シーン遷移管理

---

### PR 3-2: EventBus実装
**ブランチ名**: `feature/phase3-event-bus`

**作業内容**:
- [ ] EventBus.cs (静的イベントシステム)
- [ ] ゲームイベント定義
- [ ] イベント発火/購読のユーティリティ

---

### PR 3-3: TurnManager実装
**ブランチ名**: `feature/phase3-turn-manager`

**作業内容**:
- [ ] TurnManager.cs
- [ ] TurnPhase列挙型
- [ ] ターン開始/終了処理
- [ ] フェーズ遷移処理
- [ ] 勢力ターン管理

---

### PR 3-4: リソース管理システム
**ブランチ名**: `feature/phase3-resource-system`

**作業内容**:
- [ ] EconomyManager.cs
- [ ] 収入計算ロジック
- [ ] 支出処理
- [ ] 兵糧消費システム

---

### PR 3-5: セーブ/ロードシステム
**ブランチ名**: `feature/phase3-save-system`

**作業内容**:
- [ ] SaveManager.cs
- [ ] SaveData.cs (シリアライズ構造)
- [ ] JSON保存/読み込み
- [ ] セーブスロット管理

---

## Phase 4: 計略システム

### PR 4-1: 計略システム基盤
**ブランチ名**: `feature/phase4-stratagem-base`

**作業内容**:
- [ ] StratagemManager.cs
- [ ] IStratagemEffect.cs (インターフェース)
- [ ] 計略発動条件チェック
- [ ] 成功率計算

---

### PR 4-2: 勝戦計実装 (1-6)
**ブランチ名**: `feature/phase4-winning-stratagems`

**作業内容**:
- [ ] 瞞天過海 (軍移動隠蔽)
- [ ] 囲魏救趙 (包囲解除誘導)
- [ ] 借刀殺人 (第三勢力利用)
- [ ] 以逸待労 (防御ボーナス)
- [ ] 趁火打劫 (混乱時攻撃強化)
- [ ] 声東撃西 (陽動作戦)

---

### PR 4-3: 敵戦計実装 (7-12)
**ブランチ名**: `feature/phase4-enemy-stratagems`

**作業内容**:
- [ ] 無中生有 (偽情報)
- [ ] 暗渡陳倉 (奇襲)
- [ ] 隔岸観火 (敵同士戦闘誘導)
- [ ] 笑裏蔵刀 (外交偽装)
- [ ] 李代桃僵 (犠牲防御)
- [ ] 順手牽羊 (資源略奪)

---

### PR 4-4: 攻戦計実装 (13-18)
**ブランチ名**: `feature/phase4-attack-stratagems`

**作業内容**:
- [ ] 打草驚蛇 (偵察)
- [ ] 借屍還魂 (勢力復活)
- [ ] 調虎離山 (武将誘引)
- [ ] 欲擒姑縦 (逃走許可)
- [ ] 拋磚引玉 (罠誘導)
- [ ] 擒賊擒王 (大将狙い)

---

### PR 4-5: 混戦計実装 (19-24)
**ブランチ名**: `feature/phase4-chaos-stratagems`

**作業内容**:
- [ ] 釜底抽薪 (補給線遮断)
- [ ] 混水摸魚 (混乱利用)
- [ ] 金蝉脱殻 (偽装撤退)
- [ ] 関門捉賊 (包囲殲滅)
- [ ] 遠交近攻 (外交戦略)
- [ ] 仮道伐虢 (通過侵略)

---

### PR 4-6: 併戦計実装 (25-30)
**ブランチ名**: `feature/phase4-merge-stratagems`

**作業内容**:
- [ ] 偷梁換柱 (部隊入替)
- [ ] 指桑罵槐 (間接警告)
- [ ] 仮痴不癲 (愚装)
- [ ] 上屋抽梯 (退路遮断)
- [ ] 樹上開花 (虚勢)
- [ ] 反客為主 (主導権奪取)

---

### PR 4-7: 敗戦計実装 (31-36)
**ブランチ名**: `feature/phase4-defeat-stratagems`

**作業内容**:
- [ ] 美人計 (忠誠度低下)
- [ ] 空城計 (威嚇防御)
- [ ] 反間計 (内部分裂)
- [ ] 苦肉計 (信用獲得)
- [ ] 連環計 (連続計略)
- [ ] 走為上 (完全撤退)

---

## Phase 5: 戦闘システム

### PR 5-1: 戦闘ロジック基盤
**ブランチ名**: `feature/phase5-battle-logic`

**作業内容**:
- [ ] BattleManager.cs
- [ ] BattleCalculator.cs
- [ ] 戦闘力計算式
- [ ] 損害計算

---

### PR 5-2: 戦闘結果処理
**ブランチ名**: `feature/phase5-battle-result`

**作業内容**:
- [ ] BattleResult.cs
- [ ] 勝敗判定
- [ ] 領地制圧処理
- [ ] 武将捕獲処理

---

### PR 5-3: 戦闘計略統合
**ブランチ名**: `feature/phase5-battle-stratagem`

**作業内容**:
- [ ] 戦闘中の計略発動
- [ ] 計略による戦闘力修正
- [ ] 特殊効果処理

---

### PR 5-4: 軍移動システム
**ブランチ名**: `feature/phase5-army-movement`

**作業内容**:
- [ ] 軍の移動ロジック
- [ ] 移動経路計算
- [ ] 兵糧消費
- [ ] 移動制限

---

## Phase 6: AIシステム

### PR 6-1: AI基盤
**ブランチ名**: `feature/phase6-ai-base`

**作業内容**:
- [ ] AIController.cs
- [ ] AIPersonality列挙型
- [ ] 状況分析ロジック
- [ ] 優先度計算

---

### PR 6-2: AI意思決定
**ブランチ名**: `feature/phase6-ai-decision`

**作業内容**:
- [ ] AIDecisionMaker.cs
- [ ] 内政AI
- [ ] 外交AI
- [ ] 軍事AI

---

### PR 6-3: AI計略使用
**ブランチ名**: `feature/phase6-ai-stratagem`

**作業内容**:
- [ ] 計略選択ロジック
- [ ] 状況に応じた計略判断
- [ ] 計略対策AI

---

## Phase 7: UI実装

### PR 7-1: UI基盤・共通コンポーネント
**ブランチ名**: `feature/phase7-ui-base`

**作業内容**:
- [ ] UIManager.cs
- [ ] ModalDialog.cs
- [ ] TooltipController.cs
- [ ] 共通UIスタイル

---

### PR 7-2: タイトル・メニューUI
**ブランチ名**: `feature/phase7-menu-ui`

**作業内容**:
- [ ] TitleUIController.cs
- [ ] MainMenuController.cs
- [ ] SettingsPanel.cs
- [ ] SaveLoadPanel.cs

---

### PR 7-3: マップUI
**ブランチ名**: `feature/phase7-map-ui`

**作業内容**:
- [ ] MapViewController.cs
- [ ] TerritoryUI.cs
- [ ] ArmyMarker.cs
- [ ] マップカメラ制御

---

### PR 7-4: 情報パネルUI
**ブランチ名**: `feature/phase7-info-ui`

**作業内容**:
- [ ] FactionInfoPanel.cs
- [ ] TerritoryInfoPanel.cs
- [ ] CharacterInfoPanel.cs
- [ ] TurnInfoBar.cs

---

### PR 7-5: 計略UI
**ブランチ名**: `feature/phase7-stratagem-ui`

**作業内容**:
- [ ] StratagemPanel.cs
- [ ] StratagemCard.cs
- [ ] StratagemDetailView.cs
- [ ] 計略発動演出

---

### PR 7-6: 戦闘UI
**ブランチ名**: `feature/phase7-battle-ui`

**作業内容**:
- [ ] BattleUIController.cs
- [ ] BattleResultPanel.cs
- [ ] 戦闘演出
- [ ] ダメージ表示

---

## Phase 8: キャンペーンモード

### PR 8-1: キャンペーン基盤
**ブランチ名**: `feature/phase8-campaign-base`

**作業内容**:
- [ ] CampaignManager.cs
- [ ] ChapterData.cs
- [ ] 章解放システム
- [ ] 進行状況保存

---

### PR 8-2: チュートリアル実装
**ブランチ名**: `feature/phase8-tutorial`

**作業内容**:
- [ ] TutorialManager.cs
- [ ] チュートリアルステップ定義
- [ ] ハイライト・矢印表示
- [ ] 操作制限

---

### PR 8-3: ストーリーイベント
**ブランチ名**: `feature/phase8-story-events`

**作業内容**:
- [ ] StoryEventManager.cs
- [ ] 会話システム
- [ ] イベントトリガー
- [ ] カットシーン

---

### PR 8-4: 計略図鑑
**ブランチ名**: `feature/phase8-encyclopedia`

**作業内容**:
- [ ] EncyclopediaManager.cs
- [ ] 計略詳細表示
- [ ] 解放状況表示
- [ ] 検索・フィルター

---

## Phase 9: 仕上げ

### PR 9-1: バランス調整
**ブランチ名**: `feature/phase9-balancing`

**作業内容**:
- [ ] 計略コスト調整
- [ ] AI難易度調整
- [ ] 戦闘バランス
- [ ] 経済バランス

---

### PR 9-2: ビジュアル演出
**ブランチ名**: `feature/phase9-visual-effects`

**作業内容**:
- [ ] パーティクルエフェクト
- [ ] 画面遷移エフェクト
- [ ] アニメーション追加

---

### PR 9-3: サウンド実装
**ブランチ名**: `feature/phase9-audio`

**作業内容**:
- [ ] AudioManager.cs
- [ ] BGM実装
- [ ] SE実装
- [ ] 音量設定

---

### PR 9-4: 最終調整・ビルド
**ブランチ名**: `feature/phase9-final`

**作業内容**:
- [ ] バグ修正
- [ ] パフォーマンス最適化
- [ ] ビルド設定
- [ ] リリース準備

---

## マイルストーン

| マイルストーン | 完了Phase | 状態 |
|---------------|-----------|------|
| Alpha版 | Phase 1-5 | プレイ可能な最小構成 |
| Beta版 | Phase 6-7 | AI対戦可能、UI完成 |
| RC版 | Phase 8 | キャンペーン完成 |
| Release | Phase 9 | 製品版 |

---

## コミット規約

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Type**:
- `feat`: 新機能
- `fix`: バグ修正
- `docs`: ドキュメント
- `style`: フォーマット
- `refactor`: リファクタリング
- `test`: テスト
- `chore`: ビルド・補助ツール

**例**:
```
feat(stratagem): implement 瞞天過海 effect

- Add army stealth movement for 1 turn
- Enemy cannot detect army movement
- Cost: 2 SP

Closes #15
```

---

## PR チェックリスト

各PRで確認する項目:

- [ ] コードがコンパイルできる
- [ ] 既存機能が壊れていない
- [ ] 新機能が正常に動作する
- [ ] コメント・ドキュメント追加
- [ ] コミットメッセージが規約に従っている
