using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// 音楽プレイリスト
    /// BGMのプレイリスト管理とシャッフル再生
    /// </summary>
    public class MusicPlaylist : MonoBehaviour
    {
        [Header("プレイリスト")]
        [SerializeField] private List<PlaylistEntry> _tracks = new List<PlaylistEntry>();

        [Header("設定")]
        [SerializeField] private bool _shuffleOnStart = true;
        [SerializeField] private bool _autoPlay = true;
        [SerializeField] private float _trackGap = 1f; // トラック間の間隔

        [Header("フェード設定")]
        [SerializeField] private float _fadeInDuration = 2f;
        [SerializeField] private float _fadeOutDuration = 2f;

        // 状態
        private AudioSource _audioSource;
        private List<int> _shuffledOrder;
        private int _currentIndex = -1;
        private bool _isPlaying;
        private Coroutine _playbackCoroutine;

        // イベント
        public event Action<PlaylistEntry> OnTrackChanged;
        public event Action OnPlaylistEnded;

        /// <summary>
        /// 現在のトラック
        /// </summary>
        public PlaylistEntry CurrentTrack
        {
            get
            {
                if (_currentIndex >= 0 && _currentIndex < _tracks.Count)
                {
                    int actualIndex = _shuffledOrder != null ? _shuffledOrder[_currentIndex] : _currentIndex;
                    return _tracks[actualIndex];
                }
                return null;
            }
        }

        /// <summary>
        /// 再生中か
        /// </summary>
        public bool IsPlaying => _isPlaying;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
        }

        private void Start()
        {
            if (_shuffleOnStart)
            {
                Shuffle();
            }
            else
            {
                CreateSequentialOrder();
            }

            if (_autoPlay && _tracks.Count > 0)
            {
                Play();
            }
        }

        /// <summary>
        /// 再生開始
        /// </summary>
        public void Play()
        {
            if (_tracks.Count == 0) return;

            if (_currentIndex < 0)
            {
                _currentIndex = 0;
            }

            _isPlaying = true;

            if (_playbackCoroutine != null)
            {
                StopCoroutine(_playbackCoroutine);
            }
            _playbackCoroutine = StartCoroutine(PlaybackLoop());
        }

        /// <summary>
        /// 一時停止
        /// </summary>
        public void Pause()
        {
            _isPlaying = false;
            _audioSource.Pause();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            _isPlaying = false;

            if (_playbackCoroutine != null)
            {
                StopCoroutine(_playbackCoroutine);
                _playbackCoroutine = null;
            }

            StartCoroutine(FadeOut());
        }

        /// <summary>
        /// 次のトラック
        /// </summary>
        public void Next()
        {
            _currentIndex++;
            if (_currentIndex >= _tracks.Count)
            {
                _currentIndex = 0;
            }

            if (_isPlaying)
            {
                StartCoroutine(TransitionToCurrentTrack());
            }
        }

        /// <summary>
        /// 前のトラック
        /// </summary>
        public void Previous()
        {
            _currentIndex--;
            if (_currentIndex < 0)
            {
                _currentIndex = _tracks.Count - 1;
            }

            if (_isPlaying)
            {
                StartCoroutine(TransitionToCurrentTrack());
            }
        }

        /// <summary>
        /// シャッフル
        /// </summary>
        public void Shuffle()
        {
            _shuffledOrder = new List<int>();
            for (int i = 0; i < _tracks.Count; i++)
            {
                _shuffledOrder.Add(i);
            }

            // Fisher-Yatesシャッフル
            for (int i = _shuffledOrder.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                int temp = _shuffledOrder[i];
                _shuffledOrder[i] = _shuffledOrder[j];
                _shuffledOrder[j] = temp;
            }

            _currentIndex = 0;
        }

        /// <summary>
        /// 順番通りの再生順序を作成
        /// </summary>
        private void CreateSequentialOrder()
        {
            _shuffledOrder = new List<int>();
            for (int i = 0; i < _tracks.Count; i++)
            {
                _shuffledOrder.Add(i);
            }
        }

        /// <summary>
        /// 再生ループ
        /// </summary>
        private IEnumerator PlaybackLoop()
        {
            while (_isPlaying)
            {
                yield return StartCoroutine(PlayCurrentTrack());

                if (!_isPlaying) break;

                // トラック間の間隔
                yield return new WaitForSeconds(_trackGap);

                // 次のトラックへ
                _currentIndex++;
                if (_currentIndex >= _tracks.Count)
                {
                    _currentIndex = 0;
                    OnPlaylistEnded?.Invoke();

                    // シャッフルが有効なら再シャッフル
                    if (_shuffleOnStart)
                    {
                        Shuffle();
                    }
                }
            }
        }

        /// <summary>
        /// 現在のトラックを再生
        /// </summary>
        private IEnumerator PlayCurrentTrack()
        {
            var track = CurrentTrack;
            if (track == null || track.Clip == null) yield break;

            _audioSource.clip = track.Clip;
            OnTrackChanged?.Invoke(track);

            // フェードイン
            _audioSource.volume = 0f;
            _audioSource.Play();

            float elapsed = 0f;
            while (elapsed < _fadeInDuration)
            {
                elapsed += Time.deltaTime;
                _audioSource.volume = Mathf.Lerp(0f, track.Volume, elapsed / _fadeInDuration);
                yield return null;
            }
            _audioSource.volume = track.Volume;

            // 再生完了を待機（フェードアウト時間を考慮）
            float remainingTime = track.Clip.length - _audioSource.time - _fadeOutDuration;
            if (remainingTime > 0)
            {
                yield return new WaitForSeconds(remainingTime);
            }

            // フェードアウト
            yield return StartCoroutine(FadeOut());
        }

        /// <summary>
        /// 現在のトラックへ遷移
        /// </summary>
        private IEnumerator TransitionToCurrentTrack()
        {
            yield return StartCoroutine(FadeOut());
            yield return StartCoroutine(PlayCurrentTrack());
        }

        /// <summary>
        /// フェードアウト
        /// </summary>
        private IEnumerator FadeOut()
        {
            float startVolume = _audioSource.volume;
            float elapsed = 0f;

            while (elapsed < _fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                _audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / _fadeOutDuration);
                yield return null;
            }

            _audioSource.Stop();
            _audioSource.volume = 0f;
        }

        /// <summary>
        /// トラックを追加
        /// </summary>
        public void AddTrack(AudioClip clip, string title = null, float volume = 1f)
        {
            _tracks.Add(new PlaylistEntry
            {
                Clip = clip,
                Title = title ?? clip.name,
                Volume = volume
            });

            // 順序を更新
            if (_shuffledOrder != null)
            {
                _shuffledOrder.Add(_tracks.Count - 1);
            }
        }

        /// <summary>
        /// トラックをクリア
        /// </summary>
        public void ClearTracks()
        {
            Stop();
            _tracks.Clear();
            _shuffledOrder?.Clear();
            _currentIndex = -1;
        }
    }

    /// <summary>
    /// プレイリストエントリ
    /// </summary>
    [Serializable]
    public class PlaylistEntry
    {
        public string Title;
        public AudioClip Clip;

        [Range(0f, 1f)]
        public float Volume = 1f;

        public string Artist;
    }
}
