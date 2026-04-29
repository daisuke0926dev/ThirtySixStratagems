# 三十六計 〜天下統一への道〜 設計書

## 1. アーキテクチャ概要

### 1.1 全体構成

```
┌─────────────────────────────────────────────────────────┐
│                    Unity Application                     │
├─────────────────────────────────────────────────────────┤
│  Presentation Layer (UI/View)                            │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐       │
│  │ MapView │ │BattleUI │ │MenuUI   │ │DialougeUI│       │
│  └─────────┘ └─────────┘ └─────────┘ └─────────┘       │
├─────────────────────────────────────────────────────────┤
│  Game Logic Layer (Controllers/Managers)                 │
│  ┌───────────┐ ┌───────────┐ ┌───────────┐             │
│  │GameManager│ │TurnManager│ │BattleManager│            │
│  └───────────┘ └───────────┘ └───────────┘             │
│  ┌───────────┐ ┌───────────┐ ┌───────────┐             │
│  │AIController│ │StratagemMgr│ │DiplomacyMgr│           │
│  └───────────┘ └───────────┘ └───────────┘             │
├─────────────────────────────────────────────────────────┤
│  Data Layer (Models/ScriptableObjects)                   │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐       │
│  │Territory│ │Faction  │ │Character│ │Stratagem│       │
│  └─────────┘ └─────────┘ └─────────┘ └─────────┘       │
├─────────────────────────────────────────────────────────┤
│  Infrastructure Layer                                    │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐                   │
│  │SaveSystem│ │AudioMgr │ │EventBus │                   │
│  └─────────┘ └─────────┘ └─────────┘                   │
└─────────────────────────────────────────────────────────┘
```

### 1.2 設計原則

- **MVC/MVP パターン**: UIとロジックの分離
- **ScriptableObject**: ゲームデータの定義
- **Event-Driven**: コンポーネント間の疎結合
- **Single Responsibility**: 各クラスは単一責任

---

## 2. ディレクトリ構成

```
Assets/
├── Scenes/
│   ├── TitleScene.unity
│   ├── MainMenuScene.unity
│   ├── GameScene.unity
│   └── BattleScene.unity
│
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs
│   │   ├── TurnManager.cs
│   │   ├── EventBus.cs
│   │   └── Constants.cs
│   │
│   ├── Data/
│   │   ├── Models/
│   │   │   ├── Territory.cs
│   │   │   ├── Faction.cs
│   │   │   ├── Character.cs
│   │   │   ├── Army.cs
│   │   │   └── Stratagem.cs
│   │   │
│   │   └── ScriptableObjects/
│   │       ├── TerritoryData.cs
│   │       ├── FactionData.cs
│   │       ├── CharacterData.cs
│   │       ├── StratagemData.cs
│   │       └── MapData.cs
│   │
│   ├── Systems/
│   │   ├── Battle/
│   │   │   ├── BattleManager.cs
│   │   │   ├── BattleCalculator.cs
│   │   │   └── BattleResult.cs
│   │   │
│   │   ├── Stratagem/
│   │   │   ├── StratagemManager.cs
│   │   │   ├── StratagemExecutor.cs
│   │   │   └── Effects/
│   │   │       ├── IStratagemEffect.cs
│   │   │       ├── MilitaryEffects.cs
│   │   │       ├── DiplomacyEffects.cs
│   │   │       └── IntelligenceEffects.cs
│   │   │
│   │   ├── AI/
│   │   │   ├── AIController.cs
│   │   │   ├── AIStrategy.cs
│   │   │   └── AIDecisionMaker.cs
│   │   │
│   │   ├── Diplomacy/
│   │   │   ├── DiplomacyManager.cs
│   │   │   └── Alliance.cs
│   │   │
│   │   ├── Economy/
│   │   │   ├── EconomyManager.cs
│   │   │   └── ResourceCalculator.cs
│   │   │
│   │   └── Save/
│   │       ├── SaveManager.cs
│   │       ├── SaveData.cs
│   │       └── SaveSerializer.cs
│   │
│   ├── UI/
│   │   ├── Common/
│   │   │   ├── UIManager.cs
│   │   │   ├── ModalDialog.cs
│   │   │   └── TooltipController.cs
│   │   │
│   │   ├── Map/
│   │   │   ├── MapViewController.cs
│   │   │   ├── TerritoryUI.cs
│   │   │   └── ArmyMarker.cs
│   │   │
│   │   ├── Battle/
│   │   │   ├── BattleUIController.cs
│   │   │   └── BattleResultPanel.cs
│   │   │
│   │   ├── Stratagem/
│   │   │   ├── StratagemPanel.cs
│   │   │   ├── StratagemCard.cs
│   │   │   └── StratagemDetailView.cs
│   │   │
│   │   └── Menu/
│   │       ├── MainMenuController.cs
│   │       ├── SettingsPanel.cs
│   │       └── SaveLoadPanel.cs
│   │
│   └── Utils/
│       ├── Extensions.cs
│       ├── MathUtils.cs
│       └── Localization.cs
│
├── Data/
│   ├── Stratagems/          # ScriptableObject instances
│   ├── Characters/
│   ├── Territories/
│   ├── Maps/
│   └── Campaigns/
│
├── Prefabs/
│   ├── UI/
│   ├── Map/
│   └── Effects/
│
├── Art/
│   ├── Sprites/
│   ├── UI/
│   └── Icons/
│
├── Audio/
│   ├── BGM/
│   ├── SE/
│   └── Voice/
│
└── Resources/
    └── Localization/
```

