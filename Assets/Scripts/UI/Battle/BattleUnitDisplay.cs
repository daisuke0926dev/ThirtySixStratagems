using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThirtySixStratagems.Battle;

namespace ThirtySixStratagems.UI.Battle
{
    /// <summary>
    /// 戦闘ユニット表示
    /// 攻撃側/防御側の情報を表示
    /// </summary>
    public class BattleUnitDisplay : MonoBehaviour
    {
        [Header("基本情報")]
        [SerializeField] private TextMeshProUGUI _armyNameText;
        [SerializeField] private TextMeshProUGUI _factionNameText;
        [SerializeField] private TextMeshProUGUI _commanderNameText;
        [SerializeField] private Image _factionColorImage;

        [Header("兵力")]
        [SerializeField] private TextMeshProUGUI _soldierCountText;
        [SerializeField] private Slider _soldierSlider;
        [SerializeField] private Image _soldierSliderFill;

        [Header("士気")]
        [SerializeField] private TextMeshProUGUI _moraleText;
        [SerializeField] private Slider _moraleSlider;
        [SerializeField] private Image _moraleSliderFill;

        [Header("戦闘力")]
        [SerializeField] private TextMeshProUGUI _combatPowerText;

        [Header("効果")]
        [SerializeField] private Transform _effectsContainer;
        [SerializeField] private GameObject _effectIconPrefab;

        [Header("色設定")]
        [SerializeField] private Color _healthyColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color _damagedColor = new Color(0.8f, 0.8f, 0.2f);
        [SerializeField] private Color _criticalColor = new Color(0.8f, 0.2f, 0.2f);

        [SerializeField] private Color _highMoraleColor = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color _normalMoraleColor = new Color(0.8f, 0.8f, 0.8f);
        [SerializeField] private Color _lowMoraleColor = new Color(0.8f, 0.4f, 0.2f);

        private BattleUnit _currentUnit;

        /// <summary>
        /// 表示を更新
        /// </summary>
        public void UpdateDisplay(BattleUnit unit)
        {
            _currentUnit = unit;

            if (unit == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            UpdateBasicInfo(unit);
            UpdateSoldierDisplay(unit);
            UpdateMoraleDisplay(unit);
            UpdateCombatPower(unit);
            UpdateEffects(unit);
        }

        /// <summary>
        /// 基本情報を更新
        /// </summary>
        private void UpdateBasicInfo(BattleUnit unit)
        {
            if (_armyNameText != null)
            {
                _armyNameText.text = unit.ArmyName;
            }

            if (_factionNameText != null)
            {
                _factionNameText.text = unit.FactionName;
            }

            if (_commanderNameText != null)
            {
                _commanderNameText.text = !string.IsNullOrEmpty(unit.CommanderName)
                    ? $"指揮官: {unit.CommanderName}"
                    : "指揮官: なし";
            }
        }

        /// <summary>
        /// 兵力表示を更新
        /// </summary>
        private void UpdateSoldierDisplay(BattleUnit unit)
        {
            float ratio = (float)unit.CurrentSoldiers / Mathf.Max(1, unit.InitialSoldiers);

            if (_soldierCountText != null)
            {
                _soldierCountText.text = $"{unit.CurrentSoldiers:N0} / {unit.InitialSoldiers:N0}";
            }

            if (_soldierSlider != null)
            {
                _soldierSlider.value = ratio;
            }

            if (_soldierSliderFill != null)
            {
                _soldierSliderFill.color = GetHealthColor(ratio);
            }
        }

        /// <summary>
        /// 士気表示を更新
        /// </summary>
        private void UpdateMoraleDisplay(BattleUnit unit)
        {
            float moraleRatio = unit.Morale / 100f;

            if (_moraleText != null)
            {
                _moraleText.text = $"士気: {unit.Morale}";
            }

            if (_moraleSlider != null)
            {
                _moraleSlider.value = moraleRatio;
            }

            if (_moraleSliderFill != null)
            {
                _moraleSliderFill.color = GetMoraleColor(unit.Morale);
            }
        }

        /// <summary>
        /// 戦闘力を更新
        /// </summary>
        private void UpdateCombatPower(BattleUnit unit)
        {
            if (_combatPowerText != null)
            {
                int totalPower = unit.BaseCombatPower;

                if (unit.IsDefender)
                {
                    totalPower += unit.TerrainBonus;
                }

                // 効果による修正
                foreach (var effect in unit.ActiveEffects)
                {
                    totalPower = Mathf.RoundToInt(totalPower * (1f + effect.PowerModifier / 100f));
                }

                string powerText = $"戦闘力: {totalPower}";

                if (unit.TerrainBonus > 0 && unit.IsDefender)
                {
                    powerText += $" (+{unit.TerrainBonus} 地形)";
                }

                _combatPowerText.text = powerText;
            }
        }

        /// <summary>
        /// 効果を更新
        /// </summary>
        private void UpdateEffects(BattleUnit unit)
        {
            if (_effectsContainer == null) return;

            // 既存の効果アイコンをクリア
            foreach (Transform child in _effectsContainer)
            {
                Destroy(child.gameObject);
            }

            if (_effectIconPrefab == null) return;

            // 効果アイコンを追加
            foreach (var effect in unit.ActiveEffects)
            {
                var iconObj = Instantiate(_effectIconPrefab, _effectsContainer);

                var text = iconObj.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = GetEffectShortName(effect.EffectName);
                }

                var tooltip = iconObj.GetComponent<UITooltip>();
                if (tooltip != null)
                {
                    tooltip.SetTooltip($"{effect.EffectName}\n戦闘力 +{effect.PowerModifier}%");
                }
            }
        }

        /// <summary>
        /// 健康状態に応じた色を取得
        /// </summary>
        private Color GetHealthColor(float ratio)
        {
            if (ratio > 0.6f)
            {
                return _healthyColor;
            }
            else if (ratio > 0.3f)
            {
                return _damagedColor;
            }
            else
            {
                return _criticalColor;
            }
        }

        /// <summary>
        /// 士気に応じた色を取得
        /// </summary>
        private Color GetMoraleColor(int morale)
        {
            if (morale >= 70)
            {
                return _highMoraleColor;
            }
            else if (morale >= 40)
            {
                return _normalMoraleColor;
            }
            else
            {
                return _lowMoraleColor;
            }
        }

        /// <summary>
        /// 効果の短縮名を取得
        /// </summary>
        private string GetEffectShortName(string effectName)
        {
            switch (effectName)
            {
                case "攻撃力上昇":
                    return "攻↑";
                case "防御力上昇":
                    return "防↑";
                case "奇襲":
                    return "奇";
                default:
                    return effectName.Length > 2 ? effectName.Substring(0, 2) : effectName;
            }
        }

        /// <summary>
        /// ダメージアニメーション
        /// </summary>
        public void PlayDamageAnimation()
        {
            // TODO: ダメージ時のアニメーション
            // 例：赤く点滅、揺れるなど
        }

        /// <summary>
        /// 勝利アニメーション
        /// </summary>
        public void PlayVictoryAnimation()
        {
            // TODO: 勝利時のアニメーション
        }

        /// <summary>
        /// 敗北アニメーション
        /// </summary>
        public void PlayDefeatAnimation()
        {
            // TODO: 敗北時のアニメーション
        }
    }

    /// <summary>
    /// UIツールチップ（プレースホルダー）
    /// </summary>
    public class UITooltip : MonoBehaviour
    {
        private string _tooltipText;

        public void SetTooltip(string text)
        {
            _tooltipText = text;
        }

        public string GetTooltip()
        {
            return _tooltipText;
        }
    }
}
