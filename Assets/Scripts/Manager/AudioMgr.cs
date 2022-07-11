using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SoundType
{
    BGM=1,
    AUDIO=2,
    VOICE=3,
}

public class AudioInfo
{
    public int id;
    public string name;
    public SoundType soundType;
    public float volume;
    public bool loop;
}
/// <summary>
/// 声音管理器，初始会加载多个AudioSource，默认是用第一个AudioSource播放BGM
/// </summary>
public class AudioMgr : SingletonMono<AudioMgr>
{
    Dictionary<int, AudioInfo> _resmapSound;    

    private AudioSource[] _audioSources;

    //BGM过渡使用
    private float _fadeTime;
    private float _fadeInTime = 1.0f;
    private float _fadeOutTime = 1.0f;

    private int _nextBgmId;
    private string _nextBgmModule;
    private float _nextBgmVolume;
    private int _nextBgmLoop;

    private float _currBgmVolume;

    private bool _isBgmFadeIn;
    private bool _isBgmFadeOut;

    //当前背景音id和模块名
    public int currBgId;
    public string currModule;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        _audioSources = new AudioSource[GameMain.Inst.AudioLimit];

        for (int i = 0; i < GameMain.Inst.AudioLimit; i++)
        {
            AudioSource audioTmp = gameObject.AddComponent<AudioSource>();
            audioTmp.clip = null;
            audioTmp.volume = 1.0f;
            audioTmp.loop = false;
            _audioSources[i] = audioTmp;
        }
        gameObject.AddComponent<AudioListener>();

        _resmapSound = new Dictionary<int, AudioInfo>();

