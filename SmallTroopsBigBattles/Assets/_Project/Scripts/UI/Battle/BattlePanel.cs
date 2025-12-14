using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmallTroopsBigBattles.Core.Events;
using SmallTroopsBigBattles.Game.Battle;

namespace SmallTroopsBigBattles.UI.Battle
{
    /// <summary>
    /// 戰鬥面板 - 觀戰介面
    /// </summary>
    public class BattlePanel : BasePanel
    {
        [Header("頂部資訊")]
        [SerializeField] private TextMeshProUGUI attackerNameText;
        [SerializeField] private TextMeshProUGUI attackerCountText;
        [SerializeField] private Image attackerHealthBar;
        [SerializeField] private TextMeshProUGUI defenderNameText;
        [SerializeField] private TextMeshProUGUI defenderCountText;
        [SerializeField] private Image defenderHealthBar;

        [Header("城牆狀態")]
        [SerializeField] private GameObject wallStatusPanel;
        [SerializeField] private Image wallHealthBar;
        [SerializeField] private TextMeshProUGUI wallHealthText;

        [Header("戰場視圖")]
        [SerializeField] private RectTransform battlefieldViewport;
        [SerializeField] private RectTransform battlefieldContent;
        [SerializeField] private BattlefieldView battlefieldView;

        [Header("我的部隊面板")]
        [SerializeField] private GameObject myTroopsPanel;
        [SerializeField] private TextMeshProUGUI myOnFieldText;
        [SerializeField] private TextMeshProUGUI myWaitingText;
        [SerializeField] private TextMeshProUGUI mySpearmanText;
        [SerializeField] private TextMeshProUGUI myShieldmanText;
        [SerializeField] private TextMeshProUGUI myCavalryText;
        [SerializeField] private TextMeshProUGUI myArcherText;

        [Header("底部按鈕")]
        [SerializeField] private Button reinforceButton;
        [SerializeField] private Button retreatButton;
        [SerializeField] private Button exitButton;

        [Header("戰鬥結果")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultTitleText;
        [SerializeField] private TextMeshProUGUI resultDetailText;

        /// <summary>當前觀戰的戰場</summary>
        private Battlefield _currentBattlefield;

        /// <summary>當前玩家 ID</summary>
        private string _playerId;

        /// <summary>當前玩家國家 ID</summary>
        private string _playerNationId;

        /// <summary>更新計時器</summary>
        private float _updateTimer;

        /// <summary>更新間隔</summary>
        private const float UpdateInterval = 0.2f;

        protected override void Awake()
        {
            base.Awake();

            // 綁定按鈕事件
            if (reinforceButton != null)
                reinforceButton.onClick.AddListener(OnReinforceClick);

            if (retreatButton != null)
                retreatButton.onClick.AddListener(OnRetreatClick);

            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClick);
        }

        private void Start()
        {
            // 訂閱事件
            EventManager.Instance?.Subscribe<BattleEndedEvent>(OnBattleEnded);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventManager.Instance?.Unsubscribe<BattleEndedEvent>(OnBattleEnded);
        }

        private void Update()
        {
            if (_currentBattlefield == null || !gameObject.activeInHierarchy)
                return;

            _updateTimer += Time.deltaTime;
            if (_updateTimer >= UpdateInterval)
            {
                RefreshUI();
                _updateTimer = 0;
            }
        }

        /// <summary>
        /// 開啟戰鬥面板
        /// </summary>
        public void OpenBattle(Battlefield battlefield, string playerId, string nationId)
        {
            _currentBattlefield = battlefield;
            _playerId = playerId;
            _playerNationId = nationId;

            // 初始化戰場視圖
            if (battlefieldView != null)
            {
                battlefieldView.Initialize(battlefield);
            }

            // 隱藏結果面板
            if (resultPanel != null)
                resultPanel.SetActive(false);

            RefreshUI();
            Show();
        }