---

## 3. コアクラス設計

### 3.1 GameManager

```csharp
// ゲーム全体を統括するシングルトン
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 現在のゲーム状態
    public GameState CurrentState { get; private set; }

    // 各マネージャーへの参照
    public TurnManager TurnManager { get; private set; }
    public BattleManager BattleManager { get; private set; }
    public StratagemManager StratagemManager { get; private set; }
    public DiplomacyManager DiplomacyManager { get; private set; }
    public SaveManager SaveManager { get; private set; }

    // ゲームデータ
    public GameData CurrentGame { get; private set; }

    // メソッド
    public void StartNewGame(MapData map, FactionData playerFaction);
    public void LoadGame(SaveData saveData);
    public void EndGame(GameEndReason reason);
}

public enum GameState
{
    Title,
    MainMenu,
    Playing,
    Paused,
    Battle,
    GameOver
}
```

### 3.2 TurnManager

```csharp
// ターン進行を管理
public class TurnManager : MonoBehaviour
{
    public int CurrentTurn { get; private set; }
    public TurnPhase CurrentPhase { get; private set; }
    public Faction CurrentFaction { get; private set; }

    // イベント
    public event Action<int> OnTurnStart;
    public event Action<TurnPhase> OnPhaseChange;
    public event Action<Faction> OnFactionTurnStart;

    // メソッド
    public void StartTurn();
    public void NextPhase();
    public void EndTurn();
    public void NextFaction();
}

public enum TurnPhase
{
    Internal,    // 内政フェーズ
    Diplomacy,   // 外交フェーズ
    Military     // 軍事フェーズ
}
```

### 3.3 Territory (領地モデル)

```csharp
[Serializable]
public class Territory
{
    public string Id;
    public string Name;

    // 所有者
    public Faction Owner;

    // パラメータ
    public int Population;        // 人口
    public int Economy;           // 経済力
    public int Defense;           // 防御力

    // 駐留軍
    public Army GarrisonArmy;

    // 隣接領地
    public List<Territory> AdjacentTerritories;

    // 施設
    public List<Building> Buildings;

    // 計算プロパティ
    public int Income => CalculateIncome();
    public int MaxRecruitment => Population / 10;
}
```

### 3.4 Faction (勢力モデル)

