using System.IO;
using UnityEngine;
using UnityEditor;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Campaign;
using ThirtySixStratagems.Stratagem;
using ThirtySixStratagems.Battle;
using ThirtySixStratagems.AI;
using ThirtySixStratagems.Systems;

namespace ThirtySixStratagems.Editor
{
    /// <summary>
    /// Prefab自動生成
    /// マネージャーやUI用のPrefabを生成
    /// </summary>
    public class PrefabGenerator : EditorWindow
    {
        private const string PrefabBasePath = "Assets/Prefabs";
        private const string ManagerPrefabPath = "Assets/Prefabs/Managers";
        private const string UIPrefabPath = "Assets/Prefabs/UI";

        [MenuItem("ThirtySixStratagems/Prefab Generator")]
        public static void ShowWindow()
        {
            var window = GetWindow<PrefabGenerator>("Prefab Generator");
            window.minSize = new Vector2(350, 400);
        }

        private void OnGUI()
        {
            GUILayout.Label("Prefab ジェネレーター", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "各種Prefabを自動生成します。\n" +
                "生成されたPrefabは Assets/Prefabs/ 以下に保存されます。",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Manager Prefabs
            EditorGUILayout.LabelField("マネージャーPrefab", EditorStyles.boldLabel);

            if (GUILayout.Button("GameManagers Prefab を生成", GUILayout.Height(35)))
            {
                CreateGameManagersPrefab();
            }

            if (GUILayout.Button("BattleManagers Prefab を生成", GUILayout.Height(35)))
            {
                CreateBattleManagersPrefab();
            }

            EditorGUILayout.Space(10);

            // All at once
            EditorGUILayout.LabelField("一括生成", EditorStyles.boldLabel);

            if (GUILayout.Button("全Prefabを生成", GUILayout.Height(40)))
            {
                CreateAllPrefabs();
            }

            EditorGUILayout.Space(20);

            // Info
            EditorGUILayout.LabelField("生成されるPrefab", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "GameManagers:\n" +
                "  - GameManager\n" +
                "  - SaveLoadManager\n" +
                "  - TurnManager\n" +
                "  - ResourceManager\n" +
                "  - ScenarioLoader\n" +
                "  - CampaignManager\n" +
                "  - StratagemManager\n" +
                "  - AIManager\n" +
                "  - AudioManager\n" +
                "  - SettingsManager\n\n" +
                "BattleManagers:\n" +
                "  - BattleManager\n" +
                "  - ArmyManager",
                MessageType.None);
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }

        private void CreateAllPrefabs()
        {
            CreateGameManagersPrefab();
            CreateBattleManagersPrefab();
            Debug.Log("All prefabs created successfully!");
        }

        private void CreateGameManagersPrefab()
        {
            EnsureDirectoryExists(ManagerPrefabPath);

            // Root object
            var rootObj = new GameObject("GameManagers");

            // GameManager
            var gameManagerObj = new GameObject("GameManager");
            gameManagerObj.transform.SetParent(rootObj.transform);
            gameManagerObj.AddComponent<GameManager>();

            // SaveLoadManager
            var saveLoadObj = new GameObject("SaveLoadManager");
            saveLoadObj.transform.SetParent(rootObj.transform);
            saveLoadObj.AddComponent<SaveLoadManager>();

            // TurnManager
            var turnObj = new GameObject("TurnManager");
            turnObj.transform.SetParent(rootObj.transform);
            turnObj.AddComponent<TurnManager>();

            // ResourceManager
            var resourceObj = new GameObject("ResourceManager");
            resourceObj.transform.SetParent(rootObj.transform);
            resourceObj.AddComponent<ResourceManager>();

            // ScenarioLoader
            var scenarioObj = new GameObject("ScenarioLoader");
            scenarioObj.transform.SetParent(rootObj.transform);
            scenarioObj.AddComponent<ScenarioLoader>();

            // CampaignManager
            var campaignObj = new GameObject("CampaignManager");
            campaignObj.transform.SetParent(rootObj.transform);
            campaignObj.AddComponent<CampaignManager>();

            // StratagemManager
            var stratagemObj = new GameObject("StratagemManager");
            stratagemObj.transform.SetParent(rootObj.transform);
            stratagemObj.AddComponent<StratagemManager>();

            // AIManager
            var aiObj = new GameObject("AIManager");
            aiObj.transform.SetParent(rootObj.transform);
            aiObj.AddComponent<AIManager>();

            // AudioManager
            var audioObj = new GameObject("AudioManager");
            audioObj.transform.SetParent(rootObj.transform);
            audioObj.AddComponent<AudioManager>();

            // SettingsManager
            var settingsObj = new GameObject("SettingsManager");
            settingsObj.transform.SetParent(rootObj.transform);
            settingsObj.AddComponent<SettingsManager>();

            // Save as prefab
            string prefabPath = Path.Combine(ManagerPrefabPath, "GameManagers.prefab");
            prefabPath = prefabPath.Replace("\\", "/");

            // Remove existing prefab
            if (File.Exists(prefabPath))
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            PrefabUtility.SaveAsPrefabAsset(rootObj, prefabPath);
            DestroyImmediate(rootObj);

            AssetDatabase.Refresh();
            Debug.Log($"GameManagers prefab created at: {prefabPath}");
        }

        private void CreateBattleManagersPrefab()
        {
            EnsureDirectoryExists(ManagerPrefabPath);

            // Root object
            var rootObj = new GameObject("BattleManagers");

            // BattleManager
            var battleObj = new GameObject("BattleManager");
            battleObj.transform.SetParent(rootObj.transform);
            battleObj.AddComponent<BattleManager>();

            // ArmyManager
            var armyObj = new GameObject("ArmyManager");
            armyObj.transform.SetParent(rootObj.transform);
            armyObj.AddComponent<ArmyManager>();

            // Save as prefab
            string prefabPath = Path.Combine(ManagerPrefabPath, "BattleManagers.prefab");
            prefabPath = prefabPath.Replace("\\", "/");

            // Remove existing prefab
            if (File.Exists(prefabPath))
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            PrefabUtility.SaveAsPrefabAsset(rootObj, prefabPath);
            DestroyImmediate(rootObj);

            AssetDatabase.Refresh();
            Debug.Log($"BattleManagers prefab created at: {prefabPath}");
        }

        #region Menu Items for Quick Creation

        [MenuItem("ThirtySixStratagems/Create Prefabs/GameManagers")]
        public static void MenuCreateGameManagers()
        {
            var window = GetWindow<PrefabGenerator>();
            window.CreateGameManagersPrefab();
        }

        [MenuItem("ThirtySixStratagems/Create Prefabs/BattleManagers")]
        public static void MenuCreateBattleManagers()
        {
            var window = GetWindow<PrefabGenerator>();
            window.CreateBattleManagersPrefab();
        }

        [MenuItem("ThirtySixStratagems/Create Prefabs/All")]
        public static void MenuCreateAll()
        {
            var window = GetWindow<PrefabGenerator>();
            window.CreateAllPrefabs();
        }

        #endregion
    }
}
