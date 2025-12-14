using UnityEngine;
using SmallTroopsBigBattles.UI;

namespace SmallTroopsBigBattles.UI.Battle
{
    /// <summary>
    /// 戰鬥面板初始化器 - 確保戰鬥面板正確註冊
    /// </summary>
    public class BattlePanelInitializer : MonoBehaviour
    {
        [Header("面板預製體")]
        [SerializeField] private BattlePanel battlePanelPrefab;

        private void Start()
        {
            // 自動註冊戰鬥面板
            if (battlePanelPrefab != null)
            {
                UIManager.Instance?.RegisterPanelPrefab(battlePanelPrefab);
                Debug.Log("[BattlePanelInitializer] 戰鬥面板已註冊");
            }
            else
            {
                // 嘗試在場景中查找
                var existingPanel = FindObjectOfType<BattlePanel>();
                if (existingPanel != null)
                {
                    UIManager.Instance?.RegisterPanelPrefab(existingPanel);
                    Debug.Log("[BattlePanelInitializer] 找到場景中的戰鬥面板並註冊");
                }
            }
        }
    }
}

