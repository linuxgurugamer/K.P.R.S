using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using static KPRS.RegisterToolbar;


namespace KPRS
{
    internal class SoundPlayer: MonoBehaviour
    {
        internal static SoundPlayer Instance;
        GameObject soundPlayerObject;
        AudioSource audioSource;
        AudioClip loadedClip;
        AudioClip alternativeClip;
        int altSoundCount;

        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
            Log.Info("SoundPlayer.Awake");
        }

        bool loadingSong = false;

        public void LoadClipFromFile(string path)
        {
            StartCoroutine(LoadClipCoroutine(path));
        }

        IEnumerator LoadClipCoroutine(string path)
        {
            Log.Error("LoadClipCoroutine, path: " + path);
            StopSound();
            loadingSong = true;
            string url = string.Format("file://{0}", path);
            WWW www = new WWW(url);
            yield return www;

            loadedClip = www.GetAudioClip(false, false);
            if (loadedClip == null)
                Log.Error("LoadClipCoroutine LoadedClip is null");
            else
                Log.Error("LoadClipCoroutine Clip loaded from file");
            loadingSong = false;
        }

        internal SoundPlayer()
        {
            Log.Info("Instantiating SoundPlayer");
        }
        ~SoundPlayer()
        {
            Log.Info("Destroying SoundPlayer");
        }

        public void PlaySound(bool alternative = false)
        {
            Log.Info("SoundPlayer.PlaySound");
#if false
            if (audioSource == null)
            {
                Log.Error("source.audio is null");
                audioSource = soundPlayerObject.AddComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.volume = 0.5f;
                    audioSource.spatialBlend = 0;
                }
                else
                    Log.Error("Unable to add component AudioSource");

            }
#endif
            if (loadedClip == null)
            {
                Log.Error("PlaySound loadedClip is null");
                return;
            }
            if (alternative)
                audioSource.clip = alternativeClip;
            else
                audioSource.clip = loadedClip;
            if (audioSource.clip != null && !audioSource.isPlaying)
                audioSource.Play();
        }
        public void SetVolume(float vol)
        {
            audioSource.volume = vol / 100;
        }

        public void ToggleSound()
        {
            if (audioSource.clip != null)
            {
                if (SoundPlaying())
                    audioSource.Stop();
                else
                    audioSource.Play();
            }

        }
        public void StopSound()
        {
            if (audioSource.clip != null)
            {
                if (SoundPlaying())
                    audioSource.Stop();
            }

        }
        public bool SoundPlaying() //Returns true if sound is playing, otherwise false
        {
            if (audioSource != null)
            {
                return audioSource.isPlaying;
            }
            else
            {
                return false;
            }
        }
#if false
        public void LoadNewSound(string soundPath, int cnt)
        {
            altSoundCount = cnt;
            LoadNewSound(soundPath, true);
        }
#endif

        public void LoadNewSound(string soundPath, bool alternative = false)
        {
            Log.Info("LoadNewSound, soundPath: " + soundPath + ", exists: " + GameDatabase.Instance.ExistsAudioClip(soundPath));
            //if (alternative)
            //    alternativeClip = GameDatabase.Instance.GetAudioClip(soundPath);
            //else
            //    loadedClip = GameDatabase.Instance.GetAudioClip(soundPath);

            string fullSoundPath = KSPUtil.ApplicationRootPath + "GameData/" + soundPath;
            Log.Info("fullSoundPath: " + fullSoundPath);

            //if (loadedClip == null)
                LoadClipFromFile(fullSoundPath + ".ogg");
        }

        public void Initialize(string soundPath="")
        {
            Log.Info("SoundPlayer.Start, soundPath: " + soundPath);

            soundPlayerObject = new GameObject("KPRSsoundPlayer"); //Makes the GameObject

            //Initializing stuff;
            
                audioSource = soundPlayerObject.AddComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.volume = 0.5f;
                    audioSource.spatialBlend = 0;
                    audioSource.loop = false;
                }
                else
                    Log.Error("Unable to add component AudioSource");
                if (soundPath!="")
                {
                    LoadClipFromFile(soundPath);                    
                }
            Log.Info("Initialized Sound Player");
        }

    }

}
