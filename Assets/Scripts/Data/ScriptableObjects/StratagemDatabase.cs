using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Data.ScriptableObjects
{
    /// <summary>
    /// 三十六計データベース
    /// 全計略を管理するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "StratagemDatabase", menuName = "ThirtySixStratagems/Stratagem Database")]
    public class StratagemDatabase : ScriptableObject
    {
        [Header("全三十六計")]
        public List<StratagemData> AllStratagems = new List<StratagemData>();

        private Dictionary<string, StratagemData> _stratagemById;
        private Dictionary<int, StratagemData> _stratagemByNumber;
        private Dictionary<StratagemCategory, List<StratagemData>> _stratagemsByCategory;

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            _stratagemById = new Dictionary<string, StratagemData>();
            _stratagemByNumber = new Dictionary<int, StratagemData>();
            _stratagemsByCategory = new Dictionary<StratagemCategory, List<StratagemData>>();

            foreach (StratagemCategory category in System.Enum.GetValues(typeof(StratagemCategory)))
            {
                _stratagemsByCategory[category] = new List<StratagemData>();
            }

            foreach (var stratagem in AllStratagems)
            {
                if (stratagem != null)
                {
                    _stratagemById[stratagem.StratagemId] = stratagem;
                    _stratagemByNumber[stratagem.Number] = stratagem;
                    _stratagemsByCategory[stratagem.Category].Add(stratagem);
                }
            }
        }

        /// <summary>
        /// IDで計略を取得
        /// </summary>
        public StratagemData GetById(string id)
        {
            if (_stratagemById == null) Initialize();
            return _stratagemById.TryGetValue(id, out var stratagem) ? stratagem : null;
        }

        /// <summary>
        /// 番号で計略を取得
        /// </summary>
        public StratagemData GetByNumber(int number)
        {
            if (_stratagemByNumber == null) Initialize();
            return _stratagemByNumber.TryGetValue(number, out var stratagem) ? stratagem : null;
        }

        /// <summary>
        /// カテゴリで計略リストを取得
        /// </summary>
        public List<StratagemData> GetByCategory(StratagemCategory category)
        {
            if (_stratagemsByCategory == null) Initialize();
            return _stratagemsByCategory.TryGetValue(category, out var list) ? list : new List<StratagemData>();
        }

        /// <summary>
        /// 勝戦計を取得（第1-6計）
        /// </summary>
        public List<StratagemData> GetWinningStratagems()
        {
            return GetByCategory(StratagemCategory.Winning);
        }

        /// <summary>
        /// 敵戦計を取得（第7-12計）
        /// </summary>
        public List<StratagemData> GetEnemyStratagems()
        {
            return GetByCategory(StratagemCategory.Enemy);
        }

        /// <summary>
        /// 攻戦計を取得（第13-18計）
        /// </summary>
        public List<StratagemData> GetAttackStratagems()
        {
            return GetByCategory(StratagemCategory.Attack);
        }

        /// <summary>
        /// 混戦計を取得（第19-24計）
        /// </summary>
        public List<StratagemData> GetChaosStratagems()
        {
            return GetByCategory(StratagemCategory.Chaos);
        }

        /// <summary>
        /// 併戦計を取得（第25-30計）
        /// </summary>
        public List<StratagemData> GetMergeStratagems()
        {
            return GetByCategory(StratagemCategory.Merge);
        }

        /// <summary>
        /// 敗戦計を取得（第31-36計）
        /// </summary>
        public List<StratagemData> GetDefeatStratagems()
        {
            return GetByCategory(StratagemCategory.Defeat);
        }

        /// <summary>
        /// 計略数を取得
        /// </summary>
        public int Count => AllStratagems.Count;

        /// <summary>
        /// データ検証
        /// </summary>
        public bool Validate(out List<string> errors)
        {
            errors = new List<string>();

            if (AllStratagems.Count != 36)
            {
                errors.Add($"計略数が36ではありません: {AllStratagems.Count}");
            }

            var usedNumbers = new HashSet<int>();
            var usedIds = new HashSet<string>();

            foreach (var stratagem in AllStratagems)
            {
                if (stratagem == null)
                {
                    errors.Add("null の計略があります");
                    continue;
                }

                if (usedNumbers.Contains(stratagem.Number))
                {
                    errors.Add($"番号が重複しています: {stratagem.Number}");
                }
                usedNumbers.Add(stratagem.Number);

                if (usedIds.Contains(stratagem.StratagemId))
                {
                    errors.Add($"IDが重複しています: {stratagem.StratagemId}");
                }
                usedIds.Add(stratagem.StratagemId);

                if (string.IsNullOrEmpty(stratagem.NameJP))
                {
                    errors.Add($"第{stratagem.Number}計の日本語名がありません");
                }
            }

            return errors.Count == 0;
        }
    }
}
