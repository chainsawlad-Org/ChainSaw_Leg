using UnityEngine;
using UnityDebugSheet;
using System.Collections;
using UnityEngine.Audio;

public sealed class AudioPage : DefaultDebugPageBase
{
    protected override string Title { get; } = "Audio settings";
    private AudioMixer audioMixer;
    
    public void Initialize(AudioMixer mixer)
    {
        audioMixer = mixer;
        SetVolume("Master", GetVolume("Master"));
        SetVolume("Music", GetVolume("Music"));
        SetVolume("SFX", GetVolume("SFX"));
    }

    public override IEnumerator Initialize()
    {
        float master = GetVolume("Master");
        AddSlider(master, 0f, 1f, "General", valueChanged: x => SetVolume("Master", x));
        
        float music = GetVolume("Music");
        AddSlider(music, 0f, 1f, "Music", valueChanged: x => SetVolume("Music", x));
        
        float sfx = GetVolume("SFX");
        AddSlider(sfx, 0f, 1f, "Effects", valueChanged: x => SetVolume("SFX", x));
        
        ButtonCellModel button = new ButtonCellModel(false);
        button.CellTexts.Text = "Test Audio";
        button.Clicked += () => AudioService.Play(AudioId.muhehehe);
        AddButton(button);
        
        PlayerPrefs.Save();
        
        yield break;
    }

    private void SetVolume(string name, float value)
    {
        float dB = 20f * Mathf.Log10(value);
        audioMixer.SetFloat(name, dB);
        PlayerPrefs.SetFloat(name, value);
    }

    private float GetVolume(string name)
    {
        if (!PlayerPrefs.HasKey(name)) PlayerPrefs.SetFloat(name, 1f);
        return PlayerPrefs.GetFloat(name);
    }
}