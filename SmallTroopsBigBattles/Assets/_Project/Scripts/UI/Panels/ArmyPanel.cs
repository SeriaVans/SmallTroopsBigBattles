using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmallTroopsBigBattles.Core;
using SmallTroopsBigBattles.Core.Managers;
using SmallTroopsBigBattles.Core.Utils;

namespace SmallTroopsBigBattles.UI
{
    /// <summary>
    /// 軍隊面板 - 士兵訓練與管理
    /// </summary>
    public class ArmyPanel : BasePanel
    {
        [Header("士兵數量顯示")]
        [SerializeField] private TextMeshProUGUI _spearmanCountText;
        [SerializeField] private TextMeshProUGUI _shieldmanCountText;
        [SerializeField] private TextMeshProUGUI _cavalryCountText;
        [SerializeField] private TextMeshProUGUI _archerCountText;
        [SerializeField] private TextMeshProUGUI _totalCountText;

        [Header("訓練控制")]
        [SerializeField] private TMP_Dropdown _soldierTypeDropdown;
        [SerializeField] private TMP_InputField _trainCountInput;
        [SerializeField] private Slider _trainCountSlider;
        [SerializeField] private Button _trainButton;
        [SerializeField] private TextMeshProUGUI _trainCostText;

        [Header("訓練佇列")]
        [SerializeField] private GameObject _trainingQueuePanel;
        [SerializeField] private TextMeshProUGUI _trainingProgressText;
        [SerializeField] private Image _trainingProgressBar;
        [SerializeField] private TextMeshProUGUI _queueCountText;

        private SoldierType _selectedType = SoldierType.Spearman;
        private int _trainCount = 10;

        protected override void Awake()
        {
            base.Awake();
            SetupControls();
        }

        private void SetupControls()
        {
            if (_soldierTypeDropdown != null)
            {
                _soldierTypeDropdown.ClearOptions();
                _soldierTypeDropdown.AddOptions(new List<string>
                {
                    "槍兵", "盾兵", "騎兵", "弓兵"
                });
                _soldierTypeDropdown.onValueChanged.AddListener(OnSoldierTypeChanged);
            }

            if (_trainCountSlider != null)
            {
                _trainCountSlider.minValue = 1;
                _trainCountSlider.maxValue = 100;
                _trainCountSlider.value = _trainCount;
                _trainCountSlider.onValueChanged.AddListener(OnTrainCountSliderChanged);
            }

            if (_trainCountInput != null)
            {
                _trainCountInput.text = _trainCount.ToString();
                _trainCountInput.onEndEdit.AddListener(OnTrainCountInputChanged);
            }

            if (_trainButton != null)
            {
                _trainButton.onClick.AddListener(OnTrainClicked);
            }
        }

        protected override void OnPanelOpened()
        {
            RefreshSoldierCounts();
            RefreshTrainCost();
            RefreshTrainingQueue();
        }

        private void Update()
        {
            // 更新訓練進度
            if (ArmyManager.Instance.GetQueueLength() > 0)
            {
                RefreshTrainingQueue();
            }
        }

        /// <summary>
        /// 刷新士兵數量
        /// </summary>
        public void RefreshSoldierCounts()
        {
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null) return;

            if (_spearmanCountText != null)
                _spearmanCountText.text = player.Army.Spearman.ToString();

            if (_shieldmanCountText != null)
                _shieldmanCountText.text = player.Army.Shieldman.ToString();

            if (_cavalryCountText != null)
                _cavalryCountText.text = player.Army.Cavalry.ToString();

            if (_archerCountText != null)
                _archerCountText.text = player.Army.Archer.ToString();

            if (_totalCountText != null)
                _totalCountText.text = $"{player.Army.TotalSoldiers}/{Core.Data.PlayerArmy.MaxSoldiers}";
        }

        /// <summary>
        /// 刷新訓練費用
        /// </summary>
        public void RefreshTrainCost()
        {
            var costs = ArmyManager.Instance.GetTrainingCost(_selectedType, _trainCount);

            if (_trainCostText != null)
            {
                var parts = new List<string>();
                foreach (var cost in costs)
                {
                    string name = cost.Key switch
                    {
                        ResourceType.Copper => "銅",
                        ResourceType.Wood => "木",
                        ResourceType.Stone => "石",
                        ResourceType.Food => "糧",
                        _ => ""
                    };
                    parts.Add($"{name}:{cost.Value}");
                }
                _trainCostText.text = $"費用: {string.Join(" ", parts)}";
            }

            // 檢查資源是否足夠
            bool canAfford = ResourceManager.Instance.HasEnoughResources(costs);
            if (_trainButton != null)
            {
                _trainButton.interactable = canAfford && _trainCount > 0;
            }
        }

        /// <summary>
        /// 刷新訓練佇列
        /// </summary>
        public void RefreshTrainingQueue()
        {
            int queueLength = ArmyManager.Instance.GetQueueLength();
            bool hasTraining = queueLength > 0;

            UIHelper.SetActive(_trainingQueuePanel, hasTraining);

            if (hasTraining)
            {
                float progress = ArmyManager.Instance.GetTrainingProgress();
                float remaining = ArmyManager.Instance.GetTrainingRemainingTime();

                if (_trainingProgressBar != null)
                    _trainingProgressBar.fillAmount = progress;

                if (_trainingProgressText != null)
                    _trainingProgressText.text = TimeUtils.FormatTimeFriendly(remaining);

                if (_queueCountText != null)
                    _queueCountText.text = $"佇列: {queueLength}";
            }
        }

        #region 事件處理

        private void OnSoldierTypeChanged(int index)
        {
            _selectedType = (SoldierType)index;
            RefreshTrainCost();
        }

        private void OnTrainCountSliderChanged(float value)
        {
            _trainCount = Mathf.RoundToInt(value);
            if (_trainCountInput != null)
                _trainCountInput.text = _trainCount.ToString();
            RefreshTrainCost();
        }

        private void OnTrainCountInputChanged(string value)
        {
            if (int.TryParse(value, out int count))
            {
                _trainCount = Mathf.Clamp(count, 1, 1000);
                if (_trainCountSlider != null)
                    _trainCountSlider.value = Mathf.Min(_trainCount, _trainCountSlider.maxValue);
            }
            RefreshTrainCost();
        }

        private void OnTrainClicked()
        {
            if (ArmyManager.Instance.StartTraining(_selectedType, _trainCount))
            {
                RefreshSoldierCounts();
                RefreshTrainingQueue();

                // 刷新 HUD
                FindFirstObjectByType<HUD.GameHUD>()?.RefreshAll();
            }
        }

        #endregion
    }
}
