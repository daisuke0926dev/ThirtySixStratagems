using System.Collections.Generic;
using UnityEngine;

namespace ThirtySixStratagems.Data.ScriptableObjects
{
    /// <summary>
    /// 領地データベース
    /// 全領地データを管理するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "TerritoryDatabase", menuName = "ThirtySixStratagems/Territory Database")]
    public class TerritoryDatabase : ScriptableObject
    {
        [Header("全領地")]
        [SerializeField] private List<TerritoryData> _allTerritories = new List<TerritoryData>();

        private Dictionary<string, TerritoryData> _territoryById;

        /// <summary>
        /// 全領地リスト
        /// </summary>
        public IReadOnlyList<TerritoryData> AllTerritories => _allTerritories;

        /// <summary>
        /// 領地数
        /// </summary>
        public int Count => _allTerritories.Count;

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            _territoryById = new Dictionary<string, TerritoryData>();

            foreach (var territory in _allTerritories)
            {
                if (territory != null && !string.IsNullOrEmpty(territory.TerritoryId))
                {
                    _territoryById[territory.TerritoryId] = territory;
                }
            }
        }

        /// <summary>
        /// IDで領地を取得
        /// </summary>
        public TerritoryData GetById(string territoryId)
        {
            if (_territoryById == null) Initialize();

            if (string.IsNullOrEmpty(territoryId)) return null;

            return _territoryById.TryGetValue(territoryId, out var territory) ? territory : null;
        }

        /// <summary>
        /// 名前で領地を取得
        /// </summary>
        public TerritoryData GetByName(string territoryName)
        {
            if (string.IsNullOrEmpty(territoryName)) return null;

            foreach (var territory in _allTerritories)
            {
                if (territory != null && territory.TerritoryName == territoryName)
                {
                    return territory;
                }
            }
            return null;
        }

        /// <summary>
        /// 領地を追加
        /// </summary>
        public void AddTerritory(TerritoryData territory)
        {
            if (territory != null && !_allTerritories.Contains(territory))
            {
                _allTerritories.Add(territory);
                if (_territoryById != null)
                {
                    _territoryById[territory.TerritoryId] = territory;
                }
            }
        }

        /// <summary>
        /// 隣接領地を取得
        /// </summary>
        public List<TerritoryData> GetAdjacentTerritories(string territoryId)
        {
            var result = new List<TerritoryData>();
            var territory = GetById(territoryId);

            if (territory != null)
            {
                foreach (var adj in territory.AdjacentTerritories)
                {
                    if (adj != null)
                    {
                        result.Add(adj);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// データ検証
        /// </summary>
        public bool Validate(out List<string> errors)
        {
            errors = new List<string>();
            var usedIds = new HashSet<string>();

            foreach (var territory in _allTerritories)
            {
                if (territory == null)
                {
                    errors.Add("null の領地データがあります");
                    continue;
                }

                if (string.IsNullOrEmpty(territory.TerritoryId))
                {
                    errors.Add($"領地 {territory.TerritoryName} のIDが空です");
                }
                else if (usedIds.Contains(territory.TerritoryId))
                {
                    errors.Add($"IDが重複しています: {territory.TerritoryId}");
                }
                else
                {
                    usedIds.Add(territory.TerritoryId);
                }

                if (string.IsNullOrEmpty(territory.TerritoryName))
                {
                    errors.Add($"領地 {territory.TerritoryId} の名前が空です");
                }
            }

            return errors.Count == 0;
        }
    }
}
