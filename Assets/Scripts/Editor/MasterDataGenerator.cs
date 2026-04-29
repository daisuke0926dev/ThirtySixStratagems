#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using ThirtySixStratagems.Data;
using ThirtySixStratagems.Data.Models;
using ThirtySixStratagems.Data.ScriptableObjects;

namespace ThirtySixStratagems.Editor
{
    /// <summary>
    /// マスターデータ生成エディタ
    /// </summary>
    public class MasterDataGenerator : EditorWindow
    {
        // 生成されたアセットのキャッシュ
        private static Dictionary<string, CharacterData> _characterCache = new Dictionary<string, CharacterData>();
        private static Dictionary<string, TerritoryData> _territoryCache = new Dictionary<string, TerritoryData>();
        private static Dictionary<string, FactionData> _factionCache = new Dictionary<string, FactionData>();
        private static Dictionary<string, StratagemData> _stratagemCache = new Dictionary<string, StratagemData>();
        private static StratagemDatabase _stratagemDatabase;

        [MenuItem("ThirtySixStratagems/Generate Master Data")]
        public static void ShowWindow()
        {
            GetWindow<MasterDataGenerator>("Master Data Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("マスターデータ生成", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "このツールは三十六計とサンプルデータのScriptableObjectを生成します。\n" +
                "既存のデータは上書きされます。",
                MessageType.Info);

            EditorGUILayout.Space();

            if (GUILayout.Button("三十六計データを生成", GUILayout.Height(30)))
            {
                GenerateStratagemData();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("サンプル領地データを生成", GUILayout.Height(30)))
            {
                GenerateTerritoryData();
            }

            if (GUILayout.Button("サンプル武将データを生成", GUILayout.Height(30)))
            {
                GenerateCharacterData();
            }

            if (GUILayout.Button("サンプル勢力データを生成", GUILayout.Height(30)))
            {
                GenerateFactionData();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("シナリオ生成", EditorStyles.boldLabel);

            if (GUILayout.Button("シナリオデータを生成", GUILayout.Height(30)))
            {
                GenerateScenarioData();
            }

            if (GUILayout.Button("マップデータを生成", GUILayout.Height(30)))
            {
                GenerateMapData();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            if (GUILayout.Button("全マスターデータを生成", GUILayout.Height(40)))
            {
                GenerateAllData();
            }
        }

        private void GenerateAllData()
        {
            ClearCaches();
            GenerateStratagemData();
            GenerateTerritoryData();
            GenerateCharacterData();
            GenerateFactionData();
            LinkFactionReferences();
            GenerateMapData();
            GenerateScenarioData();
            EditorUtility.DisplayDialog("完了", "全マスターデータの生成が完了しました。", "OK");
        }

        private void ClearCaches()
        {
            _characterCache.Clear();
            _territoryCache.Clear();
            _factionCache.Clear();
            _stratagemCache.Clear();
            _stratagemDatabase = null;
        }

        private void GenerateStratagemData()
        {
            string folderPath = "Assets/Data/Stratagems";
            EnsureDirectoryExists(folderPath);

            _stratagemCache.Clear();

            // 計略データベース
            var database = ScriptableObject.CreateInstance<StratagemDatabase>();
            database.AllStratagems = new System.Collections.Generic.List<StratagemData>();

            foreach (var def in StratagemDefinitions.AllStratagems)
            {
                var stratagem = ScriptableObject.CreateInstance<StratagemData>();

                stratagem.StratagemId = $"stratagem_{def.Number:D2}";
                stratagem.Number = def.Number;
                stratagem.NameJP = def.NameJP;
                stratagem.NameCN = def.NameJP; // 同じ漢字を使用
                stratagem.Reading = def.Reading;
                stratagem.Category = def.Category;
                stratagem.OriginalText = def.OriginalText;
                stratagem.ModernTranslation = def.ModernTranslation;
                stratagem.HistoricalExample = def.HistoricalExample;
                stratagem.GameEffectDescription = GetEffectDescription(def.Effect, def.EffectValue);
                stratagem.CostSP = def.CostSP;
                stratagem.CostGold = 0;
                stratagem.TargetType = GetTargetType(def.Effect);
                stratagem.BaseSuccessRate = 70;
                stratagem.PrimaryEffect = def.Effect;
                stratagem.EffectValue = def.EffectValue;
                stratagem.Duration = def.Duration;

                string assetPath = $"{folderPath}/{def.Number:D2}_{def.NameJP}.asset";
                AssetDatabase.CreateAsset(stratagem, assetPath);

                database.AllStratagems.Add(stratagem);
                _stratagemCache[stratagem.StratagemId] = stratagem;
            }

            // データベースを保存
            AssetDatabase.CreateAsset(database, $"{folderPath}/StratagemDatabase.asset");
            _stratagemDatabase = database;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"三十六計データを生成しました: {folderPath}");
        }

        private void GenerateTerritoryData()
        {
            string folderPath = "Assets/Data/Territories";
            EnsureDirectoryExists(folderPath);

            _territoryCache.Clear();

            // 領地データベース
            var database = ScriptableObject.CreateInstance<TerritoryDatabase>();

            foreach (var def in SampleDataDefinitions.SampleTerritories)
            {
                var territory = ScriptableObject.CreateInstance<TerritoryData>();

                territory.TerritoryId = def.Id;
                territory.TerritoryName = def.Name;
                territory.InitialPopulation = def.Population;
                territory.InitialEconomy = def.Economy;
                territory.InitialDefense = def.Defense;
                territory.MapPosition = def.Position;
                territory.Description = def.Description;

                string assetPath = $"{folderPath}/{def.Name}.asset";
                AssetDatabase.CreateAsset(territory, assetPath);

                _territoryCache[def.Id] = territory;
                database.AddTerritory(territory);
            }

            // 隣接領地の設定
            SetupAdjacentTerritories();

            // データベースを保存
            AssetDatabase.CreateAsset(database, $"{folderPath}/TerritoryDatabase.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"領地データを生成しました: {folderPath}");
        }

        private void SetupAdjacentTerritories()
        {
            // 隣接関係を定義（マップの地理に基づく）
            var adjacencyMap = new Dictionary<string, string[]>
            {
                { "territory_luoyang", new[] { "territory_xuchang", "territory_changan", "territory_hanzhong" } },
                { "territory_changan", new[] { "territory_luoyang", "territory_hanzhong", "territory_yizhou" } },
                { "territory_xuchang", new[] { "territory_luoyang", "territory_youzhou", "territory_jingzhou" } },
                { "territory_jiangdong", new[] { "territory_jingzhou" } },
                { "territory_jingzhou", new[] { "territory_xuchang", "territory_jiangdong", "territory_yizhou", "territory_hanzhong" } },
                { "territory_yizhou", new[] { "territory_changan", "territory_hanzhong", "territory_jingzhou" } },
                { "territory_hanzhong", new[] { "territory_luoyang", "territory_changan", "territory_yizhou", "territory_jingzhou" } },
                { "territory_youzhou", new[] { "territory_xuchang", "territory_luoyang" } }
            };

            foreach (var kvp in adjacencyMap)
            {
                if (_territoryCache.TryGetValue(kvp.Key, out var territory))
                {
                    territory.AdjacentTerritories = new List<TerritoryData>();
                    foreach (var adjId in kvp.Value)
                    {
                        if (_territoryCache.TryGetValue(adjId, out var adjTerritory))
                        {
                            territory.AdjacentTerritories.Add(adjTerritory);
                        }
                    }
                    EditorUtility.SetDirty(territory);
                }
            }
        }

        private void GenerateCharacterData()
        {
            string folderPath = "Assets/Data/Characters";
            EnsureDirectoryExists(folderPath);

            _characterCache.Clear();

            // 武将データベース
            var database = ScriptableObject.CreateInstance<CharacterDatabase>();

            foreach (var def in SampleDataDefinitions.SampleCharacters)
            {
                var character = ScriptableObject.CreateInstance<CharacterData>();

                character.CharacterId = def.Id;
                character.CharacterName = def.Name;
                character.Type = def.Type;
                character.Strength = def.Strength;
                character.Intelligence = def.Intelligence;
                character.Leadership = def.Leadership;
                character.Politics = def.Politics;
                character.Charisma = def.Charisma;
                character.SpecialtyCategory = def.Specialty;
                character.Biography = def.Biography;

                string assetPath = $"{folderPath}/{def.Name}.asset";
                AssetDatabase.CreateAsset(character, assetPath);

                _characterCache[def.Id] = character;
                database.AddCharacter(character);
            }

            // データベースを保存
            AssetDatabase.CreateAsset(database, $"{folderPath}/CharacterDatabase.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"武将データを生成しました: {folderPath}");
        }

        private void GenerateFactionData()
        {
            string folderPath = "Assets/Data/Factions";
            EnsureDirectoryExists(folderPath);

            _factionCache.Clear();

            // 勢力データベース
            var database = ScriptableObject.CreateInstance<FactionDatabase>();

            foreach (var def in SampleDataDefinitions.SampleFactions)
            {
                var faction = ScriptableObject.CreateInstance<FactionData>();

                faction.FactionId = def.Id;
                faction.FactionName = def.Name;
                faction.FactionColor = def.Color;
                faction.InitialGold = def.InitialGold;
                faction.InitialFood = def.InitialFood;
                faction.Description = def.Description;

                // AI性格を設定
                faction.AiPersonality = GetAIPersonalityForFaction(def.Id);

                string assetPath = $"{folderPath}/{def.Name}.asset";
                AssetDatabase.CreateAsset(faction, assetPath);

                _factionCache[def.Id] = faction;
                database.AddFaction(faction);
            }

            // データベースを保存
            AssetDatabase.CreateAsset(database, $"{folderPath}/FactionDatabase.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"勢力データを生成しました: {folderPath}");
        }

        private AIPersonality GetAIPersonalityForFaction(string factionId)
        {
            switch (factionId)
            {
                case "faction_wei":
                    return AIPersonality.Aggressive; // 魏：攻撃的
                case "faction_shu":
                    return AIPersonality.Strategic;  // 蜀：計略重視
                case "faction_wu":
                    return AIPersonality.Defensive;  // 呉：防御的
                default:
                    return AIPersonality.Balanced;
            }
        }

        private void LinkFactionReferences()
        {
            // 勢力にキャラクターと領地の参照をリンク
            foreach (var factionDef in SampleDataDefinitions.SampleFactions)
            {
                if (!_factionCache.TryGetValue(factionDef.Id, out var faction))
                    continue;

                // 君主を設定
                if (_characterCache.TryGetValue(factionDef.RulerId, out var ruler))
                {
                    faction.Ruler = ruler;
                }

                // 初期武将を設定
                faction.InitialCharacters = new List<CharacterData>();
                foreach (var charId in factionDef.CharacterIds)
                {
                    if (_characterCache.TryGetValue(charId, out var character))
                    {
                        faction.InitialCharacters.Add(character);
                    }
                }

                // 初期領地を設定
                faction.InitialTerritories = new List<TerritoryData>();
                foreach (var territoryId in factionDef.TerritoryIds)
                {
                    if (_territoryCache.TryGetValue(territoryId, out var territory))
                    {
                        faction.InitialTerritories.Add(territory);
                    }
                }

                // 初期計略を設定（各勢力の得意分野から）
                faction.InitialStratagems = GetInitialStratagemsForFaction(factionDef.Id);

                EditorUtility.SetDirty(faction);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("勢力の参照をリンクしました");
        }

        private List<StratagemData> GetInitialStratagemsForFaction(string factionId)
        {
            var stratagems = new List<StratagemData>();

            // 各勢力に基本計略を付与
            var basicIds = new[] { "stratagem_01", "stratagem_04", "stratagem_13", "stratagem_32", "stratagem_36" };
            foreach (var id in basicIds)
            {
                if (_stratagemCache.TryGetValue(id, out var stratagem))
                {
                    stratagems.Add(stratagem);
                }
            }

            // 勢力ごとの特殊計略
            string[] bonusIds = factionId switch
            {
                "faction_wei" => new[] { "stratagem_03", "stratagem_05" }, // 借刀殺人、趁火打劫
                "faction_shu" => new[] { "stratagem_16", "stratagem_21" }, // 欲擒姑縦、金蝉脱殻
                "faction_wu" => new[] { "stratagem_34", "stratagem_35" }, // 苦肉計、連環計
                _ => new string[0]
            };

            foreach (var id in bonusIds)
            {
                if (_stratagemCache.TryGetValue(id, out var stratagem))
                {
                    stratagems.Add(stratagem);
                }
            }

            return stratagems;
        }

        private void GenerateMapData()
        {
            string folderPath = "Assets/Data/Maps";
            EnsureDirectoryExists(folderPath);

            // 三国志マップを生成
            var map = ScriptableObject.CreateInstance<MapData>();
            map.MapId = "map_sangokushi";
            map.MapName = "三国志";
            map.Size = MapSize.Small;
            map.Description = "三国時代の中国。魏・蜀・呉の三勢力が覇権を争う。";
            map.VictoryCondition = VictoryCondition.Conquest;
            map.VictoryTerritoryPercentage = 100;
            map.HasTurnLimit = false;

            // 領地を設定
            map.Territories = _territoryCache.Values.ToList();

            // 勢力を設定
            map.Factions = _factionCache.Values.ToList();

            string assetPath = $"{folderPath}/Map_Sangokushi.asset";
            AssetDatabase.CreateAsset(map, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"マップデータを生成しました: {folderPath}");
        }

        private void GenerateScenarioData()
        {
            string folderPath = "Assets/Data/Scenarios";
            EnsureDirectoryExists(folderPath);

            // ScenarioDatabase を生成
            var database = ScriptableObject.CreateInstance<ScenarioDatabase>();

            // チュートリアルシナリオ
            var tutorial = CreateTutorialScenario(folderPath);
            database.AddScenario(tutorial);

            // 三国志シナリオ
            var sangokushi = CreateSangokushiScenario(folderPath);
            database.AddScenario(sangokushi);

            // データベースを保存
            string dbPath = $"{folderPath}/ScenarioDatabase.asset";
            AssetDatabase.CreateAsset(database, dbPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"シナリオデータを生成しました: {folderPath}");
        }

        private ScenarioData CreateTutorialScenario(string folderPath)
        {
            var scenario = ScriptableObject.CreateInstance<ScenarioData>();

            // リフレクションを使ってプライベートフィールドに値を設定
            SetPrivateField(scenario, "_scenarioId", "scenario_tutorial");
            SetPrivateField(scenario, "_scenarioName", "計略入門");
            SetPrivateField(scenario, "_description",
                "三十六計の基本を学ぶチュートリアルシナリオ。\n" +
                "2つの勢力で計略の使い方を覚えましょう。");
            SetPrivateField(scenario, "_year", 220);
            SetPrivateField(scenario, "_difficulty", 1);
            SetPrivateField(scenario, "_startingGold", 8000);
            SetPrivateField(scenario, "_startingFood", 15000);
            SetPrivateField(scenario, "_startingStratagemPoints", 8);
            SetPrivateField(scenario, "_victoryCondition", VictoryConditionType.TerritoryCount);
            SetPrivateField(scenario, "_victoryTargetValue", 3);

            // チュートリアル用の簡易勢力
            var factions = new List<ScenarioFactionData>
            {
                CreateScenarioFaction("faction_player", "味方軍", "char_liubei", "劉備",
                    new Color(0.2f, 0.7f, 0.3f), true,
                    new[] { "territory_yizhou", "territory_hanzhong" },
                    new[] { "char_liubei", "char_zhugeliang" }, 2000, 500),
                CreateScenarioFaction("faction_enemy", "敵軍", "char_caocao", "曹操",
                    new Color(0.2f, 0.4f, 0.8f), false,
                    new[] { "territory_changan" },
                    new[] { "char_caocao", "char_simayi" }, 0, 0)
            };
            SetPrivateField(scenario, "_factions", factions);

            // チュートリアル用の領地
            var territories = new List<ScenarioTerritoryData>
            {
                CreateScenarioTerritory("territory_yizhou", "益州", "faction_player", 45000, 65, 90),
                CreateScenarioTerritory("territory_hanzhong", "漢中", "faction_player", 30000, 50, 85),
                CreateScenarioTerritory("territory_changan", "長安", "faction_enemy", 70000, 85, 80)
            };
            SetPrivateField(scenario, "_territories", territories);

            string assetPath = $"{folderPath}/Scenario_Tutorial.asset";
            AssetDatabase.CreateAsset(scenario, assetPath);

            return scenario;
        }

        private ScenarioData CreateSangokushiScenario(string folderPath)
        {
            var scenario = ScriptableObject.CreateInstance<ScenarioData>();

            SetPrivateField(scenario, "_scenarioId", "scenario_sangokushi");
            SetPrivateField(scenario, "_scenarioName", "三国鼎立");
            SetPrivateField(scenario, "_description",
                "西暦220年、魏・蜀・呉の三国が鼎立する時代。\n" +
                "天下統一を目指し、三十六計を駆使して覇権を争え！");
            SetPrivateField(scenario, "_year", 220);
            SetPrivateField(scenario, "_difficulty", 3);
            SetPrivateField(scenario, "_startingGold", 5000);
            SetPrivateField(scenario, "_startingFood", 10000);
            SetPrivateField(scenario, "_startingStratagemPoints", 5);
            SetPrivateField(scenario, "_victoryCondition", VictoryConditionType.Conquest);
            SetPrivateField(scenario, "_victoryTargetValue", 0);

            // 三勢力
            var factions = new List<ScenarioFactionData>
            {
                CreateScenarioFaction("faction_wei", "魏", "char_caocao", "曹操",
                    new Color(0.2f, 0.4f, 0.8f), true,
                    new[] { "territory_luoyang", "territory_xuchang", "territory_youzhou" },
                    new[] { "char_caocao", "char_simayi", "char_xiahouyuan", "char_xuhuang" }, 2000, 1000),
                CreateScenarioFaction("faction_shu", "蜀", "char_liubei", "劉備",
                    new Color(0.2f, 0.7f, 0.3f), true,
                    new[] { "territory_yizhou", "territory_hanzhong" },
                    new[] { "char_liubei", "char_zhugeliang", "char_guanyu", "char_zhangfei" }, 0, 500),
                CreateScenarioFaction("faction_wu", "呉", "char_sunquan", "孫権",
                    new Color(0.8f, 0.2f, 0.2f), true,
                    new[] { "territory_jiangdong", "territory_jingzhou", "territory_changan" },
                    new[] { "char_sunquan", "char_zhouyu", "char_lümeng", "char_luxun" }, 1000, 1500)
            };
            SetPrivateField(scenario, "_factions", factions);

            // 全8領地
            var territories = new List<ScenarioTerritoryData>();
            foreach (var def in SampleDataDefinitions.SampleTerritories)
            {
                string ownerId = GetTerritoryOwner(def.Id);
                territories.Add(CreateScenarioTerritory(def.Id, def.Name, ownerId,
                    def.Population, def.Economy, def.Defense, def.Position));
            }
            SetPrivateField(scenario, "_territories", territories);

            string assetPath = $"{folderPath}/Scenario_Sangokushi.asset";
            AssetDatabase.CreateAsset(scenario, assetPath);

            return scenario;
        }

        private string GetTerritoryOwner(string territoryId)
        {
            foreach (var faction in SampleDataDefinitions.SampleFactions)
            {
                if (faction.TerritoryIds.Contains(territoryId))
                    return faction.Id;
            }
            return "";
        }

        private ScenarioFactionData CreateScenarioFaction(
            string factionId, string factionName, string leaderId, string leaderName,
            Color color, bool isPlayable, string[] territoryIds, string[] characterIds,
            int bonusGold, int bonusSoldiers)
        {
            var faction = new ScenarioFactionData();
            SetPrivateField(faction, "_factionId", factionId);
            SetPrivateField(faction, "_factionName", factionName);
            SetPrivateField(faction, "_leaderId", leaderId);
            SetPrivateField(faction, "_leaderName", leaderName);
            SetPrivateField(faction, "_factionColor", color);
            SetPrivateField(faction, "_isPlayable", isPlayable);
            SetPrivateField(faction, "_startingTerritoryIds", new List<string>(territoryIds));
            SetPrivateField(faction, "_startingCharacterIds", new List<string>(characterIds));
            SetPrivateField(faction, "_bonusGold", bonusGold);
            SetPrivateField(faction, "_bonusSoldiers", bonusSoldiers);
            return faction;
        }

        private ScenarioTerritoryData CreateScenarioTerritory(
            string territoryId, string territoryName, string ownerId,
            int population, int economy, int defense, Vector2 position = default)
        {
            var territory = new ScenarioTerritoryData();
            SetPrivateField(territory, "_territoryId", territoryId);
            SetPrivateField(territory, "_territoryName", territoryName);
            SetPrivateField(territory, "_ownerId", ownerId);
            SetPrivateField(territory, "_population", population);
            SetPrivateField(territory, "_economy", economy);
            SetPrivateField(territory, "_defense", defense);
            SetPrivateField(territory, "_mapPosition", position);
            return territory;
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parentFolder = Path.GetDirectoryName(path);
                string newFolderName = Path.GetFileName(path);

                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    Directory.CreateDirectory(parentFolder);
                }

                AssetDatabase.CreateFolder(parentFolder, newFolderName);
            }
        }

        private string GetEffectDescription(StratagemEffectType effect, int value)
        {
            switch (effect)
            {
                case StratagemEffectType.StealthMovement:
                    return "軍の移動を敵に察知されない";
                case StratagemEffectType.ForceRetreat:
                    return $"敵軍を強制的に撤退させる（成功率{value}%）";
                case StratagemEffectType.FactionConflict:
                    return "敵同士を争わせる";
                case StratagemEffectType.DefenseBoost:
                    return $"防御力が{value}%上昇";
                case StratagemEffectType.AttackBoost:
                    return $"攻撃力が{value}%上昇";
                case StratagemEffectType.Ambush:
                    return $"奇襲攻撃（ダメージ{value}%増加）";
                case StratagemEffectType.Disinformation:
                    return "敵に偽情報を流す";
                case StratagemEffectType.Diplomacy:
                    return "外交関係を操作する";
                case StratagemEffectType.ResourcePlunder:
                    return $"敵から資源を{value}%略奪";
                case StratagemEffectType.Reconnaissance:
                    return "敵の情報を探る";
                case StratagemEffectType.TerritoryControl:
                    return "領地支配に影響を与える";
                case StratagemEffectType.LoyaltyReduce:
                    return $"敵武将の忠誠度を{value}低下";
                case StratagemEffectType.CharacterCapture:
                    return $"敵将を捕獲（成功率{value}%）";
                case StratagemEffectType.SupplyDisrupt:
                    return "敵の補給線を遮断";
                case StratagemEffectType.Escape:
                    return "安全に撤退できる";
                case StratagemEffectType.InternalStrife:
                    return "敵内部に混乱を起こす";
                case StratagemEffectType.Composite:
                    return "複合的な効果を発揮";
                default:
                    return "";
            }
        }

        private StratagemTarget GetTargetType(StratagemEffectType effect)
        {
            switch (effect)
            {
                case StratagemEffectType.DefenseBoost:
                case StratagemEffectType.StealthMovement:
                case StratagemEffectType.Escape:
                    return StratagemTarget.Self;

                case StratagemEffectType.AttackBoost:
                case StratagemEffectType.Ambush:
                case StratagemEffectType.ForceRetreat:
                    return StratagemTarget.EnemyArmy;

                case StratagemEffectType.LoyaltyReduce:
                case StratagemEffectType.CharacterCapture:
                    return StratagemTarget.EnemyCharacter;

                case StratagemEffectType.SupplyDisrupt:
                case StratagemEffectType.ResourcePlunder:
                    return StratagemTarget.EnemyTerritory;

                case StratagemEffectType.FactionConflict:
                case StratagemEffectType.Disinformation:
                case StratagemEffectType.InternalStrife:
                    return StratagemTarget.EnemyFaction;

                case StratagemEffectType.Diplomacy:
                    return StratagemTarget.Any;

                default:
                    return StratagemTarget.EnemyFaction;
            }
        }
    }
}
#endif
