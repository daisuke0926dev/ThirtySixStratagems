using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThirtySixStratagems.Core;

namespace ThirtySixStratagems.UI.Notification
{
    /// <summary>
    /// 通知システム
    /// ゲーム内イベントの通知を管理
    /// </summary>
    public class NotificationSystem : MonoBehaviour
    {
        public static NotificationSystem Instance { get; private set; }

        [Header("通知設定")]
        [SerializeField] private Transform _notificationContainer;
        [SerializeField] private GameObject _notificationPrefab;
        [SerializeField] private int _maxVisibleNotifications = 5;
        [SerializeField] private float _defaultDuration = 3f;
        [SerializeField] private float _fadeOutDuration = 0.5f;

        [Header("通知タイプ別アイコン")]
        [SerializeField] private Sprite _infoIcon;
        [SerializeField] private Sprite _warningIcon;
        [SerializeField] private Sprite _errorIcon;
        [SerializeField] private Sprite _successIcon;
        [SerializeField] private Sprite _battleIcon;
        [SerializeField] private Sprite _diplomacyIcon;
        [SerializeField] private Sprite _stratagemIcon;

        [Header("通知タイプ別カラー")]
        [SerializeField] private Color _infoColor = new Color(0.3f, 0.5f, 0.8f);
        [SerializeField] private Color _warningColor = new Color(0.9f, 0.7f, 0.2f);
        [SerializeField] private Color _errorColor = new Color(0.8f, 0.3f, 0.3f);
        [SerializeField] private Color _successColor = new Color(0.3f, 0.8f, 0.4f);
        [SerializeField] private Color _battleColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color _diplomacyColor = new Color(0.5f, 0.3f, 0.8f);
        [SerializeField] private Color _stratagemColor = new Color(0.2f, 0.6f, 0.8f);

        [Header("サウンド")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _notificationSound;
        [SerializeField] private AudioClip _warningSound;
        [SerializeField] private AudioClip _errorSound;

        // 通知キュー
        private Queue<NotificationData> _pendingNotifications = new Queue<NotificationData>();
        private List<NotificationItem> _activeNotifications = new List<NotificationItem>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
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

        #region Event Subscription

        /// <summary>
        /// イベント購読
        /// </summary>
        private void SubscribeToEvents()
        {
            EventBus.OnBattleStarted += OnBattleStarted;
            EventBus.OnBattleEnded += OnBattleEnded;
            EventBus.OnTerritoryConquered += OnTerritoryConquered;
            EventBus.OnStratagemExecuted += OnStratagemExecuted;
            EventBus.OnFactionTurnStarted += OnFactionTurnStarted;
            EventBus.OnTurnEnded += OnTurnEnded;
        }

        /// <summary>
        /// イベント購読解除
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            EventBus.OnBattleStarted -= OnBattleStarted;
            EventBus.OnBattleEnded -= OnBattleEnded;
            EventBus.OnTerritoryConquered -= OnTerritoryConquered;
            EventBus.OnStratagemExecuted -= OnStratagemExecuted;
            EventBus.OnFactionTurnStarted -= OnFactionTurnStarted;
            EventBus.OnTurnEnded -= OnTurnEnded;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 通知を表示
        /// </summary>
        public void ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 0)
        {
            if (duration <= 0)
                duration = _defaultDuration;

            var data = new NotificationData
            {
                Message = message,
                Type = type,
                Duration = duration,
                Timestamp = Time.time
            };

            _pendingNotifications.Enqueue(data);
            ProcessNotificationQueue();
        }

        /// <summary>
        /// 通知を表示（詳細版）
        /// </summary>
        public void ShowNotification(string title, string message, NotificationType type, Action onClick = null)
        {
            var data = new NotificationData
            {
                Title = title,
                Message = message,
                Type = type,
                Duration = _defaultDuration,
                Timestamp = Time.time,
                OnClick = onClick
            };

            _pendingNotifications.Enqueue(data);
            ProcessNotificationQueue();
        }

        /// <summary>
        /// 全ての通知をクリア
        /// </summary>
        public void ClearAllNotifications()
        {
            _pendingNotifications.Clear();

            foreach (var notification in _activeNotifications)
            {
                if (notification != null)
                {
                    Destroy(notification.gameObject);
                }
            }
            _activeNotifications.Clear();
        }

        #endregion

        #region Notification Processing

        /// <summary>
        /// 通知キューを処理
        /// </summary>
        private void ProcessNotificationQueue()
        {
            // アクティブ通知が上限に達している場合は待機
            while (_pendingNotifications.Count > 0 && _activeNotifications.Count < _maxVisibleNotifications)
            {
                var data = _pendingNotifications.Dequeue();
                CreateNotification(data);
            }
        }

