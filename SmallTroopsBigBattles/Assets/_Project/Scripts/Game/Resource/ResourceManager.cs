using UnityEngine;
using SmallTroopsBigBattles.Core;
using SmallTroopsBigBattles.Core.Events;
using SmallTroopsBigBattles.Game.Data;

namespace SmallTroopsBigBattles.Game.Resource
{
    /// <summary>
    /// 資源管理器 - 負責玩家資源的管理與產出計算
    /// </summary>
    public class ResourceManager : SingletonBase<ResourceManager>
    {
        /// <summary>
        /// 當前玩家的資源數據
        /// </summary>
        public PlayerResources PlayerResources { get; private set; }

        /// <summary>
        /// 資源產出間隔（秒）
        /// </summary>
        [SerializeField] private float productionInterval = 60f;

        private float _productionTimer;

        protected override void OnSingletonAwake()
        {
            Debug.Log("[ResourceManager] 資源管理器初始化完成");
        }

        /// <summary>
        /// 初始化玩家資源
        /// </summary>
        public void Initialize(PlayerResources resources)
        {
            PlayerResources = resources;
            _productionTimer = 0f;
            Debug.Log($"[ResourceManager] 載入玩家資源 - 銅錢:{resources.Copper} 木材:{resources.Wood} 石頭:{resources.Stone} 糧草:{resources.Food}");
        }

        private void Update()
        {
            if (PlayerResources == null) return;

            // 資源產出計時
            _productionTimer += Time.deltaTime;
            if (_productionTimer >= productionInterval)
            {
                _productionTimer = 0f;
                ProduceResources();
            }
        }

        /// <summary>
        /// 計算並產出資源
        /// </summary>
        private void ProduceResources()
        {
            // TODO: 根據建築等級計算產出
            // 暫時使用固定產出值
            int copperProduction = 10;
            int woodProduction = 5;
            int stoneProduction = 5;
            int foodProduction = 8;

            AddResource(ResourceType.Copper, copperProduction);
            AddResource(ResourceType.Wood, woodProduction);
            AddResource(ResourceType.Stone, stoneProduction);
            AddResource(ResourceType.Food, foodProduction);

            Debug.Log($"[ResourceManager] 資源產出 - 銅錢+{copperProduction} 木材+{woodProduction} 石頭+{stoneProduction} 糧草+{foodProduction}");
        }

        /// <summary>
        /// 獲取指定資源數量
        /// </summary>
        public int GetResource(ResourceType type)
        {
            return PlayerResources?.GetResource(type) ?? 0;
        }

        /// <summary>
        /// 增加資源
        /// </summary>
        public void AddResource(ResourceType type, int amount)
        {
            PlayerResources?.AddResource(type, amount);
        }

        /// <summary>
        /// 消耗資源
        /// </summary>
        public bool ConsumeResource(ResourceType type, int amount)
        {
            return PlayerResources?.ConsumeResource(type, amount) ?? false;
        }

        /// <summary>
        /// 消耗多種資源
        /// </summary>
        public bool ConsumeResources(int copper = 0, int wood = 0, int stone = 0, int food = 0)
        {
            return PlayerResources?.ConsumeResources(copper, wood, stone, food) ?? false;
        }

        /// <summary>
        /// 檢查是否有足夠資源
        /// </summary>
        public bool HasEnoughResource(ResourceType type, int amount)
        {
            return PlayerResources?.HasEnoughResource(type, amount) ?? false;
        }

        /// <summary>
        /// 檢查是否有足夠的多種資源
        /// </summary>
        public bool HasEnoughResources(int copper = 0, int wood = 0, int stone = 0, int food = 0)
        {
            return PlayerResources?.HasEnoughResources(copper, wood, stone, food) ?? false;
        }

        /// <summary>
        /// 獲取資源的顯示名稱
        /// </summary>
        public static string GetResourceDisplayName(ResourceType type)
        {
            return type switch
            {
                ResourceType.Copper => "銅錢",
                ResourceType.Wood => "木材",
                ResourceType.Stone => "石頭",
                ResourceType.Food => "糧草",
                _ => "未知"
            };
        }
    }
}

