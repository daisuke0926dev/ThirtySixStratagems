using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Data.ScriptableObjects
{
    /// <summary>
    /// 勢力データベース
    /// 全勢力データを管理するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "FactionDatabase", menuName = "ThirtySixStratagems/Faction Database")]
    public class FactionDatabase : ScriptableObject
    {
        [Header("全勢力")]
        [SerializeField] private List<FactionData> _allFactions = new List<FactionData>();

        private Dictionary<string, FactionData> _factionById;

        /// <summary>
        /// 全勢力リスト
        /// </summary>
        public IReadOnlyList<FactionData> AllFactions => _allFactions;

        /// <summary>
        /// 勢力数
        /// </summary>
        public int Count => _allFactions.Count;

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            _factionById = new Dictionary<string, FactionData>();

            foreach (var faction in _allFactions)
            {
                if (faction != null && !string.IsNullOrEmpty(faction.FactionId))
                {
                    _factionById[faction.FactionId] = faction;
                }
            }
        }

        /// <summary>
        /// IDで勢力を取得
        /// </summary>
        public FactionData GetById(string factionId)
        {
            if (_factionById == null) Initialize();

            if (string.IsNullOrEmpty(factionId)) return null;

            return _factionById.TryGetValue(factionId, out var faction) ? faction : null;
        }

        /// <summary>
        /// 名前で勢力を取得
        /// </summary>
        public FactionData GetByName(string factionName)
        {
            if (string.IsNullOrEmpty(factionName)) return null;

            foreach (var faction in _allFactions)
            {
                if (faction != null && faction.FactionName == factionName)
                {
                    return faction;
                }
            }
            return null;
        }

        /// <summary>
        /// AI性格で勢力を取得
        /// </summary>
        public List<FactionData> GetByPersonality(AIPersonality personality)
        {
            var result = new List<FactionData>();
            foreach (var faction in _allFactions)
            {
                if (faction != null && faction.AiPersonality == personality)
                {
                    result.Add(faction);
                }
            }
            return result;
        }

        /// <summary>
        /// 勢力を追加
        /// </summary>
        public void AddFaction(FactionData faction)
        {
            if (faction != null && !_allFactions.Contains(faction))
            {
                _allFactions.Add(faction);
                if (_factionById != null)
                {
                    _factionById[faction.FactionId] = faction;
                }
            }
        }

        /// <summary>
        /// データ検証
        /// </summary>
        public bool Validate(out List<string> errors)
        {
            errors = new List<string>();
            var usedIds = new HashSet<string>();

            foreach (var faction in _allFactions)
            {
                if (faction == null)
                {
                    errors.Add("null の勢力データがあります");
                    continue;
                }

                if (string.IsNullOrEmpty(faction.FactionId))
                {
                    errors.Add($"勢力 {faction.FactionName} のIDが空です");
                }
                else if (usedIds.Contains(faction.FactionId))
                {
                    errors.Add($"IDが重複しています: {faction.FactionId}");
                }
                else
                {
                    usedIds.Add(faction.FactionId);
                }

                if (string.IsNullOrEmpty(faction.FactionName))
                {
                    errors.Add($"勢力 {faction.FactionId} の名前が空です");
                }

                if (faction.Ruler == null)
                {
                    errors.Add($"勢力 {faction.FactionName} の君主が設定されていません");
                }
            }

            return errors.Count == 0;
        }
    }
}
