using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SmallTroopsBigBattles.UI
{
    /// <summary>
    /// 設定面板
    /// </summary>
    public class SettingsPanel : BasePanel
    {
        [Header("音效設定")]
        [SerializeField] private Slider _bgmVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private Toggle _bgmMuteToggle;
        [SerializeField] private Toggle _sfxMuteToggle;

        [Header("顯示設定")]
        [SerializeField] private TMP_Dropdown _qualityDropdown;
        [SerializeField] private Toggle _showDamageNumberToggle;

        [Header("操作按鈕")]
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _logoutButton;

        protected override void Awake()
        {
            base.Awake();
            SetupControls();
        }

        private void SetupControls()
        {
            if (_bgmVolumeSlider != null)
            {
                _bgmVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
                _bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }

            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
                _sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            if (_saveButton != null)
                _saveButton.onClick.AddListener(OnSaveClicked);

            if (_logoutButton != null)
                _logoutButton.onClick.AddListener(OnLogoutClicked);
        }

        protected override void OnPanelOpened()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            // 載入已儲存的設定
            if (_bgmVolumeSlider != null)
                _bgmVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);

            if (_sfxVolumeSlider != null)
                _sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        }

        private void OnBGMVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("BGMVolume", value);
            // TODO: 實際調整音量
        }

        private void OnSFXVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("SFXVolume", value);
            // TODO: 實際調整音量
        }

        private void OnSaveClicked()
        {
            PlayerPrefs.Save();
            Debug.Log("[SettingsPanel] 設定已儲存");
            UIManager.Instance.ClosePanel<SettingsPanel>();
        }

        private void OnLogoutClicked()
        {
            // TODO: 登出邏輯
            Debug.Log("[SettingsPanel] 登出");
        }
    }
}
