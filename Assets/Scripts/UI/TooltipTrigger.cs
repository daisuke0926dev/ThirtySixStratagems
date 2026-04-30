using UnityEngine;
using UnityEngine.EventSystems;

namespace ThirtySixStratagems.UI
{
    /// <summary>
    /// ツールチップトリガー
    /// UI要素にアタッチしてツールチップを表示
    /// </summary>
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("ツールチップ内容")]
        [SerializeField] private string _title;
        [TextArea(2, 5)]
        [SerializeField] private string _description;

        [Header("設定")]
        [SerializeField] private bool _showImmediate = false;

        /// <summary>
        /// タイトルを設定
        /// </summary>
        public void SetTitle(string title)
        {
            _title = title;
        }

        /// <summary>
        /// 説明を設定
        /// </summary>
        public void SetDescription(string description)
        {
            _description = description;
        }

        /// <summary>
        /// タイトルと説明を設定
        /// </summary>
        public void SetContent(string title, string description)
        {
            _title = title;
            _description = description;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (TooltipManager.Instance == null) return;

            if (_showImmediate)
            {
                TooltipManager.Instance.ShowImmediate(_title, _description);
            }
            else
            {
                TooltipManager.Instance.Show(_title, _description);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.Hide();
            }
        }

        private void OnDisable()
        {
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.Hide();
            }
        }
    }
}
