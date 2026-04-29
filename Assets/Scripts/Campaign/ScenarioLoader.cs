using System;
using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;
using ThirtySixStratagems.Data.ScriptableObjects;

namespace ThirtySixStratagems.Campaign
{
    /// <summary>
    /// シナリオローダー
    /// シナリオデータからゲーム状態を初期化
    /// </summary>
    public class ScenarioLoader : MonoBehaviour
    {
        public static ScenarioLoader Instance { get; private set; }

        [Header("データベース")]
        [SerializeField] private ScenarioDatabase _scenarioDatabase;
        [SerializeField] private StratagemDatabase _stratagemDatabase;

        [Header("デフォルト値")]
        [SerializeField] private int _defaultSoldiers = 5000;
        [SerializeField] private int _defaultMorale = 70;

        // イベント
        public event Action<string> OnScenarioLoadStarted;
        public event Action<string> OnScenarioLoadCompleted;
        public event Action<string, string> OnScenarioLoadFailed;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region Load Scenario

        /// <summary>
        /// シナリオを読み込み
        /// </summary>
        public bool LoadScenario(string scenarioId, string playerFactionId)
        {
            OnScenarioLoadStarted?.Invoke(scenarioId);

            var scenario = _scenarioDatabase?.GetScenario(scenarioId);
            if (scenario == null)
            {
                string error = $"Scenario not found: {scenarioId}";
                Debug.LogError(error);
                OnScenarioLoadFailed?.Invoke(scenarioId, error);
                return false;
            }

            try
            {
                // GameDataを初期化
                var gameData = CreateGameData(scenario, playerFactionId);

                // GameManagerに設定
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetGameData(gameData);
                    GameManager.Instance.SetCurrentYear(scenario.Year);
                }

                Debug.Log($"Scenario loaded successfully: {scenario.ScenarioName}");
                OnScenarioLoadCompleted?.Invoke(scenarioId);

                return true;
            }
            catch (Exception ex)
            {
                string error = $"Failed to load scenario: {ex.Message}";
                Debug.LogError(error);
                OnScenarioLoadFailed?.Invoke(scenarioId, error);
                return false;
            }
        }

        /// <summary>
        /// GameDataを作成
        /// </summary>
        private GameData CreateGameData(ScenarioData scenario, string playerFactionId)
        {
            var gameData = new GameData
            {
                Factions = new Dictionary<string, Faction>(),
                Territories = new Dictionary<string, Territory>(),
                Characters = new Dictionary<string, Character>(),
                Armies = new Dictionary<string, Army>()
            };

            // 領地を作成
            CreateTerritories(gameData, scenario);

            // 勢力を作成
            CreateFactions(gameData, scenario, playerFactionId);

            // 武将を作成
            CreateCharacters(gameData, scenario);

            // 軍を作成
            CreateArmies(gameData, scenario);

            return gameData;
        }

        #endregion

        #region Create Entities

        /// <summary>
        /// 領地を作成
        /// </summary>
        private void CreateTerritories(GameData gameData, ScenarioData scenario)
        {
            foreach (var terrData in scenario.Territories)
            {
                var territory = new Territory
                {
                    Id = terrData.TerritoryId,
                    Name = terrData.TerritoryName,
                    OwnerId = terrData.OwnerId,
                    Population = terrData.Population,
                    Economy = terrData.Economy,
                    Defense = terrData.Defense,
                    AdjacentTerritoryIds = new List<string>(terrData.AdjacentTerritoryIds)
                };

                gameData.Territories[territory.Id] = territory;
            }
        }

        /// <summary>
        /// 勢力を作成
        /// </summary>
        private void CreateFactions(GameData gameData, ScenarioData scenario, string playerFactionId)
        {
            foreach (var factionData in scenario.Factions)
            {
                var faction = new Faction
                {
                    Id = factionData.FactionId,
                    Name = factionData.FactionName,
                    LeaderId = factionData.LeaderId,
                    IsPlayer = factionData.FactionId == playerFactionId,
                    Gold = scenario.StartingGold + factionData.BonusGold,
                    Food = scenario.StartingFood,
                    StratagemPoints = scenario.StartingStratagemPoints,
                    TerritoryIds = new List<string>(factionData.StartingTerritoryIds),
                    CharacterIds = new List<string>(factionData.StartingCharacterIds),
                    ArmyIds = new List<string>()
                };

                gameData.Factions[faction.Id] = faction;

                // 領地の所有者を設定
                foreach (var terrId in faction.TerritoryIds)
                {
                    if (gameData.Territories.TryGetValue(terrId, out var territory))
                    {
                        territory.OwnerId = faction.Id;
                    }
                }
            }
        }

