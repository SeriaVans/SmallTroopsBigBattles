using System;
using System.Collections.Generic;
using SmallTroopsBigBattles.Core.Events;

namespace SmallTroopsBigBattles.Game.Data
{
    /// <summary>
    /// 將領數據
    /// </summary>
    [Serializable]
    public class GeneralData
    {
        public long GeneralId;
        public string Name;
        public int Rarity;              // 稀有度: 1-5星
        public GeneralClass Class;      // 職業類型

        /// <summary>
        /// 等級上限
        /// </summary>
        public const int MaxLevel = 50;

        /// <summary>
        /// 最高星級
        /// </summary>
        public const int MaxStars = 5;

        // 等級和升星
        public int Level = 1;           // 等級 (1-50)
        public int Stars = 1;           // 升星 (1-5)
        public int Experience;          // 當前經驗值

        // 四維屬性
        public float Strength;          // 武力 - 影響部隊攻擊
        public float Intelligence;      // 智力 - 影響技能效果
        public float Command;           // 統帥 - 影響帶兵上限
        public float Speed;             // 速度 - 影響行軍/攻擊速度

        // 擅長兵種
        public List<SoldierType> Proficiency = new();

        /// <summary>
        /// 帶兵上限 = 500 + (統帥 × 10)
        /// </summary>
        public int MaxTroops => 500 + (int)(Command * 10);

        /// <summary>
        /// 戰鬥力計算
        /// </summary>
        public int Power => (int)((Strength + Intelligence + Command + Speed) * Level * (1 + (Stars - 1) * 0.1f));

        /// <summary>
        /// 是否可以升級
        /// </summary>
        public bool CanLevelUp => Level < MaxLevel && Level < Stars * 10;

        /// <summary>
        /// 是否可以升星
        /// </summary>
        public bool CanStarUp => Stars < MaxStars;

        /// <summary>
        /// 創建一個隨機將領
        /// </summary>
        public static GeneralData CreateRandom(int rarity, GeneralClass generalClass)
        {
            var general = new GeneralData
            {
                GeneralId = DateTime.Now.Ticks,
                Rarity = Math.Clamp(rarity, 1, 5),
                Class = generalClass,
                Level = 1,
                Stars = 1
            };

            // 根據稀有度生成基礎屬性
            float baseValue = 10 + (rarity * 5);
            var random = new Random();

            general.Strength = baseValue + random.Next(0, 10);
            general.Intelligence = baseValue + random.Next(0, 10);
            general.Command = baseValue + random.Next(0, 10);
            general.Speed = baseValue + random.Next(0, 10);

            // 根據職業設置擅長兵種
            switch (generalClass)
            {
                case GeneralClass.Commander:
                    general.Proficiency.Add(SoldierType.Spearman);
                    general.Proficiency.Add(SoldierType.Shieldman);
                    general.Strength += 5;  // 統帥額外武力
                    general.Command += 5;   // 統帥額外統帥
                    break;
                case GeneralClass.Vanguard:
                    general.Proficiency.Add(SoldierType.Cavalry);
                    general.Strength += 8;  // 先鋒額外武力
                    general.Speed += 5;     // 先鋒額外速度
                    break;
                case GeneralClass.Strategist:
                    general.Proficiency.Add(SoldierType.Archer);
                    general.Intelligence += 8;  // 軍師額外智力
                    break;
            }

            // 生成名字
            general.Name = GenerateRandomName(generalClass);

            return general;
        }

        /// <summary>
        /// 生成隨機名字
        /// </summary>
        private static string GenerateRandomName(GeneralClass generalClass)
        {
            string[] surnames = { "張", "王", "李", "趙", "劉", "陳", "楊", "黃", "周", "吳" };
            string[] commanderNames = { "飛", "雲", "虎", "龍", "威", "武", "剛", "勇" };
            string[] vanguardNames = { "風", "雷", "電", "閃", "疾", "迅", "奔", "馳" };
            string[] strategistNames = { "謀", "智", "策", "文", "思", "明", "睿", "哲" };

            var random = new Random();
            string surname = surnames[random.Next(surnames.Length)];

            string[] names = generalClass switch
            {
                GeneralClass.Commander => commanderNames,
                GeneralClass.Vanguard => vanguardNames,
                GeneralClass.Strategist => strategistNames,
                _ => commanderNames
            };

            string name = names[random.Next(names.Length)];
            return surname + name;
        }

        /// <summary>
        /// 增加經驗值
        /// </summary>
        /// <returns>是否升級</returns>
        public bool AddExperience(int amount)
        {
            Experience += amount;
            int expNeeded = GetExpForNextLevel();

            bool leveledUp = false;
            while (Experience >= expNeeded && CanLevelUp)
            {
                Experience -= expNeeded;
                Level++;
                leveledUp = true;
                expNeeded = GetExpForNextLevel();
            }

            return leveledUp;
        }

        /// <summary>
        /// 獲取升到下一級所需經驗
        /// </summary>
        public int GetExpForNextLevel()
        {
            return Level * 100;
        }

        /// <summary>
        /// 升星
        /// </summary>
        public bool TryStarUp()
        {
            if (!CanStarUp) return false;

            Stars++;
            // 升星後屬性提升 10%
            Strength *= 1.1f;
            Intelligence *= 1.1f;
            Command *= 1.1f;
            Speed *= 1.1f;

            return true;
        }

        /// <summary>
        /// 計算對特定兵種的加成
        /// </summary>
        public float GetBonusForSoldierType(SoldierType type)
        {
            float bonus = 1f + (Strength * 0.01f);  // 基礎加成

            if (Proficiency.Contains(type))
            {
                bonus += 0.15f;  // 擅長兵種額外 15% 加成
            }

            return bonus;
        }
    }
}

