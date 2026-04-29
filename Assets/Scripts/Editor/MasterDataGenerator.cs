#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
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

            if (GUILayout.Button("全マスターデータを生成", GUILayout.Height(40)))
            {
                GenerateAllData();
            }
        }

        private void GenerateAllData()
        {
            GenerateStratagemData();
            GenerateTerritoryData();
            GenerateCharacterData();
            GenerateFactionData();
            EditorUtility.DisplayDialog("完了", "全マスターデータの生成が完了しました。", "OK");
        }

        private void GenerateStratagemData()
        {
            string folderPath = "Assets/Data/Stratagems";
            EnsureDirectoryExists(folderPath);

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
            }

            // データベースを保存
            AssetDatabase.CreateAsset(database, $"{folderPath}/StratagemDatabase.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"三十六計データを生成しました: {folderPath}");
        }

        private void GenerateTerritoryData()
        {
            string folderPath = "Assets/Data/Territories";
            EnsureDirectoryExists(folderPath);

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
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"領地データを生成しました: {folderPath}");
        }

        private void GenerateCharacterData()
        {
            string folderPath = "Assets/Data/Characters";
            EnsureDirectoryExists(folderPath);

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
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"武将データを生成しました: {folderPath}");
        }

        private void GenerateFactionData()
        {
            string folderPath = "Assets/Data/Factions";
            EnsureDirectoryExists(folderPath);

            foreach (var def in SampleDataDefinitions.SampleFactions)
            {
                var faction = ScriptableObject.CreateInstance<FactionData>();

                faction.FactionId = def.Id;
                faction.FactionName = def.Name;
                faction.FactionColor = def.Color;
                faction.InitialGold = def.InitialGold;
                faction.InitialFood = def.InitialFood;
                faction.Description = def.Description;

                string assetPath = $"{folderPath}/{def.Name}.asset";
                AssetDatabase.CreateAsset(faction, assetPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"勢力データを生成しました: {folderPath}");
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