```csharp
[Serializable]
public class Faction
{
    public string Id;
    public string Name;
    public Color FactionColor;

    // 君主
    public Character Ruler;

    // リソース
    public int Gold;
    public int Food;
    public int StratagemPoints;
    public int MaxStratagemPoints;

    // 所有
    public List<Territory> Territories;
    public List<Character> Characters;
    public List<Army> Armies;

    // 外交
    public List<Alliance> Alliances;
    public Dictionary<Faction, int> Relations; // -100 to 100

    // 解放済み計略
    public List<Stratagem> UnlockedStratagems;

    // プレイヤーかCPUか
    public bool IsPlayer;
}
```

### 3.5 Character (武将モデル)

```csharp
[Serializable]
public class Character
{
    public string Id;
    public string Name;
    public CharacterType Type;

    // 能力値
    public int Strength;    // 武力
    public int Intelligence; // 知力
    public int Leadership;  // 統率
    public int Politics;    // 政治
    public int Charisma;    // 魅力

    // 状態
    public int Loyalty;     // 忠誠度 (0-100)
    public int Health;      // 体力

    // 所属
    public Faction BelongsTo;
    public Territory Location;

    // 得意計略カテゴリ
    public StratagemCategory SpecialtyCategory;

    // 計算プロパティ
    public int MaxCommandableArmy => Leadership * 100;
    public int StratagemPointRecovery => Intelligence / 20;
}

public enum CharacterType
{
    Ruler,      // 君主
    Strategist, // 軍師
    General,    // 武将
    Spy         // 間者
}
```

### 3.6 Stratagem (計略モデル)

```csharp
[CreateAssetMenu(fileName = "Stratagem", menuName = "Game/Stratagem")]
public class StratagemData : ScriptableObject
{
    [Header("基本情報")]
    public string Id;
    public string NameJP;           // 日本語名
    public string NameCN;           // 中国語名
    public string Reading;          // 読み方
    public int Number;              // 三十六計の番号

    [Header("カテゴリ")]
    public StratagemCategory Category;

    [Header("説明")]
    [TextArea(3, 5)]
    public string OriginalText;     // 原典
    [TextArea(3, 5)]
    public string ModernTranslation; // 現代語訳
    [TextArea(3, 5)]
    public string HistoricalExample; // 歴史的使用例
    [TextArea(2, 4)]
    public string GameEffect;       // ゲーム内効果説明

    [Header("ゲームパラメータ")]
    public int CostSP;              // 消費計略ポイント
    public int CostGold;            // 消費金
    public StratagemTarget TargetType;
    public int SuccessRate;         // 基本成功率 (%)
    public int Duration;            // 効果持続ターン

    [Header("発動条件")]
    public StratagemCondition[] Conditions;

    [Header("効果")]
    public StratagemEffectType EffectType;
    public int EffectValue;

    [Header("リソース")]
    public Sprite Icon;
}

public enum StratagemCategory
{
    Winning,    // 勝戦計
    Enemy,      // 敵戦計
    Attack,     // 攻戦計
    Chaos,      // 混戦計
    Merge,      // 併戦計
    Defeat      // 敗戦計
}

public enum StratagemTarget
{
    Self,           // 自勢力
    EnemyFaction,   // 敵勢力
    EnemyArmy,      // 敵軍
    EnemyTerritory, // 敵領地
    EnemyCharacter, // 敵武将
    AllyFaction,    // 同盟勢力
    Any             // 任意
}
```

### 3.7 Army (軍隊モデル)

```csharp
[Serializable]
public class Army
{
    public string Id;
    public Faction Owner;

    // 兵力
    public int Soldiers;

    // 指揮官
    public Character Commander;
    public List<Character> Officers;

    // 位置
    public Territory Location;

    // 状態
    public ArmyState State;
    public int Morale;          // 士気 (0-100)
    public int Supplies;        // 兵糧

    // 計算プロパティ
    public int CombatPower => CalculateCombatPower();

    private int CalculateCombatPower()
    {
        float commanderBonus = 1 + (Commander?.Strength ?? 0) / 100f;
        float moraleBonus = Morale / 100f;
        return (int)(Soldiers * commanderBonus * moraleBonus);
    }
}

public enum ArmyState
{
    Idle,       // 待機
    Moving,     // 移動中
    Sieging,    // 包囲中
    Defending,  // 防衛中
    Retreating  // 撤退中
}
```

