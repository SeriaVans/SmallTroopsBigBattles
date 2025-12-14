using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SmallTroopsBigBattles.Core.Events;

namespace SmallTroopsBigBattles.UI
{
    /// <summary>
    /// 面板層級
    /// </summary>
    public enum PanelLayer
    {
        Normal,     // 普通層（主要面板）
        Popup,      // 彈窗層（確認框等）
        Top         // 頂層（系統提示等）
    }

    /// <summary>
    /// UI 面板基類
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BasePanel : MonoBehaviour
    {
        [Header("面板設定")]
        [SerializeField] protected PanelLayer panelLayer = PanelLayer.Normal;
        [SerializeField] protected bool showAnimation = true;
        [SerializeField] protected float animationDuration = 0.2f;

        [Header("關閉按鈕")]
        [SerializeField] protected Button closeButton;

        protected CanvasGroup canvasGroup;
        protected RectTransform rectTransform;

        /// <summary>
        /// 面板名稱
        /// </summary>
        public virtual string PanelName => GetType().Name;

        /// <summary>
        /// 面板層級
        /// </summary>
        public PanelLayer Layer => panelLayer;

        /// <summary>
        /// 面板是否顯示中
        /// </summary>
        public bool IsShowing { get; private set; }

        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClick);
            }
        }

        protected virtual void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClick);
            }
        }

        /// <summary>
        /// 顯示面板
        /// </summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);
            IsShowing = true;

            if (showAnimation)
            {
                StartCoroutine(ShowAnimation());
            }
            else
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            OnShow();
            EventManager.Instance?.Publish(new PanelOpenedEvent(PanelName));
        }

        /// <summary>
        /// 隱藏面板
        /// </summary>
        public virtual void Hide()
        {
            IsShowing = false;

            if (showAnimation && gameObject.activeInHierarchy)
            {
                StartCoroutine(HideAnimation());
            }
            else
            {
                gameObject.SetActive(false);
            }

            OnHide();
            EventManager.Instance?.Publish(new PanelClosedEvent(PanelName));
        }

        /// <summary>
        /// 顯示動畫
        /// </summary>
        protected virtual IEnumerator ShowAnimation()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            float elapsedTime = 0f;
            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / animationDuration;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// 隱藏動畫
        /// </summary>
        protected virtual IEnumerator HideAnimation()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            float elapsedTime = 0f;
            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / animationDuration;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 關閉按鈕點擊
        /// </summary>
        protected virtual void OnCloseButtonClick()
        {
            UIManager.Instance?.ClosePanel(PanelName);
        }

        /// <summary>
        /// 面板顯示時調用 - 子類覆寫
        /// </summary>
        protected virtual void OnShow()
        {
        }

        /// <summary>
        /// 面板隱藏時調用 - 子類覆寫
        /// </summary>
        protected virtual void OnHide()
        {
        }

        /// <summary>
        /// 刷新面板數據 - 子類覆寫
        /// </summary>
        public virtual void RefreshData()
        {
        }
    }
}

