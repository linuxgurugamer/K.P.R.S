using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using static KPRS.RegisterToolbar;


namespace KPRS
{
    internal class SoundPlayer : MonoBehaviour
    {
        GameObject soundPlayerObject;
        internal AudioSource audioSource;
        float targetVolume;
        AudioClip loadedClip;
        string clipName = "";
        string playerName = "";
        internal bool loadingSong = false;
        internal bool readyToPlay = false;
        void Awake()
        {
            DontDestroyOnLoad(this);
            Log.Info("SoundPlayer.Awake");
        }

        void OnDestroy()
        {
            Log.Info("SoundPlayer.OnDestroy, playerName: " + playerName);
        }

        internal string Name { get { return playerName; } set { playerName = value; } }

        public void LoadClipFromFile(string path, bool alternative = false)
        {
            StartCoroutine(LoadClipCoroutine(path));
        }

        public void AdjustToTargetVolume(float volume)
        {
            audioSource.volume = targetVolume = volume;
            return;

            StopCoroutine(AdjustToTargetVolume());
            targetVolume = volume;
            StartCoroutine(AdjustToTargetVolume());
        }

        IEnumerator AdjustToTargetVolume()
        {
            Log.Info("AdjustToTargetVolume, targetVolume: " + targetVolume + ", volume: " + audioSource.volume);
                
            float volDiff = targetVolume - audioSource.volume;
            if (volDiff > 0f)
            {
                audioSource.volume += Math.Min(0.1f, volDiff);
            }
            else
            {
                audioSource.volume -= Math.Min(0.1f, volDiff);
            }
            if (Math.Abs(volDiff) <= 0.1f)
                yield return null;
            yield return new WaitForSeconds(0.1f);
        }

        IEnumerator LoadClipCoroutine(string path)
        {
#if true
            Log.Info("LoadClipCoroutine, playerName: " + Name + ", path: " + path + ", exists: " + System.IO.File.Exists(path));
#endif

            StopSound();

            loadingSong = true;
            UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip("file:///" + path, AudioType.OGGVORBIS);
            // yield return req.SendWebRequest();
            yield return req.SendWebRequest();

            loadedClip = DownloadHandlerAudioClip.GetContent(req);
            clipName = path;
            if (loadedClip == null)
                Log.Error("LoadClipCoroutine, playerName: " + Name + " LoadedClip is null, path: " + path);

#if false
            loadingSong = true;
            string url = string.Format("file://{0}", path);
            clipName = url;
#pragma warning disable 0618
            WWW www = new WWW(url);
#pragma warning restore 0618
            yield return www;

            {
                loadedClip = www.GetAudioClip(false, false, AudioType.OGGVORBIS);
                if (loadedClip == null)
                    Log.Error("LoadClipCoroutine, playerName: " + Name + " LoadedClip is null, path: " + path);
            }
#if false
            
            Log.Info("LoadClipCoroutine, playerName: " + Name +", Clip loaded from file: " + path);
            Log.Info("LoadClipCoroutine 2, loadingSong: " + loadingSong + ", clip len: " + loadedClip.length);
#endif
#endif
            loadingSong = false;
            readyToPlay = true;

        }

#if false
        internal SoundPlayer()
        {
            Log.Info("Instantiating SoundPlayer");
        }
        ~SoundPlayer()
        {
            Log.Info("Destroying SoundPlayer, playerName: " + Name );
        }
#endif

        public void PlaySound(string caller)
        {
#if true
            Log.Info("SoundPlayer.PlaySound caller: " + caller + ", clipName: " + clipName + ", loadingSong: " + loadingSong);
#endif
            if (loadingSong)
            {
                return;
            }
            readyToPlay = false;
            if (loadedClip == null)
            {
                Log.Error("PlaySound, playerName: " + Name + ", loadedClip is null: " + clipName);
                return;
            }

            if (audioSource != null)
            {
                audioSource.clip = loadedClip;

                if (audioSource.clip != null && !audioSource.isPlaying)
                {
#if false
                    Log.Info("PlaySound, clip name: " + clipName + ", starting Play");
#endif
                    audioSource.Play();
                }

#if false
                Log.Info("audioSource.Volume: " + audioSource.volume);
#endif
            }
            else
                Log.Error("PlaySound, audioSource is null");
        }

        public void SetVolume(float vol)
        {
            if (audioSource != null && vol != targetVolume)
            {
                //audioSource.volume = vol;
                AdjustToTargetVolume(vol);
#if false
                Log.Info("SetVolume targetVolume: " + vol);
#endif
            }
            else
            {
                if (audioSource == null)
                    Log.Error("SetVolume, audioSource is null");
            }
        }

        public void ToggleSound()
        {
            if (loadingSong)
            {
#if false
                Log.Info("ToggleSound, loadingSong is true, clipName: " + clipName);
#endif
                return;
            }
            if (audioSource != null)
            {
                if (audioSource.clip != null)
                {
                    if (SoundPlaying())
                    {
                        audioSource.Stop();
                    }
                    else
                    {
#if false
                    Log.Info("PlaySound, clip name: " + clipName);
                    Log.Info("ToggleSound, playerName: " + Name +", audioSource.Volume: " + audioSource.volume);
#endif
                        audioSource.Play();

                    }
                }
                else
                    Log.Error("ToggleSound, audioSource.clip is null");
            }
            else
                Log.Error("ToggleSound, audioSource is null");

        }
        public void StopSound()
        {
#if false
            Log.Info("SoundPlayer, playerName: " + Name);
#endif
            if (audioSource != null)
            {
                if (audioSource.clip != null)
                {
                    if (SoundPlaying())
                    {
                        audioSource.Stop();
                        audioSource.loop = false;
                    }
                }
            }
            else
                Log.Error("StopSound, audioSource is null");
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
            string fullSoundPath = KSPUtil.ApplicationRootPath + "GameData/" + soundPath;
#if false
            Log.Info("LoadNewSound, playerName: " + Name +", soundPath: " + soundPath);
            Log.Info("LoadNewSound, playerName: " + Name +", fullSoundPath: " + fullSoundPath);
#endif

            //if (soundPath.Contains("static"))
            //    LoadClipFromFile(fullSoundPath + ".mp3", alternative);
            //else
            LoadClipFromFile(fullSoundPath + ".ogg", alternative);

        }

        public void Initialize(string soundPath = "")
        {
            if (soundPlayerObject == null)
            {
#if false
                Log.Info("SoundPlayer.Start, playerName: " + Name +", soundPath: " + soundPath);
#endif

                soundPlayerObject = new GameObject("KPRSsoundPlayer"); //Makes the GameObject

                //Initializing stuff;

                audioSource = soundPlayerObject.AddComponent<AudioSource>();
                if (audioSource != null)
                {
                    //audioSource.volume = 0.5f;
                    AdjustToTargetVolume(0.5f);

                    audioSource.spatialBlend = 0;
                    audioSource.loop = false;
                }
                else
                    Log.Error("Initialize, playerName: " + Name + ", Unable to add component AudioSource for audioSource");

                if (soundPath != "")
                {
                    LoadClipFromFile(soundPath);
                }
                Log.Info("Initialized Sound Player, playerName: " + Name);
            }
        }
    }

}
