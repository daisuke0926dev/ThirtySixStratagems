using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThirtySixStratagems.Data.ScriptableObjects
{
    /// <summary>
    /// シナリオデータベース
    /// ゲーム内で選択可能なシナリオを管理
    /// </summary>
    [CreateAssetMenu(fileName = "ScenarioDatabase", menuName = "ThirtySixStratagems/Scenario Database")]
    public class ScenarioDatabase : ScriptableObject
    {
        [SerializeField] private List<ScenarioData> _scenarios = new List<ScenarioData>();

        /// <summary>
        /// 全シナリオ
        /// </summary>
        public IReadOnlyList<ScenarioData> Scenarios => _scenarios;

        /// <summary>
        /// シナリオを取得
        /// </summary>
        public ScenarioData GetScenario(string scenarioId)
        {
            return _scenarios.Find(s => s.ScenarioId == scenarioId);
        }

        /// <summary>
        /// シナリオを追加
        /// </summary>
        public void AddScenario(ScenarioData scenario)
        {
            if (!_scenarios.Contains(scenario))
            {
                _scenarios.Add(scenario);
            }
        }
    }

    /// <summary>
    /// シナリオデータ
    /// 1つのゲームシナリオを定義
    /// </summary>
    [CreateAssetMenu(fileName = "NewScenario", menuName = "ThirtySixStratagems/Scenario Data")]
    public class ScenarioData : ScriptableObject
    {
        [Header("基本情報")]
        [SerializeField] private string _scenarioId;
        [SerializeField] private string _scenarioName;
        [SerializeField, TextArea(3, 10)] private string _description;
        [SerializeField] private int _year;
        [SerializeField] private Sprite _thumbnail;

        [Header("難易度")]
        [SerializeField, Range(1, 5)] private int _difficulty = 3;

        [Header("勢力")]
        [SerializeField] private List<ScenarioFactionData> _factions = new List<ScenarioFactionData>();

        [Header("領地")]
        [SerializeField] private List<ScenarioTerritoryData> _territories = new List<ScenarioTerritoryData>();

        [Header("初期設定")]
        [SerializeField] private int _startingGold = 5000;
        [SerializeField] private int _startingFood = 10000;
        [SerializeField] private int _startingStratagemPoints = 5;

        [Header("勝利条件")]
        [SerializeField] private VictoryConditionType _victoryCondition = VictoryConditionType.Conquest;
        [SerializeField] private int _victoryTargetValue;

        // プロパティ
        public string ScenarioId => _scenarioId;
        public string ScenarioName => _scenarioName;
        public string Description => _description;
        public int Year => _year;
        public Sprite Thumbnail => _thumbnail;
        public int Difficulty => _difficulty;
        public IReadOnlyList<ScenarioFactionData> Factions => _factions;
        public IReadOnlyList<ScenarioTerritoryData> Territories => _territories;
        public int StartingGold => _startingGold;
        public int StartingFood => _startingFood;
        public int StartingStratagemPoints => _startingStratagemPoints;
        public VictoryConditionType VictoryCondition => _victoryCondition;
        public int VictoryTargetValue => _victoryTargetValue;

        /// <summary>
        /// 勢力データを取得
        /// </summary>
        public ScenarioFactionData GetFaction(string factionId)
        {
            return _factions.Find(f => f.FactionId == factionId);
        }
    }

    /// <summary>
    /// シナリオ勢力データ
    /// </summary>
    [Serializable]
    public class ScenarioFactionData
    {
        [SerializeField] private string _factionId;
        [SerializeField] private string _factionName;
        [SerializeField] private string _leaderId;
        [SerializeField] private string _leaderName;
        [SerializeField] private Color _factionColor = Color.white;
        [SerializeField] private Sprite _bannerSprite;
        [SerializeField] private bool _isPlayable = true;
        [SerializeField] private List<string> _startingTerritoryIds = new List<string>();
        [SerializeField] private List<string> _startingCharacterIds = new List<string>();
        [SerializeField] private int _bonusGold;
        [SerializeField] private int _bonusSoldiers;

        // プロパティ
        public string FactionId => _factionId;
        public string FactionName => _factionName;
        public string LeaderId => _leaderId;
        public string LeaderName => _leaderName;
        public Color FactionColor => _factionColor;
        public Sprite BannerSprite => _bannerSprite;
        public bool IsPlayable => _isPlayable;
        public IReadOnlyList<string> StartingTerritoryIds => _startingTerritoryIds;
        public IReadOnlyList<string> StartingCharacterIds => _startingCharacterIds;
        public int BonusGold => _bonusGold;
        public int BonusSoldiers => _bonusSoldiers;
    }

    /// <summary>
    /// シナリオ領地データ
    /// </summary>
    [Serializable]
    public class ScenarioTerritoryData
    {
        [SerializeField] private string _territoryId;
        [SerializeField] private string _territoryName;
        [SerializeField] private string _ownerId;
        [SerializeField] private int _population = 10000;
        [SerializeField] private int _economy = 50;
        [SerializeField] private int _defense = 50;
        [SerializeField] private List<string> _adjacentTerritoryIds = new List<string>();
        [SerializeField] private Vector2 _mapPosition;

        // プロパティ
        public string TerritoryId => _territoryId;
        public string TerritoryName => _territoryName;
        public string OwnerId => _ownerId;
        public int Population => _population;
        public int Economy => _economy;
        public int Defense => _defense;
        public IReadOnlyList<string> AdjacentTerritoryIds => _adjacentTerritoryIds;
        public Vector2 MapPosition => _mapPosition;
    }

    /// <summary>
    /// 勝利条件タイプ
    /// </summary>
    public enum VictoryConditionType
    {
        Conquest,           // 全領地制覇
        TerritoryCount,     // 指定数の領地獲得
        SurviveYears,       // 指定年数生存
        DefeatFaction,      // 特定勢力撃破
        Alliance            // 同盟による統一
    }
}
