using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThirtySixStratagems.Data.Models
{
    /// <summary>
    /// 領地モデル
    /// </summary>
    [Serializable]
    public class Territory
    {
        [Header("基本情報")]
        public string Id;
        public string Name;

        [Header("所有者")]
        public string OwnerId;

        [Header("パラメータ")]
        [Range(1000, 100000)]
        public int Population = 10000;

        [Range(1, 100)]
        public int Economy = 50;

        [Range(1, 100)]
        public int Defense = 50;

        [Header("駐留軍")]
        public string GarrisonArmyId;

        [Header("隣接領地")]
        public List<string> AdjacentTerritoryIds = new List<string>();

        [Header("施設")]
        public List<Building> Buildings = new List<Building>();

        [Header("位置")]
        public Vector2 MapPosition;

        /// <summary>
        /// 収入を計算
        /// </summary>
        public int CalculateIncome()
        {
            int baseIncome = Economy * 10;
            int populationBonus = Population / 1000;

            // 市場ボーナス
            int marketBonus = 0;
            foreach (var building in Buildings)
            {
                if (building.Type == BuildingType.Market)
                {
                    marketBonus += building.Level * 5;
                }
            }

            return baseIncome + populationBonus + marketBonus;
        }

        /// <summary>
        /// 最大徴兵数を計算
        /// </summary>
        public int CalculateMaxRecruitment()
        {
            int baseRecruitment = Population / 10;

            // 兵舎ボーナス
            int barracksBonus = 0;
            foreach (var building in Buildings)
            {
                if (building.Type == BuildingType.Barracks)
                {
                    barracksBonus += building.Level * 100;
                }
            }

            return baseRecruitment + barracksBonus;
        }

        /// <summary>
        /// 食料生産量を計算
        /// </summary>
        public int CalculateFoodProduction()
        {
            int baseFoodProduction = Population / 5;

            // 農場ボーナス
            int farmBonus = 0;
            foreach (var building in Buildings)
            {
                if (building.Type == BuildingType.Farm)
                {
                    farmBonus += building.Level * 50;
                }
            }

            return baseFoodProduction + farmBonus;
        }

        /// <summary>
        /// 防御力ボーナスを計算
        /// </summary>
        public int CalculateDefenseBonus()
        {
            int bonus = 0;

            foreach (var building in Buildings)
            {
                switch (building.Type)
                {
                    case BuildingType.Castle:
                        bonus += building.Level * 10;
                        break;
                    case BuildingType.Watchtower:
                        bonus += building.Level * 5;
                        break;
                }
            }

            return Defense + bonus;
        }
    }

    /// <summary>
    /// 建物
    /// </summary>
    [Serializable]
    public class Building
    {
        public BuildingType Type;

        [Range(1, 5)]
        public int Level = 1;

        public bool IsUnderConstruction;
        public int ConstructionTurnsLeft;
    }
}
