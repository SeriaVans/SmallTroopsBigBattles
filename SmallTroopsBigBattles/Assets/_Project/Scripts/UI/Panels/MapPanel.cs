using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SmallTroopsBigBattles.UI
{
    /// <summary>
    /// 地圖面板 - 顯示世界地圖 (預留)
    /// </summary>
    public class MapPanel : BasePanel
    {
        [Header("地圖顯示")]
        [SerializeField] private RectTransform _mapContainer;
        [SerializeField] private TextMeshProUGUI _mapTitleText;

        protected override void OnPanelOpened()
        {
            if (_mapTitleText != null)
                _mapTitleText.text = "世界地圖";

            Debug.Log("[MapPanel] 地圖系統開發中...");
        }
    }
}