        /// <summary>
        /// 通知を作成
        /// </summary>
        private void CreateNotification(NotificationData data)
        {
            if (_notificationPrefab == null || _notificationContainer == null) return;

            var notificationObj = Instantiate(_notificationPrefab, _notificationContainer);
            var notificationItem = notificationObj.GetComponent<NotificationItem>();

            if (notificationItem == null)
            {
                notificationItem = notificationObj.AddComponent<NotificationItem>();
            }

            notificationItem.Setup(data, GetIcon(data.Type), GetColor(data.Type));
            notificationItem.OnDismissed += () => OnNotificationDismissed(notificationItem);

            _activeNotifications.Add(notificationItem);

            // サウンド再生
            PlayNotificationSound(data.Type);

            // 自動非表示
            StartCoroutine(AutoDismissCoroutine(notificationItem, data.Duration));
        }

        /// <summary>
        /// 自動非表示コルーチン
        /// </summary>
        private IEnumerator AutoDismissCoroutine(NotificationItem item, float duration)
        {
            yield return new WaitForSeconds(duration);

            if (item != null && _activeNotifications.Contains(item))
            {
                item.Dismiss(_fadeOutDuration);
            }
        }

        /// <summary>
        /// 通知が閉じられた
        /// </summary>
        private void OnNotificationDismissed(NotificationItem item)
        {
            _activeNotifications.Remove(item);
            Destroy(item.gameObject);
            ProcessNotificationQueue();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 通知タイプに応じたアイコンを取得
        /// </summary>
        private Sprite GetIcon(NotificationType type)
        {
            return type switch
            {
                NotificationType.Info => _infoIcon,
                NotificationType.Warning => _warningIcon,
                NotificationType.Error => _errorIcon,
                NotificationType.Success => _successIcon,
                NotificationType.Battle => _battleIcon,
                NotificationType.Diplomacy => _diplomacyIcon,
                NotificationType.Stratagem => _stratagemIcon,
                _ => _infoIcon
            };
        }

        /// <summary>
        /// 通知タイプに応じた色を取得
        /// </summary>
        private Color GetColor(NotificationType type)
        {
            return type switch
            {
                NotificationType.Info => _infoColor,
                NotificationType.Warning => _warningColor,
                NotificationType.Error => _errorColor,
                NotificationType.Success => _successColor,
                NotificationType.Battle => _battleColor,
                NotificationType.Diplomacy => _diplomacyColor,
                NotificationType.Stratagem => _stratagemColor,
                _ => _infoColor
            };
        }

        /// <summary>
        /// 通知サウンドを再生
        /// </summary>
        private void PlayNotificationSound(NotificationType type)
        {
            if (_audioSource == null) return;

            AudioClip clip = type switch
            {
                NotificationType.Warning => _warningSound,
                NotificationType.Error => _errorSound,
                _ => _notificationSound
            };

            if (clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }

        #endregion

        #region Event Handlers

        private void OnBattleStarted(BattleEventArgs args)
        {
            var territory = GameManager.Instance?.GetTerritory(args.TerritoryId);
            ShowNotification($"戦闘開始: {territory?.Name ?? "不明"}", NotificationType.Battle);
        }

        private void OnBattleEnded(BattleResultEventArgs args)
        {
            var victor = GameManager.Instance?.GetFaction(args.VictorFactionId);
            string result = args.TerritoryConquered ? "領地を制圧" : "勝利";
            ShowNotification($"{victor?.Name ?? "不明"}が{result}!", NotificationType.Battle);
        }

        private void OnTerritoryConquered(TerritoryConqueredEventArgs args)
        {
            var territory = GameManager.Instance?.GetTerritory(args.TerritoryId);
            var newOwner = GameManager.Instance?.GetFaction(args.NewOwnerId);
            ShowNotification($"{territory?.Name}が{newOwner?.Name}の領地に", NotificationType.Success);
        }

        private void OnStratagemExecuted(StratagemEventArgs args)
        {
            if (args.Success)
            {
                ShowNotification($"計略「{args.StratagemName}」成功!", NotificationType.Stratagem);
            }
            else
            {
                ShowNotification($"計略「{args.StratagemName}」失敗", NotificationType.Warning);
            }
        }

        private void OnFactionTurnStarted(string factionId)
        {
            var faction = GameManager.Instance?.GetFaction(factionId);
            if (faction != null && faction.IsPlayer)
            {
                ShowNotification("あなたのターンです", NotificationType.Info);
            }
        }

        private void OnTurnEnded(int turnNumber)
        {
            // ターン終了時の通知（必要に応じて）
        }

        #endregion
    }

    #region Data Classes

    /// <summary>
    /// 通知タイプ
    /// </summary>
    public enum NotificationType
    {
        Info,
        Warning,
        Error,
        Success,
        Battle,
        Diplomacy,
        Stratagem
    }

    /// <summary>
    /// 通知データ
    /// </summary>
    public class NotificationData
    {
        public string Title;
        public string Message;
        public NotificationType Type;
        public float Duration;
        public float Timestamp;
        public Action OnClick;
    }

    #endregion
}
