using UnityEngine;
using TMPro;

namespace SmallTroopsBigBattles.UI
{
    public class MapPanel : BasePanel
    {
        [SerializeField] private TextMeshProUGUI _mapTitleText;

        protected override void OnPanelOpened()
        {
            if (_mapTitleText) _mapTitleText.text = "世界地圖";
            Debug.Log("[MapPanel] 地圖系統開發中...");
        }
    }
}
