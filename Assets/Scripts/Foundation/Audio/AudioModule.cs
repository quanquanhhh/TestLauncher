using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Foundation.AudioModule
{
    public class AudioModule : SingletonComponent<AudioModule>
    {
        
        private GameObject _gameObject =>SingleObj;
        
        protected Dictionary<string, AudioClip> soundClips = new Dictionary<string, AudioClip>(2);
        protected Dictionary<string,AudioSource> bgAudioList = new Dictionary<string, AudioSource>(2);
        protected List<AudioSource> _sfxPool = new List<AudioSource>(8);
        private AudioSource playingBGM;
        protected float musicMaxVolume = 1.0f;
        private bool playAudio = true;

        public void SetPlayAudioOn(bool isOn)
        {
            playAudio = isOn;
        }
 
        public void SoundOpen(bool isopen)
        {
            if (playingBGM != null)
            {
                playingBGM.volume = isopen ? 1 : 0;
            }
            musicMaxVolume = isopen ? 1 : 0;
        }

        public void AudioOpen(bool isopen)
        {
            playAudio = isopen;
        }

        public void OnDestroy()
        {
            int count = _sfxPool.Count;
            for (int i = 0; i < count; i++)
            {
                AudioSource source = _sfxPool[i];
                if (source != null)
                {
                    source.Stop();
                    source.clip = null;
                }
            }
            _sfxPool.Clear();

            foreach (var audioSource in bgAudioList)
            {
                audioSource.Value.Stop();
                audioSource.Value.clip = null;
            } 
            bgAudioList.Clear();

            if (_gameObject != null)
            {
                GameObject.Destroy(_gameObject);
                // _gameObject = null;
            }
        }
        
        public void PlayBgm(string path)
        {
            PlayBgMusic(path);
        }

        private void PlayBgMusic(string path)
        {
            if (playingBGM != null && playingBGM.name == path && playingBGM.isPlaying)
            {
                return;
            }

            if (!bgAudioList.ContainsKey(path))
            {
                var bgm = _gameObject.AddComponent<AudioSource>();
                bgAudioList.Add(path, bgm);
            }

            if (!soundClips.ContainsKey(path))
            {
                var clip = AssetLoad.Instance.LoadAssetSync<AudioClip>(path);
                if (clip != null) 
                {
                    soundClips.Add(path, clip);
                }
                else
                {
                    Debug.LogError(" no clips" + path);
                    return;
                }
            }

            if (playingBGM != null && playingBGM.isPlaying)
            {
                var lastPlaying = playingBGM;
                lastPlaying.DOKill();
                lastPlaying.DOFade(0, 0.6f).OnKill(() => { lastPlaying.volume = 0;});
            }


            var playing = bgAudioList[path];
            playing.clip = soundClips[path];
            playing.loop = true;
            playing.time = 0;
            playing.Play();
            playing.DOFade(musicMaxVolume, 0.6f).OnKill(() => { playing.volume = musicMaxVolume;});

            playingBGM = playing;
            
        }

        public void FadeBgm(float volum = 1)
        {

            if (playingBGM != null)
            {
                float v = Math.Min(musicMaxVolume, volum);
                playingBGM.DOFade(v, 0.6f);
            }
        } 
        public void PlayOneShotSfx(string path,bool isLoop = false, float volume = 1f)
        {
            if (string.IsNullOrEmpty(path)|| !playAudio)
            {
                return;
            }

            if (GetPlayingSfx(path) != null)
            {
                return;
            }
            AudioClip clip = AssetLoad.Instance.LoadAssetSync<AudioClip>(path);
            if (clip == null)
            {
                return;
            }
            AudioSource source = GetAvailableSfxSource();
            source.clip = clip;
            source.loop = isLoop;
            source.volume = volume;
            source.Play();
            _sfxPool.Add(source);
        }
        public void PlaySfx(string path,bool isLoop = false, float volume = 1f)
        {
            if (string.IsNullOrEmpty(path)|| !playAudio)
            {
                return;
            } 
            AudioClip clip = AssetLoad.Instance.LoadAssetSync<AudioClip>(path);
            if (clip == null)
            {
                return;
            }
            AudioSource source = GetAvailableSfxSource();
            source.clip = clip;
            source.loop = isLoop;
            source.volume = volume;
            source.Play();
            _sfxPool.Add(source);
        }

        public void StopBGM()
        {
            if (playingBGM != null)
            {
                playingBGM.DOFade(0,0.6f).OnKill(() => { playingBGM.volume = 0; });
            }
        }

        public void UnPauseBGM()
        {
            if (playingBGM != null)
            {
                playingBGM.DOFade(musicMaxVolume, 0.6f).OnKill(() => { playingBGM.volume = musicMaxVolume; });
            }
        }

        public void StopSfx(string path)
        {
            AudioSource sfx = GetPlayingSfx(path);
            sfx.Stop();
        }

        private AudioSource GetPlayingSfx(string path)
        {
            int count = _sfxPool.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                AudioSource source = _sfxPool[i];
                if (source.clip.name == path && source.isPlaying)
                {
                    return source;
                }
            } 
            return null;
        }
        
        private AudioSource GetAvailableSfxSource()
        {
            
            int count = _sfxPool.Count;
            for (int i = 0; i < count; i++)
            {
                AudioSource source = _sfxPool[i];
                if (source != null && !source.isPlaying)
                {
                    return source;
                }
            }  

            AudioSource newSource = _gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            newSource.loop = false;
            _sfxPool.Add(newSource);
            return newSource;
        }

        public void ClickAudio()
        {
            PlayOneShotSfx("click_common");
        }
    }
}