        OnInit();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _resmapSound.Clear();
    }

    //读取音效数据
    public void OnInit()
    {
        TableHandler table = TableHandler.OpenFromResmap("Audio");
        for (int row = 0; row < table.GetRecordsNum(); row++)
        {
            AudioInfo info = new AudioInfo();
            info.id = Convert.ToInt32(table.GetValue(row, 0));
            info.name = table.GetValue(row, 1);
            string type = table.GetValue(row, 2).Trim();
            if (type.Equals("1"))
                info.soundType = SoundType.BGM;
            else if(type.Equals("3"))
                info.soundType=SoundType.VOICE;
            else 
                info.soundType=SoundType.AUDIO;
            info.volume = Convert.ToSingle(table.GetValue(row, 3));
            info.loop = table.GetValue(row, 4).Trim().Equals("1");
            _resmapSound.Add(info.id, info);
        }
    }

    /// <summary>
    /// 播放声音
    /// </summary>
    /// <param name="loop">0：不循环，1：循环</param>
    public int Play(int id, string module,int loop = -1, float volume = -1f, bool immediate = false)
    {
        if (!_resmapSound.ContainsKey(id))
        {
            Debug.LogErrorFormat("<AudioMgr> 播放音乐错误，表格中不存在id={0}的数据", id);
            return -1;
        }
        if (string.IsNullOrEmpty(module))
        {
            Debug.LogError("<AudioMgr> 资源模块名称不能为空！");
            return -1;
        }
        AudioInfo info = _resmapSound[id];
        AudioClip clip = ResMgr.Inst.LoadAsset<AudioClip>(info.name, 3, module);//先加载资源，放置module不一致时，资源被卸载掉
        int idx = 0;
        switch (info.soundType)
        {
            case SoundType.BGM:
                AudioSource bgmAudioSource = GetAudioSource(idx);
                if (bgmAudioSource.clip==null)//此时第一次播放声音
                {
                    _isBgmFadeIn = true;
                    _nextBgmVolume = volume < 0 ? info.volume : volume;
                    volume = 0;
                    currBgId = id;
                    currModule = module;
                }
                else
                {
                    if (bgmAudioSource.clip.name.Equals(info.name)) return idx; //play一个和当前bgm相同的bgm
                    if (!immediate)
                    {
                        if (_nextBgmId == id) return idx; //短时间多次切换到同一个bgm
                        _isBgmFadeOut = true;
                        _isBgmFadeIn = false;
                        _nextBgmId = id;
                        _nextBgmLoop = loop;
                        _nextBgmVolume = volume < 0 ? info.volume : volume;
                        _nextBgmModule = module;
                        _currBgmVolume = bgmAudioSource.volume;//记录当前BGM的音量
                        currBgId = id;
                        currModule = module;
                        return idx;
                    }
                }
                break;
            case SoundType.AUDIO:
            case SoundType.VOICE:
                idx = GetIdleAudioSource();
                break;
        }
        AudioSource audio = GetAudioSource(idx);
        audio.loop = loop == -1 ? info.loop : loop == 1;
        audio.clip = clip;
        audio.volume = volume < 0 ? info.volume : volume;
        audio.Play();

        return idx;
    }

    /// <summary>
    /// 停止指定声音，默认停止BGM
    /// </summary>
    /// <param name="idx">调用Play时得到的返回值</param>
    /// <param name="soundId">传入soundId后，会对指定的idx的AudioSource进行Check，如果不一致，就不停止</param>
    public void Stop(int idx=0, int soundId = -1)
    {
        AudioSource audio_tmp = GetAudioSource(idx);
        if (soundId != -1)
        {
            //校验不存在，说明该音效已经播放完毕，不需要停止
            AudioInfo info;
            if (!_resmapSound.TryGetValue(soundId, out info))
            {
                Debug.LogErrorFormat("<AudioMgr> 播放音乐错误，表格中不存在id={0}的数据", soundId);
                return;
            }
            if (audio_tmp.clip == null) return;
            if (!audio_tmp.clip.name.Equals(info.name)) return;
        }
        if (audio_tmp)
        {
            audio_tmp.Stop();
        }
    }

    //获取可用的AudioSource
    private int GetIdleAudioSource()
    {
        for (int i = 1; i < _audioSources.Length; ++i)
        {
            AudioSource audioSource = _audioSources[i];
            if (!audioSource.isPlaying)
                return i;
        }

        _audioSources[1].Stop();
        return 1;
    }

    private AudioSource GetAudioSource(int idx)
    {
        if (idx >= 0 && idx < _audioSources.Length)
            return _audioSources[idx];
        return null;
    }

    /// <summary>
    /// 暂停指定声音
    /// </summary>
    /// <param name="idx">调用play时得到的返回值，默认暂停BGM</param>
    public void Pause(int idx=0)
    {
        AudioSource audioSource = GetAudioSource(idx);
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }
    /// <summary>
    /// 重新播放指定声音，对于非BGM，慎用！！
    /// </summary>
    /// <param name="idx">调用play时得到的返回值，默认重播BGM</param>
    public void Resume(int idx=0)
    {
        AudioSource audioSource = GetAudioSource(idx);
        if (!audioSource.isPlaying)
        {
            audioSource.UnPause();
        }
    }
    /// <summary>
    /// 判断指定声音是否正在播放
    /// </summary>
    /// <param name="idx">调用play时得到的返回值，默认BGM</param>
    public bool IsPlaying(int idx=0)
    {
        AudioSource audioSource = GetAudioSource(idx);
        return audioSource.isPlaying;
    }

    public void ClearAllClip()
    {
        for (int i = 0; i < _audioSources.Length; ++i)
        {
            _audioSources[i].clip = null;
        }
    }

    void LateUpdate()
    {
        if (_isBgmFadeOut)
        {
            OnFadeOutUpdate(Time.deltaTime);
        }
        if (_isBgmFadeIn)
        {
            OnFadeInUpdate(Time.deltaTime);
        }
        
    }

    void OnFadeOutUpdate(float dt)
    {
        _fadeTime += dt;
        _audioSources[0].volume = (1 - _fadeTime / _fadeOutTime) * _currBgmVolume;

        if (_fadeTime >= _fadeOutTime)
        {
            _fadeTime = 0;
            _isBgmFadeOut = false;
            _isBgmFadeIn = true;
            PlayNextBgm();
        }
    }

    void PlayNextBgm()
    {
        Play(_nextBgmId, _nextBgmModule, _nextBgmLoop, _nextBgmVolume, true);
        GetAudioSource(0).volume = 0;
    }

    void OnFadeInUpdate(float dt)
    {
        _fadeTime += dt;
        _audioSources[0].volume = _fadeTime > _fadeInTime ? _nextBgmVolume : _fadeTime / _fadeInTime * _nextBgmVolume;

        if (_fadeTime > _fadeInTime)
        {
            _isBgmFadeIn = false;
            _nextBgmId = 0;
            _fadeTime = 0;
        }
    }

    ///静音操作：后续做语音可用
    public void Mute(bool mute)
    {
        for (int i = 0; i < _audioSources.Length; ++i)
        {
            if (_audioSources[i] != null)
                _audioSources[i].mute = mute;
        }
    }
}