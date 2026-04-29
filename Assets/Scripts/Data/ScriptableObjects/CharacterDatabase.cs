using System.Collections.Generic;
using UnityEngine;

namespace ThirtySixStratagems.Data.ScriptableObjects
{
    /// <summary>
    /// 武将データベース
    /// 全武将データを管理するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterDatabase", menuName = "ThirtySixStratagems/Character Database")]
    public class CharacterDatabase : ScriptableObject
    {
        [Header("全武将")]
        [SerializeField] private List<CharacterData> _allCharacters = new List<CharacterData>();

        private Dictionary<string, CharacterData> _characterById;

        /// <summary>
        /// 全武将リスト
        /// </summary>
        public IReadOnlyList<CharacterData> AllCharacters => _allCharacters;

        /// <summary>
        /// 武将数
        /// </summary>
        public int Count => _allCharacters.Count;

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            _characterById = new Dictionary<string, CharacterData>();

            foreach (var character in _allCharacters)
            {
                if (character != null && !string.IsNullOrEmpty(character.CharacterId))
                {
                    _characterById[character.CharacterId] = character;
                }
            }
        }

        /// <summary>
        /// IDで武将を取得
        /// </summary>
        public CharacterData GetById(string characterId)
        {
            if (_characterById == null) Initialize();

            if (string.IsNullOrEmpty(characterId)) return null;

            return _characterById.TryGetValue(characterId, out var character) ? character : null;
        }

        /// <summary>
        /// 名前で武将を取得
        /// </summary>
        public CharacterData GetByName(string characterName)
        {
            if (string.IsNullOrEmpty(characterName)) return null;

            foreach (var character in _allCharacters)
            {
                if (character != null && character.CharacterName == characterName)
                {
                    return character;
                }
            }
            return null;
        }

        /// <summary>
        /// タイプで武将を取得
        /// </summary>
        public List<CharacterData> GetByType(Models.CharacterType type)
        {
            var result = new List<CharacterData>();
            foreach (var character in _allCharacters)
            {
                if (character != null && character.Type == type)
                {
                    result.Add(character);
                }
            }
            return result;
        }

        /// <summary>
        /// 得意カテゴリで武将を取得
        /// </summary>
        public List<CharacterData> GetBySpecialty(Models.StratagemCategory specialty)
        {
            var result = new List<CharacterData>();
            foreach (var character in _allCharacters)
            {
                if (character != null && character.SpecialtyCategory == specialty)
                {
                    result.Add(character);
                }
            }
            return result;
        }

        /// <summary>
        /// 武将を追加
        /// </summary>
        public void AddCharacter(CharacterData character)
        {
            if (character != null && !_allCharacters.Contains(character))
            {
                _allCharacters.Add(character);
                if (_characterById != null)
                {
                    _characterById[character.CharacterId] = character;
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

            foreach (var character in _allCharacters)
            {
                if (character == null)
                {
                    errors.Add("null の武将データがあります");
                    continue;
                }

                if (string.IsNullOrEmpty(character.CharacterId))
                {
                    errors.Add($"武将 {character.CharacterName} のIDが空です");
                }
                else if (usedIds.Contains(character.CharacterId))
                {
                    errors.Add($"IDが重複しています: {character.CharacterId}");
                }
                else
                {
                    usedIds.Add(character.CharacterId);
                }

                if (string.IsNullOrEmpty(character.CharacterName))
                {
                    errors.Add($"武将 {character.CharacterId} の名前が空です");
                }
            }

            return errors.Count == 0;
        }
    }
}
