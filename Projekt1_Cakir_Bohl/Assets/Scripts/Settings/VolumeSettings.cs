using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


// https://www.youtube.com/watch?v=G-JUp8AMEx0
public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _effectsSlider;
    private string _musicVolume = "MusicVolume";
    private string _effectsVolume = "EffectsVolume";

    void Start()
    {
        LoadVolume();
    }

    public void SetMusicVolume()
    {
        float volume = _musicSlider.value;

        _audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20);

        PlayerPrefs.SetFloat(_musicVolume, volume);
    }

    public void SetEffectsVolume()
    {
        float volume = _effectsSlider.value;

        _audioMixer.SetFloat("Effects", Mathf.Log10(volume) * 20);

        PlayerPrefs.SetFloat(_effectsVolume, volume);
    }

    private void LoadVolume()
    {
        if (PlayerPrefs.HasKey(_musicVolume))
        {
            _musicSlider.value = PlayerPrefs.GetFloat(_musicVolume);
        }
        else
        {
            SetMusicVolume();           
        }

        if (PlayerPrefs.HasKey(_effectsVolume))
        {
            _effectsSlider.value = PlayerPrefs.GetFloat(_effectsVolume);
        }
        else
        {
            SetEffectsVolume();          
        }
    }
}
