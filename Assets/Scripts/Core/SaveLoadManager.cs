using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Core
{
    /// <summary>
    /// セーブ/ロード管理システム
    /// </summary>
    public class SaveLoadManager : MonoBehaviour
    {
        public static SaveLoadManager Instance { get; private set; }

        private string _saveFolderPath;

        // イベント
        public event Action<string> OnGameSaved;
        public event Action<string> OnGameLoaded;
        public event Action<string> OnSaveDeleted;
        public event Action<string> OnSaveError;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeSavePath();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// セーブフォルダパスを初期化
        /// </summary>
        private void InitializeSavePath()
        {
            _saveFolderPath = Path.Combine(Application.persistentDataPath, Constants.Save.SaveFolderName);

            if (!Directory.Exists(_saveFolderPath))
            {
                Directory.CreateDirectory(_saveFolderPath);
                Debug.Log($"Created save folder: {_saveFolderPath}");
            }
        }

        #region Save Operations

        /// <summary>
        /// ゲームをセーブ
        /// </summary>
        public bool SaveGame(int slotIndex, string saveName = null)
        {
            if (slotIndex < 0 || slotIndex >= Constants.Save.MaxSaveSlots)
            {
                Debug.LogError($"Invalid save slot index: {slotIndex}");
                return false;
            }

            if (GameManager.Instance?.GameData == null)
            {
                Debug.LogError("No game data to save");
                return false;
            }

            try
            {
                var saveData = CreateSaveData(saveName ?? $"Save {slotIndex + 1}");
                string json = JsonUtility.ToJson(saveData, true);
                string filePath = GetSaveFilePath(slotIndex);

                File.WriteAllText(filePath, json);

                Debug.Log($"Game saved to slot {slotIndex}: {filePath}");
                OnGameSaved?.Invoke(filePath);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game: {e.Message}");
                OnSaveError?.Invoke(e.Message);
                return false;
            }
        }

        /// <summary>
        /// クイックセーブ
        /// </summary>
        public bool QuickSave()
        {
            return SaveGame(0, "QuickSave");
        }

        /// <summary>
        /// セーブデータを作成
        /// </summary>
        private SaveData CreateSaveData(string saveName)
        {
            var gameData = GameManager.Instance.GameData;

            var saveData = new SaveData
            {
                SaveVersion = Constants.Save.SaveVersion,
                SaveName = saveName,
                SaveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                PlayTime = Time.realtimeSinceStartup, // TODO: 実際のプレイ時間を追跡
                CurrentTurn = gameData.CurrentTurn,
                PlayerFactionId = gameData.PlayerFactionId,
                CurrentFactionId = gameData.CurrentFactionId
            };

            // 領地データをシリアライズ
            saveData.Territories = new List<TerritorySaveData>();
            foreach (var territory in gameData.Territories.Values)
            {
                saveData.Territories.Add(SerializeTerritory(territory));
            }

            // 勢力データをシリアライズ
            saveData.Factions = new List<FactionSaveData>();
            foreach (var faction in gameData.Factions.Values)
            {
                saveData.Factions.Add(SerializeFaction(faction));
            }

            // 武将データをシリアライズ
            saveData.Characters = new List<CharacterSaveData>();
            foreach (var character in gameData.Characters.Values)
            {
                saveData.Characters.Add(SerializeCharacter(character));
            }

            // 軍データをシリアライズ
            saveData.Armies = new List<ArmySaveData>();
            foreach (var army in gameData.Armies.Values)
            {
                saveData.Armies.Add(SerializeArmy(army));
            }

            return saveData;
        }

        #endregion

        #region Load Operations

        /// <summary>
        /// ゲームをロード
        /// </summary>
        public bool LoadGame(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= Constants.Save.MaxSaveSlots)
            {
                Debug.LogError($"Invalid save slot index: {slotIndex}");
                return false;
            }

            string filePath = GetSaveFilePath(slotIndex);

            if (!File.Exists(filePath))
            {
                Debug.LogError($"Save file not found: {filePath}");
                return false;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                var saveData = JsonUtility.FromJson<SaveData>(json);

                if (saveData.SaveVersion != Constants.Save.SaveVersion)
                {
                    Debug.LogWarning($"Save version mismatch: {saveData.SaveVersion} != {Constants.Save.SaveVersion}");
                    // TODO: バージョンマイグレーション
                }

                var gameData = DeserializeSaveData(saveData);
                GameManager.Instance.LoadGame(gameData);

                Debug.Log($"Game loaded from slot {slotIndex}");
                OnGameLoaded?.Invoke(filePath);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
                OnSaveError?.Invoke(e.Message);
                return false;
            }
        }

        /// <summary>
        /// クイックロード
        /// </summary>
        public bool QuickLoad()
        {
            return LoadGame(0);
        }

        /// <summary>
        /// セーブデータをデシリアライズ
        /// </summary>
        private GameData DeserializeSaveData(SaveData saveData)
        {
            var gameData = new GameData
            {
                CurrentTurn = saveData.CurrentTurn,
                PlayerFactionId = saveData.PlayerFactionId,
                CurrentFactionId = saveData.CurrentFactionId,
                Territories = new Dictionary<string, Territory>(),
                Factions = new Dictionary<string, Faction>(),
                Characters = new Dictionary<string, Character>(),
                Armies = new Dictionary<string, Army>()
            };

            // 領地をデシリアライズ
            foreach (var data in saveData.Territories)
            {
                var territory = DeserializeTerritory(data);
                gameData.Territories[territory.Id] = territory;
            }

            // 勢力をデシリアライズ
            foreach (var data in saveData.Factions)
            {
                var faction = DeserializeFaction(data);
                gameData.Factions[faction.Id] = faction;
            }

            // 武将をデシリアライズ
            foreach (var data in saveData.Characters)
            {
                var character = DeserializeCharacter(data);
                gameData.Characters[character.Id] = character;
            }

            // 軍をデシリアライズ
            foreach (var data in saveData.Armies)
            {
                var army = DeserializeArmy(data);
                gameData.Armies[army.Id] = army;
            }

            return gameData;
        }

        #endregion

        #region Save Slot Management

        /// <summary>
        /// セーブスロット情報を取得
        /// </summary>
        public SaveSlotInfo GetSaveSlotInfo(int slotIndex)
        {
            string filePath = GetSaveFilePath(slotIndex);

            if (!File.Exists(filePath))
            {
                return new SaveSlotInfo
                {
                    SlotIndex = slotIndex,
                    IsEmpty = true
                };
            }

            try
            {
                string json = File.ReadAllText(filePath);
                var saveData = JsonUtility.FromJson<SaveData>(json);

                return new SaveSlotInfo
                {
                    SlotIndex = slotIndex,
                    IsEmpty = false,
                    SaveName = saveData.SaveName,
                    SaveDate = saveData.SaveDate,
                    CurrentTurn = saveData.CurrentTurn,
                    PlayerFactionId = saveData.PlayerFactionId,
                    PlayTime = saveData.PlayTime
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to read save slot info: {e.Message}");
                return new SaveSlotInfo
                {
                    SlotIndex = slotIndex,
                    IsEmpty = true
                };
            }
        }

        /// <summary>
        /// 全セーブスロット情報を取得
        /// </summary>
        public List<SaveSlotInfo> GetAllSaveSlotInfo()
        {
            var slots = new List<SaveSlotInfo>();

            for (int i = 0; i < Constants.Save.MaxSaveSlots; i++)
            {
                slots.Add(GetSaveSlotInfo(i));
            }

            return slots;
        }

        /// <summary>
        /// セーブスロットが使用されているか
        /// </summary>
        public bool IsSaveSlotUsed(int slotIndex)
        {
            return File.Exists(GetSaveFilePath(slotIndex));
        }

        /// <summary>
        /// セーブデータを削除
        /// </summary>
        public bool DeleteSave(int slotIndex)
        {
            string filePath = GetSaveFilePath(slotIndex);

            if (!File.Exists(filePath))
            {
                return false;
            }

            try
            {
                File.Delete(filePath);
                Debug.Log($"Save deleted: slot {slotIndex}");
                OnSaveDeleted?.Invoke(filePath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete save: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// セーブファイルパスを取得
        /// </summary>
        private string GetSaveFilePath(int slotIndex)
        {
            return Path.Combine(_saveFolderPath, $"save_{slotIndex}{Constants.Save.SaveFileExtension}");
        }

        #endregion

        #region Serialization Helpers

        private TerritorySaveData SerializeTerritory(Territory territory)
        {
            return new TerritorySaveData
            {
                Id = territory.Id,
                Name = territory.Name,
                OwnerId = territory.OwnerId,
                Population = territory.Population,
                Economy = territory.Economy,
                Defense = territory.Defense,
                PositionX = territory.MapPosition.x,
                PositionY = territory.MapPosition.y,
                Buildings = new List<string>(territory.BuildingIds),
                AdjacentTerritories = new List<string>(territory.AdjacentTerritoryIds)
            };
        }

        private Territory DeserializeTerritory(TerritorySaveData data)
        {
            return new Territory
            {
                Id = data.Id,
                Name = data.Name,
                OwnerId = data.OwnerId,
                Population = data.Population,
                Economy = data.Economy,
                Defense = data.Defense,
                MapPosition = new Vector2(data.PositionX, data.PositionY),
                BuildingIds = new List<string>(data.Buildings ?? new List<string>()),
                AdjacentTerritoryIds = new List<string>(data.AdjacentTerritories ?? new List<string>())
            };
        }

        private FactionSaveData SerializeFaction(Faction faction)
        {
            return new FactionSaveData
            {
                Id = faction.Id,
                Name = faction.Name,
                IsPlayer = faction.IsPlayer,
                Gold = faction.Gold,
                Food = faction.Food,
                StratagemPoints = faction.StratagemPoints,
                TerritoryIds = new List<string>(faction.TerritoryIds),
                CharacterIds = new List<string>(faction.CharacterIds),
                ColorR = faction.Color.r,
                ColorG = faction.Color.g,
                ColorB = faction.Color.b
            };
        }

        private Faction DeserializeFaction(FactionSaveData data)
        {
            return new Faction
            {
                Id = data.Id,
                Name = data.Name,
                IsPlayer = data.IsPlayer,
                Gold = data.Gold,
                Food = data.Food,
                StratagemPoints = data.StratagemPoints,
                TerritoryIds = new List<string>(data.TerritoryIds ?? new List<string>()),
                CharacterIds = new List<string>(data.CharacterIds ?? new List<string>()),
                Color = new Color(data.ColorR, data.ColorG, data.ColorB)
            };
        }

        private CharacterSaveData SerializeCharacter(Character character)
        {
            return new CharacterSaveData
            {
                Id = character.Id,
                Name = character.Name,
                FactionId = character.FactionId,
                TerritoryId = character.TerritoryId,
                ArmyId = character.ArmyId,
                Type = character.Type,
                Strength = character.Strength,
                Intelligence = character.Intelligence,
                Leadership = character.Leadership,
                Politics = character.Politics,
                Charisma = character.Charisma,
                Loyalty = character.Loyalty,
                Experience = character.Experience
            };
        }

        private Character DeserializeCharacter(CharacterSaveData data)
        {
            return new Character
            {
                Id = data.Id,
                Name = data.Name,
                FactionId = data.FactionId,
                TerritoryId = data.TerritoryId,
                ArmyId = data.ArmyId,
                Type = data.Type,
                Strength = data.Strength,
                Intelligence = data.Intelligence,
                Leadership = data.Leadership,
                Politics = data.Politics,
                Charisma = data.Charisma,
                Loyalty = data.Loyalty,
                Experience = data.Experience
            };
        }

        private ArmySaveData SerializeArmy(Army army)
        {
            return new ArmySaveData
            {
                Id = army.Id,
                Name = army.Name,
                FactionId = army.FactionId,
                TerritoryId = army.TerritoryId,
                CommanderId = army.CommanderId,
                SoldierCount = army.SoldierCount,
                Morale = army.Morale,
                IsMoving = army.IsMoving,
                TargetTerritoryId = army.TargetTerritoryId,
                MovementProgress = army.MovementProgress
            };
        }

        private Army DeserializeArmy(ArmySaveData data)
        {
            return new Army
            {
                Id = data.Id,
                Name = data.Name,
                FactionId = data.FactionId,
                TerritoryId = data.TerritoryId,
                CommanderId = data.CommanderId,
                SoldierCount = data.SoldierCount,
                Morale = data.Morale,
                IsMoving = data.IsMoving,
                TargetTerritoryId = data.TargetTerritoryId,
                MovementProgress = data.MovementProgress
            };
        }

        #endregion
    }

    #region Save Data Classes

    /// <summary>
    /// セーブデータ全体
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int SaveVersion;
        public string SaveName;
        public string SaveDate;
        public float PlayTime;
        public int CurrentTurn;
        public string PlayerFactionId;
        public string CurrentFactionId;

        public List<TerritorySaveData> Territories;
        public List<FactionSaveData> Factions;
        public List<CharacterSaveData> Characters;
        public List<ArmySaveData> Armies;
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
        public string SaveDate;
        public int CurrentTurn;
        public string PlayerFactionId;
        public float PlayTime;
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
        public float PositionX;
        public float PositionY;
        public List<string> Buildings;
        public List<string> AdjacentTerritories;
    }

    /// <summary>
    /// 勢力セーブデータ
    /// </summary>
    [Serializable]
    public class FactionSaveData
    {
        public string Id;
        public string Name;
        public bool IsPlayer;
        public int Gold;
        public int Food;
        public int StratagemPoints;
        public List<string> TerritoryIds;
        public List<string> CharacterIds;
        public float ColorR;
        public float ColorG;
        public float ColorB;
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
        public string TerritoryId;
        public string ArmyId;
        public CharacterType Type;
        public int Strength;
        public int Intelligence;
        public int Leadership;
        public int Politics;
        public int Charisma;
        public int Loyalty;
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
        public string TerritoryId;
        public string CommanderId;
        public int SoldierCount;
        public int Morale;
        public bool IsMoving;
        public string TargetTerritoryId;
        public float MovementProgress;
    }

    #endregion
}
