using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SmallTroopsBigBattles.UI
{
    /// <summary>
    /// 任務面板 - 顯示任務列表 (預留)
    /// </summary>
    public class QuestPanel : BasePanel
    {
        [Header("任務分頁")]
        [SerializeField] private Button _dailyTabButton;
        [SerializeField] private Button _mainTabButton;

        [Header("任務列表")]
        [SerializeField] private Transform _questListContainer;
        [SerializeField] private TextMeshProUGUI _questTitleText;

        protected override void OnPanelOpened()
        {
            if (_questTitleText != null)
                _questTitleText.text = "任務";

            Debug.Log("[QuestPanel] 任務系統開發中...");
        }
    }
}