---

## 4. システム設計

### 4.1 戦闘システム

```csharp
public class BattleManager : MonoBehaviour
{
    public BattleResult ResolveBattle(Army attacker, Army defender, Territory location)
    {
        var result = new BattleResult();

        // 基本戦闘力計算
        int attackPower = CalculateAttackPower(attacker);
        int defensePower = CalculateDefensePower(defender, location);

        // 計略効果適用
        attackPower = ApplyStratagemModifiers(attackPower, attacker);
        defensePower = ApplyStratagemModifiers(defensePower, defender);

        // 勝敗判定
        float ratio = (float)attackPower / defensePower;
        result.Victor = ratio > 1.0f ? attacker.Owner : defender.Owner;

        // 損害計算
        result.AttackerLosses = CalculateLosses(attacker, ratio, false);
        result.DefenderLosses = CalculateLosses(defender, ratio, true);

        return result;
    }
}

public class BattleResult
{
    public Faction Victor;
    public int AttackerLosses;
    public int DefenderLosses;
    public List<Character> CapturedCharacters;
    public bool TerritoryConquered;
}
```

### 4.2 計略システム

```csharp
public class StratagemManager : MonoBehaviour
{
    private Dictionary<string, IStratagemEffect> _effects;

    public StratagemResult Execute(
        StratagemData stratagem,
        Faction executor,
        object target,
        Character caster)
    {
        // 発動条件チェック
        if (!CheckConditions(stratagem, executor, target))
        {
            return new StratagemResult { Success = false, Reason = "条件未達成" };
        }

        // コスト支払い
        if (!PayCost(stratagem, executor))
        {
            return new StratagemResult { Success = false, Reason = "リソース不足" };
        }

        // 成功判定
        int successRate = CalculateSuccessRate(stratagem, caster);
        bool success = Random.Range(0, 100) < successRate;

        if (success)
        {
            // 効果適用
            var effect = _effects[stratagem.EffectType.ToString()];
            effect.Apply(stratagem, executor, target);
        }

        return new StratagemResult
        {
            Success = success,
            Stratagem = stratagem
        };
    }
}

// 計略効果インターフェース
public interface IStratagemEffect
{
    void Apply(StratagemData stratagem, Faction executor, object target);
    void Remove(StratagemData stratagem, Faction executor, object target);
}
```

### 4.3 AIシステム

```csharp
public class AIController : MonoBehaviour
{
    public AIPersonality Personality;

    public void ExecuteTurn(Faction faction)
    {
        // 内政フェーズ
        ExecuteInternalPhase(faction);

        // 外交フェーズ
        ExecuteDiplomacyPhase(faction);

        // 軍事フェーズ
        ExecuteMilitaryPhase(faction);
    }

    private void ExecuteMilitaryPhase(Faction faction)
    {
        // 状況分析
        var situation = AnalyzeSituation(faction);

        // 最適な行動を決定
        var decisions = _decisionMaker.MakeDecisions(faction, situation);

        // 行動実行
        foreach (var decision in decisions)
        {
            ExecuteDecision(decision);
        }
    }
}

public enum AIPersonality
{
    Aggressive,  // 攻撃的
    Defensive,   // 防御的
    Diplomatic,  // 外交重視
    Strategic,   // 計略重視
    Balanced     // バランス型
}
```

### 4.4 セーブシステム

```csharp
[Serializable]
public class SaveData
{
    public int SaveVersion;
    public DateTime SaveTime;

    // ゲーム状態
    public int CurrentTurn;
    public string CurrentFactionId;

    // 勢力データ
    public List<FactionSaveData> Factions;

    // マップデータ
    public List<TerritorySaveData> Territories;

    // キャラクターデータ
    public List<CharacterSaveData> Characters;

    // 軍隊データ
    public List<ArmySaveData> Armies;
}

public class SaveManager : MonoBehaviour
{
    private const string SAVE_FOLDER = "Saves";

    public void Save(string slotName)
    {
        var saveData = CreateSaveData();
        string json = JsonUtility.ToJson(saveData, true);

        string path = Path.Combine(Application.persistentDataPath, SAVE_FOLDER, slotName + ".json");
        File.WriteAllText(path, json);
    }

    public SaveData Load(string slotName)
    {
        string path = Path.Combine(Application.persistentDataPath, SAVE_FOLDER, slotName + ".json");
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }
}
```

