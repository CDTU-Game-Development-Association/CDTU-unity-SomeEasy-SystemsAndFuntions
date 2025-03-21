using UnityEngine;
using System.Collections.Generic;
using Managers;
using UnityEngine.UI;
using System;
using System.Collections;
using SaveSystem;
using AudioSettingsSystem = SaveSystem.AudioSettings;

public class AudioManager : Singleton<AudioManager>, ISaveSettings
{
    [Header("音量数据")]
    [SerializeField] private AudioSettingsSO settingsSO;
    private AudioSettingsSystem audioSettings;

    /// <summary>
    /// 设置变更时触发的事件
    /// </summary>
    public event EventHandler SettingsChanged;

    #region 音频源配置
    [Header("音频源/播放器")]
    [Tooltip("背景音乐播放器")]
    [SerializeField] private AudioSource bgmSource;
    [Tooltip("音效播放器列表")]
    private List<AudioSource> sfxSourceList = new();

    [Header("音频剪辑数据")]
    private Dictionary<string, AudioClip> bgmClipDictionary = new();
    private Dictionary<string, AudioClip> sfxClipDictionary = new();

    [System.Serializable]
    public class BGMClipData
    {
        public string name;
        public AudioClip clip;
    }

    [System.Serializable]
    public class SFXClipData
    {
        public string name;
        public AudioClip clip;
    }

    [SerializeField] private List<BGMClipData> audioClipList = new();
    [SerializeField] private List<SFXClipData> sfxClipList = new();

    [Header("音量控制UI")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    // 音量改变事件
    public event EventHandler<EventArgs> OnVolumeChanged;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        audioSettings = new AudioSettingsSystem(settingsSO);
        audioSettings.OnVolumeChanged += HandleVolumeChanged;
        
        // 订阅设置变更事件
        if (audioSettings is ISaveSettings saveSettings)
        {
            saveSettings.SettingsChanged += HandleSettingsChanged;
        }
        
        InitializeAudioSources();
    }

    private void OnDestroy()
    {
        if (audioSettings != null)
        {
            audioSettings.OnVolumeChanged -= HandleVolumeChanged;
            
            // 取消订阅事件，防止内存泄漏
            if (audioSettings is ISaveSettings saveSettings)
            {
                saveSettings.SettingsChanged -= HandleSettingsChanged;
            }
        }
    }
    
    /// <summary>
    /// 处理设置变更事件
    /// </summary>
    private void HandleSettingsChanged(object sender, EventArgs e)
    {
        // 将设置变更事件向上传播
        SettingsChanged?.Invoke(this, e);
    }

    private void Start()
    {
        // 绑定滑动条事件
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(value => audioSettings.MasterVolume = value);
        if (bgmVolumeSlider != null)
            bgmVolumeSlider.onValueChanged.AddListener(value => audioSettings.BGMVolume = value);
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(value => audioSettings.SFXVolume = value);

        Load();
        UpdateSliders();
    }

    private void HandleVolumeChanged()
    {
        UpdateVolumes();
        UpdateSliders();
    }

    private void UpdateSliders()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = audioSettings.MasterVolume;
        if (bgmVolumeSlider != null)
            bgmVolumeSlider.value = audioSettings.BGMVolume;
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = audioSettings.SFXVolume;
    }

    #region 初始化方法
    /// <summary>
    /// 初始化音频源和音频剪辑字典
    /// </summary>
    private void InitializeAudioSources()
    {
        // 初始化BGM字典
        foreach (var clipData in audioClipList)
        {
            if (clipData.clip != null && !string.IsNullOrEmpty(clipData.name))
            {
                bgmClipDictionary[clipData.name] = clipData.clip;
            }
        }

        // 初始化SFX字典
        foreach (var clipData in sfxClipList)
        {
            if (clipData.clip != null && !string.IsNullOrEmpty(clipData.name))
            {
                sfxClipDictionary[clipData.name] = clipData.clip;
            }
        }
    }
    #endregion

    #region 音频播放控制
    /// <summary>
    /// 通过名称播放背景音乐
    /// </summary>
    /// <param name="name">背景音乐名称</param>
    public void PlayBGMByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        if (bgmClipDictionary.TryGetValue(name, out AudioClip clip))
        {
            PlayBGM(clip);
        }
        else
        {
            Debug.LogWarning($"未找到名为 {name} 的背景音乐");
        }
    }

    /// <summary>
    /// 通过名称播放音效
    /// </summary>
    /// <param name="name">音效名称</param>
    public void PlaySFXByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        if (sfxClipDictionary.TryGetValue(name, out AudioClip clip))
        {
            PlaySFX(clip);
        }
        else
        {
            Debug.LogWarning($"未找到名为 {name} 的音效");
        }
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        bgmSource.clip = clip;
        bgmSource.volume = audioSettings.GetActualBGMVolume();
        bgmSource.Play();
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        AudioSource sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.clip = clip;
        sfxSource.volume = audioSettings.GetActualSFXVolume();
        sfxSource.Play();

        sfxSourceList.Add(sfxSource);
        StartCoroutine(RemoveSFXSource(sfxSource, clip.length));
    }

    /// <summary>
    /// 延迟销毁音效音源
    /// </summary>
    private IEnumerator RemoveSFXSource(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (sfxSourceList.Contains(source))
        {
            sfxSourceList.Remove(source);
        }
        Destroy(source);
    }

    /// <summary>
    /// 更新所有音源的音量
    /// </summary>
    private void UpdateVolumes()
    {
        if (audioSettings == null) return;

        if (bgmSource != null && bgmSource.clip != null)
        {
            bgmSource.volume = audioSettings.GetActualBGMVolume();
        }

        foreach (var source in sfxSourceList.ToArray())
        {
            if (source != null)
            {
                source.volume = audioSettings.GetActualSFXVolume();
            }
        }
    }
    #endregion

    #region 保存数据接口
    public void Save()
    {
        // 使用基类的Save方法替代之前的向后兼容方法
        if (audioSettings != null)
        {
            audioSettings.Save();
        }
    }

    public void Load()
    {
        // 使用基类的Load方法替代之前的向后兼容方法
        if (audioSettings != null)
        {
            audioSettings.Load();
        }
    }
    
    public void ResetToDefault()
    {
        audioSettings.ResetToDefault();
    }
    #endregion
}

