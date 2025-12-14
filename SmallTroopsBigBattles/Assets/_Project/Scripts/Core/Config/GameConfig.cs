using UnityEngine;

namespace SmallTroopsBigBattles.Core.Config
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "SmallTroopsBigBattles/Config/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("遊戲設定")]
        public int TargetFrameRate = 60;
        public string GameVersion = "0.1.0";

        [Header("玩家設定")]
        public int MaxTerritories = 3;
        public int MaxSoldiers = 5000;
        public int StartingCopper = 1000;
        public int StartingWood = 500;
        public int StartingStone = 500;
        public int StartingFood = 1000;

        [Header("領地設定")]
        public int BaseBuildingSlots = 15;
        public int MaxBuildingSlots = 20;

        [Header("將領設定")]
        public int MaxGeneralLevel = 50;
        public float RarityInfluenceMin = 0.5f;
        public float RarityInfluenceMax = 0.8f;

        [Header("戰鬥設定")]
        public int MaxSoldiersPerNation = 1000;
        public float SpectateDelay = 1.5f;

        private static GameConfig _instance;
        public static GameConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<GameConfig>("Config/GameConfig");
                    if (_instance == null) _instance = CreateInstance<GameConfig>();
                }
                return _instance;
            }
        }
    }
}
