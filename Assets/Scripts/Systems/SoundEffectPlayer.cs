using UnityEngine;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// サウンドエフェクトプレイヤー
    /// ゲームオブジェクトにアタッチして簡単にSEを再生
    /// </summary>
    public class SoundEffectPlayer : MonoBehaviour
    {
        [Header("再生設定")]
        [SerializeField] private SEType _soundType = SEType.ButtonClick;
        [SerializeField] private AudioClip _customClip;
        [SerializeField] private bool _useCustomClip = false;

        [Header("音量設定")]
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
        [SerializeField, Range(0.5f, 2f)] private float _pitchMin = 1f;
        [SerializeField, Range(0.5f, 2f)] private float _pitchMax = 1f;

        [Header("3Dサウンド")]
        [SerializeField] private bool _is3DSound = false;
        [SerializeField] private float _maxDistance = 20f;

        [Header("再生タイミング")]
        [SerializeField] private bool _playOnEnable = false;
        [SerializeField] private bool _playOnDisable = false;
        [SerializeField] private bool _playOnDestroy = false;

        [Header("ループ設定")]
        [SerializeField] private bool _loop = false;
        [SerializeField] private float _loopInterval = 0f;

        private AudioSource _audioSource;
        private float _nextPlayTime;

        private void Awake()
        {
            if (_is3DSound || _loop)
            {
                _audioSource = GetComponent<AudioSource>();
                if (_audioSource == null)
                {
                    _audioSource = gameObject.AddComponent<AudioSource>();
                }

                _audioSource.playOnAwake = false;
                _audioSource.loop = _loop && _loopInterval <= 0;
                _audioSource.spatialBlend = _is3DSound ? 1f : 0f;
                _audioSource.maxDistance = _maxDistance;
            }
        }

        private void OnEnable()
        {
            if (_playOnEnable)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            if (_playOnDisable)
            {
                Play();
            }
        }

        private void OnDestroy()
        {
            if (_playOnDestroy)
            {
                PlayOneShot();
            }
        }

        private void Update()
        {
            if (_loop && _loopInterval > 0 && Time.time >= _nextPlayTime)
            {
                Play();
                _nextPlayTime = Time.time + _loopInterval;
            }
        }

        /// <summary>
        /// サウンドを再生
        /// </summary>
        public void Play()
        {
            if (_useCustomClip && _customClip != null)
            {
                PlayClip(_customClip);
            }
            else if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySE(_soundType);
            }
        }

        /// <summary>
        /// ワンショット再生（複数同時再生可能）
        /// </summary>
        public void PlayOneShot()
        {
            if (_useCustomClip && _customClip != null)
            {
                if (_is3DSound)
                {
                    AudioSource.PlayClipAtPoint(_customClip, transform.position, _volume);
                }
                else if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySE(_customClip);
                }
            }
            else if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySE(_soundType);
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            if (_audioSource != null)
            {
                _audioSource.Stop();
            }
        }

        /// <summary>
        /// クリップを再生
        /// </summary>
        private void PlayClip(AudioClip clip)
        {
            if (clip == null) return;

            float pitch = Random.Range(_pitchMin, _pitchMax);

            if (_audioSource != null)
            {
                _audioSource.clip = clip;
                _audioSource.volume = _volume;
                _audioSource.pitch = pitch;
                _audioSource.Play();
            }
            else if (_is3DSound)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position, _volume);
            }
            else if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySE(clip);
            }
        }

        /// <summary>
        /// サウンドタイプを設定
        /// </summary>
        public void SetSoundType(SEType type)
        {
            _soundType = type;
            _useCustomClip = false;
        }

        /// <summary>
        /// カスタムクリップを設定
        /// </summary>
        public void SetCustomClip(AudioClip clip)
        {
            _customClip = clip;
            _useCustomClip = true;
        }
    }
}
