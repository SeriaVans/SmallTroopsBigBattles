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
        private Dictionary<long, GeneralData> _generals = new Dictionary<long, GeneralData>();
        private long _nextGeneralId = 1;

        public event Action<GeneralData> OnGeneralObtained;
        public event Action<GeneralData> OnGeneralLevelUp;
        public event Action<long> OnGeneralDismissed;

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
            OnGeneralObtained?.Invoke(general);
            EventManager.Instance.Publish(new GeneralObtainedEvent { General = general });
            return general;
        }

        public List<GeneralData> GetAllGenerals() => _generals.Values.ToList();
        public GeneralData GetGeneral(long generalId) => _generals.TryGetValue(generalId, out var g) ? g : null;
        public List<GeneralData> GetGeneralsByClass(GeneralClass c) => _generals.Values.Where(g => g.Class == c).ToList();

        public void AddExperience(long generalId, int exp)
        {
            var general = GetGeneral(generalId);
            if (general == null || general.Level >= GeneralData.MaxLevel) return;

            general.Exp += exp;
            while (general.Exp >= GetExpForNextLevel(general.Level) && general.Level < GeneralData.MaxLevel)
            {
                general.Exp -= GetExpForNextLevel(general.Level);
                LevelUp(general);
            }
        }

        private void LevelUp(GeneralData general)
        {
            general.Level++;
            float growthRate = GetGrowthRate(general.Rarity);
            general.Strength += growthRate;
            general.Intelligence += growthRate;
            general.Command += growthRate;
            general.Speed += growthRate;
            OnGeneralLevelUp?.Invoke(general);
        }

        public int GetExpForNextLevel(int level) => Mathf.RoundToInt(100 * level * (1 + level * 0.1f));

        private float GetGrowthRate(int rarity) => rarity switch { 1 => 1f, 2 => 1.5f, 3 => 2f, 4 => 2.5f, 5 => 3f, _ => 1f };

        public bool DismissGeneral(long generalId)
        {
            if (!_generals.ContainsKey(generalId)) return false;
            _generals.Remove(generalId);
            OnGeneralDismissed?.Invoke(generalId);
            return true;
        }

        public int GetGeneralCount() => _generals.Count;

        public int CalculatePower(GeneralData general)
        {
            float basePower = general.Strength + general.Intelligence + general.Command + general.Speed;
            return Mathf.RoundToInt(basePower + general.Level * 10 + general.Rarity * 50);
        }
    }

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
