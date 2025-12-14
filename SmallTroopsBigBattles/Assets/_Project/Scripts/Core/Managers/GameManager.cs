using System;
using UnityEngine;
using SmallTroopsBigBattles.Core.Data;
using SmallTroopsBigBattles.Core.Events;

namespace SmallTroopsBigBattles.Core.Managers
{
    /// <summary>
    /// 遊戲主管理器 - 控制整體遊戲流程
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [Header("遊戲狀態")]
        [SerializeField] private GamePhase _currentPhase = GamePhase.Development;
        [SerializeField] private bool _isInitialized = false;

        // 當前玩家資料
        private PlayerData _currentPlayer;
        public PlayerData CurrentPlayer => _currentPlayer;

        // 遊戲階段
        public GamePhase CurrentPhase => _currentPhase;
        public bool IsInitialized => _isInitialized;

        // 事件
        public event Action<GamePhase> OnPhaseChanged;
        public event Action OnGameInitialized;

        protected override void OnSingletonAwake()
        {
            Application.targetFrameRate = 60;
            InitializeGame();
        }

        private void InitializeGame()
        {
            Debug.Log("[GameManager] 初始化遊戲...");

            // 初始化各個子系統
            InitializeManagers();

            // 建立測試玩家資料 (正式版從伺服器獲取)
            CreateTestPlayerData();

            _isInitialized = true;
            OnGameInitialized?.Invoke();

            Debug.Log("[GameManager] 遊戲初始化完成");
        }

        private void InitializeManagers()
        {
            // 確保各 Manager 被初始化
            var eventManager = EventManager.Instance;
            var resourceManager = ResourceManager.Instance;
            var territoryManager = TerritoryManager.Instance;
            var armyManager = ArmyManager.Instance;

            Debug.Log("[GameManager] 所有 Manager 初始化完成");
        }

        private void CreateTestPlayerData()
        {
            _currentPlayer = new PlayerData
            {
                PlayerId = 1,
                Name = "測試玩家",
                NationId = 1,
                Level = 1,
                JoinTime = DateTime.Now,
                Resources = new PlayerResources
                {
                    Copper = 1000,
                    Wood = 500,
                    Stone = 500,
                    Food = 1000
                },
                Army = new PlayerArmy()
            };

            Debug.Log($"[GameManager] 建立測試玩家: {_currentPlayer.Name}");
        }

        /// <summary>
        /// 設置遊戲階段
        /// </summary>
        public void SetPhase(GamePhase newPhase)
        {
            if (_currentPhase == newPhase) return;

            var oldPhase = _currentPhase;
            _currentPhase = newPhase;

            Debug.Log($"[GameManager] 遊戲階段變更: {oldPhase} -> {newPhase}");
            OnPhaseChanged?.Invoke(newPhase);

            // 發布事件
            EventManager.Instance.Publish(new PhaseChangedEvent
            {
                OldPhase = oldPhase,
                NewPhase = newPhase
            });
        }

        /// <summary>
        /// 檢查是否在特定階段
        /// </summary>
        public bool IsInPhase(GamePhase phase)
        {
            return _currentPhase == phase;
        }

        /// <summary>
        /// 儲存遊戲 (本地暫存)
        /// </summary>
        public void SaveGame()
        {
            // TODO: 實作本地儲存邏輯
            Debug.Log("[GameManager] 遊戲已儲存");
        }

        /// <summary>
        /// 載入遊戲
        /// </summary>
        public void LoadGame()
        {
            // TODO: 實作載入邏輯
            Debug.Log("[GameManager] 遊戲已載入");
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

    /// <summary>
    /// 遊戲階段變更事件
    /// </summary>
    public class PhaseChangedEvent : IGameEvent
    {
        public GamePhase OldPhase;
        public GamePhase NewPhase;
    }
}