        /// <summary>
        /// 刷新 UI
        /// </summary>
        private void RefreshUI()
        {
            if (_currentBattlefield == null) return;

            RefreshFactionInfo();
            RefreshWallStatus();
            RefreshMyTroops();
            RefreshButtons();
        }

        /// <summary>
        /// 刷新陣營資訊
        /// </summary>
        private void RefreshFactionInfo()
        {
            var attacker = _currentBattlefield.GetAttacker();
            var defender = _currentBattlefield.GetDefender();

            // 攻方
            if (attacker != null)
            {
                if (attackerNameText != null)
                    attackerNameText.text = GetNationDisplayName(attacker.NationId);

                if (attackerCountText != null)
                    attackerCountText.text = $"{attacker.AliveSoldiers} / {attacker.TotalSoldiers}";

                if (attackerHealthBar != null)
                {
                    float ratio = attacker.TotalSoldiers > 0
                        ? (float)attacker.AliveSoldiers / attacker.TotalSoldiers
                        : 0;
                    attackerHealthBar.fillAmount = ratio;
                }
            }

            // 守方
            if (defender != null)
            {
                if (defenderNameText != null)
                    defenderNameText.text = GetNationDisplayName(defender.NationId);

                if (defenderCountText != null)
                    defenderCountText.text = $"{defender.AliveSoldiers} / {defender.TotalSoldiers}";

                if (defenderHealthBar != null)
                {
                    float ratio = defender.TotalSoldiers > 0
                        ? (float)defender.AliveSoldiers / defender.TotalSoldiers
                        : 0;
                    defenderHealthBar.fillAmount = ratio;
                }
            }
        }

        /// <summary>
        /// 刷新城牆狀態
        /// </summary>
        private void RefreshWallStatus()
        {
            if (wallStatusPanel == null) return;

            wallStatusPanel.SetActive(_currentBattlefield.WallMaxHp > 0);

            if (wallHealthBar != null)
            {
                float ratio = _currentBattlefield.WallMaxHp > 0
                    ? _currentBattlefield.WallCurrentHp / _currentBattlefield.WallMaxHp
                    : 0;
                wallHealthBar.fillAmount = ratio;
            }

            if (wallHealthText != null)
            {
                int percent = Mathf.RoundToInt(
                    (_currentBattlefield.WallCurrentHp / _currentBattlefield.WallMaxHp) * 100);
                wallHealthText.text = $"{percent}%";
            }
        }

        /// <summary>
        /// 刷新我的部隊資訊
        /// </summary>
        private void RefreshMyTroops()
        {
            if (myTroopsPanel == null) return;

            var status = BattleManager.Instance?.GetPlayerBattleStatus(
                _currentBattlefield.BattlefieldId, _playerId);

            if (status == null)
            {
                myTroopsPanel.SetActive(false);
                return;
            }

            myTroopsPanel.SetActive(true);

            if (myOnFieldText != null)
                myOnFieldText.text = $"戰場中: {status.TotalOnField}";

            if (myWaitingText != null)
                myWaitingText.text = $"待上陣: {status.TotalWaiting}";

            // 各兵種數量
            UpdateSoldierTypeText(mySpearmanText, Game.Battle.SoldierType.Spearman, status);
            UpdateSoldierTypeText(myShieldmanText, Game.Battle.SoldierType.Shieldman, status);
            UpdateSoldierTypeText(myCavalryText, Game.Battle.SoldierType.Cavalry, status);
            UpdateSoldierTypeText(myArcherText, Game.Battle.SoldierType.Archer, status);
        }

