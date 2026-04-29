using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Data.ScriptableObjects
{
    /// <summary>
    /// 領地データ（ScriptableObject）
    /// </summary>
    [CreateAssetMenu(fileName = "Territory", menuName = "ThirtySixStratagems/Territory Data")]
    public class TerritoryData : ScriptableObject
    {
        [Header("基本情報")]
        public string TerritoryId;
        public string TerritoryName;

        [Header("初期パラメータ")]
        [Range(1000, 100000)]
        public int InitialPopulation = 10000;

        [Range(1, 100)]
        public int InitialEconomy = 50;

        [Range(1, 100)]
        public int InitialDefense = 50;

        [Header("位置")]
        public Vector2 MapPosition;

        [Header("隣接領地")]
        public List<TerritoryData> AdjacentTerritories = new List<TerritoryData>();

        [Header("初期建物")]
        public List<BuildingSetup> InitialBuildings = new List<BuildingSetup>();

        [Header("ビジュアル")]
        public Sprite TerritoryIcon;
        public Color TerritoryColor = Color.white;

        [Header("説明")]
        [TextArea(2, 4)]
        public string Description;

        /// <summary>
        /// Territoryモデルを生成
        /// </summary>
        public Territory CreateTerritory()
        {
            var territory = new Territory
            {
                Id = TerritoryId,
                Name = TerritoryName,
                Population = InitialPopulation,
                Economy = InitialEconomy,
                Defense = InitialDefense,
                MapPosition = MapPosition
            };

            foreach (var adjTerritory in AdjacentTerritories)
            {
                if (adjTerritory != null)
                {
                    territory.AdjacentTerritoryIds.Add(adjTerritory.TerritoryId);
                }
            }

            foreach (var buildingSetup in InitialBuildings)
            {
                territory.Buildings.Add(new Building
                {
                    Type = buildingSetup.Type,
                    Level = buildingSetup.Level,
                    IsUnderConstruction = false
                });
            }

            return territory;
        }
    }

    /// <summary>
    /// 建物セットアップ
    /// </summary>
    [System.Serializable]
    public class BuildingSetup
    {
        public BuildingType Type;
        [Range(1, 5)]
        public int Level = 1;
    }
}
