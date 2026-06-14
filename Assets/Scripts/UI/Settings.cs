using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{ 
    [SerializeField] private AudioMixer _masterMixer;
    [SerializeField] private string _volumeParameterName;
    [SerializeField] private GameObject _slider, _volumeText;
    private float _volume;
    private bool _isFullScreen = true;
    private string _keyForSave = "setting";
    private void Awake()
    {
        LoadVolume();
    }

    public void SetVolume(float volume)
    {
        _volume = volume;
        _volumeText.GetComponent<TMP_Text>().text = volume.ToString();
    }
    public void ChangeSreenMode(bool _isOn)
    {
        _isFullScreen = _isOn;
    }
    public void AplySettings()
    {
        _masterMixer.SetFloat(_volumeParameterName, _volume - 80);
        if (_isFullScreen)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }

        PlayerPrefs.SetFloat(_keyForSave, _volume);
        PlayerPrefs.Save();
    }
    private void LoadVolume()
    {
        SetVolume(PlayerPrefs.GetFloat(_keyForSave, 1f));
        _slider.GetComponent<Slider>().value = _volume;
        AplySettings();
    }
}
