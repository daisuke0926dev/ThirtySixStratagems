using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Data.ScriptableObjects
{
    /// <summary>
    /// 計略データ（ScriptableObject）
    /// 三十六計の各計略を定義
    /// </summary>
    [CreateAssetMenu(fileName = "Stratagem", menuName = "ThirtySixStratagems/Stratagem Data")]
    public class StratagemData : ScriptableObject
    {
        [Header("基本情報")]
        public string StratagemId;

        [Tooltip("三十六計の番号 (1-36)")]
        [Range(1, 36)]
        public int Number = 1;

        [Header("名称")]
        public string NameJP;           // 日本語名（漢字）
        public string NameCN;           // 中国語名
        public string Reading;          // 読み方（ひらがな）

        [Header("カテゴリ")]
        public StratagemCategory Category;

        [Header("説明")]
        [TextArea(2, 4)]
        public string OriginalText;     // 原典の文章

        [TextArea(2, 4)]
        public string ModernTranslation; // 現代語訳

        [TextArea(2, 4)]
        public string HistoricalExample; // 歴史的使用例

        [TextArea(2, 4)]
        public string GameEffectDescription; // ゲーム内効果説明

        [Header("コスト")]
        [Range(1, 10)]
        public int CostSP = 2;          // 消費計略ポイント

        [Min(0)]
        public int CostGold = 0;        // 消費金

        [Header("対象")]
        public StratagemTarget TargetType = StratagemTarget.EnemyFaction;

        [Header("成功率")]
        [Range(1, 100)]
        public int BaseSuccessRate = 70; // 基本成功率 (%)

        [Header("効果")]
        public StratagemEffectType PrimaryEffect;
        public int EffectValue = 10;
        public int Duration = 1;        // 効果持続ターン

        [Header("発動条件")]
        public List<StratagemCondition> Conditions = new List<StratagemCondition>();

        [Header("ビジュアル")]
        public Sprite Icon;
        public Color EffectColor = Color.white;

        [Header("サウンド")]
        public AudioClip ActivationSound;

        /// <summary>
        /// カテゴリ名を取得
        /// </summary>
        public string GetCategoryName()
        {
            switch (Category)
            {
                case StratagemCategory.Winning: return "勝戦計";
                case StratagemCategory.Enemy: return "敵戦計";
                case StratagemCategory.Attack: return "攻戦計";
                case StratagemCategory.Chaos: return "混戦計";
                case StratagemCategory.Merge: return "併戦計";
                case StratagemCategory.Defeat: return "敗戦計";
                default: return "";
            }
        }

        /// <summary>
        /// カテゴリ番号を取得（第X套）
        /// </summary>
        public int GetCategoryNumber()
        {
            return (int)Category + 1;
        }

        /// <summary>
        /// フル名称を取得（例: "第一計 瞞天過海"）
        /// </summary>
        public string GetFullName()
        {
            return $"第{NumberToKanji(Number)}計 {NameJP}";
        }

        /// <summary>
        /// 数字を漢数字に変換
        /// </summary>
        private string NumberToKanji(int num)
        {
            string[] kanji = { "", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
            string[] tens = { "", "十", "二十", "三十" };

            if (num <= 0 || num > 36) return num.ToString();

            int ten = num / 10;
            int one = num % 10;

            return tens[ten] + kanji[one];
        }

        /// <summary>
        /// 条件を満たしているか確認
        /// </summary>
        public bool CheckConditions(ConditionContext context)
        {
            foreach (var condition in Conditions)
            {
                if (!condition.IsMet(context))
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// 計略発動条件
    /// </summary>
    [System.Serializable]
    public class StratagemCondition
    {
        public ConditionType Type;
        public ComparisonOperator Operator = ComparisonOperator.GreaterThanOrEqual;
        public int Value;

        [TextArea(1, 2)]
        public string Description;

        /// <summary>
        /// 条件を満たしているか確認
        /// </summary>
        public bool IsMet(ConditionContext context)
        {
            int compareValue = GetCompareValue(context);
            return Compare(compareValue, Value, Operator);
        }

        private int GetCompareValue(ConditionContext context)
        {
            switch (Type)
            {
                case ConditionType.MinSoldiers:
                    return context.ArmySoldiers;
                case ConditionType.MaxSoldiers:
                    return context.ArmySoldiers;
                case ConditionType.MinGold:
                    return context.FactionGold;
                case ConditionType.MinStratagemPoints:
                    return context.StratagemPoints;
                case ConditionType.EnemyInWar:
                    return context.IsAtWar ? 1 : 0;
                case ConditionType.HasAlliance:
                    return context.HasAlliance ? 1 : 0;
                case ConditionType.TerritoryCount:
                    return context.TerritoryCount;
                case ConditionType.CharacterIntelligence:
                    return context.CasterIntelligence;
                default:
                    return 0;
            }
        }

        private bool Compare(int a, int b, ComparisonOperator op)
        {
            switch (op)
            {
                case ComparisonOperator.Equal:
                    return a == b;
                case ComparisonOperator.NotEqual:
                    return a != b;
                case ComparisonOperator.GreaterThan:
                    return a > b;
                case ComparisonOperator.GreaterThanOrEqual:
                    return a >= b;
                case ComparisonOperator.LessThan:
                    return a < b;
                case ComparisonOperator.LessThanOrEqual:
                    return a <= b;
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// 条件タイプ
    /// </summary>
    public enum ConditionType
    {
        MinSoldiers,            // 最小兵力
        MaxSoldiers,            // 最大兵力
        MinGold,                // 最小金
        MinStratagemPoints,     // 最小計略ポイント
        EnemyInWar,             // 敵と戦争中
        HasAlliance,            // 同盟を持っている
        TerritoryCount,         // 領地数
        CharacterIntelligence   // 使用者の知力
    }

    /// <summary>
    /// 比較演算子
    /// </summary>
    public enum ComparisonOperator
    {
        Equal,              // ==
        NotEqual,           // !=
        GreaterThan,        // >
        GreaterThanOrEqual, // >=
        LessThan,           // <
        LessThanOrEqual     // <=
    }

    /// <summary>
    /// 条件判定用コンテキスト
    /// </summary>
    public class ConditionContext
    {
        public int ArmySoldiers;
        public int FactionGold;
        public int StratagemPoints;
        public bool IsAtWar;
        public bool HasAlliance;
        public int TerritoryCount;
        public int CasterIntelligence;
    }
}
