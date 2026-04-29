using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Data.ScriptableObjects
{
    /// <summary>
    /// 勢力データ（ScriptableObject）
    /// </summary>
    [CreateAssetMenu(fileName = "Faction", menuName = "ThirtySixStratagems/Faction Data")]
    public class FactionData : ScriptableObject
    {
        [Header("基本情報")]
        public string FactionId;
        public string FactionName;
        public Color FactionColor = Color.red;

        [Header("初期リソース")]
        public int InitialGold = 1000;
        public int InitialFood = 500;
        public int InitialStratagemPoints = 5;
        public int MaxStratagemPoints = 10;

        [Header("君主")]
        public CharacterData Ruler;

        [Header("初期武将")]
        public List<CharacterData> InitialCharacters = new List<CharacterData>();

        [Header("初期領地")]
        public List<TerritoryData> InitialTerritories = new List<TerritoryData>();

        [Header("AI設定")]
        public AIPersonality AiPersonality = AIPersonality.Balanced;

        [Header("初期解放計略")]
        public List<StratagemData> InitialStratagems = new List<StratagemData>();

        [Header("ビジュアル")]
        public Sprite FactionEmblem;

        [Header("説明")]
        [TextArea(2, 4)]
        public string Description;

        /// <summary>
        /// Factionモデルを生成
        /// </summary>
        public Faction CreateFaction()
        {
            var faction = new Faction
            {
                Id = FactionId,
                Name = FactionName,
                FactionColor = FactionColor,
                Gold = InitialGold,
                Food = InitialFood,
                StratagemPoints = InitialStratagemPoints,
                MaxStratagemPoints = MaxStratagemPoints,
                AiPersonality = AiPersonality
            };

            if (Ruler != null)
            {
                faction.RulerId = Ruler.CharacterId;
            }

            foreach (var character in InitialCharacters)
            {
                if (character != null)
                {
                    faction.CharacterIds.Add(character.CharacterId);
                }
            }

            foreach (var territory in InitialTerritories)
            {
                if (territory != null)
                {
                    faction.TerritoryIds.Add(territory.TerritoryId);
                }
            }

            foreach (var stratagem in InitialStratagems)
            {
                if (stratagem != null)
                {
                    faction.UnlockedStratagemIds.Add(stratagem.StratagemId);
                }
            }

            return faction;
        }
    }
}
