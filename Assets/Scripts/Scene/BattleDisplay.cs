using System.Collections;
using UnityEngine;
using TMPro;
using ThirtySixStratagems.Battle;

namespace ThirtySixStratagems.Scene
{
    /// <summary>
    /// 戦闘表示
    /// 戦闘シーンのビジュアル表現を管理
    /// </summary>
    public class BattleDisplay : MonoBehaviour
    {
        [Header("攻撃側表示")]
        [SerializeField] private Transform _attackerPosition;
        [SerializeField] private SpriteRenderer _attackerSprite;
        [SerializeField] private TextMeshPro _attackerNameLabel;
        [SerializeField] private TextMeshPro _attackerSoldiersLabel;
        [SerializeField] private SpriteRenderer _attackerHealthBar;

        [Header("防御側表示")]
        [SerializeField] private Transform _defenderPosition;
        [SerializeField] private SpriteRenderer _defenderSprite;
        [SerializeField] private TextMeshPro _defenderNameLabel;
        [SerializeField] private TextMeshPro _defenderSoldiersLabel;
        [SerializeField] private SpriteRenderer _defenderHealthBar;

        [Header("エフェクト")]
        [SerializeField] private GameObject _attackEffectPrefab;
        [SerializeField] private GameObject _damageTextPrefab;
        [SerializeField] private Transform _effectsContainer;

        [Header("アニメーション設定")]
        [SerializeField] private float _attackAnimationDuration = 0.5f;
        [SerializeField] private float _damageAnimationDuration = 0.3f;
        [SerializeField] private float _shakeIntensity = 0.1f;
        [SerializeField] private AnimationCurve _attackCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("色設定")]
        [SerializeField] private Color _attackerColor = new Color(0.2f, 0.5f, 1f);
        [SerializeField] private Color _defenderColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private Color _healthFullColor = Color.green;
        [SerializeField] private Color _healthLowColor = Color.red;

        // 状態
        private BattleState _currentBattle;
        private Vector3 _attackerOriginalPos;
        private Vector3 _defenderOriginalPos;

        private void Awake()
        {
            // オリジナル位置を保存
            if (_attackerPosition != null)
                _attackerOriginalPos = _attackerPosition.localPosition;
            if (_defenderPosition != null)
                _defenderOriginalPos = _defenderPosition.localPosition;
        }

        /// <summary>
        /// 戦闘表示を初期化
        /// </summary>
        public void Initialize(BattleState battle)
        {
            _currentBattle = battle;

            if (battle == null) return;

            // 攻撃側の表示
            SetupUnitDisplay(
                _attackerSprite,
                _attackerNameLabel,
                _attackerSoldiersLabel,
                _attackerHealthBar,
                battle.Attacker,
                _attackerColor);

            // 防御側の表示
            SetupUnitDisplay(
                _defenderSprite,
                _defenderNameLabel,
                _defenderSoldiersLabel,
                _defenderHealthBar,
                battle.Defender,
                _defenderColor);

            Debug.Log($"BattleDisplay initialized: {battle.Attacker.ArmyName} vs {battle.Defender.ArmyName}");
        }

        /// <summary>
        /// ユニット表示をセットアップ
        /// </summary>
        private void SetupUnitDisplay(
            SpriteRenderer sprite,
            TextMeshPro nameLabel,
            TextMeshPro soldiersLabel,
            SpriteRenderer healthBar,
            BattleUnit unit,
            Color color)
        {
            if (sprite != null)
            {
                sprite.color = color;
            }

            if (nameLabel != null)
            {
                nameLabel.text = $"{unit.ArmyName}\n({unit.FactionName})";
            }

            UpdateSoldiersLabel(soldiersLabel, unit);
            UpdateHealthBar(healthBar, unit);
        }

        /// <summary>
        /// 兵力ラベルを更新
        /// </summary>
        private void UpdateSoldiersLabel(TextMeshPro label, BattleUnit unit)
        {
            if (label == null) return;

            label.text = $"兵力: {unit.CurrentSoldiers:N0}\n士気: {unit.Morale}";
        }

        /// <summary>
        /// 体力バーを更新
        /// </summary>
        private void UpdateHealthBar(SpriteRenderer healthBar, BattleUnit unit)
        {
            if (healthBar == null) return;

            float ratio = (float)unit.CurrentSoldiers / unit.InitialSoldiers;
            ratio = Mathf.Clamp01(ratio);

            // スケールで表現
            healthBar.transform.localScale = new Vector3(ratio, 1f, 1f);

            // 色で表現
            healthBar.color = Color.Lerp(_healthLowColor, _healthFullColor, ratio);
        }

        /// <summary>
        /// 表示を更新
        /// </summary>
        public void UpdateDisplay(BattleState battle)
        {
            if (battle == null) return;

            _currentBattle = battle;

            // 攻撃側
            UpdateSoldiersLabel(_attackerSoldiersLabel, battle.Attacker);
            UpdateHealthBar(_attackerHealthBar, battle.Attacker);

            // 防御側
            UpdateSoldiersLabel(_defenderSoldiersLabel, battle.Defender);
            UpdateHealthBar(_defenderHealthBar, battle.Defender);
        }

