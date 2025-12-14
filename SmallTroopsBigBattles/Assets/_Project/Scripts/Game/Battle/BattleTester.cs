using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmallTroopsBigBattles.Game.Battle;
using SmallTroopsBigBattles.UI.Battle;
using SmallTroopsBigBattles.UI;
using SmallTroopsBigBattles.Core.Events;

namespace SmallTroopsBigBattles.Game.Battle
{
    /// <summary>
    /// 戰鬥測試器 - 用於測試戰鬥系統
    /// </summary>
    public class BattleTester : MonoBehaviour
    {
        [Header("測試按鈕")]
        [SerializeField] private Button createTestBattleButton;
        [SerializeField] private Button addSoldiersButton;
        [SerializeField] private Button speedUpButton;
        [SerializeField] private Button pauseButton;

        [Header("測試資訊顯示")]
        [SerializeField] private TextMeshProUGUI battleInfoText;
        [SerializeField] private TextMeshProUGUI soldierCountText;

        [Header("測試設定")]
        [SerializeField] private int testSoldierCount = 50;
        [SerializeField] private float timeScale = 1f;

        /// <summary>當前測試戰場</summary>
        private Battlefield _testBattlefield;

        /// <summary>是否暫停</summary>
        private bool _isPaused;

        /// <summary>原始時間縮放</summary>
        private float _originalTimeScale = 1f;

        private void Start()
        {
            // 綁定按鈕
            if (createTestBattleButton != null)
                createTestBattleButton.onClick.AddListener(CreateTestBattle);

            if (addSoldiersButton != null)
            {
                addSoldiersButton.onClick.AddListener(AddTestSoldiers);
                addSoldiersButton.interactable = false;
            }

            if (speedUpButton != null)
                speedUpButton.onClick.AddListener(ToggleSpeedUp);

            if (pauseButton != null)
                pauseButton.onClick.AddListener(TogglePause);

            _originalTimeScale = Time.timeScale;
        }

        private void Update()
        {
            UpdateInfoDisplay();
        }

        private void OnDestroy()
        {
            Time.timeScale = _originalTimeScale;
        }

        /// <summary>
        /// 創建測試戰場
        /// </summary>
        public void CreateTestBattle()
        {
            var battleManager = BattleManager.Instance;
            if (battleManager == null)
            {
                Debug.LogError("[BattleTester] BattleManager 未初始化！");
                return;
            }

            // 創建測試戰場
            _testBattlefield = battleManager.CreateBattle(
                "test_city_1",
                "nation_shu",
                "nation_wei",
                5000f  // 城牆血量
            );

            // 添加初始士兵
            AddTestSoldiers();

            // 開始戰鬥
            battleManager.StartBattle(_testBattlefield.BattlefieldId);

            // 開始觀戰
            battleManager.StartSpectating(_testBattlefield.BattlefieldId);

            // 打開戰鬥面板（如果存在）
            var battlePanel = UIManager.Instance?.GetPanel<BattlePanel>();
            if (battlePanel != null)
            {
                battlePanel.OpenBattle(_testBattlefield, "test_player_1", "nation_shu");
            }
            else
            {
                Debug.Log("[BattleTester] BattlePanel 未找到，將在戰鬥面板創建後自動打開");
            }

            // 啟用添加士兵按鈕
            if (addSoldiersButton != null)
                addSoldiersButton.interactable = true;

            // 訂閱戰鬥結束事件，自動打開戰鬥面板
            EventManager.Instance?.Subscribe<BattleEndedEvent>(OnTestBattleEnded);

            Debug.Log("[BattleTester] 測試戰場創建完成！");
        }

        /// <summary>
        /// 添加測試士兵
        /// </summary>
        public void AddTestSoldiers()
        {
            if (_testBattlefield == null)
            {
                Debug.LogWarning("[BattleTester] 沒有活躍的測試戰場！");
                return;
            }

            var battleManager = BattleManager.Instance;
            if (battleManager == null) return;

            // 攻方士兵
            var attackerSoldiers = new Dictionary<SoldierType, int>
            {
                { SoldierType.Spearman, testSoldierCount },
                { SoldierType.Shieldman, testSoldierCount / 2 },
                { SoldierType.Cavalry, testSoldierCount / 3 },
                { SoldierType.Archer, testSoldierCount / 2 }
            };

            battleManager.DeploySoldiers(
                _testBattlefield.BattlefieldId,
                "test_player_1",
                "nation_shu",
                attackerSoldiers
            );

            // 守方士兵
            var defenderSoldiers = new Dictionary<SoldierType, int>
            {
                { SoldierType.Spearman, testSoldierCount },
                { SoldierType.Shieldman, testSoldierCount / 2 },
                { SoldierType.Cavalry, testSoldierCount / 3 },
                { SoldierType.Archer, testSoldierCount / 2 }
            };

            battleManager.DeploySoldiers(
                _testBattlefield.BattlefieldId,
                "test_player_2",
                "nation_wei",
                defenderSoldiers
            );

            Debug.Log($"[BattleTester] 添加了 {testSoldierCount * 2} 名測試士兵");
        }

