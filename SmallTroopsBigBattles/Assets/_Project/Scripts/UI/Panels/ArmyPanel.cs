using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmallTroopsBigBattles.Core;
using SmallTroopsBigBattles.Core.Managers;
using SmallTroopsBigBattles.Core.Utils;

namespace SmallTroopsBigBattles.UI
{
    public class ArmyPanel : BasePanel
    {
        [Header("士兵數量")]
        [SerializeField] private TextMeshProUGUI _spearmanCountText;
        [SerializeField] private TextMeshProUGUI _shieldmanCountText;
        [SerializeField] private TextMeshProUGUI _cavalryCountText;
        [SerializeField] private TextMeshProUGUI _archerCountText;
        [SerializeField] private TextMeshProUGUI _totalCountText;

        [Header("訓練控制")]
        [SerializeField] private TMP_Dropdown _soldierTypeDropdown;
        [SerializeField] private Slider _trainCountSlider;
        [SerializeField] private TMP_InputField _trainCountInput;
        [SerializeField] private Button _trainButton;
        [SerializeField] private TextMeshProUGUI _trainCostText;

        [Header("訓練佇列")]
        [SerializeField] private GameObject _trainingQueuePanel;
        [SerializeField] private TextMeshProUGUI _trainingProgressText;
        [SerializeField] private Image _trainingProgressBar;

        private SoldierType _selectedType = SoldierType.Spearman;
        private int _trainCount = 10;

        protected override void Awake()
        {
            base.Awake();
            if (_soldierTypeDropdown)
            {
                _soldierTypeDropdown.ClearOptions();
                _soldierTypeDropdown.AddOptions(new List<string> { "槍兵", "盾兵", "騎兵", "弓兵" });
                _soldierTypeDropdown.onValueChanged.AddListener(i => { _selectedType = (SoldierType)i; RefreshTrainCost(); });
            }
            if (_trainCountSlider)
            {
                _trainCountSlider.minValue = 1; _trainCountSlider.maxValue = 100; _trainCountSlider.value = _trainCount;
                _trainCountSlider.onValueChanged.AddListener(v => { _trainCount = Mathf.RoundToInt(v); if (_trainCountInput) _trainCountInput.text = _trainCount.ToString(); RefreshTrainCost(); });
            }
            if (_trainCountInput) _trainCountInput.onEndEdit.AddListener(v => { if (int.TryParse(v, out int c)) _trainCount = Mathf.Clamp(c, 1, 1000); RefreshTrainCost(); });
            if (_trainButton) _trainButton.onClick.AddListener(OnTrainClicked);
        }

        protected override void OnPanelOpened() { RefreshSoldierCounts(); RefreshTrainCost(); RefreshTrainingQueue(); }

        private void Update() { if (ArmyManager.Instance.GetQueueLength() > 0) RefreshTrainingQueue(); }

        public void RefreshSoldierCounts()
        {
            var p = GameManager.Instance?.CurrentPlayer;
            if (p == null) return;
            if (_spearmanCountText) _spearmanCountText.text = p.Army.Spearman.ToString();
            if (_shieldmanCountText) _shieldmanCountText.text = p.Army.Shieldman.ToString();
            if (_cavalryCountText) _cavalryCountText.text = p.Army.Cavalry.ToString();
            if (_archerCountText) _archerCountText.text = p.Army.Archer.ToString();
            if (_totalCountText) _totalCountText.text = $"{p.Army.TotalSoldiers}/{Core.Data.PlayerArmy.MaxSoldiers}";
        }

        public void RefreshTrainCost()
        {
            var costs = ArmyManager.Instance.GetTrainingCost(_selectedType, _trainCount);
            if (_trainCostText)
            {
                var parts = new List<string>();
                foreach (var c in costs) parts.Add($"{c.Key}:{c.Value}");
                _trainCostText.text = string.Join(" ", parts);
            }
            if (_trainButton) _trainButton.interactable = ResourceManager.Instance.HasEnoughResources(costs) && _trainCount > 0;
        }

        public void RefreshTrainingQueue()
        {
            int q = ArmyManager.Instance.GetQueueLength();
            UIHelper.SetActive(_trainingQueuePanel, q > 0);
            if (q > 0)
            {
                if (_trainingProgressBar) _trainingProgressBar.fillAmount = ArmyManager.Instance.GetTrainingProgress();
                if (_trainingProgressText) _trainingProgressText.text = TimeUtils.FormatTimeFriendly(ArmyManager.Instance.GetTrainingRemainingTime());
            }
        }

        private void OnTrainClicked()
        {
            if (ArmyManager.Instance.StartTraining(_selectedType, _trainCount))
            {
                RefreshSoldierCounts();
                RefreshTrainingQueue();
            }
        }
    }
}