        /// <summary>
        /// 更新兵種數量文字
        /// </summary>
        private void UpdateSoldierTypeText(TextMeshProUGUI text, Game.Battle.SoldierType type, PlayerBattleStatus status)
        {
            if (text == null) return;

            int onField = status.OnFieldSoldiers.TryGetValue(type, out var of) ? of : 0;
            int waiting = status.WaitingSoldiers.TryGetValue(type, out var w) ? w : 0;

            string typeName = type switch
            {
                Game.Battle.SoldierType.Spearman => "槍兵",
                Game.Battle.SoldierType.Shieldman => "盾兵",
                Game.Battle.SoldierType.Cavalry => "騎兵",
                Game.Battle.SoldierType.Archer => "弓兵",
                _ => type.ToString()
            };

            text.text = $"{typeName}: {onField} (+{waiting})";
        }

        /// <summary>
        /// 刷新按鈕狀態
        /// </summary>
        private void RefreshButtons()
        {
            bool isParticipating = _currentBattlefield.GetFaction(_playerNationId) != null;
            bool isFighting = _currentBattlefield.State == BattleState.Fighting;

            if (reinforceButton != null)
                reinforceButton.interactable = isParticipating && isFighting;

            if (retreatButton != null)
                retreatButton.interactable = isParticipating && isFighting;
        }

        /// <summary>
        /// 獲取國家顯示名稱
        /// </summary>
        private string GetNationDisplayName(string nationId)
        {
            return nationId switch
            {
                "nation_shu" => "蜀國",
                "nation_wei" => "魏國",
                "nation_wu" => "吳國",
                _ => nationId
            };
        }

        #region 按鈕事件

        /// <summary>
        /// 增援按鈕點擊
        /// </summary>
        private void OnReinforceClick()
        {
            Debug.Log("[BattlePanel] 增援功能開發中...");
            // TODO: 打開增援選擇介面
        }

        /// <summary>
        /// 撤退按鈕點擊
        /// </summary>
        private void OnRetreatClick()
        {
            if (_currentBattlefield == null) return;

            BattleManager.Instance?.PlayerRetreat(_currentBattlefield.BattlefieldId, _playerId);
        }

        /// <summary>
        /// 離開按鈕點擊
        /// </summary>
        private void OnExitClick()
        {
            BattleManager.Instance?.StopSpectating();
            Hide();
        }

        #endregion

        #region 事件處理

        /// <summary>
        /// 戰鬥結束事件
        /// </summary>
        private void OnBattleEnded(BattleEndedEvent evt)
        {
            if (_currentBattlefield?.BattlefieldId != evt.Battlefield.BattlefieldId)
                return;

            ShowResult(evt.Result);
        }

        /// <summary>
        /// 顯示戰鬥結果
        /// </summary>
        private void ShowResult(BattleResult result)
        {
            if (resultPanel == null) return;

            resultPanel.SetActive(true);

            string title = result switch
            {
                BattleResult.AttackerWin => "攻方勝利",
                BattleResult.DefenderWin => "守方勝利",
                BattleResult.ThirdPartyWin => "第三方勝利",
                BattleResult.Draw => "平局",
                _ => "戰鬥結束"
            };

            if (resultTitleText != null)
                resultTitleText.text = title;

            // 生成詳細資訊
            if (resultDetailText != null)
            {
                var attacker = _currentBattlefield.GetAttacker();
                var defender = _currentBattlefield.GetDefender();

                string detail = $"戰鬥時長: {FormatDuration(_currentBattlefield.Duration)}\n\n";

                if (attacker != null)
                {
                    detail += $"攻方 ({GetNationDisplayName(attacker.NationId)})\n";
                    detail += $"  投入: {attacker.TotalSoldiers}  陣亡: {attacker.DeadSoldiers}\n\n";
                }

                if (defender != null)
                {
                    detail += $"守方 ({GetNationDisplayName(defender.NationId)})\n";
                    detail += $"  投入: {defender.TotalSoldiers}  陣亡: {defender.DeadSoldiers}\n";
                }

                resultDetailText.text = detail;
            }
        }

        /// <summary>
        /// 格式化持續時間
        /// </summary>
        private string FormatDuration(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60);
            int secs = Mathf.FloorToInt(seconds % 60);
            return $"{minutes}分{secs}秒";
        }

        #endregion
    }
}

