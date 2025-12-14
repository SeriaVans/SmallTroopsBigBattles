using UnityEngine;
using UnityEngine.UI;

namespace SmallTroopsBigBattles.UI
{
    /// <summary>
    /// 面板基類 - 所有 UI 面板繼承此類
    /// </summary>
    public abstract class BasePanel : MonoBehaviour
    {
        [Header("面板設定")]
        [SerializeField] private PanelLayer _panelLayer = PanelLayer.Normal;
        [SerializeField] private bool _showAnimation = true;

        [Header("UI 元件")]
        [SerializeField] protected Button _closeButton;
        [SerializeField] protected CanvasGroup _canvasGroup;

        public PanelLayer PanelLayer => _panelLayer;

        protected virtual void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(OnCloseButtonClicked);
            }
        }

        /// <summary>
        /// 面板開啟時調用
        /// </summary>
        public virtual void OnOpen()
        {
            gameObject.SetActive(true);

            if (_showAnimation && _canvasGroup != null)
            {
                PlayOpenAnimation();
            }

            OnPanelOpened();
        }

        /// <summary>
        /// 面板關閉時調用
        /// </summary>
        public virtual void OnClose()
        {
            OnPanelClosed();
        }

        /// <summary>
        /// 子類覆寫 - 面板開啟邏輯
        /// </summary>
        protected virtual void OnPanelOpened() { }

        /// <summary>
        /// 子類覆寫 - 面板關閉邏輯
        /// </summary>
        protected virtual void OnPanelClosed() { }

        /// <summary>
        /// 關閉按鈕點擊
        /// </summary>
        protected virtual void OnCloseButtonClicked()
        {
            UIManager.Instance.ClosePanel(GetType());
        }

        /// <summary>
        /// 播放開啟動畫
        /// </summary>
        protected virtual void PlayOpenAnimation()
        {
            if (_canvasGroup == null) return;

            _canvasGroup.alpha = 0;
            StartCoroutine(FadeIn());
        }

        private System.Collections.IEnumerator FadeIn()
        {
            float duration = 0.2f;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / duration);
                yield return null;
            }

            _canvasGroup.alpha = 1;
        }

        /// <summary>
        /// 設定互動性
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = interactable;
                _canvasGroup.blocksRaycasts = interactable;
            }
        }

        protected virtual void OnDestroy()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            }
        }
    }
}
