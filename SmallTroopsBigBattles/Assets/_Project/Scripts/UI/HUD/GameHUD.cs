using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmallTroopsBigBattles.Core;
using SmallTroopsBigBattles.Core.Managers;
using SmallTroopsBigBattles.Core.Events;

namespace SmallTroopsBigBattles.UI.HUD
{
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
        [SerializeField] private Button _settingsButton;

        private void Start()
        {
            EventManager.Instance.Subscribe<ResourceChangedEvent>(e => RefreshResources());
            EventManager.Instance.Subscribe<SoldierTrainedEvent>(e => RefreshArmy());

            if (_territoryButton) _territoryButton.onClick.AddListener(() => UIManager.Instance.OpenPanel<TerritoryPanel>());
            if (_armyButton) _armyButton.onClick.AddListener(() => UIManager.Instance.OpenPanel<ArmyPanel>());
            if (_generalButton) _generalButton.onClick.AddListener(() => UIManager.Instance.OpenPanel<GeneralPanel>());
            if (_mapButton) _mapButton.onClick.AddListener(() => UIManager.Instance.OpenPanel<MapPanel>());
            if (_settingsButton) _settingsButton.onClick.AddListener(() => UIManager.Instance.OpenPanel<SettingsPanel>());

            RefreshAll();
        }

        public void RefreshAll() { RefreshResources(); RefreshArmy(); RefreshPlayerInfo(); }

        public void RefreshResources()
        {
            var p = GameManager.Instance?.CurrentPlayer;
            if (p == null) return;
            if (_copperText) _copperText.text = UIHelper.FormatResource(p.Resources.Copper, p.Resources.MaxCopper);
            if (_woodText) _woodText.text = UIHelper.FormatResource(p.Resources.Wood, p.Resources.MaxWood);
            if (_stoneText) _stoneText.text = UIHelper.FormatResource(p.Resources.Stone, p.Resources.MaxStone);
            if (_foodText) _foodText.text = UIHelper.FormatResource(p.Resources.Food, p.Resources.MaxFood);
        }

        public void RefreshArmy()
        {
            var p = GameManager.Instance?.CurrentPlayer;
            if (p == null) return;
            int total = p.Army.TotalSoldiers, max = Core.Data.PlayerArmy.MaxSoldiers;
            if (_soldierCountText) _soldierCountText.text = $"{total}/{max}";
            if (_soldierFillBar) _soldierFillBar.fillAmount = (float)total / max;
        }

        public void RefreshPlayerInfo()
        {
            var p = GameManager.Instance?.CurrentPlayer;
            if (p == null) return;
            if (_playerNameText) _playerNameText.text = p.Name;
            if (_playerLevelText) _playerLevelText.text = $"Lv.{p.Level}";
        }
    }
}
