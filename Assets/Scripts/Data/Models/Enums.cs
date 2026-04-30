namespace ThirtySixStratagems.Data.Models
{
    /// <summary>
    /// ゲームの状態
    /// </summary>
    public enum GameState
    {
        Title,
        MainMenu,
        Playing,
        Paused,
        Battle,
        GameOver
    }

    /// <summary>
    /// ターンフェーズ
    /// </summary>
    public enum TurnPhase
    {
        Internal,   // 内政フェーズ
        Diplomacy,  // 外交フェーズ
        Military    // 軍事フェーズ
    }

    /// <summary>
    /// キャラクタータイプ
    /// </summary>
    public enum CharacterType
    {
        Ruler,      // 君主
        Strategist, // 軍師
        General,    // 武将
        Spy         // 間者
    }

    /// <summary>
    /// 軍隊の状態
    /// </summary>
    public enum ArmyState
    {
        Idle,       // 待機
        Moving,     // 移動中
        Sieging,    // 包囲中
        Defending,  // 防衛中
        Retreating  // 撤退中
    }

    /// <summary>
    /// 計略カテゴリ（三十六計の6分類）
    /// </summary>
    public enum StratagemCategory
    {
        Winning,    // 勝戦計（第一套）
        Enemy,      // 敵戦計（第二套）
        Attack,     // 攻戦計（第三套）
        Chaos,      // 混戦計（第四套）
        Merge,      // 併戦計（第五套）
        Defeat      // 敗戦計（第六套）
    }

    /// <summary>
    /// 計略の対象タイプ
    /// </summary>
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

    /// <summary>
    /// 計略効果タイプ
    /// </summary>
    public enum StratagemEffectType
    {
        // 軍事系
        AttackBoost,        // 攻撃力上昇
        DefenseBoost,       // 防御力上昇
        StealthMovement,    // 隠密移動
        ForceRetreat,       // 強制撤退
        Ambush,             // 奇襲
        SupplyDisrupt,      // 補給線遮断

        // 外交系
        LoyaltyReduce,      // 忠誠度低下
        AllianceBreak,      // 同盟破棄誘導
        FactionConflict,    // 勢力間対立誘発
        Diplomacy,          // 外交効果

        // 情報系
        Reconnaissance,     // 偵察
        Disinformation,     // 偽情報
        InternalStrife,     // 内部分裂

        // 特殊
        ResourcePlunder,    // 資源略奪
        CharacterCapture,   // 武将捕獲
        TerritoryControl,   // 領地支配
        Escape,             // 撤退・逃走
        Composite           // 複合効果
    }

    /// <summary>
    /// AI性格タイプ
    /// </summary>
    public enum AIPersonality
    {
        Aggressive,  // 攻撃的
        Defensive,   // 防御的
        Diplomatic,  // 外交重視
        Strategic,   // 計略重視
        Balanced     // バランス型
    }

    /// <summary>
    /// ゲーム終了理由
    /// </summary>
    public enum GameEndReason
    {
        Conquest,       // 制覇勝利
        Surrender,      // 降伏勝利
        Alliance,       // 盟主勝利
        Defeat,         // 敗北
        Abandoned       // 放棄
    }

    /// <summary>
    /// 建物タイプ
    /// </summary>
    public enum BuildingType
    {
        Castle,         // 城
        Barracks,       // 兵舎
        Market,         // 市場
        Farm,           // 農場
        Watchtower,     // 物見櫓
        Workshop        // 工房
    }

    /// <summary>
    /// 地形タイプ
    /// </summary>
    public enum TerrainType
    {
        Plain,          // 平地
        Mountain,       // 山地
        Forest,         // 森林
        River,          // 河川
        Fortress,       // 要塞
        Capital         // 都市
    }

    /// <summary>
    /// 攻撃エフェクトタイプ
    /// </summary>
    public enum AttackEffectType
    {
        Normal,         // 通常攻撃
        Slash,          // 斬撃
        Charge,         // 突撃
        Arrow,          // 矢
        Critical        // クリティカル
    }

    /// <summary>
    /// 外交関係
    /// </summary>
    public enum DiplomaticStatus
    {
        War,            // 戦争中
        Hostile,        // 敵対
        Neutral,        // 中立
        Friendly,       // 友好
        Alliance        // 同盟
    }
}
