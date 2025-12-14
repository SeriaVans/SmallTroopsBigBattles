using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SmallTroopsBigBattles.Core.Data;
using SmallTroopsBigBattles.Core.Events;

namespace SmallTroopsBigBattles.Core.Managers
{
    /// <summary>
    /// 將領管理器 - 處理將領招募、升級、編制
    /// </summary>
    public class GeneralManager : Singleton<GeneralManager>
    {
        // 玩家擁有的將領
        private Dictionary<long, GeneralData> _generals = new Dictionary<long, GeneralData>();
        private long _nextGeneralId = 1;

        // 事件
        public event Action<GeneralData> OnGeneralObtained;
        public event Action<GeneralData> OnGeneralLevelUp;
        public event Action<long> OnGeneralDismissed;

        protected override void OnSingletonAwake()
        {
            // 初始化
        }

        /// <summary>
        /// 新增將領 (招募/獎勵獲得)
        /// </summary>
        public GeneralData AddGeneral(string name, int rarity, GeneralClass generalClass,
            float strength, float intelligence, float command, float speed)
        {
            var general = new GeneralData
            {
                GeneralId = _nextGeneralId++,
                Name = name,
                Rarity = Mathf.Clamp(rarity, 1, 5),
                Class = generalClass,
                Level = 1,
                Exp = 0,
                Strength = strength,
                Intelligence = intelligence,
                Command = command,
                Speed = speed
            };

            _generals[general.GeneralId] = general;

            Debug.Log($"[GeneralManager] 獲得將領: {general.Name} ({general.Rarity}星 {general.Class})");

            OnGeneralObtained?.Invoke(general);
            EventManager.Instance.Publish(new GeneralObtainedEvent { General = general });

            return general;
        }

        /// <summary>
        /// 從模板建立將領
        /// </summary>
        public GeneralData AddGeneralFromTemplate(GeneralTemplate template)
        {
            // 根據稀有度計算屬性波動 (50%-80% 基於稀有度)
            float rarityFactor = 0.5f + (template.Rarity - 1) * 0.075f; // 1星=50%, 5星=80%

            return AddGeneral(
                template.Name,
                template.Rarity,
                template.Class,
                template.BaseStrength * rarityFactor,
                template.BaseIntelligence * rarityFactor,
                template.BaseCommand * rarityFactor,
                template.BaseSpeed * rarityFactor
            );
        }

        /// <summary>
        /// 取得所有將領
        /// </summary>
        public List<GeneralData> GetAllGenerals()
        {
            return _generals.Values.ToList();
        }

        /// <summary>
        /// 取得特定將領
        /// </summary>
        public GeneralData GetGeneral(long generalId)
        {
            return _generals.TryGetValue(generalId, out var general) ? general : null;
        }

        /// <summary>
        /// 取得特定職業的將領
        /// </summary>
        public List<GeneralData> GetGeneralsByClass(GeneralClass generalClass)
        {
            return _generals.Values.Where(g => g.Class == generalClass).ToList();
        }

        /// <summary>
        /// 增加經驗值
        /// </summary>
        public void AddExperience(long generalId, int exp)
        {
            var general = GetGeneral(generalId);
            if (general == null) return;

            if (general.Level >= GeneralData.MaxLevel)
            {
                Debug.Log("[GeneralManager] 將領已達最高等級");
                return;
            }

            general.Exp += exp;

            // 檢查升級
            while (general.Exp >= GetExpForNextLevel(general.Level) &&
                   general.Level < GeneralData.MaxLevel)
            {
                general.Exp -= GetExpForNextLevel(general.Level);
                LevelUp(general);
            }
        }

        private void LevelUp(GeneralData general)
        {
            general.Level++;

            // 升級時屬性成長
            float growthRate = GetGrowthRate(general.Rarity);
            general.Strength += growthRate;
            general.Intelligence += growthRate;
            general.Command += growthRate;
            general.Speed += growthRate;

            Debug.Log($"[GeneralManager] {general.Name} 升級至 Lv.{general.Level}");

            OnGeneralLevelUp?.Invoke(general);
        }

        /// <summary>
        /// 取得升級所需經驗
        /// </summary>
        public int GetExpForNextLevel(int currentLevel)
        {
            // 經驗公式: 100 * level * (1 + level * 0.1)
            return Mathf.RoundToInt(100 * currentLevel * (1 + currentLevel * 0.1f));
        }

        /// <summary>
        /// 取得成長率 (基於稀有度)
        /// </summary>
        private float GetGrowthRate(int rarity)
        {
            return rarity switch
            {
                1 => 1.0f,
                2 => 1.5f,
                3 => 2.0f,
                4 => 2.5f,
                5 => 3.0f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// 遣散將領
        /// </summary>
        public bool DismissGeneral(long generalId)
        {
            if (!_generals.ContainsKey(generalId))
            {
                Debug.LogWarning("[GeneralManager] 找不到將領");
                return false;
            }

            var general = _generals[generalId];
            _generals.Remove(generalId);

            Debug.Log($"[GeneralManager] 遣散將領: {general.Name}");
            OnGeneralDismissed?.Invoke(generalId);

            return true;
        }

        /// <summary>
        /// 取得將領數量
        /// </summary>
        public int GetGeneralCount()
        {
            return _generals.Count;
        }

        /// <summary>
        /// 計算將領戰力
        /// </summary>
        public int CalculatePower(GeneralData general)
        {
            float basePower = general.Strength + general.Intelligence +
                              general.Command + general.Speed;
            float levelBonus = general.Level * 10;
            float rarityBonus = general.Rarity * 50;

            return Mathf.RoundToInt(basePower + levelBonus + rarityBonus);
        }
    }

    /// <summary>
    /// 將領模板 (用於配置)
    /// </summary>
    [Serializable]
    public class GeneralTemplate
    {
        public string TemplateId;
        public string Name;
        public int Rarity;
        public GeneralClass Class;
        public float BaseStrength;
        public float BaseIntelligence;
        public float BaseCommand;
        public float BaseSpeed;
    }
}