        /// <summary>
        /// 戦闘アニメーションを再生
        /// </summary>
        public IEnumerator PlayCombatAnimation(BattleRoundResult result)
        {
            // 攻撃側の攻撃アニメーション
            yield return StartCoroutine(PlayAttackAnimation(_attackerPosition, _defenderPosition));

            // 防御側へのダメージ表示
            ShowDamageText(_defenderPosition, result.DefenderCasualties);
            yield return StartCoroutine(PlayShakeAnimation(_defenderPosition, _defenderOriginalPos));

            yield return new WaitForSeconds(0.2f);

            // 防御側の反撃アニメーション
            yield return StartCoroutine(PlayAttackAnimation(_defenderPosition, _attackerPosition));

            // 攻撃側へのダメージ表示
            ShowDamageText(_attackerPosition, result.AttackerCasualties);
            yield return StartCoroutine(PlayShakeAnimation(_attackerPosition, _attackerOriginalPos));

            // 表示を更新
            if (_currentBattle != null)
            {
                UpdateDisplay(_currentBattle);
            }
        }

        /// <summary>
        /// 攻撃アニメーション
        /// </summary>
        private IEnumerator PlayAttackAnimation(Transform attacker, Transform target)
        {
            if (attacker == null || target == null) yield break;

            Vector3 startPos = attacker.localPosition;
            Vector3 targetPos = Vector3.Lerp(startPos, target.localPosition, 0.3f);

            float elapsed = 0f;

            // 前進
            while (elapsed < _attackAnimationDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = _attackCurve.Evaluate(elapsed / (_attackAnimationDuration * 0.5f));
                attacker.localPosition = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            // エフェクト表示
            if (_attackEffectPrefab != null && _effectsContainer != null)
            {
                var effect = Instantiate(_attackEffectPrefab, target.position, Quaternion.identity, _effectsContainer);
                Destroy(effect, 1f);
            }

            // 後退
            elapsed = 0f;
            while (elapsed < _attackAnimationDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = _attackCurve.Evaluate(elapsed / (_attackAnimationDuration * 0.5f));
                attacker.localPosition = Vector3.Lerp(targetPos, startPos, t);
                yield return null;
            }

            attacker.localPosition = startPos;
        }

        /// <summary>
        /// シェイクアニメーション
        /// </summary>
        private IEnumerator PlayShakeAnimation(Transform target, Vector3 originalPos)
        {
            if (target == null) yield break;

            float elapsed = 0f;

            while (elapsed < _damageAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float intensity = _shakeIntensity * (1f - elapsed / _damageAnimationDuration);
                Vector3 shake = new Vector3(
                    Random.Range(-intensity, intensity),
                    Random.Range(-intensity, intensity),
                    0);
                target.localPosition = originalPos + shake;
                yield return null;
            }

            target.localPosition = originalPos;
        }

        /// <summary>
        /// ダメージテキストを表示
        /// </summary>
        private void ShowDamageText(Transform position, int damage)
        {
            if (_damageTextPrefab == null || position == null) return;

            var damageObj = Instantiate(_damageTextPrefab, position.position + Vector3.up, Quaternion.identity, _effectsContainer);

            var textMesh = damageObj.GetComponent<TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = $"-{damage}";
                textMesh.color = Color.red;
            }

            // フロートアップアニメーション
            StartCoroutine(FloatUpAndFade(damageObj));
        }

        /// <summary>
        /// フロートアップ＆フェードアウト
        /// </summary>
        private IEnumerator FloatUpAndFade(GameObject obj)
        {
            if (obj == null) yield break;

            float duration = 1f;
            float elapsed = 0f;
            Vector3 startPos = obj.transform.position;
            var textMesh = obj.GetComponent<TextMeshPro>();
            Color startColor = textMesh != null ? textMesh.color : Color.white;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                obj.transform.position = startPos + Vector3.up * t * 0.5f;

                if (textMesh != null)
                {
                    textMesh.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
                }

                yield return null;
            }

            Destroy(obj);
        }

        /// <summary>
        /// 結果を表示
        /// </summary>
        public void ShowResult(BattleResult result)
        {
            if (result == null) return;

            // 勝者の強調表示
            if (result.AttackerVictory)
            {
                StartCoroutine(PlayVictoryAnimation(_attackerPosition));
                StartCoroutine(PlayDefeatAnimation(_defenderPosition));
            }
            else
            {
                StartCoroutine(PlayVictoryAnimation(_defenderPosition));
                StartCoroutine(PlayDefeatAnimation(_attackerPosition));
            }
        }

        /// <summary>
        /// 勝利アニメーション
        /// </summary>
        private IEnumerator PlayVictoryAnimation(Transform target)
        {
            if (target == null) yield break;

            float duration = 0.5f;
            float elapsed = 0f;
            Vector3 originalScale = target.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float scale = 1f + Mathf.Sin(elapsed / duration * Mathf.PI) * 0.2f;
                target.localScale = originalScale * scale;
                yield return null;
            }

            target.localScale = originalScale;
        }

        /// <summary>
        /// 敗北アニメーション
        /// </summary>
        private IEnumerator PlayDefeatAnimation(Transform target)
        {
            if (target == null) yield break;

            float duration = 1f;
            float elapsed = 0f;

            var spriteRenderer = target.GetComponentInChildren<SpriteRenderer>();
            Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / duration) * 0.5f;

                if (spriteRenderer != null)
                {
                    spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                }

                yield return null;
            }
        }

        /// <summary>
        /// リセット
        /// </summary>
        public void Reset()
        {
            if (_attackerPosition != null)
                _attackerPosition.localPosition = _attackerOriginalPos;
            if (_defenderPosition != null)
                _defenderPosition.localPosition = _defenderOriginalPos;

            _currentBattle = null;
        }
    }
}
