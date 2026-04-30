using System;
using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Systems;

namespace ThirtySixStratagems.Data.ScriptableObjects
{
    /// <summary>
    /// オーディオクリップデータベース
    /// BGMとSEのクリップを一元管理
    /// </summary>
    [CreateAssetMenu(fileName = "AudioClipDatabase", menuName = "ThirtySixStratagems/Audio/Clip Database")]
    public class AudioClipDatabase : ScriptableObject
    {
        [Header("BGM")]
        [SerializeField] private List<BGMEntry> _bgmEntries = new List<BGMEntry>();

        [Header("SE")]
        [SerializeField] private List<SEEntry> _seEntries = new List<SEEntry>();

        [Header("環境音")]
        [SerializeField] private List<AmbientEntry> _ambientEntries = new List<AmbientEntry>();

        // キャッシュ
        private Dictionary<BGMType, AudioClip> _bgmCache;
        private Dictionary<SEType, AudioClip[]> _seCache;
        private Dictionary<string, AudioClip> _ambientCache;

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            BuildCache();
        }

        private void OnEnable()
        {
            BuildCache();
        }

        private void BuildCache()
        {
            // BGMキャッシュ
            _bgmCache = new Dictionary<BGMType, AudioClip>();
            foreach (var entry in _bgmEntries)
            {
                if (!_bgmCache.ContainsKey(entry.Type) && entry.Clip != null)
                {
                    _bgmCache[entry.Type] = entry.Clip;
                }
            }

            // SEキャッシュ（複数バリエーション対応）
            _seCache = new Dictionary<SEType, AudioClip[]>();
            var seGroups = new Dictionary<SEType, List<AudioClip>>();

            foreach (var entry in _seEntries)
            {
                if (entry.Clip == null) continue;

                if (!seGroups.ContainsKey(entry.Type))
                {
                    seGroups[entry.Type] = new List<AudioClip>();
                }
                seGroups[entry.Type].Add(entry.Clip);
            }

            foreach (var kvp in seGroups)
            {
                _seCache[kvp.Key] = kvp.Value.ToArray();
            }

            // 環境音キャッシュ
            _ambientCache = new Dictionary<string, AudioClip>();
            foreach (var entry in _ambientEntries)
            {
                if (!string.IsNullOrEmpty(entry.Id) && entry.Clip != null)
                {
                    _ambientCache[entry.Id] = entry.Clip;
                }
            }
        }

        /// <summary>
        /// BGMクリップを取得
        /// </summary>
        public AudioClip GetBGM(BGMType type)
        {
            if (_bgmCache == null) BuildCache();
            return _bgmCache.TryGetValue(type, out var clip) ? clip : null;
        }

        /// <summary>
        /// SEクリップを取得（ランダムバリエーション）
        /// </summary>
        public AudioClip GetSE(SEType type)
        {
            if (_seCache == null) BuildCache();

            if (_seCache.TryGetValue(type, out var clips) && clips.Length > 0)
            {
                return clips[UnityEngine.Random.Range(0, clips.Length)];
            }
            return null;
        }

        /// <summary>
        /// SEクリップをすべて取得
        /// </summary>
        public AudioClip[] GetAllSE(SEType type)
        {
            if (_seCache == null) BuildCache();
            return _seCache.TryGetValue(type, out var clips) ? clips : Array.Empty<AudioClip>();
        }

        /// <summary>
        /// 環境音クリップを取得
        /// </summary>
        public AudioClip GetAmbient(string id)
        {
            if (_ambientCache == null) BuildCache();
            return _ambientCache.TryGetValue(id, out var clip) ? clip : null;
        }

        /// <summary>
        /// BGMが存在するか確認
        /// </summary>
        public bool HasBGM(BGMType type)
        {
            if (_bgmCache == null) BuildCache();
            return _bgmCache.ContainsKey(type);
        }

        /// <summary>
        /// SEが存在するか確認
        /// </summary>
        public bool HasSE(SEType type)
        {
            if (_seCache == null) BuildCache();
            return _seCache.ContainsKey(type);
        }
    }

    /// <summary>
    /// BGMエントリ
    /// </summary>
    [Serializable]
    public class BGMEntry
    {
        public BGMType Type;
        public AudioClip Clip;

        [Range(0f, 1f)]
        public float Volume = 1f;

        public bool Loop = true;
    }

    /// <summary>
    /// SEエントリ
    /// </summary>
    [Serializable]
    public class SEEntry
    {
        public SEType Type;
        public AudioClip Clip;

        [Range(0f, 1f)]
        public float Volume = 1f;

        [Range(0.5f, 2f)]
        public float PitchMin = 1f;

        [Range(0.5f, 2f)]
        public float PitchMax = 1f;
    }

    /// <summary>
    /// 環境音エントリ
    /// </summary>
    [Serializable]
    public class AmbientEntry
    {
        public string Id;
        public string DisplayName;
        public AudioClip Clip;

        [Range(0f, 1f)]
        public float Volume = 0.5f;

        public bool Loop = true;
    }
}
