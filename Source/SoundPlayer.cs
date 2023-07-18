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
    internal class SoundPlayer : MonoBehaviour
    {
        internal SoundPlayer Instance;
        GameObject soundPlayerObject;
        AudioSource audioSource;
        AudioClip loadedClip;
        string clipName = "";

        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
            Log.Info("SoundPlayer.Awake");
        }

        public void LoadClipFromFile(string path, bool alternative = false)
        {
            StartCoroutine(LoadClipCoroutine(path));
        }

        IEnumerator LoadClipCoroutine(string path)
        {
#if false
            Log.Info("LoadClipCoroutine, path: " + path + ", exists: " + System.IO.File.Exists(path));
#endif

            StopSound();
            //loadingSong = true;
            string url = string.Format("file://{0}", path);
            clipName = url;
#pragma warning disable 0618
            WWW www = new WWW(url);
#pragma warning restore 0618
            yield return www;
            {
                loadedClip = www.GetAudioClip(false, false, AudioType.OGGVORBIS);
                if (loadedClip == null)
                    Log.Error("LoadClipCoroutine LoadedClip is null, path: " + path);
            }
#if false
            else
                Log.Info("LoadClipCoroutine Clip loaded from file");
#endif
            //loadingSong = false;
        }

        internal SoundPlayer()
        {
            Log.Info("Instantiating SoundPlayer");
        }
        ~SoundPlayer()
        {
            Log.Info("Destroying SoundPlayer");
        }

        public void PlaySound()
        {
#if false
            Log.Info("SoundPlayer.PlaySound");
#endif
            if (loadedClip == null)
            {
                Log.Error("PlaySound loadedClip is null: " + clipName);
                return;
            }

            if (audioSource != null)
            {
                audioSource.clip = loadedClip;

                if (audioSource.clip != null && !audioSource.isPlaying)
                    audioSource.Play();
#if false
                Log.Info("audioSource.Volume: " + audioSource.volume);
#endif
            }
        }

        public void SetVolume(float vol)
        {
            if (audioSource != null)
            {
                audioSource.volume = vol / 100f;
                //Log.Info("SetVolume.audioSource.Volume: " + audioSource.volume);
            }
        }

        public void ToggleSound()
        {
            if (audioSource != null && audioSource.clip != null)
            {
                if (SoundPlaying())
                {
                    audioSource.Stop();
                }
                else
                {
                    audioSource.Play();

                    Log.Info("audioSource.Volume: " + audioSource.volume);
                }
            }
        }
        public void StopSound()
        {
            if (audioSource != null && audioSource.clip != null)
            {
                if (SoundPlaying())
                {
                    audioSource.Stop();
                }
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
            Log.Info("LoadNewSound, soundPath: " + soundPath);
            string fullSoundPath = KSPUtil.ApplicationRootPath + "GameData/" + soundPath;
            Log.Info("fullSoundPath: " + fullSoundPath);

            //if (soundPath.Contains("static"))
            //    LoadClipFromFile(fullSoundPath + ".mp3", alternative);
            //else
            LoadClipFromFile(fullSoundPath + ".ogg", alternative);

        }

        public void Initialize(string soundPath = "")
        {
            if (soundPlayerObject == null)
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
                    Log.Error("Unable to add component AudioSource for audioSource");

                if (soundPath != "")
                {
                    LoadClipFromFile(soundPath);
                }
                Log.Info("Initialized Sound Player");
            }
        }
    }

}