        /// <summary>
        /// 武将を作成
        /// </summary>
        private void CreateCharacters(GameData gameData, ScenarioData scenario)
        {
            // シナリオに定義された武将を作成
            // 実際の実装ではCharacterDatabaseから読み込む

            foreach (var factionData in scenario.Factions)
            {
                foreach (var charId in factionData.StartingCharacterIds)
                {
                    // 既存のCharacterDatabaseから取得するか、デフォルト作成
                    var character = CreateDefaultCharacter(charId, factionData.FactionId, scenario.Year);
                    gameData.Characters[character.Id] = character;
                }

                // リーダーを確認
                if (!string.IsNullOrEmpty(factionData.LeaderId) &&
                    !gameData.Characters.ContainsKey(factionData.LeaderId))
                {
                    var leader = CreateDefaultCharacter(factionData.LeaderId, factionData.FactionId, scenario.Year);
                    leader.Name = factionData.LeaderName;
                    gameData.Characters[leader.Id] = leader;
                }
            }
        }

        /// <summary>
        /// デフォルト武将を作成
        /// </summary>
        private Character CreateDefaultCharacter(string charId, string factionId, int year)
        {
            return new Character
            {
                Id = charId,
                Name = $"武将_{charId}",
                FactionId = factionId,
                Strength = UnityEngine.Random.Range(40, 80),
                Intelligence = UnityEngine.Random.Range(40, 80),
                Leadership = UnityEngine.Random.Range(40, 80),
                Politics = UnityEngine.Random.Range(40, 80),
                Charm = UnityEngine.Random.Range(40, 80),
                Loyalty = 80,
                BirthYear = year - UnityEngine.Random.Range(20, 50),
                Experience = 0
            };
        }

        /// <summary>
        /// 軍を作成
        /// </summary>
        private void CreateArmies(GameData gameData, ScenarioData scenario)
        {
            foreach (var factionData in scenario.Factions)
            {
                if (factionData.StartingTerritoryIds.Count == 0) continue;

                // 首都に初期軍を配置
                string capitalId = factionData.StartingTerritoryIds[0];
                int soldiers = _defaultSoldiers + factionData.BonusSoldiers;

                var army = new Army
                {
                    Id = $"army_{factionData.FactionId}_main",
                    Name = $"{factionData.FactionName}本軍",
                    FactionId = factionData.FactionId,
                    CommanderId = factionData.LeaderId,
                    TerritoryId = capitalId,
                    SoldierCount = soldiers,
                    Morale = _defaultMorale,
                    IsMoving = false
                };

                gameData.Armies[army.Id] = army;

                // 勢力の軍リストに追加
                if (gameData.Factions.TryGetValue(factionData.FactionId, out var faction))
                {
                    faction.ArmyIds.Add(army.Id);
                }
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// シナリオ一覧を取得
        /// </summary>
        public IReadOnlyList<ScenarioData> GetAvailableScenarios()
        {
            return _scenarioDatabase?.Scenarios ?? new List<ScenarioData>();
        }

        /// <summary>
        /// シナリオを取得
        /// </summary>
        public ScenarioData GetScenario(string scenarioId)
        {
            return _scenarioDatabase?.GetScenario(scenarioId);
        }

        /// <summary>
        /// シナリオの勢力一覧を取得
        /// </summary>
        public IReadOnlyList<ScenarioFactionData> GetPlayableFactions(string scenarioId)
        {
            var scenario = GetScenario(scenarioId);
            if (scenario == null) return new List<ScenarioFactionData>();

            var playable = new List<ScenarioFactionData>();
            foreach (var faction in scenario.Factions)
            {
                if (faction.IsPlayable)
                {
                    playable.Add(faction);
                }
            }
            return playable;
        }

        /// <summary>
        /// シナリオのプレビュー情報を取得
        /// </summary>
        public ScenarioPreview GetScenarioPreview(string scenarioId)
        {
            var scenario = GetScenario(scenarioId);
            if (scenario == null) return null;

            return new ScenarioPreview
            {
                ScenarioId = scenario.ScenarioId,
                Name = scenario.ScenarioName,
                Description = scenario.Description,
                Year = scenario.Year,
                Difficulty = scenario.Difficulty,
                FactionCount = scenario.Factions.Count,
                TerritoryCount = scenario.Territories.Count,
                Thumbnail = scenario.Thumbnail
            };
        }

        #endregion
    }

    /// <summary>
    /// シナリオプレビュー情報
    /// </summary>
    public class ScenarioPreview
    {
        public string ScenarioId;
        public string Name;
        public string Description;
        public int Year;
        public int Difficulty;
        public int FactionCount;
        public int TerritoryCount;
        public Sprite Thumbnail;
    }
}