        /// <summary>
        /// 切換加速
        /// </summary>
        public void ToggleSpeedUp()
        {
            if (Time.timeScale == 1f)
            {
                Time.timeScale = 3f;  // 3倍速
                if (speedUpButton != null)
                {
                    var text = speedUpButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                        text.text = "正常速度";
                }
            }
            else
            {
                Time.timeScale = 1f;
                if (speedUpButton != null)
                {
                    var text = speedUpButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                        text.text = "加速 (3x)";
                }
            }
        }

        /// <summary>
        /// 切換暫停
        /// </summary>
        public void TogglePause()
        {
            _isPaused = !_isPaused;

            if (_isPaused)
            {
                Time.timeScale = 0f;
                if (pauseButton != null)
                {
                    var text = pauseButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                        text.text = "繼續";
                }
            }
            else
            {
                Time.timeScale = _originalTimeScale;
                if (pauseButton != null)
                {
                    var text = pauseButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                        text.text = "暫停";
                }
            }
        }

        /// <summary>
        /// 更新資訊顯示
        /// </summary>
        private void UpdateInfoDisplay()
        {
            if (_testBattlefield == null)
            {
                if (battleInfoText != null)
                    battleInfoText.text = "無活躍戰鬥";

                if (soldierCountText != null)
                    soldierCountText.text = "";

                return;
            }

            // 戰鬥資訊
            if (battleInfoText != null)
            {
                var attacker = _testBattlefield.GetAttacker();
                var defender = _testBattlefield.GetDefender();

                string info = $"戰鬥狀態: {_testBattlefield.State}\n";
                info += $"持續時間: {_testBattlefield.Duration:F1}秒\n";

                if (attacker != null)
                {
                    info += $"攻方存活: {attacker.AliveSoldiers}/{attacker.TotalSoldiers}\n";
                }

                if (defender != null)
                {
                    info += $"守方存活: {defender.AliveSoldiers}/{defender.TotalSoldiers}\n";
                }

                if (_testBattlefield.WallMaxHp > 0)
                {
                    float wallPercent = (_testBattlefield.WallCurrentHp / _testBattlefield.WallMaxHp) * 100f;
                    info += $"城牆血量: {wallPercent:F1}%";
                }

                battleInfoText.text = info;
            }

            // 士兵數量
            if (soldierCountText != null)
            {
                int aliveCount = 0;
                foreach (var soldier in _testBattlefield.Soldiers)
                {
                    if (soldier.IsAlive)
                        aliveCount++;
                }

                soldierCountText.text = $"戰場士兵: {aliveCount}";
            }
        }

        /// <summary>
        /// 快速測試（一鍵創建並開始戰鬥）
        /// </summary>
        [ContextMenu("快速測試戰鬥")]
        public void QuickTest()
        {
            CreateTestBattle();
        }

        /// <summary>
        /// 添加大量士兵測試
        /// </summary>
        [ContextMenu("添加大量士兵")]
        public void AddManySoldiers()
        {
            testSoldierCount = 200;
            AddTestSoldiers();
        }

        /// <summary>
        /// 結束測試戰鬥
        /// </summary>
        public void EndTestBattle()
        {
            if (_testBattlefield == null) return;

            var battleManager = BattleManager.Instance;
            if (battleManager != null)
            {
                battleManager.StopSpectating();
            }

            _testBattlefield = null;

            if (addSoldiersButton != null)
                addSoldiersButton.interactable = false;

            EventManager.Instance?.Unsubscribe<BattleEndedEvent>(OnTestBattleEnded);

            Debug.Log("[BattleTester] 測試戰鬥結束");
        }

        /// <summary>
        /// 測試戰鬥結束事件處理
        /// </summary>
        private void OnTestBattleEnded(BattleEndedEvent evt)
        {
            if (evt.Battlefield?.BattlefieldId == _testBattlefield?.BattlefieldId)
            {
                Debug.Log($"[BattleTester] 測試戰鬥結束，結果：{evt.Result}");
                if (evt.Report != null)
                {
                    Debug.Log(evt.Report.GenerateSummary());
                }
            }
        }
    }
}

