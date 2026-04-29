using System.Collections.Generic;
using UnityEngine;

namespace ThirtySixStratagems.Data.ScriptableObjects
{
    /// <summary>
    /// マップデータ（ScriptableObject）
    /// </summary>
    [CreateAssetMenu(fileName = "Map", menuName = "ThirtySixStratagems/Map Data")]
    public class MapData : ScriptableObject
    {
        [Header("基本情報")]
        public string MapId;
        public string MapName;

        [Header("マップサイズ")]
        public MapSize Size = MapSize.Medium;

        [Header("領地")]
        public List<TerritoryData> Territories = new List<TerritoryData>();

        [Header("勢力")]
        public List<FactionData> Factions = new List<FactionData>();

        [Header("プレイヤー勢力")]
        public FactionData PlayerFaction;

        [Header("勝利条件")]
        public VictoryCondition VictoryCondition = VictoryCondition.Conquest;
        public int VictoryTerritoryPercentage = 100;

        [Header("ターン制限")]
        public bool HasTurnLimit;
        public int MaxTurns = 100;

        [Header("ビジュアル")]
        public Sprite MapBackground;
        public Vector2 MapSize = new Vector2(1920, 1080);

        [Header("説明")]
        [TextArea(2, 4)]
        public string Description;

        /// <summary>
        /// 領地数
        /// </summary>
        public int TerritoryCount => Territories.Count;

        /// <summary>
        /// 勢力数
        /// </summary>
        public int FactionCount => Factions.Count;
    }

    /// <summary>
    /// マップサイズ
    /// </summary>
    public enum MapSize
    {
        Small,      // 8領地
        Medium,     // 12領地
        Large       // 20領地
    }

    /// <summary>
    /// 勝利条件
    /// </summary>
    public enum VictoryCondition
    {
        Conquest,       // 制覇（全領地支配）
        Domination,     // 支配（一定割合の領地）
        Elimination,    // 殲滅（全敵勢力滅亡）
        Survival        // 生存（一定ターン数生き残る）
    }
}
