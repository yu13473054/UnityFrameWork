using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LuaFramework {
    public class SoundMgr : MonoBehaviour {
        #region 初始化
        private static SoundMgr _inst;
        public static SoundMgr Inst
        {
            get { return _inst; }
        }
        public static void Init()
        {
            if (_inst)
            {
                return;
            }
            GameObject go = new GameObject("SoundMgr");
            for (int i = 0; i < AppConst.AudioClipLimit; i++)
            {
                AudioSource audio = go.AddComponent<AudioSource>();
                audio.clip = null;
                audio.volume = 1.0f;
                audio.loop = false;
            }
            go.AddComponent<AudioListener>();
            GameObject child = go.AddChild();
            child.name = "BGM";

            go.AddComponent<SoundMgr>();

        }
        #endregion

        private Dictionary<int, int> _androidMusicDic = new Dictionary<int, int>();
        private AudioSource _bgmAS;
//        private int _bgmMusicId;
        private AudioSource[] _channelAS;

        /// <summary>
        /// 战斗音效的字典
        /// </summary>
        private Dictionary<string, AudioClip> _audioBattleDics;
        public List<string> DownloadSongsList = new List<string>();

        void Awake()
        {
            _inst = this;
            DontDestroyOnLoad(gameObject);

            _channelAS = GetComponents<AudioSource>();
            _bgmAS = transform.Find("BGM").GetComponent<AudioSource>();

        }

        void OnDestroy()
        {

        }

        AudioClip LoadAudioClip(string name)
        {
            return GetAudioClipFromAB(name);
        }

        AudioClip GetAudioClipFromAB(string name)
        {
//            AudioClip ret = AssetBundleMgr.instance.DownloadBundlesMgr.LoadAsset<AudioClip>(
//                DownloadBundlesType.DBT_Sound, name, "sound.ab", false);
            return null;
        }

//        private int GetIdleChannel()
//        {
//            for (int i = 1; i < _channel.Length; ++i)
//            {
//                AudioSource audio = _channel[i];
//                if (!audio.isPlaying)
//                    return i;
//            }
//
//            _channel[1].Stop();
//            return 1;
//        }

        private AudioSource GetChannel(int idx)
        {
            if (idx >= 0 && idx < _channelAS.Length)
                return _channelAS[idx];
            return null;
        }

        public int PlaySound(int id, bool loop = false, float factor = 1.0f)
        {
//            if (id == 0 || id == 100000 || DataBase.instance == null || factor == 0f)
//            {
//                return -1;
//            }
//            if (_androidMusicDic.ContainsKey(id))
//            {
//                AndroidNativeAudio.play(_androidMusicDic[id], 1.0f, 1.0f, 1, loop ? 1 : 0);
//                return -1;
//            }
//            var musicDataCfg = DataBase.instance.MusicDataConfig.GetMusicDataById(id);
//            if (musicDataCfg == null)
//            {
//                string tmp = string.Format("不存在ID={0}的音效，请检查音效表MusicData", id);
//                MyDebug.LogErrorDebug(tmp);
//                return -1;
//            }
//
//            string clipname = musicDataCfg._prefabName;
//            if (string.IsNullOrEmpty(clipname))
//            {
//                string tmp = string.Format("音效（id={0},name={1})没有配置prefabName数据，请检查音效表MusicData", id, musicDataCfg._prefabName);
//                MyDebug.LogErrorDebug(tmp);
//                return -1;
//            }
//
//            float volume = musicDataCfg._musicVolume;
//            int idx = GetIdleChannel();
//            AudioSource audio = GetChannel(idx);
//
//            AudioClip clip = null;
//            if (_audioBattleDics != null && _audioBattleDics.ContainsKey(clipname))
//            {
//                clip = _audioBattleDics[clipname];
//            }
//            else
//            {
//                clip = LoadAudioClip(clipname);
//            }
//
//            if (clip == null)
//            {
//                MyDebug.LogWarningDebug(string.Format("播放音效[{0}], 加载AudioClip[{1}]失败", id, clipname));
//                return -1;
//            }
//            audio.loop = loop;
//            audio.clip = clip;
//            audio.volume = volume * factor;
//            audio.Play();

//            return idx;
            return 0;
        }

        public void StopSound(string soundName)
        {
            for (int i = 0; i < _channelAS.Length; i++)
            {
                AudioSource audioS_tmp = _channelAS[i];
                if (audioS_tmp.clip == null) continue;
                if (audioS_tmp.clip.name.Equals(soundName))
                {
                    audioS_tmp.Stop();
                    return;
                }
            }

        }

        /// <summary>
        /// 停止播放的音效
        /// </summary>
        /// <param name="idx"> playSound时得到的index </param>
        /// <param name="soundId"> 校验该idx的audioSource是否正在播放该音效，如果不是，不停止 </param>
        public void StopSound(int idx, int soundId=-1)
        {
            AudioSource audio_tmp;
            if (soundId!=-1)
            {
                //校验不存在，说明该音效已经播放完毕，不需要停止
                IsPlayingSound(soundId, out audio_tmp);
            }
            else
            {
                audio_tmp = GetChannel(idx);
            }
            if (audio_tmp)
            {
                audio_tmp.Stop();
            }
        }

        bool IsPlayingSound(int soundId, out AudioSource audioSource)
        {
            string soundName_tmp = "";
            for (int i = 0; i < _channelAS.Length; i++)
            {
                AudioSource audioS_tmp = _channelAS[i];
                if (audioS_tmp.clip == null) continue;
                if (audioS_tmp.clip.name.Equals(soundName_tmp))
                {
                    audioSource = audioS_tmp;
                    return true;
                }
            }
            audioSource = null;
            return false;
        }

        private float _fadeInTime = 1.0f;
        private float _fadeOutTime = 1.0f;
        private float _fadeTimer = 0.0f;
        private string _nextbgm = "";
        private int _nextBgmId = 0;
        private AudioClip _nextbgmClip = null;
        private float _bgmVolume = 1.0f;

        void InitState()
        {
//            _state_bgm_fadein = new StateInterface();
//            _state_bgm_fadeout = new StateInterface();
//            _state_bgm_playing = new StateInterface();
//
//            _state_bgm_fadeout._OnEnter = OnStateFadeOutEnter;
//            _state_bgm_fadeout._OnUpdate = OnStateFadeOutUpdate;
//
//            _state_bgm_fadein._OnEnter = OnStateFadeInEnter;
//            _state_bgm_fadein._OnUpdate = OnStateFadeInUpdate;
//
//            _state_bgm_playing._OnUpdate = OnStatePlayingUpdate;
        }

        private AudioClip LoadBgmFromAB(string clipname)
        {
//            AudioClip clip = AssetBundleMgr.instance.DownloadBundlesMgr.LoadAsset<AudioClip>(
//                DownloadBundlesType.DBT_Bgm, clipname, clipname + ".ab", true);
//            return clip;
            return null;
        }

        delegate void AfterLoadBgm(AudioClip audio);
        IEnumerator LoadBgmFromABAsync(string clipname, AfterLoadBgm dlgt)
        {
            yield break;
//            AsyncABLoadRequest req = new AsyncABLoadRequest();
//            yield return AssetBundleMgr.instance.DownloadBundlesMgr.LoadAssetAsync<AudioClip>(
//                req, DownloadBundlesType.DBT_Bgm, clipname, clipname + ".ab", true);
//
//            if (req.asset != null && dlgt != null)
//            {
//                dlgt(req.asset as AudioClip);
//            }
//            else
//            {
//                MyDebug.LogError("!!!!!!!!!!!!!!!!LoadBgmFromABAsync 加载歌曲失败！" + clipname);
//                dlgt(null);
//            }
        }


        public void PlayBgm(int id)
        {
            if (id == 0 || id == 100000)
            {
                return;
            }

            if (id == _nextBgmId && _bgmAS.clip != null)
            {
                ////如果是当前播放的音乐，则直接播放，不用再加载
                //if (id == _bgmMusicId)
                //{
                //    _nextbgmClip = _bgmSource.clip;
                //    Push(_state_bgm_fadein);
                //}
                //要加载的音乐正在加载，不用再加载
                return;
            }

//            MusicDataProtype tMusicDataProtype = DataBase.instance.MusicDataConfig.GetMusicDataById(id);
//            if (tMusicDataProtype == null)
//            {
//                string tmp = string.Format("不存在ID={0}的音效，请检查音效表MusicData", id);
//                MyDebug.LogError(tmp);
//                return;
//            }
//            ClearMusicTime();
//            _nextbgm = tMusicDataProtype._musicName;
//            _nextBgmId = id;
//            if (_nextbgmClip != null)
//                Resources.UnloadAsset(_nextbgmClip);
//            _nextbgmClip = null;
//            Push(_state_bgm_fadeout);
        }

        public void StopBgm()
        {
//            Push(null);
//            _bgmSource.Stop();
//            _bgmSource.time = 0;
//
//            if (unloadResource)
//            {
//                if (_bgmSource.clip != null)
//                    Resources.UnloadAsset(_bgmSource.clip);
//                if (_nextbgmClip != null)
//                    Resources.UnloadAsset(_nextbgmClip);
//                _bgmSource.clip = null;
//                _nextbgmClip = null;
//                _bgmMusicId = 0;
//                _nextBgmId = 0;
//            }
        }

        public void PauseBgm()
        {
//            if (_bgmSource.isPlaying && !_bgmPausing)
//            {
//                _bgmPausing = true;
//                _bgmSource.Pause();
//                _timebgmpaused = Time.time;
//            }
        }

        public void ResumeBgm()
        {
//            if (!_bgmSource.isPlaying && _bgmPausing)
//            {
//                _bgmPausing = false;
//                _bgmSource.UnPause();
//                _timebgmpausedSum += Time.time - _timebgmpaused;
//            }
        }

        public float GetBgmTime()
        {
            return _bgmAS.time;
        }

        public bool IsBgmPlaying()
        {
            if (_bgmAS && _bgmAS.isPlaying)
                return true;

            return false;
        }

        public void ReleaseAllClipRefrence()
        {
            _bgmAS.clip = null;
            for (int i = 0; i < _channelAS.Length; ++i)
            {
                _channelAS[i].clip = null;
            }
        }

        public void Mute(bool mute)
        {
            _bgmAS.mute = mute;
            for (int i = 0; i < _channelAS.Length; ++i)
            {
                if (_channelAS[i] != null)
                    _channelAS[i].mute = mute;
            }
        }

    }
}