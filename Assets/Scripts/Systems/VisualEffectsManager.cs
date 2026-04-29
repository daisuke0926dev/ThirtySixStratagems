using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ThirtySixStratagems.Core;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// ビジュアルエフェクト管理システム
    /// 画面効果、パーティクル、UIアニメーションを統括
    /// </summary>
    public class VisualEffectsManager : MonoBehaviour
    {
        public static VisualEffectsManager Instance { get; private set; }

        [Header("スクリーンエフェクト")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Image _screenFlashImage;
        [SerializeField] private Image _screenFadeImage;
        [SerializeField] private CanvasGroup _screenOverlay;

        [Header("シェイク設定")]
        [SerializeField] private float _defaultShakeDuration = 0.3f;
        [SerializeField] private float _defaultShakeMagnitude = 0.1f;
        [SerializeField] private float _shakeDecay = 1f;

        [Header("フラッシュ設定")]
        [SerializeField] private float _defaultFlashDuration = 0.2f;
        [SerializeField] private Color _defaultFlashColor = Color.white;

        [Header("フェード設定")]
        [SerializeField] private float _defaultFadeDuration = 1f;

        [Header("パーティクルプール")]
        [SerializeField] private int _poolSize = 20;
        [SerializeField] private ParticleSystem _hitEffectPrefab;
        [SerializeField] private ParticleSystem _stratagemEffectPrefab;
        [SerializeField] private ParticleSystem _victoryEffectPrefab;
        [SerializeField] private ParticleSystem _defeatEffectPrefab;

        // パーティクルプール
        private Dictionary<EffectType, Queue<ParticleSystem>> _particlePools = new Dictionary<EffectType, Queue<ParticleSystem>>();

        // アクティブなコルーチン
        private Coroutine _shakeCoroutine;
        private Coroutine _flashCoroutine;
        private Coroutine _fadeCoroutine;

        // 元のカメラ位置
        private Vector3 _originalCameraPosition;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
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
        /// 初期化
        /// </summary>
        private void Initialize()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            if (_mainCamera != null)
            {
                _originalCameraPosition = _mainCamera.transform.localPosition;
            }

            InitializeParticlePools();
            InitializeScreenEffects();
        }

        /// <summary>
        /// パーティクルプールを初期化
        /// </summary>
        private void InitializeParticlePools()
        {
            foreach (EffectType type in Enum.GetValues(typeof(EffectType)))
            {
                _particlePools[type] = new Queue<ParticleSystem>();
            }

            // プールを事前生成
            CreatePooledParticles(EffectType.Hit, _hitEffectPrefab);
            CreatePooledParticles(EffectType.Stratagem, _stratagemEffectPrefab);
            CreatePooledParticles(EffectType.Victory, _victoryEffectPrefab);
            CreatePooledParticles(EffectType.Defeat, _defeatEffectPrefab);
        }

        /// <summary>
        /// プール用パーティクルを作成
        /// </summary>
        private void CreatePooledParticles(EffectType type, ParticleSystem prefab)
        {
            if (prefab == null) return;

            for (int i = 0; i < _poolSize; i++)
            {
                var particle = Instantiate(prefab, transform);
                particle.gameObject.SetActive(false);
                _particlePools[type].Enqueue(particle);
            }
        }

        /// <summary>
        /// スクリーンエフェクトを初期化
        /// </summary>
        private void InitializeScreenEffects()
        {
            if (_screenFlashImage != null)
            {
                _screenFlashImage.color = new Color(1, 1, 1, 0);
                _screenFlashImage.gameObject.SetActive(false);
            }

            if (_screenFadeImage != null)
            {
                _screenFadeImage.color = new Color(0, 0, 0, 0);
                _screenFadeImage.gameObject.SetActive(false);
            }

            if (_screenOverlay != null)
            {
                _screenOverlay.alpha = 0;
            }
        }

        /// <summary>
        /// イベント購読
        /// </summary>
        private void SubscribeToEvents()
        {
            EventBus.OnBattleStarted += OnBattleStarted;
            EventBus.OnBattleEnded += OnBattleEnded;
            EventBus.OnStratagemExecuted += OnStratagemExecuted;
            EventBus.OnTerritoryConquered += OnTerritoryConquered;
        }

        /// <summary>
        /// イベント購読解除
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            EventBus.OnBattleStarted -= OnBattleStarted;
            EventBus.OnBattleEnded -= OnBattleEnded;
            EventBus.OnStratagemExecuted -= OnStratagemExecuted;
            EventBus.OnTerritoryConquered -= OnTerritoryConquered;
        }

        #endregion

        #region Screen Shake

        /// <summary>
        /// 画面を揺らす
        /// </summary>
        public void ShakeScreen(float duration = -1f, float magnitude = -1f)
        {
            if (duration < 0) duration = _defaultShakeDuration;
            if (magnitude < 0) magnitude = _defaultShakeMagnitude;

            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                ResetCameraPosition();
            }

            _shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
        }

        /// <summary>
        /// 画面シェイクコルーチン
        /// </summary>
        private IEnumerator ShakeCoroutine(float duration, float magnitude)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
                float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;

                if (_mainCamera != null)
                {
                    _mainCamera.transform.localPosition = _originalCameraPosition + new Vector3(x, y, 0);
                }

                elapsed += Time.deltaTime;
                magnitude = Mathf.Lerp(magnitude, 0, _shakeDecay * Time.deltaTime);

                yield return null;
            }

            ResetCameraPosition();
        }

        /// <summary>
        /// カメラ位置をリセット
        /// </summary>
        private void ResetCameraPosition()
        {
            if (_mainCamera != null)
            {
                _mainCamera.transform.localPosition = _originalCameraPosition;
            }
        }

        #endregion

        #region Screen Flash

        /// <summary>
        /// 画面フラッシュ
        /// </summary>
        public void FlashScreen(Color? color = null, float duration = -1f)
        {
            if (duration < 0) duration = _defaultFlashDuration;
            Color flashColor = color ?? _defaultFlashColor;

            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }

            _flashCoroutine = StartCoroutine(FlashCoroutine(flashColor, duration));
        }

        /// <summary>
        /// 画面フラッシュコルーチン
        /// </summary>
        private IEnumerator FlashCoroutine(Color color, float duration)
        {
            if (_screenFlashImage == null) yield break;

            _screenFlashImage.gameObject.SetActive(true);
            _screenFlashImage.color = color;

            float elapsed = 0f;
            float halfDuration = duration / 2f;

            // フェードイン
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0, 1, elapsed / halfDuration);
                _screenFlashImage.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }

            // フェードアウト
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1, 0, elapsed / halfDuration);
                _screenFlashImage.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }

            _screenFlashImage.color = new Color(color.r, color.g, color.b, 0);
            _screenFlashImage.gameObject.SetActive(false);
        }

        #endregion

        #region Screen Fade

        /// <summary>
        /// 画面フェードイン（黒から透明へ）
        /// </summary>
        public void FadeIn(float duration = -1f, Action onComplete = null)
        {
            if (duration < 0) duration = _defaultFadeDuration;

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeCoroutine(1f, 0f, duration, onComplete));
        }

        /// <summary>
        /// 画面フェードアウト（透明から黒へ）
        /// </summary>
        public void FadeOut(float duration = -1f, Action onComplete = null)
        {
            if (duration < 0) duration = _defaultFadeDuration;

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeCoroutine(0f, 1f, duration, onComplete));
        }

        /// <summary>
        /// フェード→コールバック→フェードイン
        /// </summary>
        public void FadeOutAndIn(float duration = -1f, Action onFadeComplete = null)
        {
            if (duration < 0) duration = _defaultFadeDuration;

            FadeOut(duration / 2f, () =>
            {
                onFadeComplete?.Invoke();
                FadeIn(duration / 2f);
            });
        }

        /// <summary>
        /// フェードコルーチン
        /// </summary>
        private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float duration, Action onComplete)
        {
            if (_screenFadeImage == null) yield break;

            _screenFadeImage.gameObject.SetActive(true);

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                _screenFadeImage.color = new Color(0, 0, 0, alpha);
                yield return null;
            }

            _screenFadeImage.color = new Color(0, 0, 0, endAlpha);

            if (endAlpha <= 0)
            {
                _screenFadeImage.gameObject.SetActive(false);
            }

            onComplete?.Invoke();
        }

        #endregion

        #region Particle Effects

        /// <summary>
        /// エフェクトを再生
        /// </summary>
        public void PlayEffect(EffectType type, Vector3 position)
        {
            var particle = GetPooledParticle(type);
            if (particle != null)
            {
                particle.transform.position = position;
                particle.gameObject.SetActive(true);
                particle.Play();

                StartCoroutine(ReturnToPoolAfterPlay(particle, type));
            }
        }

        /// <summary>
        /// ヒットエフェクトを再生
        /// </summary>
        public void PlayHitEffect(Vector3 position)
        {
            PlayEffect(EffectType.Hit, position);
        }

        /// <summary>
        /// 計略エフェクトを再生
        /// </summary>
        public void PlayStratagemEffect(Vector3 position)
        {
            PlayEffect(EffectType.Stratagem, position);
            FlashScreen(new Color(0.5f, 0f, 0.5f, 0.5f), 0.3f);
        }

        /// <summary>
        /// 勝利エフェクトを再生
        /// </summary>
        public void PlayVictoryEffect(Vector3 position)
        {
            PlayEffect(EffectType.Victory, position);
            FlashScreen(new Color(1f, 0.8f, 0f, 0.3f), 0.5f);
        }

        /// <summary>
        /// 敗北エフェクトを再生
        /// </summary>
        public void PlayDefeatEffect(Vector3 position)
        {
            PlayEffect(EffectType.Defeat, position);
            ShakeScreen(0.5f, 0.15f);
        }

        /// <summary>
        /// プールからパーティクルを取得
        /// </summary>
        private ParticleSystem GetPooledParticle(EffectType type)
        {
            if (_particlePools.TryGetValue(type, out var pool) && pool.Count > 0)
            {
                return pool.Dequeue();
            }

            // プールが空の場合は新規作成
            return CreateNewParticle(type);
        }

        /// <summary>
        /// 新規パーティクルを作成
        /// </summary>
        private ParticleSystem CreateNewParticle(EffectType type)
        {
            ParticleSystem prefab = type switch
            {
                EffectType.Hit => _hitEffectPrefab,
                EffectType.Stratagem => _stratagemEffectPrefab,
                EffectType.Victory => _victoryEffectPrefab,
                EffectType.Defeat => _defeatEffectPrefab,
                _ => null
            };

            if (prefab == null) return null;

            var particle = Instantiate(prefab, transform);
            return particle;
        }

        /// <summary>
        /// 再生後にプールに戻す
        /// </summary>
        private IEnumerator ReturnToPoolAfterPlay(ParticleSystem particle, EffectType type)
        {
            yield return new WaitWhile(() => particle.isPlaying);

            particle.gameObject.SetActive(false);
            _particlePools[type].Enqueue(particle);
        }

        #endregion

        #region UI Animations

        /// <summary>
        /// UIをスケールアニメーション
        /// </summary>
        public void AnimateScale(Transform target, Vector3 fromScale, Vector3 toScale, float duration, Action onComplete = null)
        {
            StartCoroutine(ScaleAnimationCoroutine(target, fromScale, toScale, duration, onComplete));
        }

        /// <summary>
        /// UIをポップイン
        /// </summary>
        public void PopIn(Transform target, float duration = 0.3f, Action onComplete = null)
        {
            target.localScale = Vector3.zero;
            AnimateScale(target, Vector3.zero, Vector3.one, duration, onComplete);
        }

        /// <summary>
        /// UIをポップアウト
        /// </summary>
        public void PopOut(Transform target, float duration = 0.2f, Action onComplete = null)
        {
            AnimateScale(target, Vector3.one, Vector3.zero, duration, onComplete);
        }

        /// <summary>
        /// スケールアニメーションコルーチン
        /// </summary>
        private IEnumerator ScaleAnimationCoroutine(Transform target, Vector3 fromScale, Vector3 toScale, float duration, Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutBack(elapsed / duration);
                target.localScale = Vector3.Lerp(fromScale, toScale, t);
                yield return null;
            }

            target.localScale = toScale;
            onComplete?.Invoke();
        }

        /// <summary>
        /// UIをスライドアニメーション
        /// </summary>
        public void SlideIn(RectTransform target, Vector2 fromPosition, Vector2 toPosition, float duration, Action onComplete = null)
        {
            StartCoroutine(SlideAnimationCoroutine(target, fromPosition, toPosition, duration, onComplete));
        }

        /// <summary>
        /// スライドアニメーションコルーチン
        /// </summary>
        private IEnumerator SlideAnimationCoroutine(RectTransform target, Vector2 fromPosition, Vector2 toPosition, float duration, Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutQuad(elapsed / duration);
                target.anchoredPosition = Vector2.Lerp(fromPosition, toPosition, t);
                yield return null;
            }

            target.anchoredPosition = toPosition;
            onComplete?.Invoke();
        }

        /// <summary>
        /// UIをフェード
        /// </summary>
        public void FadeUI(CanvasGroup target, float fromAlpha, float toAlpha, float duration, Action onComplete = null)
        {
            StartCoroutine(FadeUICoroutine(target, fromAlpha, toAlpha, duration, onComplete));
        }

        /// <summary>
        /// UIフェードコルーチン
        /// </summary>
        private IEnumerator FadeUICoroutine(CanvasGroup target, float fromAlpha, float toAlpha, float duration, Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                target.alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
                yield return null;
            }

            target.alpha = toAlpha;
            onComplete?.Invoke();
        }

        #endregion

        #region Easing Functions

        /// <summary>
        /// EaseOutBack
        /// </summary>
        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
        }

        /// <summary>
        /// EaseOutQuad
        /// </summary>
        private float EaseOutQuad(float t)
        {
            return 1 - (1 - t) * (1 - t);
        }

        /// <summary>
        /// EaseInOutQuad
        /// </summary>
        private float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
        }

        #endregion

        #region Event Handlers

        private void OnBattleStarted(BattleEventArgs args)
        {
            ShakeScreen(0.2f, 0.05f);
            FlashScreen(new Color(1f, 0f, 0f, 0.2f), 0.3f);
        }

        private void OnBattleEnded(BattleResultEventArgs args)
        {
            var playerFactionId = GetPlayerFactionId();
            bool isVictory = args.VictorFactionId == playerFactionId;

            if (isVictory)
            {
                PlayVictoryEffect(Vector3.zero);
            }
            else
            {
                PlayDefeatEffect(Vector3.zero);
            }
        }

        private void OnStratagemExecuted(StratagemEventArgs args)
        {
            if (args.Success)
            {
                PlayStratagemEffect(Vector3.zero);
            }
        }

        private void OnTerritoryConquered(TerritoryEventArgs args)
        {
            FlashScreen(new Color(0f, 1f, 0f, 0.3f), 0.4f);
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

    /// <summary>
    /// エフェクトタイプ
    /// </summary>
    public enum EffectType
    {
        Hit,
        Stratagem,
        Victory,
        Defeat,
        Explosion,
        Heal,
        Buff,
        Debuff
    }
}
