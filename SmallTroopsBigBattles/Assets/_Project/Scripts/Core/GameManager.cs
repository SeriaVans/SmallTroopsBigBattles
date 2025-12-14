using UnityEngine;
using SmallTroopsBigBattles.Core.Events;
using SmallTroopsBigBattles.Game.Data;
using SmallTroopsBigBattles.Game.Resource;
using SmallTroopsBigBattles.Game.Territory;
using SmallTroopsBigBattles.Game.Army;
using SmallTroopsBigBattles.Game.General;
using SmallTroopsBigBattles.Game.Map;
using SmallTroopsBigBattles.Game.City;
using SmallTroopsBigBattles.Game.Battle;
using SmallTroopsBigBattles.UI;

namespace SmallTroopsBigBattles.Core
{
    /// <summary>
    /// 遊戲管理器 - 負責遊戲全局控制和初始化
    /// </summary>
    public class GameManager : SingletonBase<GameManager>
    {
        /// <summary>
        /// 當前玩家的完整數據
        /// </summary>
        public PlayerFullData CurrentPlayer { get; private set; }

        /// <summary>
        /// 遊戲是否已初始化
        /// </summary>
        public bool IsInitialized { get; private set; }

        [Header("遊戲設定")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private string testPlayerName = "測試玩家";
        [SerializeField] private int testNationId = 1;

        protected override void OnSingletonAwake()
        {
            Debug.Log("[GameManager] 遊戲管理器初始化");
        }

        private void Start()
        {
            if (autoInitialize)
            {
                InitializeGame();
            }
        }

        /// <summary>
        /// 初始化遊戲
        /// </summary>
        public void InitializeGame()
        {
            if (IsInitialized)
            {
                Debug.LogWarning("[GameManager] 遊戲已經初始化");
                return;
            }

            Debug.Log("[GameManager] 開始初始化遊戲...");

            // 確保所有 Manager 已創建
            EnsureManagers();

            // 創建測試玩家數據
            CreateTestPlayer();

            // 初始化各個子系統
            InitializeSubsystems();

            IsInitialized = true;
            Debug.Log("[GameManager] 遊戲初始化完成！");

            // 發送初始化完成事件
            EventManager.Instance.Publish(new GameInitializedEvent());
        }

        /// <summary>
        /// 確保所有 Manager 已創建
        /// </summary>
        private void EnsureManagers()
        {
            // 訪問 Instance 會自動創建
            _ = EventManager.Instance;
            
            // 初始化 UIManager 層級
            var uiManager = UIManager.Instance;
            if (uiManager != null)
            {
                var mainCanvas = GameObject.Find("MainCanvas");
                if (mainCanvas != null)
                {
                    var normalLayer = mainCanvas.transform.Find("NormalLayer");
                    var popupLayer = mainCanvas.transform.Find("PopupLayer");
                    var topLayer = mainCanvas.transform.Find("TopLayer");
                    
                    if (normalLayer != null && popupLayer != null && topLayer != null)
                    {
                        uiManager.SetupLayers(normalLayer, popupLayer, topLayer);
                        Debug.Log("[GameManager] UIManager 層級設置完成");
                    }
                }
            }
            
            _ = ResourceManager.Instance;
            _ = TerritoryManager.Instance;
            _ = ArmyManager.Instance;
            _ = GeneralManager.Instance;
            _ = MapManager.Instance;
            _ = NationManager.Instance;
            _ = BattleManager.Instance;

            Debug.Log("[GameManager] 所有 Manager 初始化完成");
        }

        /// <summary>
        /// 創建測試玩家數據
        /// </summary>
        private void CreateTestPlayer()
        {
            long playerId = System.DateTime.Now.Ticks;
            CurrentPlayer = new PlayerFullData(playerId, testPlayerName, testNationId);

            // 設置初始資源
            CurrentPlayer.Resources.Copper = 5000;
            CurrentPlayer.Resources.Wood = 2000;
            CurrentPlayer.Resources.Stone = 2000;
            CurrentPlayer.Resources.Food = 3000;

            Debug.Log($"[GameManager] 創建測試玩家: {testPlayerName}");
        }

        /// <summary>
        /// 初始化子系統
        /// </summary>
        private void InitializeSubsystems()
        {
            // 初始化資源管理器
            ResourceManager.Instance.Initialize(CurrentPlayer.Resources);

            // 初始化領地管理器
            TerritoryManager.Instance.Initialize(CurrentPlayer.Territories);

            // 初始化軍隊管理器
            ArmyManager.Instance.Initialize(CurrentPlayer.Army);

            // 初始化將領管理器
            GeneralManager.Instance.Initialize(CurrentPlayer.Generals);

            // 初始化地圖管理器
            MapManager.Instance.Initialize();

            // 初始化國家管理器
            NationManager.Instance.Initialize();

            // 設置玩家所屬國家（測試用，預設蜀國）
            NationManager.Instance.SetPlayerNation("nation_shu");
        }

        /// <summary>
        /// 獲取玩家名稱
        /// </summary>
        public string GetPlayerName()
        {
            return CurrentPlayer?.BasicInfo?.Name ?? "未知玩家";
        }

        /// <summary>
        /// 獲取玩家等級
        /// </summary>
        public int GetPlayerLevel()
        {
            return CurrentPlayer?.BasicInfo?.Level ?? 1;
        }

        /// <summary>
        /// 保存遊戲（預留）
        /// </summary>
        public void SaveGame()
        {
            Debug.Log("[GameManager] 保存遊戲... (功能預留)");
            // TODO: 實現本地存檔或伺服器同步
        }

        /// <summary>
        /// 載入遊戲（預留）
        /// </summary>
        public void LoadGame()
        {
            Debug.Log("[GameManager] 載入遊戲... (功能預留)");
            // TODO: 實現本地讀檔或伺服器同步
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveGame();
            }
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }
    }
}

