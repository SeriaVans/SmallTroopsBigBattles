using UnityEngine;
using UnityEngine.UI;

namespace SmallTroopsBigBattles.UI
{
    public abstract class BasePanel : MonoBehaviour
    {
        [Header("面板設定")]
        [SerializeField] private PanelLayer _panelLayer = PanelLayer.Normal;
        [SerializeField] private bool _showAnimation = true;
        [SerializeField] protected Button _closeButton;
        [SerializeField] protected CanvasGroup _canvasGroup;

        public PanelLayer PanelLayer => _panelLayer;

        protected virtual void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_closeButton != null) _closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        public virtual void OnOpen()
        {
            gameObject.SetActive(true);
            if (_showAnimation && _canvasGroup != null) PlayOpenAnimation();
            OnPanelOpened();
        }

        public virtual void OnClose() => OnPanelClosed();

        protected virtual void OnPanelOpened() { }
        protected virtual void OnPanelClosed() { }

        protected virtual void OnCloseButtonClicked() => UIManager.Instance.ClosePanel(GetType());

        protected virtual void PlayOpenAnimation()
        {
            if (_canvasGroup == null) return;
            _canvasGroup.alpha = 0;
            StartCoroutine(FadeIn());
        }

        private System.Collections.IEnumerator FadeIn()
        {
            float duration = 0.2f, elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / duration);
                yield return null;
            }
            _canvasGroup.alpha = 1;
        }

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
            if (_closeButton != null) _closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }
    }
}
