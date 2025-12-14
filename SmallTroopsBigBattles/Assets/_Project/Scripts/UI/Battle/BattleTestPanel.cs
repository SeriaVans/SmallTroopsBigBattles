using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmallTroopsBigBattles.Game.Battle;

namespace SmallTroopsBigBattles.UI.Battle
{
    /// <summary>
    /// 戰鬥測試面板 - 簡化的測試介面
    /// </summary>
    public class BattleTestPanel : BasePanel
    {
        [Header("測試按鈕")]
        [SerializeField] private Button createBattleButton;
        [SerializeField] private Button addSoldiersButton;
        [SerializeField] private Button speedUpButton;
        [SerializeField] private Button pauseButton;

        [Header("資訊顯示")]
        [SerializeField] private TextMeshProUGUI infoText;

        private BattleTester _tester;

        protected override void Awake()
        {
            base.Awake();

            // 獲取或創建 BattleTester
            _tester = FindObjectOfType<BattleTester>();
            if (_tester == null)
            {
                var testerObj = new GameObject("BattleTester");
                _tester = testerObj.AddComponent<BattleTester>();
            }

            // 綁定按鈕
            if (createBattleButton != null)
                createBattleButton.onClick.AddListener(() => _tester?.CreateTestBattle());

            if (addSoldiersButton != null)
            {
                addSoldiersButton.onClick.AddListener(() => _tester?.AddTestSoldiers());
                addSoldiersButton.interactable = false;
            }

            if (speedUpButton != null)
                speedUpButton.onClick.AddListener(() => _tester?.ToggleSpeedUp());

            if (pauseButton != null)
                pauseButton.onClick.AddListener(() => _tester?.TogglePause());
        }

        private void Update()
        {
            if (_tester != null && infoText != null)
            {
                // 更新資訊顯示（通過 BattleTester）
            }
        }
    }
}

