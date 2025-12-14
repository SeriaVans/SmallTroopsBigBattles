using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmallTroopsBigBattles.Core;
using SmallTroopsBigBattles.Core.Managers;
using SmallTroopsBigBattles.Core.Events;

namespace SmallTroopsBigBattles.UI.HUD
{
    /// <summary>
    /// 遊戲主 HUD - 顯示資源、士兵等資訊
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("資源顯示")]
        [SerializeField] private TextMeshProUGUI _copperText;
        [SerializeField] private TextMeshProUGUI _woodText;
        [SerializeField] private TextMeshProUGUI _stoneText;
        [SerializeField] private TextMeshProUGUI _foodText;

        [Header("軍隊顯示")]
        [SerializeField] private TextMeshProUGUI _soldierCountText;
        [SerializeField] private Image _soldierFillBar;

        [Header("玩家資訊")]
        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private TextMeshProUGUI _playerLevelText;

        [Header("功能按鈕")]
        [SerializeField] private Button _territoryButton;
        [SerializeField] private Button _armyButton;
        [SerializeField] private Button _generalButton;
        [SerializeField] private Button _mapButton;
        [SerializeField] private Button _questButton;
        [SerializeField] private Button _settingsButton;

        private void Start()
        {
            BindEvents();
            SetupButtons();
            RefreshAll();
        }

        private void BindEvents()
        {
            // 訂閱資源變更事件
            EventManager.Instance.Subscribe<ResourceChangedEvent>(OnResourceChanged);
            EventManager.Instance.Subscribe<SoldierTrainedEvent>(OnSoldierTrained);
        }

        private void SetupButtons()
        {
            if (_territoryButton != null)
                _territoryButton.onClick.AddListener(OnTerritoryClicked);

            if (_armyButton != null)
                _armyButton.onClick.AddListener(OnArmyClicked);

            if (_generalButton != null)
                _generalButton.onClick.AddListener(OnGeneralClicked);

            if (_mapButton != null)
                _mapButton.onClick.AddListener(OnMapClicked);

            if (_questButton != null)
                _questButton.onClick.AddListener(OnQuestClicked);

            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        /// <summary>
        /// 刷新所有顯示
        /// </summary>
        public void RefreshAll()
        {
            RefreshResources();
            RefreshArmy();
            RefreshPlayerInfo();
        }

        /// <summary>
        /// 刷新資源顯示
        /// </summary>
        public void RefreshResources()
        {
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null) return;

            SetResourceText(_copperText, player.Resources.Copper, player.Resources.MaxCopper);
            SetResourceText(_woodText, player.Resources.Wood, player.Resources.MaxWood);
            SetResourceText(_stoneText, player.Resources.Stone, player.Resources.MaxStone);
            SetResourceText(_foodText, player.Resources.Food, player.Resources.MaxFood);
        }

        private void SetResourceText(TextMeshProUGUI text, int current, int max)
        {
            if (text == null) return;
            text.text = UIHelper.FormatResource(current, max);
        }

        /// <summary>
        /// 刷新軍隊顯示
        /// </summary>
        public void RefreshArmy()
        {
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null) return;

            int total = player.Army.TotalSoldiers;
            int max = Core.Data.PlayerArmy.MaxSoldiers;

            if (_soldierCountText != null)
            {
                _soldierCountText.text = $"{total}/{max}";
            }

            if (_soldierFillBar != null)
            {
                _soldierFillBar.fillAmount = (float)total / max;
            }
        }

        /// <summary>
        /// 刷新玩家資訊
        /// </summary>
        public void RefreshPlayerInfo()
        {
            var player = GameManager.Instance?.CurrentPlayer;
            if (player == null) return;

            if (_playerNameText != null)
            {
                _playerNameText.text = player.Name;
            }

            if (_playerLevelText != null)
            {
                _playerLevelText.text = $"Lv.{player.Level}";
            }
        }

        #region 事件處理

        private void OnResourceChanged(ResourceChangedEvent evt)
        {
            RefreshResources();
        }

        private void OnSoldierTrained(SoldierTrainedEvent evt)
        {
            RefreshArmy();
        }

        #endregion

        #region 按鈕點擊

        private void OnTerritoryClicked()
        {
            UIManager.Instance.OpenPanel<TerritoryPanel>();
        }

        private void OnArmyClicked()
        {
            UIManager.Instance.OpenPanel<ArmyPanel>();
        }

        private void OnGeneralClicked()
        {
            UIManager.Instance.OpenPanel<GeneralPanel>();
        }

        private void OnMapClicked()
        {
            UIManager.Instance.OpenPanel<MapPanel>();
        }

        private void OnQuestClicked()
        {
            UIManager.Instance.OpenPanel<QuestPanel>();
        }

        private void OnSettingsClicked()
        {
            UIManager.Instance.OpenPanel<SettingsPanel>();
        }

        #endregion

        private void OnDestroy()
        {
            EventManager.Instance?.Unsubscribe<ResourceChangedEvent>(OnResourceChanged);
            EventManager.Instance?.Unsubscribe<SoldierTrainedEvent>(OnSoldierTrained);
        }
    }
}
