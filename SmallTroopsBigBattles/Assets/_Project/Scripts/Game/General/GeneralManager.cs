using System.Collections.Generic;
using UnityEngine;
using SmallTroopsBigBattles.Core;
using SmallTroopsBigBattles.Core.Events;
using SmallTroopsBigBattles.Game.Data;

namespace SmallTroopsBigBattles.Game.General
{
    /// <summary>
    /// 將領管理器 - 負責將領的招募與管理
    /// </summary>
    public class GeneralManager : SingletonBase<GeneralManager>
    {
        /// <summary>
        /// 玩家擁有的將領列表
        /// </summary>
        public List<GeneralData> Generals { get; private set; } = new();

        /// <summary>
        /// 當前選中的將領
        /// </summary>
        public GeneralData SelectedGeneral { get; private set; }

        protected override void OnSingletonAwake()
        {
            Debug.Log("[GeneralManager] 將領管理器初始化完成");
        }

        /// <summary>
        /// 初始化將領列表
        /// </summary>
        public void Initialize(List<GeneralData> generals)
        {
            Generals = generals ?? new List<GeneralData>();

            // 如果沒有將領，給予一個初始將領
            if (Generals.Count == 0)
            {
                var starterGeneral = GeneralData.CreateRandom(2, GeneralClass.Commander);
                starterGeneral.Name = "初始統帥";
                AddGeneral(starterGeneral);
            }

            Debug.Log($"[GeneralManager] 載入 {Generals.Count} 名將領");
        }

        /// <summary>
        /// 添加將領
        /// </summary>
        public void AddGeneral(GeneralData general)
        {
            Generals.Add(general);
            EventManager.Instance.Publish(new GeneralObtainedEvent(general.GeneralId));
            Debug.Log($"[GeneralManager] 獲得將領: {general.Name} ({general.Rarity}★ {GetClassDisplayName(general.Class)})");
        }

        /// <summary>
        /// 選擇將領
        /// </summary>
        public void SelectGeneral(long generalId)
        {
            SelectedGeneral = Generals.Find(g => g.GeneralId == generalId);
        }

        /// <summary>
        /// 獲取將領
        /// </summary>
        public GeneralData GetGeneral(long generalId)
        {
            return Generals.Find(g => g.GeneralId == generalId);
        }

        /// <summary>
        /// 普通招募（消耗銅錢）
        /// </summary>
        public GeneralData NormalRecruit()
        {
            const int cost = 1000;
            if (!Resource.ResourceManager.Instance.HasEnoughResource(ResourceType.Copper, cost))
            {
                Debug.LogWarning("[GeneralManager] 銅錢不足，無法招募");
                return null;
            }

            Resource.ResourceManager.Instance.ConsumeResource(ResourceType.Copper, cost);

            // 隨機生成 1-3 星將領
            int rarity = Random.Range(1, 4);
            var generalClass = (GeneralClass)Random.Range(0, 3);
            var general = GeneralData.CreateRandom(rarity, generalClass);

            AddGeneral(general);
            return general;
        }

        /// <summary>
        /// 高級招募（消耗招募令，暫用銅錢代替）
        /// </summary>
        public GeneralData PremiumRecruit()
        {
            const int cost = 5000;
            if (!Resource.ResourceManager.Instance.HasEnoughResource(ResourceType.Copper, cost))
            {
                Debug.LogWarning("[GeneralManager] 銅錢不足，無法高級招募");
                return null;
            }

            Resource.ResourceManager.Instance.ConsumeResource(ResourceType.Copper, cost);

            // 隨機生成 3-5 星將領
            int rarity = Random.Range(3, 6);
            var generalClass = (GeneralClass)Random.Range(0, 3);
            var general = GeneralData.CreateRandom(rarity, generalClass);

            AddGeneral(general);
            return general;
        }

        /// <summary>
        /// 將領升級
        /// </summary>
        public bool LevelUpGeneral(long generalId, int expAmount)
        {
            var general = GetGeneral(generalId);
            if (general == null) return false;

            bool leveledUp = general.AddExperience(expAmount);
            if (leveledUp)
            {
                EventManager.Instance.Publish(new GeneralLevelUpEvent(generalId, general.Level));
                Debug.Log($"[GeneralManager] 將領 {general.Name} 升級至 Lv{general.Level}");
            }

            return leveledUp;
        }

        /// <summary>
        /// 遣散將領
        /// </summary>
        public bool DismissGeneral(long generalId)
        {
            var general = GetGeneral(generalId);
            if (general == null) return false;

            // 返還部分資源
            int returnCopper = general.Rarity * 100;
            Resource.ResourceManager.Instance.AddResource(ResourceType.Copper, returnCopper);

            Generals.Remove(general);
            Debug.Log($"[GeneralManager] 遣散將領 {general.Name}，返還 {returnCopper} 銅錢");
            return true;
        }

        /// <summary>
        /// 獲取職業顯示名稱
        /// </summary>
        public static string GetClassDisplayName(GeneralClass generalClass)
        {
            return generalClass switch
            {
                GeneralClass.Commander => "統帥",
                GeneralClass.Vanguard => "先鋒",
                GeneralClass.Strategist => "軍師",
                _ => "未知"
            };
        }

        /// <summary>
        /// 獲取職業描述
        /// </summary>
        public static string GetClassDescription(GeneralClass generalClass)
        {
            return generalClass switch
            {
                GeneralClass.Commander => "擅長指揮步兵（槍兵、盾兵）",
                GeneralClass.Vanguard => "擅長帶領騎兵衝鋒",
                GeneralClass.Strategist => "擅長運用弓兵遠程攻擊",
                _ => ""
            };
        }
    }
}

