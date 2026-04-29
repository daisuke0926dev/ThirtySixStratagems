using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// セーブ/ロードシステム
    /// ゲームデータの保存と読み込みを管理
    /// </summary>
    public class SaveLoadSystem : MonoBehaviour
    {
        public static SaveLoadSystem Instance { get; private set; }

        [Header("設定")]
        [SerializeField] private int _maxSaveSlots = 10;
        [SerializeField] private bool _enableAutoSave = true;
        [SerializeField] private int _autoSaveIntervalTurns = 5;
        [SerializeField] private string _saveFileExtension = ".sav";

        [Header("暗号化")]
        [SerializeField] private bool _encryptSaveData = false;
        [SerializeField] private string _encryptionKey = "ThirtySixStratagems2024";

        // パス
        private string _saveDirectory;
        private const string SAVE_FILE_PREFIX = "save_";
        private const string AUTOSAVE_FILE = "autosave";
        private const string QUICKSAVE_FILE = "quicksave";

        // 状態
        private int _turnsSinceLastAutoSave = 0;

        // イベント
        public event Action<string> OnSaveStarted;
        public event Action<string, bool> OnSaveCompleted;
        public event Action<string> OnLoadStarted;
        public event Action<string, bool> OnLoadCompleted;
        public event Action<List<SaveSlotInfo>> OnSaveSlotsUpdated;

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

        private void OnEnable()
        {
            EventBus.OnTurnEnded += OnTurnEnded;
        }

        private void OnDisable()
        {
            EventBus.OnTurnEnded -= OnTurnEnded;
        }

        #region Initialization

        /// <summary>
        /// 初期化
        /// </summary>
        private void Initialize()
        {
            _saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");

            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }

            Debug.Log($"Save directory: {_saveDirectory}");
        }

        #endregion

        #region Save

        /// <summary>
        /// スロットに保存
        /// </summary>
        public bool SaveToSlot(int slotIndex, string saveName = null)
        {
            if (slotIndex < 0 || slotIndex >= _maxSaveSlots)
            {
                Debug.LogError($"Invalid save slot index: {slotIndex}");
                return false;
            }

            string fileName = $"{SAVE_FILE_PREFIX}{slotIndex:D2}";
            return Save(fileName, saveName ?? $"セーブ {slotIndex + 1}");
        }

        /// <summary>
        /// クイックセーブ
        /// </summary>
        public bool QuickSave()
        {
            return Save(QUICKSAVE_FILE, "クイックセーブ");
        }

        /// <summary>
        /// オートセーブ
        /// </summary>
        public bool AutoSave()
        {
            return Save(AUTOSAVE_FILE, "オートセーブ");
        }

        /// <summary>
        /// 保存を実行
        /// </summary>
        private bool Save(string fileName, string displayName)
        {
            string filePath = GetSaveFilePath(fileName);
            OnSaveStarted?.Invoke(displayName);

            try
            {
                var saveData = CreateSaveData(displayName);
                string json = JsonUtility.ToJson(saveData, true);

                if (_encryptSaveData)
                {
                    json = EncryptString(json);
                }

                File.WriteAllText(filePath, json);

                Debug.Log($"Game saved to: {filePath}");
                OnSaveCompleted?.Invoke(displayName, true);
                OnSaveSlotsUpdated?.Invoke(GetAllSaveSlots());

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save game: {ex.Message}");
                OnSaveCompleted?.Invoke(displayName, false);
                return false;
            }
        }

        /// <summary>
        /// セーブデータを作成
        /// </summary>
        private SaveData CreateSaveData(string displayName)
        {
            var gameData = GameManager.Instance?.GameData;
            if (gameData == null)
            {
                throw new InvalidOperationException("GameData is null");
            }

            var saveData = new SaveData
            {
                SaveName = displayName,
                SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                PlayTime = GetPlayTime(),
                Version = Application.version,

                // ゲーム状態
                CurrentTurn = GameManager.Instance.CurrentTurn,
                CurrentYear = GameManager.Instance.CurrentYear,
                ScenarioId = GameManager.Instance.CurrentScenarioId,

                // 勢力データ
                Factions = new List<FactionSaveData>(),
                Territories = new List<TerritorySaveData>(),
                Characters = new List<CharacterSaveData>(),
                Armies = new List<ArmySaveData>()
            };

            // 勢力を保存
            foreach (var faction in gameData.Factions.Values)
            {
                saveData.Factions.Add(new FactionSaveData
                {
                    Id = faction.Id,
                    Name = faction.Name,
                    LeaderId = faction.LeaderId,
                    IsPlayer = faction.IsPlayer,
                    Gold = faction.Gold,
                    Food = faction.Food,
                    StratagemPoints = faction.StratagemPoints,
                    TerritoryIds = new List<string>(faction.TerritoryIds),
                    CharacterIds = new List<string>(faction.CharacterIds),
                    ArmyIds = new List<string>(faction.ArmyIds)
                });
            }

            // 領地を保存
            foreach (var territory in gameData.Territories.Values)
            {
                saveData.Territories.Add(new TerritorySaveData
                {
                    Id = territory.Id,
                    Name = territory.Name,
                    OwnerId = territory.OwnerId,
                    Population = territory.Population,
                    Economy = territory.Economy,
                    Defense = territory.Defense,
                    AdjacentTerritoryIds = new List<string>(territory.AdjacentTerritoryIds)
                });
            }

            // 武将を保存
            foreach (var character in gameData.Characters.Values)
            {
                saveData.Characters.Add(new CharacterSaveData
                {
                    Id = character.Id,
                    Name = character.Name,
                    FactionId = character.FactionId,
                    Strength = character.Strength,
                    Intelligence = character.Intelligence,
                    Leadership = character.Leadership,
                    Politics = character.Politics,
                    Charm = character.Charm,
                    Loyalty = character.Loyalty,
                    BirthYear = character.BirthYear,
                    Experience = character.Experience
                });
            }

            // 軍を保存
            foreach (var army in gameData.Armies.Values)
            {
                saveData.Armies.Add(new ArmySaveData
                {
                    Id = army.Id,
                    Name = army.Name,
                    FactionId = army.FactionId,
                    CommanderId = army.CommanderId,
                    TerritoryId = army.TerritoryId,
                    SoldierCount = army.SoldierCount,
                    Morale = army.Morale,
                    IsMoving = army.IsMoving
                });
            }

            return saveData;
        }

        #endregion

        #region Load

        /// <summary>
        /// スロットから読み込み
        /// </summary>
        public bool LoadFromSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _maxSaveSlots)
            {
                Debug.LogError($"Invalid save slot index: {slotIndex}");
                return false;
            }

            string fileName = $"{SAVE_FILE_PREFIX}{slotIndex:D2}";
            return Load(fileName);
        }

        /// <summary>
        /// クイックロード
        /// </summary>
        public bool QuickLoad()
        {
            return Load(QUICKSAVE_FILE);
        }

        /// <summary>
        /// オートセーブから読み込み
        /// </summary>
        public bool LoadAutoSave()
        {
            return Load(AUTOSAVE_FILE);
        }

        /// <summary>
        /// 読み込みを実行
        /// </summary>
        private bool Load(string fileName)
        {
            string filePath = GetSaveFilePath(fileName);
            OnLoadStarted?.Invoke(fileName);

            if (!File.Exists(filePath))
            {
                Debug.LogError($"Save file not found: {filePath}");
                OnLoadCompleted?.Invoke(fileName, false);
                return false;
            }

            try
            {
                string json = File.ReadAllText(filePath);

                if (_encryptSaveData)
                {
                    json = DecryptString(json);
                }

                var saveData = JsonUtility.FromJson<SaveData>(json);
                ApplySaveData(saveData);

                Debug.Log($"Game loaded from: {filePath}");
                OnLoadCompleted?.Invoke(fileName, true);

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load game: {ex.Message}");
                OnLoadCompleted?.Invoke(fileName, false);
                return false;
            }
        }

        /// <summary>
        /// セーブデータを適用
        /// </summary>
        private void ApplySaveData(SaveData saveData)
        {
            if (GameManager.Instance == null)
            {
                throw new InvalidOperationException("GameManager is null");
            }

            var gameData = new GameData
            {
                Factions = new Dictionary<string, Faction>(),
                Territories = new Dictionary<string, Territory>(),
                Characters = new Dictionary<string, Character>(),
                Armies = new Dictionary<string, Army>()
            };

            // 勢力を復元
            foreach (var factionData in saveData.Factions)
            {
                gameData.Factions[factionData.Id] = new Faction
                {
                    Id = factionData.Id,
                    Name = factionData.Name,
                    LeaderId = factionData.LeaderId,
                    IsPlayer = factionData.IsPlayer,
                    Gold = factionData.Gold,
                    Food = factionData.Food,
                    StratagemPoints = factionData.StratagemPoints,
                    TerritoryIds = new List<string>(factionData.TerritoryIds),
                    CharacterIds = new List<string>(factionData.CharacterIds),
                    ArmyIds = new List<string>(factionData.ArmyIds)
                };
            }

            // 領地を復元
            foreach (var terrData in saveData.Territories)
            {
                gameData.Territories[terrData.Id] = new Territory
                {
                    Id = terrData.Id,
                    Name = terrData.Name,
                    OwnerId = terrData.OwnerId,
                    Population = terrData.Population,
                    Economy = terrData.Economy,
                    Defense = terrData.Defense,
                    AdjacentTerritoryIds = new List<string>(terrData.AdjacentTerritoryIds)
                };
            }

            // 武将を復元
            foreach (var charData in saveData.Characters)
            {
                gameData.Characters[charData.Id] = new Character
                {
                    Id = charData.Id,
                    Name = charData.Name,
                    FactionId = charData.FactionId,
                    Strength = charData.Strength,
                    Intelligence = charData.Intelligence,
                    Leadership = charData.Leadership,
                    Politics = charData.Politics,
                    Charm = charData.Charm,
                    Loyalty = charData.Loyalty,
                    BirthYear = charData.BirthYear,
                    Experience = charData.Experience
                };
            }

            // 軍を復元
            foreach (var armyData in saveData.Armies)
            {
                gameData.Armies[armyData.Id] = new Army
                {
                    Id = armyData.Id,
                    Name = armyData.Name,
                    FactionId = armyData.FactionId,
                    CommanderId = armyData.CommanderId,
                    TerritoryId = armyData.TerritoryId,
                    SoldierCount = armyData.SoldierCount,
                    Morale = armyData.Morale,
                    IsMoving = armyData.IsMoving
                };
            }

            // GameManagerに設定
            GameManager.Instance.SetGameData(gameData);
            GameManager.Instance.SetCurrentTurn(saveData.CurrentTurn);
            GameManager.Instance.SetCurrentYear(saveData.CurrentYear);
        }

        #endregion

        #region Slot Management

        /// <summary>
        /// 全セーブスロット情報を取得
        /// </summary>
        public List<SaveSlotInfo> GetAllSaveSlots()
        {
            var slots = new List<SaveSlotInfo>();

            for (int i = 0; i < _maxSaveSlots; i++)
            {
                slots.Add(GetSaveSlotInfo(i));
            }

            return slots;
        }

        /// <summary>
        /// セーブスロット情報を取得
        /// </summary>
        public SaveSlotInfo GetSaveSlotInfo(int slotIndex)
        {
            string fileName = $"{SAVE_FILE_PREFIX}{slotIndex:D2}";
            string filePath = GetSaveFilePath(fileName);

            var info = new SaveSlotInfo
            {
                SlotIndex = slotIndex,
                IsEmpty = true
            };

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    if (_encryptSaveData)
                    {
                        json = DecryptString(json);
                    }

                    var saveData = JsonUtility.FromJson<SaveData>(json);

                    info.IsEmpty = false;
                    info.SaveName = saveData.SaveName;
                    info.SaveTime = saveData.SaveTime;
                    info.PlayTime = saveData.PlayTime;
                    info.CurrentTurn = saveData.CurrentTurn;
                    info.ScenarioId = saveData.ScenarioId;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to read save slot {slotIndex}: {ex.Message}");
                }
            }

            return info;
        }

        /// <summary>
        /// セーブスロットを削除
        /// </summary>
        public bool DeleteSaveSlot(int slotIndex)
        {
            string fileName = $"{SAVE_FILE_PREFIX}{slotIndex:D2}";
            string filePath = GetSaveFilePath(fileName);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    OnSaveSlotsUpdated?.Invoke(GetAllSaveSlots());
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to delete save: {ex.Message}");
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// クイックセーブが存在するか
        /// </summary>
        public bool HasQuickSave()
        {
            return File.Exists(GetSaveFilePath(QUICKSAVE_FILE));
        }

        /// <summary>
        /// オートセーブが存在するか
        /// </summary>
        public bool HasAutoSave()
        {
            return File.Exists(GetSaveFilePath(AUTOSAVE_FILE));
        }

        #endregion

        #region Auto Save

        /// <summary>
        /// ターン終了時のハンドラ
        /// </summary>
        private void OnTurnEnded(int turnNumber)
        {
            if (!_enableAutoSave) return;

            _turnsSinceLastAutoSave++;

            if (_turnsSinceLastAutoSave >= _autoSaveIntervalTurns)
            {
                AutoSave();
                _turnsSinceLastAutoSave = 0;
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// セーブファイルパスを取得
        /// </summary>
        private string GetSaveFilePath(string fileName)
        {
            return Path.Combine(_saveDirectory, fileName + _saveFileExtension);
        }

        /// <summary>
        /// プレイ時間を取得
        /// </summary>
        private float GetPlayTime()
        {
            return Time.realtimeSinceStartup;
        }

        /// <summary>
        /// 文字列を暗号化
        /// </summary>
        private string EncryptString(string plainText)
        {
            // シンプルなXOR暗号化
            char[] result = new char[plainText.Length];
            for (int i = 0; i < plainText.Length; i++)
            {
                result[i] = (char)(plainText[i] ^ _encryptionKey[i % _encryptionKey.Length]);
            }
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(new string(result)));
        }

        /// <summary>
        /// 文字列を復号化
        /// </summary>
        private string DecryptString(string encryptedText)
        {
            string decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encryptedText));
            char[] result = new char[decoded.Length];
            for (int i = 0; i < decoded.Length; i++)
            {
                result[i] = (char)(decoded[i] ^ _encryptionKey[i % _encryptionKey.Length]);
            }
            return new string(result);
        }

        #endregion
    }

    #region Save Data Classes

    /// <summary>
    /// セーブデータ
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public string SaveName;
        public string SaveTime;
        public float PlayTime;
        public string Version;

        public int CurrentTurn;
        public int CurrentYear;
        public string ScenarioId;

        public List<FactionSaveData> Factions;
        public List<TerritorySaveData> Territories;
        public List<CharacterSaveData> Characters;
        public List<ArmySaveData> Armies;
    }

    /// <summary>
    /// 勢力セーブデータ
    /// </summary>
    [Serializable]
    public class FactionSaveData
    {
        public string Id;
        public string Name;
        public string LeaderId;
        public bool IsPlayer;
        public int Gold;
        public int Food;
        public int StratagemPoints;
        public List<string> TerritoryIds;
        public List<string> CharacterIds;
        public List<string> ArmyIds;
    }

    /// <summary>
    /// 領地セーブデータ
    /// </summary>
    [Serializable]
    public class TerritorySaveData
    {
        public string Id;
        public string Name;
        public string OwnerId;
        public int Population;
        public int Economy;
        public int Defense;
        public List<string> AdjacentTerritoryIds;
    }

    /// <summary>
    /// 武将セーブデータ
    /// </summary>
    [Serializable]
    public class CharacterSaveData
    {
        public string Id;
        public string Name;
        public string FactionId;
        public int Strength;
        public int Intelligence;
        public int Leadership;
        public int Politics;
        public int Charm;
        public int Loyalty;
        public int BirthYear;
        public int Experience;
    }

    /// <summary>
    /// 軍セーブデータ
    /// </summary>
    [Serializable]
    public class ArmySaveData
    {
        public string Id;
        public string Name;
        public string FactionId;
        public string CommanderId;
        public string TerritoryId;
        public int SoldierCount;
        public int Morale;
        public bool IsMoving;
    }

    /// <summary>
    /// セーブスロット情報
    /// </summary>
    [Serializable]
    public class SaveSlotInfo
    {
        public int SlotIndex;
        public bool IsEmpty;
        public string SaveName;
        public string SaveTime;
        public float PlayTime;
        public int CurrentTurn;
        public string ScenarioId;
    }

    #endregion
}
