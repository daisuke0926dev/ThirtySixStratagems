using UnityEngine;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Data.ScriptableObjects
{
    /// <summary>
    /// キャラクターデータ（ScriptableObject）
    /// </summary>
    [CreateAssetMenu(fileName = "Character", menuName = "ThirtySixStratagems/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("基本情報")]
        public string CharacterId;
        public string CharacterName;
        public CharacterType Type = CharacterType.General;

        [Header("能力値")]
        [Range(1, 100)]
        public int Strength = 50;

        [Range(1, 100)]
        public int Intelligence = 50;

        [Range(1, 100)]
        public int Leadership = 50;

        [Range(1, 100)]
        public int Politics = 50;

        [Range(1, 100)]
        public int Charisma = 50;

        [Header("得意計略")]
        public StratagemCategory SpecialtyCategory = StratagemCategory.Winning;

        [Header("ビジュアル")]
        public Sprite Portrait;

        [Header("説明")]
        [TextArea(2, 4)]
        public string Biography;

        /// <summary>
        /// 能力値の合計
        /// </summary>
        public int TotalStats => Strength + Intelligence + Leadership + Politics + Charisma;

        /// <summary>
        /// Characterモデルを生成
        /// </summary>
        public Character CreateCharacter()
        {
            return new Character
            {
                Id = CharacterId,
                Name = CharacterName,
                Type = Type,
                Strength = Strength,
                Intelligence = Intelligence,
                Leadership = Leadership,
                Politics = Politics,
                Charisma = Charisma,
                SpecialtyCategory = SpecialtyCategory,
                Loyalty = 100,
                Health = 100,
                IsAlive = true,
                IsCaptured = false,
                PortraitId = CharacterId
            };
        }
    }
}
