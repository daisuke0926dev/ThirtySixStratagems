using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ThirtySixStratagems.Data.Models;
using ThirtySixStratagems.Data.ScriptableObjects;

namespace ThirtySixStratagems.Core
{
    /// <summary>
    /// ゲーム全体を統括するシングルトンマネージャー
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("現在の状態")]
        [SerializeField] private GameState _currentState = GameState.Title;

        [Header("ゲームデータ")]
        [SerializeField] private MapData _currentMapData;
        [SerializeField] private StratagemDatabase _stratagemDatabase;

        // ランタイムデータ
        private GameData _gameData;
        private int _currentYear;

        // イベント
        public event Action<GameState> OnGameStateChanged;
        public event Action<GameEndReason> OnGameEnded;

        /// <summary>
        /// 現在のゲーム状態
        /// </summary>
        public GameState CurrentState
        {
            get => _currentState;
            private set
            {
                if (_currentState != value)
                {
                    var previousState = _currentState;
                    _currentState = value;
                    OnGameStateChanged?.Invoke(_currentState);
                    Debug.Log($"GameState changed: {previousState} -> {_currentState}");
                }
            }
        }

        /// <summary>
        /// 現在のゲームデータ
        /// </summary>
        public GameData GameData => _gameData;

        /// <summary>
        /// マップデータ
        /// </summary>
        public MapData CurrentMapData => _currentMapData;

        /// <summary>
        /// 計略データベース
        /// </summary>
        public StratagemDatabase StratagemDatabase => _stratagemDatabase;

        /// <summary>
        /// 現在の年
        /// </summary>
        public int CurrentYear => _currentYear;

        /// <summary>
        /// ゲームがプレイ中か
        /// </summary>
        public bool IsPlaying => _currentState == GameState.Playing || _currentState == GameState.Battle;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            Debug.Log("GameManager initialized");

            // 計略データベースの初期化
            if (_stratagemDatabase != null)
            {
                _stratagemDatabase.Initialize();
            }
        }

        /// <summary>
        /// 新しいゲームを開始
        /// </summary>
        public void StartNewGame(MapData mapData, string playerFactionId)
        {
            if (mapData == null)
            {
                Debug.LogError("MapData is null");
                return;
            }

            _currentMapData = mapData;
            _gameData = CreateGameData(mapData, playerFactionId);

            CurrentState = GameState.Playing;

            Debug.Log($"New game started: {mapData.MapName}, Player: {playerFactionId}");

            // ゲームシーンに遷移
            LoadScene("GameScene");
        }

        /// <summary>
        /// ゲームデータを設定（ScenarioLoaderから呼ばれる）
        /// </summary>
        public void SetGameData(GameData gameData)
        {
            _gameData = gameData;
            CurrentState = GameState.Playing;
            Debug.Log($"GameData set: {gameData?.Factions?.Count ?? 0} factions, {gameData?.Territories?.Count ?? 0} territories");
        }

        /// <summary>
        /// 現在の年を設定
        /// </summary>
        public void SetCurrentYear(int year)
        {
            _currentYear = year;
            Debug.Log($"Current year set to: {year}");
        }

        /// <summary>
        /// ゲームデータを作成
        /// </summary>
        private GameData CreateGameData(MapData mapData, string playerFactionId)
        {
            var gameData = new GameData
            {
                CurrentTurn = 1,
                PlayerFactionId = playerFactionId,
                Territories = new Dictionary<string, Territory>(),
                Factions = new Dictionary<string, Faction>(),
                Characters = new Dictionary<string, Character>(),
                Armies = new Dictionary<string, Army>()
            };

            // 領地データを作成
            foreach (var territoryData in mapData.Territories)
            {
                if (territoryData != null)
                {
                    var territory = territoryData.CreateTerritory();
                    gameData.Territories[territory.Id] = territory;
                }
            }

            // 勢力データを作成
            foreach (var factionData in mapData.Factions)
            {
                if (factionData != null)
                {
                    var faction = factionData.CreateFaction();
                    faction.IsPlayer = (faction.Id == playerFactionId);
                    gameData.Factions[faction.Id] = faction;

                    // 領地の所有者を設定
                    foreach (var territoryId in faction.TerritoryIds)
                    {
                        if (gameData.Territories.TryGetValue(territoryId, out var territory))
                        {
                            territory.OwnerId = faction.Id;
                        }
                    }

                    // 武将データを作成
                    foreach (var characterData in factionData.InitialCharacters)
                    {
                        if (characterData != null)
                        {
                            var character = characterData.CreateCharacter();
                            character.FactionId = faction.Id;
                            gameData.Characters[character.Id] = character;
                        }
                    }

                    // 君主も追加
                    if (factionData.Ruler != null && !gameData.Characters.ContainsKey(factionData.Ruler.CharacterId))
                    {
                        var ruler = factionData.Ruler.CreateCharacter();
                        ruler.FactionId = faction.Id;
                        gameData.Characters[ruler.Id] = ruler;
                    }
                }
            }

            return gameData;
        }

        /// <summary>
        /// ゲームをロード
        /// </summary>
        public void LoadGame(GameData saveData)
        {
            if (saveData == null)
            {
                Debug.LogError("SaveData is null");
                return;
            }

            _gameData = saveData;
            CurrentState = GameState.Playing;

            Debug.Log($"Game loaded: Turn {saveData.CurrentTurn}");

            LoadScene("GameScene");
        }

        /// <summary>
        /// ゲームをポーズ
        /// </summary>
        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                CurrentState = GameState.Paused;
                Time.timeScale = 0f;
            }
        }

        /// <summary>
        /// ゲームを再開
        /// </summary>
        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                CurrentState = GameState.Playing;
                Time.timeScale = 1f;
            }
        }

        /// <summary>
        /// 戦闘を開始
        /// </summary>
        public void StartBattle(string attackerArmyId, string defenderArmyId, string territoryId)
        {
            if (CurrentState != GameState.Playing)
            {
                return;
            }

            CurrentState = GameState.Battle;

            // 戦闘データを設定
            _gameData.CurrentBattleData = new BattleData
            {
                AttackerArmyId = attackerArmyId,
                DefenderArmyId = defenderArmyId,
                TerritoryId = territoryId
            };

            Debug.Log($"Battle started at {territoryId}");
        }

        /// <summary>
        /// 戦闘を終了
        /// </summary>
        public void EndBattle()
        {
            if (CurrentState == GameState.Battle)
            {
                CurrentState = GameState.Playing;
                _gameData.CurrentBattleData = null;
            }
        }

        /// <summary>
        /// ゲームを終了
        /// </summary>
        public void EndGame(GameEndReason reason)
        {
            CurrentState = GameState.GameOver;
            OnGameEnded?.Invoke(reason);

            Debug.Log($"Game ended: {reason}");
        }

        /// <summary>
        /// タイトルに戻る
        /// </summary>
        public void ReturnToTitle()
        {
            _gameData = null;
            _currentMapData = null;
            CurrentState = GameState.Title;
            Time.timeScale = 1f;

            LoadScene("TitleScene");
        }

        /// <summary>
        /// メインメニューに戻る
        /// </summary>
        public void ReturnToMainMenu()
        {
            _gameData = null;
            _currentMapData = null;
            CurrentState = GameState.MainMenu;
            Time.timeScale = 1f;

            LoadScene("MainMenuScene");
        }

        /// <summary>
        /// シーンをロード
        /// </summary>
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// 勢力を取得
        /// </summary>
        public Faction GetFaction(string factionId)
        {
            if (_gameData != null && _gameData.Factions.TryGetValue(factionId, out var faction))
            {
                return faction;
            }
            return null;
        }

        /// <summary>
        /// プレイヤー勢力を取得
        /// </summary>
        public Faction GetPlayerFaction()
        {
            return GetFaction(_gameData?.PlayerFactionId);
        }

        /// <summary>
        /// 領地を取得
        /// </summary>
        public Territory GetTerritory(string territoryId)
        {
            if (_gameData != null && _gameData.Territories.TryGetValue(territoryId, out var territory))
            {
                return territory;
            }
            return null;
        }

        /// <summary>
        /// 武将を取得
        /// </summary>
        public Character GetCharacter(string characterId)
        {
            if (_gameData != null && _gameData.Characters.TryGetValue(characterId, out var character))
            {
                return character;
            }
            return null;
        }

        /// <summary>
        /// 軍隊を取得
        /// </summary>
        public Army GetArmy(string armyId)
        {
            if (_gameData != null && _gameData.Armies.TryGetValue(armyId, out var army))
            {
                return army;
            }
            return null;
        }

        /// <summary>
        /// 勝利条件を確認
        /// </summary>
        public bool CheckVictoryCondition(string factionId)
        {
            if (_gameData == null || _currentMapData == null)
            {
                return false;
            }

            var faction = GetFaction(factionId);
            if (faction == null)
            {
                return false;
            }

            int totalTerritories = _gameData.Territories.Count;
            int ownedTerritories = faction.TerritoryIds.Count;
            float percentage = (float)ownedTerritories / totalTerritories * 100;

            switch (_currentMapData.VictoryCondition)
            {
                case VictoryCondition.Conquest:
                    return ownedTerritories >= totalTerritories;

                case VictoryCondition.Domination:
                    return percentage >= _currentMapData.VictoryTerritoryPercentage;

                case VictoryCondition.Elimination:
                    int aliveFactions = 0;
                    foreach (var f in _gameData.Factions.Values)
                    {
                        if (f.TerritoryIds.Count > 0)
                        {
                            aliveFactions++;
                        }
                    }
                    return aliveFactions == 1 && faction.TerritoryIds.Count > 0;

                case VictoryCondition.Survival:
                    return _gameData.CurrentTurn >= _currentMapData.MaxTurns && faction.TerritoryIds.Count > 0;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 敗北条件を確認
        /// </summary>
        public bool CheckDefeatCondition(string factionId)
        {
            var faction = GetFaction(factionId);
            if (faction == null)
            {
                return true;
            }

            // 領地が0になったら敗北
            return faction.TerritoryIds.Count == 0;
        }

        private void OnApplicationQuit()
        {
            // クリーンアップ
            _gameData = null;
        }
    }

    /// <summary>
    /// ゲームランタイムデータ
    /// </summary>
    [Serializable]
    public class GameData
    {
        public int CurrentTurn;
        public string PlayerFactionId;
        public string CurrentFactionId;

        public Dictionary<string, Territory> Territories;
        public Dictionary<string, Faction> Factions;
        public Dictionary<string, Character> Characters;
        public Dictionary<string, Army> Armies;

        public BattleData CurrentBattleData;
    }

    /// <summary>
    /// 戦闘データ
    /// </summary>
    [Serializable]
    public class BattleData
    {
        public string AttackerArmyId;
        public string DefenderArmyId;
        public string TerritoryId;
    }
}
