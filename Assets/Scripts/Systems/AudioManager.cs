using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Core;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// オーディオ管理システム
    /// BGM、SE、音声の再生を統括
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("オーディオソース")]
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioSource _seSource;
        [SerializeField] private AudioSource _voiceSource;
        [SerializeField] private int _sePoolSize = 10;

        [Header("音量設定")]
        [SerializeField, Range(0, 1)] private float _masterVolume = 1f;
        [SerializeField, Range(0, 1)] private float _bgmVolume = 0.8f;
        [SerializeField, Range(0, 1)] private float _seVolume = 1f;
        [SerializeField, Range(0, 1)] private float _voiceVolume = 1f;

        [Header("フェード設定")]
        [SerializeField] private float _bgmFadeDuration = 1f;

        [Header("BGMクリップ")]
        [SerializeField] private AudioClip _titleBGM;
        [SerializeField] private AudioClip _mapBGM;
        [SerializeField] private AudioClip _battleBGM;
        [SerializeField] private AudioClip _victoryBGM;
        [SerializeField] private AudioClip _defeatBGM;

        [Header("SEクリップ")]
        [SerializeField] private AudioClip _buttonClickSE;
        [SerializeField] private AudioClip _notificationSE;
        [SerializeField] private AudioClip _stratagemSuccessSE;
        [SerializeField] private AudioClip _stratagemFailSE;
        [SerializeField] private AudioClip _battleStartSE;
        [SerializeField] private AudioClip _battleWinSE;
        [SerializeField] private AudioClip _battleLoseSE;
        [SerializeField] private AudioClip _turnEndSE;

        // SEプール
        private List<AudioSource> _sePool = new List<AudioSource>();
        private int _currentSeIndex = 0;

        // 状態
        private Coroutine _bgmFadeCoroutine;
        private BGMType _currentBGMType = BGMType.None;

        // イベント
        public event Action<BGMType> OnBGMChanged;

        /// <summary>
        /// マスター音量
        /// </summary>
        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp01(value);
                UpdateVolumes();
                SaveVolumeSettings();
            }
        }

        /// <summary>
        /// BGM音量
        /// </summary>
        public float BGMVolume
        {
            get => _bgmVolume;
            set
            {
                _bgmVolume = Mathf.Clamp01(value);
                UpdateVolumes();
                SaveVolumeSettings();
            }
        }

        /// <summary>
        /// SE音量
        /// </summary>
        public float SEVolume
        {
            get => _seVolume;
            set
            {
                _seVolume = Mathf.Clamp01(value);
                UpdateVolumes();
                SaveVolumeSettings();
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSources();
                LoadVolumeSettings();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        #region Initialization

        /// <summary>
        /// オーディオソースを初期化
        /// </summary>
        private void InitializeAudioSources()
        {
            // BGMソース
            if (_bgmSource == null)
            {
                _bgmSource = gameObject.AddComponent<AudioSource>();
                _bgmSource.loop = true;
                _bgmSource.playOnAwake = false;
            }

            // メインSEソース
            if (_seSource == null)
            {
                _seSource = gameObject.AddComponent<AudioSource>();
                _seSource.playOnAwake = false;
            }

            // ボイスソース
            if (_voiceSource == null)
            {
                _voiceSource = gameObject.AddComponent<AudioSource>();
                _voiceSource.playOnAwake = false;
            }

            // SEプールを作成
            for (int i = 0; i < _sePoolSize; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                _sePool.Add(source);
            }

            UpdateVolumes();
        }

        /// <summary>
        /// イベント購読
        /// </summary>
        private void SubscribeToEvents()
        {
            EventBus.OnGameStarted += OnGameStarted;
            EventBus.OnBattleStarted += OnBattleStarted;
            EventBus.OnBattleEnded += OnBattleEnded;
            EventBus.OnStratagemExecuted += OnStratagemExecuted;
            EventBus.OnTurnEnded += OnTurnEnded;
        }

        /// <summary>
        /// イベント購読解除
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            EventBus.OnGameStarted -= OnGameStarted;
            EventBus.OnBattleStarted -= OnBattleStarted;
            EventBus.OnBattleEnded -= OnBattleEnded;
            EventBus.OnStratagemExecuted -= OnStratagemExecuted;
            EventBus.OnTurnEnded -= OnTurnEnded;
        }

        #endregion

        #region BGM

        /// <summary>
        /// BGMを再生
        /// </summary>
        public void PlayBGM(BGMType type, bool fade = true)
        {
            if (_currentBGMType == type && _bgmSource.isPlaying) return;

            AudioClip clip = GetBGMClip(type);
            if (clip == null) return;

            _currentBGMType = type;

            if (fade && _bgmSource.isPlaying)
            {
                CrossFadeBGM(clip);
            }
            else
            {
                _bgmSource.clip = clip;
                _bgmSource.Play();
            }

            OnBGMChanged?.Invoke(type);
        }

        /// <summary>
        /// BGMを停止
        /// </summary>
        public void StopBGM(bool fade = true)
        {
            if (fade)
            {
                FadeOutBGM();
            }
            else
            {
                _bgmSource.Stop();
            }

            _currentBGMType = BGMType.None;
        }

        /// <summary>
        /// BGMをクロスフェード
        /// </summary>
        private void CrossFadeBGM(AudioClip newClip)
        {
            if (_bgmFadeCoroutine != null)
            {
                StopCoroutine(_bgmFadeCoroutine);
            }
            _bgmFadeCoroutine = StartCoroutine(CrossFadeBGMCoroutine(newClip));
        }

        private IEnumerator CrossFadeBGMCoroutine(AudioClip newClip)
        {
            float startVolume = _bgmSource.volume;

            // フェードアウト
            float elapsed = 0f;
            while (elapsed < _bgmFadeDuration / 2)
            {
                elapsed += Time.deltaTime;
                _bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (_bgmFadeDuration / 2));
                yield return null;
            }

            // クリップ切り替え
            _bgmSource.Stop();
            _bgmSource.clip = newClip;
            _bgmSource.Play();

            // フェードイン
            elapsed = 0f;
            float targetVolume = _bgmVolume * _masterVolume;
            while (elapsed < _bgmFadeDuration / 2)
            {
                elapsed += Time.deltaTime;
                _bgmSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / (_bgmFadeDuration / 2));
                yield return null;
            }

            _bgmSource.volume = targetVolume;
        }

        /// <summary>
        /// BGMをフェードアウト
        /// </summary>
        private void FadeOutBGM()
        {
            if (_bgmFadeCoroutine != null)
            {
                StopCoroutine(_bgmFadeCoroutine);
            }
            _bgmFadeCoroutine = StartCoroutine(FadeOutBGMCoroutine());
        }

        private IEnumerator FadeOutBGMCoroutine()
        {
            float startVolume = _bgmSource.volume;
            float elapsed = 0f;

            while (elapsed < _bgmFadeDuration)
            {
                elapsed += Time.deltaTime;
                _bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / _bgmFadeDuration);
                yield return null;
            }

            _bgmSource.Stop();
            _bgmSource.volume = _bgmVolume * _masterVolume;
        }

        /// <summary>
        /// BGMクリップを取得
        /// </summary>
        private AudioClip GetBGMClip(BGMType type)
        {
            return type switch
            {
                BGMType.Title => _titleBGM,
                BGMType.Map => _mapBGM,
                BGMType.Battle => _battleBGM,
                BGMType.Victory => _victoryBGM,
                BGMType.Defeat => _defeatBGM,
                _ => null
            };
        }

        #endregion

        #region SE

        /// <summary>
        /// SEを再生
        /// </summary>
        public void PlaySE(SEType type)
        {
            AudioClip clip = GetSEClip(type);
            if (clip != null)
            {
                PlaySE(clip);
            }
        }

        /// <summary>
        /// SEを再生（クリップ指定）
        /// </summary>
        public void PlaySE(AudioClip clip)
        {
            if (clip == null) return;

            var source = GetAvailableSESource();
            source.clip = clip;
            source.volume = _seVolume * _masterVolume;
            source.Play();
        }

        /// <summary>
        /// SEを再生（位置指定）
        /// </summary>
        public void PlaySEAtPosition(AudioClip clip, Vector3 position)
        {
            if (clip == null) return;

            AudioSource.PlayClipAtPoint(clip, position, _seVolume * _masterVolume);
        }

        /// <summary>
        /// 利用可能なSEソースを取得
        /// </summary>
        private AudioSource GetAvailableSESource()
        {
            // 再生中でないソースを探す
            foreach (var source in _sePool)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            // 全て使用中なら最も古いものを使用
            var oldest = _sePool[_currentSeIndex];
            _currentSeIndex = (_currentSeIndex + 1) % _sePool.Count;
            return oldest;
        }

        /// <summary>
        /// SEクリップを取得
        /// </summary>
        private AudioClip GetSEClip(SEType type)
        {
            return type switch
            {
                SEType.ButtonClick => _buttonClickSE,
                SEType.Notification => _notificationSE,
                SEType.StratagemSuccess => _stratagemSuccessSE,
                SEType.StratagemFail => _stratagemFailSE,
                SEType.BattleStart => _battleStartSE,
                SEType.BattleWin => _battleWinSE,
                SEType.BattleLose => _battleLoseSE,
                SEType.TurnEnd => _turnEndSE,
                _ => null
            };
        }

        #endregion

        #region Voice

        /// <summary>
        /// ボイスを再生
        /// </summary>
        public void PlayVoice(AudioClip clip)
        {
            if (clip == null) return;

            _voiceSource.Stop();
            _voiceSource.clip = clip;
            _voiceSource.volume = _voiceVolume * _masterVolume;
            _voiceSource.Play();
        }

        /// <summary>
        /// ボイスを停止
        /// </summary>
        public void StopVoice()
        {
            _voiceSource.Stop();
        }

        #endregion

        #region Volume

        /// <summary>
        /// 音量を更新
        /// </summary>
        private void UpdateVolumes()
        {
            if (_bgmSource != null)
                _bgmSource.volume = _bgmVolume * _masterVolume;

            if (_voiceSource != null)
                _voiceSource.volume = _voiceVolume * _masterVolume;

            foreach (var source in _sePool)
            {
                if (source.isPlaying)
                    source.volume = _seVolume * _masterVolume;
            }
        }

        /// <summary>
        /// 音量設定を保存
        /// </summary>
        private void SaveVolumeSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", _masterVolume);
            PlayerPrefs.SetFloat("BGMVolume", _bgmVolume);
            PlayerPrefs.SetFloat("SEVolume", _seVolume);
            PlayerPrefs.SetFloat("VoiceVolume", _voiceVolume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 音量設定を読み込み
        /// </summary>
        private void LoadVolumeSettings()
        {
            _masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            _bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
            _seVolume = PlayerPrefs.GetFloat("SEVolume", 1f);
            _voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 1f);
            UpdateVolumes();
        }

        #endregion

        #region Event Handlers

        private void OnGameStarted()
        {
            PlayBGM(BGMType.Map);
        }

        private void OnBattleStarted(BattleEventArgs args)
        {
            PlaySE(SEType.BattleStart);
            PlayBGM(BGMType.Battle);
        }

        private void OnBattleEnded(BattleResultEventArgs args)
        {
            var playerFaction = GetPlayerFactionId();
            bool isVictory = args.VictorFactionId == playerFaction;

            PlaySE(isVictory ? SEType.BattleWin : SEType.BattleLose);
            PlayBGM(BGMType.Map);
        }

        private void OnStratagemExecuted(StratagemEventArgs args)
        {
            PlaySE(args.Success ? SEType.StratagemSuccess : SEType.StratagemFail);
        }

        private void OnTurnEnded(int turnNumber)
        {
            PlaySE(SEType.TurnEnd);
        }

        private string GetPlayerFactionId()
        {
            if (GameManager.Instance?.GameData == null) return null;

            foreach (var faction in GameManager.Instance.GameData.Factions.Values)
            {
                if (faction.IsPlayer) return faction.Id;
            }
            return null;
        }

        #endregion
    }

    #region Enums

    /// <summary>
    /// BGMタイプ
    /// </summary>
    public enum BGMType
    {
        None,
        Title,
        Map,
        Battle,
        Victory,
        Defeat
    }

    /// <summary>
    /// SEタイプ
    /// </summary>
    public enum SEType
    {
        // UI
        ButtonClick,
        ButtonHover,
        PanelOpen,
        PanelClose,
        Notification,
        Error,
        Confirm,
        Cancel,

        // ゲーム
        TurnStart,
        TurnEnd,
        PhaseChange,
        ResourceGain,
        ResourceLose,

        // 戦闘
        BattleStart,
        BattleWin,
        BattleLose,
        Attack,
        AttackHit,
        AttackCritical,
        Defend,
        Damage,
        Death,
        Retreat,
        Charge,
        Arrow,

        // 計略
        StratagemSelect,
        StratagemExecute,
        StratagemSuccess,
        StratagemFail,

        // マップ
        TerritorySelect,
        TerritoryCapture,
        ArmyMove,
        ArmyArrive
    }

    #endregion
}
