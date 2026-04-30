using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// 環境音マネージャー
    /// バックグラウンドの環境音を管理
    /// </summary>
    public class AmbientSoundManager : MonoBehaviour
    {
        private static AmbientSoundManager _instance;
        public static AmbientSoundManager Instance => _instance;

        [Header("設定")]
        [SerializeField, Range(0f, 1f)] private float _ambientVolume = 0.5f;
        [SerializeField] private float _fadeDuration = 2f;
        [SerializeField] private int _maxConcurrentSounds = 3;

        [Header("環境音プリセット")]
        [SerializeField] private AmbientPreset[] _presets;

        // 内部状態
        private List<AudioSource> _activeSources = new List<AudioSource>();
        private Dictionary<string, AudioSource> _namedSources = new Dictionary<string, AudioSource>();
        private string _currentPresetId;

        /// <summary>
        /// 環境音量
        /// </summary>
        public float AmbientVolume
        {
            get => _ambientVolume;
            set
            {
                _ambientVolume = Mathf.Clamp01(value);
                UpdateAllVolumes();
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// プリセットを適用
        /// </summary>
        public void ApplyPreset(string presetId)
        {
            if (_currentPresetId == presetId) return;

            // 現在の環境音を停止
            StopAllAmbient();

            // プリセットを検索
            AmbientPreset preset = null;
            foreach (var p in _presets)
            {
                if (p.Id == presetId)
                {
                    preset = p;
                    break;
                }
            }

            if (preset == null)
            {
                Debug.LogWarning($"AmbientSoundManager: Preset '{presetId}' not found");
                return;
            }

            _currentPresetId = presetId;

            // プリセットの環境音を再生
            foreach (var sound in preset.Sounds)
            {
                PlayAmbient(sound.Clip, sound.Id, sound.Volume, sound.Loop);
            }
        }

        /// <summary>
        /// 環境音を再生
        /// </summary>
        public AudioSource PlayAmbient(AudioClip clip, string id = null, float volume = 1f, bool loop = true)
        {
            if (clip == null) return null;

            // 同じIDの音が既に再生中なら停止
            if (!string.IsNullOrEmpty(id) && _namedSources.ContainsKey(id))
            {
                StopAmbient(id);
            }

            // 最大同時再生数チェック
            if (_activeSources.Count >= _maxConcurrentSounds)
            {
                // 最も古いものを停止
                if (_activeSources.Count > 0)
                {
                    StopAmbientSource(_activeSources[0]);
                }
            }

            // 新しいソースを作成
            var source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = 0f;
            source.loop = loop;
            source.playOnAwake = false;
            source.Play();

            _activeSources.Add(source);

            if (!string.IsNullOrEmpty(id))
            {
                _namedSources[id] = source;
            }

            // フェードイン
            StartCoroutine(FadeIn(source, volume * _ambientVolume));

            return source;
        }

        /// <summary>
        /// 環境音を停止
        /// </summary>
        public void StopAmbient(string id)
        {
            if (_namedSources.TryGetValue(id, out var source))
            {
                StartCoroutine(FadeOutAndDestroy(source, id));
            }
        }

        /// <summary>
        /// すべての環境音を停止
        /// </summary>
        public void StopAllAmbient()
        {
            var sourcesToStop = new List<AudioSource>(_activeSources);
            foreach (var source in sourcesToStop)
            {
                StartCoroutine(FadeOutAndDestroySource(source));
            }

            _namedSources.Clear();
            _currentPresetId = null;
        }

        /// <summary>
        /// ソースを停止
        /// </summary>
        private void StopAmbientSource(AudioSource source)
        {
            if (source == null) return;

            _activeSources.Remove(source);

            // 名前付きソースから削除
            string keyToRemove = null;
            foreach (var kvp in _namedSources)
            {
                if (kvp.Value == source)
                {
                    keyToRemove = kvp.Key;
                    break;
                }
            }

            if (keyToRemove != null)
            {
                _namedSources.Remove(keyToRemove);
            }

            Destroy(source);
        }

        /// <summary>
        /// フェードイン
        /// </summary>
        private IEnumerator FadeIn(AudioSource source, float targetVolume)
        {
            float elapsed = 0f;

            while (elapsed < _fadeDuration && source != null)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(0f, targetVolume, elapsed / _fadeDuration);
                yield return null;
            }

            if (source != null)
            {
                source.volume = targetVolume;
            }
        }

        /// <summary>
        /// フェードアウトして削除
        /// </summary>
        private IEnumerator FadeOutAndDestroy(AudioSource source, string id)
        {
            yield return StartCoroutine(FadeOut(source));

            _activeSources.Remove(source);
            _namedSources.Remove(id);
            Destroy(source);
        }

        /// <summary>
        /// フェードアウトして削除（ID無し）
        /// </summary>
        private IEnumerator FadeOutAndDestroySource(AudioSource source)
        {
            yield return StartCoroutine(FadeOut(source));

            _activeSources.Remove(source);
            Destroy(source);
        }

        /// <summary>
        /// フェードアウト
        /// </summary>
        private IEnumerator FadeOut(AudioSource source)
        {
            if (source == null) yield break;

            float startVolume = source.volume;
            float elapsed = 0f;

            while (elapsed < _fadeDuration && source != null)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, elapsed / _fadeDuration);
                yield return null;
            }

            if (source != null)
            {
                source.Stop();
            }
        }

        /// <summary>
        /// 全ての音量を更新
        /// </summary>
        private void UpdateAllVolumes()
        {
            foreach (var source in _activeSources)
            {
                if (source != null)
                {
                    source.volume = _ambientVolume;
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var source in _activeSources)
            {
                if (source != null)
                {
                    Destroy(source);
                }
            }
            _activeSources.Clear();
            _namedSources.Clear();
        }
    }

    /// <summary>
    /// 環境音プリセット
    /// </summary>
    [System.Serializable]
    public class AmbientPreset
    {
        public string Id;
        public string DisplayName;
        public AmbientSound[] Sounds;
    }

    /// <summary>
    /// 環境音エントリ
    /// </summary>
    [System.Serializable]
    public class AmbientSound
    {
        public string Id;
        public AudioClip Clip;

        [Range(0f, 1f)]
        public float Volume = 0.5f;

        public bool Loop = true;
    }
}