---

## 5. イベントシステム

```csharp
// グローバルイベントバス
public static class EventBus
{
    // ターン関連
    public static event Action<int> OnTurnStarted;
    public static event Action<int> OnTurnEnded;
    public static event Action<TurnPhase> OnPhaseChanged;

    // 戦闘関連
    public static event Action<BattleResult> OnBattleEnded;
    public static event Action<Territory> OnTerritoryConquered;

    // 計略関連
    public static event Action<StratagemResult> OnStratagemExecuted;

    // 外交関連
    public static event Action<Alliance> OnAllianceFormed;
    public static event Action<Alliance> OnAllianceBroken;

    // キャラクター関連
    public static event Action<Character, Faction> OnCharacterRecruited;
    public static event Action<Character> OnCharacterDied;

    // 発火メソッド
    public static void TurnStarted(int turn) => OnTurnStarted?.Invoke(turn);
    // ... 他のメソッド
}
```

---

## 6. UI設計

### 6.1 画面遷移図

```
┌─────────┐
│ タイトル │
└────┬────┘
     ↓
┌─────────┐    ┌─────────┐
│メインメニュー│←→│  設定   │
└────┬────┘    └─────────┘
     ↓
┌────────────────────────────┐
│     モード選択              │
├──────┬──────┬──────────────┤
│キャンペーン│フリー│計略図鑑      │
└──────┴──────┴──────────────┘
     ↓
┌─────────────────────────────┐
│      ゲームプレイ            │
│  ┌─────┐ ┌─────┐ ┌─────┐  │
│  │マップ│ │計略  │ │武将 │  │
│  └─────┘ └─────┘ └─────┘  │
│           ↓                 │
│      ┌─────────┐           │
│      │  戦闘   │           │
│      └─────────┘           │
└─────────────────────────────┘
```

### 6.2 メインゲーム画面レイアウト

```
┌────────────────────────────────────────────────────────┐
│ [ターン: 15]  [フェーズ: 軍事]              [メニュー] │
├────────────────────────────────────────┬───────────────┤
│                                        │ 勢力: 魏      │
│                                        │ 金: 5,000     │
│           戦 略 マ ッ プ                │ 兵糧: 3,000   │
│                                        │ 兵力: 12,000  │
│      (領地・軍の配置を表示)              │ CP: 7/10     │
│                                        ├───────────────┤
│                                        │ [領地情報]    │
│                                        │ 選択中: 洛陽  │
│                                        │ 人口: 50,000  │
│                                        │ 経済: 80      │
├────────────────────────────────────────┴───────────────┤
│ 計略: [瞞天過海][囲魏救趙][借刀殺人][以逸待労][...] [+] │
├────────────────────────────────────────────────────────┤
│ [内政] [外交] [軍事] [武将] [計略図鑑]   [ターン終了]  │
└────────────────────────────────────────────────────────┘
```

---

## 7. データフロー

```
ユーザー入力
     ↓
┌─────────────┐
│ UIController │
└──────┬──────┘
       ↓ (コマンド発行)
┌─────────────┐
│ GameManager  │
└──────┬──────┘
       ↓ (処理委譲)
┌─────────────────────────────────┐
│ 各種Manager                      │
│ (Battle/Stratagem/Diplomacy等)   │
└──────┬──────────────────────────┘
       ↓ (データ更新)
┌─────────────┐
│ Models       │
└──────┬──────┘
       ↓ (イベント発火)
┌─────────────┐
│ EventBus     │
└──────┬──────┘
       ↓ (UI更新通知)
┌─────────────┐
│ UIController │
└─────────────┘
```
