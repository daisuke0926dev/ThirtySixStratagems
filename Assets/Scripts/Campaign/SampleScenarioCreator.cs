using UnityEngine;
using ThirtySixStratagems.Data.ScriptableObjects;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ThirtySixStratagems.Campaign
{
    /// <summary>
    /// サンプルシナリオ作成ユーティリティ
    /// 開発用のサンプルシナリオを作成
    /// </summary>
    public static class SampleScenarioCreator
    {
#if UNITY_EDITOR
        [MenuItem("ThirtySixStratagems/Create Sample Scenario")]
        public static void CreateSampleScenario()
        {
            // ScenarioDatabaseを作成
            var database = ScriptableObject.CreateInstance<ScenarioDatabase>();

            // チュートリアルシナリオ
            var tutorial = CreateTutorialScenario();
            database.AddScenario(tutorial);

            // 標準シナリオ
            var standard = CreateStandardScenario();
            database.AddScenario(standard);

            // 保存
            string path = "Assets/Data/Scenarios/ScenarioDatabase.asset";
            EnsureDirectoryExists(path);
            AssetDatabase.CreateAsset(database, path);

            // 個別シナリオも保存
            AssetDatabase.CreateAsset(tutorial, "Assets/Data/Scenarios/Tutorial.asset");
            AssetDatabase.CreateAsset(standard, "Assets/Data/Scenarios/Standard.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Sample scenarios created successfully!");
        }

        private static ScenarioData CreateTutorialScenario()
        {
            var scenario = ScriptableObject.CreateInstance<ScenarioData>();

            // リフレクションで private フィールドを設定
            SetPrivateField(scenario, "_scenarioId", "tutorial");
            SetPrivateField(scenario, "_scenarioName", "計略入門");
            SetPrivateField(scenario, "_description", "三十六計の基本を学ぶチュートリアルシナリオです。");
            SetPrivateField(scenario, "_year", 200);
            SetPrivateField(scenario, "_difficulty", 1);
            SetPrivateField(scenario, "_startingGold", 10000);
            SetPrivateField(scenario, "_startingFood", 20000);
            SetPrivateField(scenario, "_startingStratagemPoints", 10);
            SetPrivateField(scenario, "_victoryCondition", VictoryConditionType.TerritoryCount);
            SetPrivateField(scenario, "_victoryTargetValue", 3);

            // 勢力
            var factions = new System.Collections.Generic.List<ScenarioFactionData>();

            // プレイヤー勢力
            var playerFaction = CreateFactionData("player", "学習軍", "leader_player", "諸葛亮",
                new[] { "territory_1" }, new[] { "leader_player", "char_1", "char_2" }, true, 0, 1000);
            factions.Add(playerFaction);

            // 敵勢力
            var enemyFaction = CreateFactionData("enemy", "敵軍", "leader_enemy", "司馬懿",
                new[] { "territory_2", "territory_3" }, new[] { "leader_enemy", "char_3" }, false, 0, 0);
            factions.Add(enemyFaction);

            SetPrivateField(scenario, "_factions", factions);

            // 領地
            var territories = new System.Collections.Generic.List<ScenarioTerritoryData>();
            territories.Add(CreateTerritoryData("territory_1", "成都", "player", 50000, 70, 60,
                new[] { "territory_2" }, new Vector2(0, 0)));
            territories.Add(CreateTerritoryData("territory_2", "漢中", "enemy", 30000, 50, 50,
                new[] { "territory_1", "territory_3" }, new Vector2(100, 0)));
            territories.Add(CreateTerritoryData("territory_3", "長安", "enemy", 80000, 80, 70,
                new[] { "territory_2" }, new Vector2(200, 0)));

            SetPrivateField(scenario, "_territories", territories);

            return scenario;
        }

        private static ScenarioData CreateStandardScenario()
        {
            var scenario = ScriptableObject.CreateInstance<ScenarioData>();

            SetPrivateField(scenario, "_scenarioId", "standard");
            SetPrivateField(scenario, "_scenarioName", "三国鼎立");
            SetPrivateField(scenario, "_description", "三つの勢力が覇権を争う標準シナリオです。三十六計を駆使して天下統一を目指しましょう。");
            SetPrivateField(scenario, "_year", 220);
            SetPrivateField(scenario, "_difficulty", 3);
            SetPrivateField(scenario, "_startingGold", 5000);
            SetPrivateField(scenario, "_startingFood", 10000);
            SetPrivateField(scenario, "_startingStratagemPoints", 5);
            SetPrivateField(scenario, "_victoryCondition", VictoryConditionType.Conquest);
            SetPrivateField(scenario, "_victoryTargetValue", 0);

            // 勢力
            var factions = new System.Collections.Generic.List<ScenarioFactionData>();

            factions.Add(CreateFactionData("wei", "魏", "caocao", "曹操",
                new[] { "luoyang", "xuchang", "yecheng" },
                new[] { "caocao", "xiahouyuan", "xuchu" }, true, 2000, 2000));

            factions.Add(CreateFactionData("shu", "蜀", "liubei", "劉備",
                new[] { "chengdu", "hanzhong" },
                new[] { "liubei", "zhugeliang", "guanyu" }, true, 0, 0));

            factions.Add(CreateFactionData("wu", "呉", "sunquan", "孫権",
                new[] { "jianye", "wuchang" },
                new[] { "sunquan", "zhouyu", "lusu" }, true, 1000, 1000));

            SetPrivateField(scenario, "_factions", factions);

            // 領地
            var territories = new System.Collections.Generic.List<ScenarioTerritoryData>();
            territories.Add(CreateTerritoryData("luoyang", "洛陽", "wei", 100000, 90, 80,
                new[] { "xuchang", "changan" }, new Vector2(150, 100)));
            territories.Add(CreateTerritoryData("xuchang", "許昌", "wei", 60000, 70, 60,
                new[] { "luoyang", "yecheng" }, new Vector2(180, 80)));
            territories.Add(CreateTerritoryData("yecheng", "鄴城", "wei", 80000, 75, 70,
                new[] { "xuchang" }, new Vector2(200, 120)));
            territories.Add(CreateTerritoryData("chengdu", "成都", "shu", 70000, 80, 75,
                new[] { "hanzhong" }, new Vector2(50, 50)));
            territories.Add(CreateTerritoryData("hanzhong", "漢中", "shu", 40000, 60, 65,
                new[] { "chengdu", "changan" }, new Vector2(80, 80)));
            territories.Add(CreateTerritoryData("changan", "長安", "", 90000, 85, 80,
                new[] { "luoyang", "hanzhong" }, new Vector2(100, 100)));
            territories.Add(CreateTerritoryData("jianye", "建業", "wu", 85000, 85, 70,
                new[] { "wuchang" }, new Vector2(220, 30)));
            territories.Add(CreateTerritoryData("wuchang", "武昌", "wu", 50000, 65, 60,
                new[] { "jianye" }, new Vector2(180, 40)));

            SetPrivateField(scenario, "_territories", territories);

            return scenario;
        }

        private static ScenarioFactionData CreateFactionData(string id, string name, string leaderId, string leaderName,
            string[] territories, string[] characters, bool playable, int bonusGold, int bonusSoldiers)
        {
            var data = new ScenarioFactionData();
            SetPrivateField(data, "_factionId", id);
            SetPrivateField(data, "_factionName", name);
            SetPrivateField(data, "_leaderId", leaderId);
            SetPrivateField(data, "_leaderName", leaderName);
            SetPrivateField(data, "_isPlayable", playable);
            SetPrivateField(data, "_startingTerritoryIds", new System.Collections.Generic.List<string>(territories));
            SetPrivateField(data, "_startingCharacterIds", new System.Collections.Generic.List<string>(characters));
            SetPrivateField(data, "_bonusGold", bonusGold);
            SetPrivateField(data, "_bonusSoldiers", bonusSoldiers);
            return data;
        }

        private static ScenarioTerritoryData CreateTerritoryData(string id, string name, string owner,
            int population, int economy, int defense, string[] adjacent, Vector2 position)
        {
            var data = new ScenarioTerritoryData();
            SetPrivateField(data, "_territoryId", id);
            SetPrivateField(data, "_territoryName", name);
            SetPrivateField(data, "_ownerId", owner);
            SetPrivateField(data, "_population", population);
            SetPrivateField(data, "_economy", economy);
            SetPrivateField(data, "_defense", defense);
            SetPrivateField(data, "_adjacentTerritoryIds", new System.Collections.Generic.List<string>(adjacent));
            SetPrivateField(data, "_mapPosition", position);
            return data;
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
        }

        private static void EnsureDirectoryExists(string path)
        {
            string directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
        }
#endif
    }
}
