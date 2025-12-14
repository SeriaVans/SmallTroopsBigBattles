using UnityEngine;
using UnityEngine.UI;

namespace SmallTroopsBigBattles.UI
{
    public class SettingsPanel : BasePanel
    {
        [Header("音效設定")]
        [SerializeField] private Slider _bgmVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;

        [Header("操作按鈕")]
        [SerializeField] private Button _saveButton;

        protected override void Awake()
        {
            base.Awake();
            if (_bgmVolumeSlider)
            {
                _bgmVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
                _bgmVolumeSlider.onValueChanged.AddListener(v => PlayerPrefs.SetFloat("BGMVolume", v));
            }
            if (_sfxVolumeSlider)
            {
                _sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
                _sfxVolumeSlider.onValueChanged.AddListener(v => PlayerPrefs.SetFloat("SFXVolume", v));
            }
            if (_saveButton) _saveButton.onClick.AddListener(() => { PlayerPrefs.Save(); UIManager.Instance.ClosePanel<SettingsPanel>(); });
        }
    }
}
