using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPJamPack {
    public class VirtualAudioManager : MonoBehaviour
    {
        private Dictionary<int, AudioClip> clipDicts;

        public AudioSource oneShotAudioSrc;
        private PrefabPool<AudioOneShotPlayer> oneShotPlayerPool;

        public AudioSource bgmAudioSrc, secondaryBgmAudioSrc;

        [SerializeField]
        private AudioPreset defaultPreset;

        private void Awake() {
            clipDicts = new Dictionary<int, AudioClip>();

            oneShotPlayerPool = new PrefabPool<AudioOneShotPlayer>(delegate {
                GameObject obj = new GameObject("AudioOneShotPlayer");
                return obj.AddComponent<AudioOneShotPlayer>();
            }, true, "ParticleCollection");

            if (defaultPreset != null) LoadAudioPreset(defaultPreset);
        }

        private void Start() {
            if (oneShotAudioSrc == null) {
                oneShotAudioSrc = gameObject.AddComponent<AudioSource>();
            }

            if (bgmAudioSrc == null) {
                bgmAudioSrc = gameObject.AddComponent<AudioSource>();
                bgmAudioSrc.loop = true;
            }
            if (secondaryBgmAudioSrc == null) {
                secondaryBgmAudioSrc = gameObject.AddComponent<AudioSource>();
                secondaryBgmAudioSrc.loop = true;
            }
        }

    #region Audio Load/ Unload
        public void LoadAudioPreset(AudioPreset preset, bool overrideExist=false) {
            for (int i = 0; i < preset.Audios.Length; i++)
            {
                int ID = (int) preset.Audios[i].Type;

                if (clipDicts.ContainsKey(ID))
                {
                    if (!overrideExist) {
                    #if UNITY_EDITOR
                        Debug.LogWarningFormat("Audio '{0}' already exist", ID);
                    #endif
                        continue;
                    }
                    else clipDicts[ID] = preset.Audios[i].Clip;
                }
                else clipDicts.Add(ID, preset.Audios[i].Clip);
            }
        }

        public void LoadAudio(int ID, AudioClip clip, bool overrideExist=false) {
            if (clipDicts.ContainsKey(ID))
            {
                if (!overrideExist)
                {
                #if UNITY_EDITOR
                    Debug.LogWarningFormat("Audio '{0}' already exist", ID);
                #endif
                }
                else clipDicts[ID] = clip;
            }
            else clipDicts.Add(ID, clip);
        }

        public void UnloadAudioPreset(AudioPreset preset) {
            for (int i = 0; i < preset.Audios.Length; i++)
            {
                int ID = (int)preset.Audios[i].Type;

                if (clipDicts.ContainsKey(ID)) clipDicts.Remove(ID);
            }
        }

        public void UnloadAudio(int ID) {
            if (clipDicts.ContainsKey(ID)) clipDicts.Remove(ID);
        }
    #endregion

    #region Audio One Shot
        public void PlayOneShot(AudioIDEnum enumID, float volumeMultiplier = 1) {
            int ID = (int) enumID;
            if (clipDicts.ContainsKey(ID)) {
                oneShotAudioSrc.PlayOneShot(clipDicts[ID], volumeMultiplier);
            }
            else {
            #if UNITY_EDITOR
                Debug.LogWarningFormat("Audio '{0}' doesn't exist", ID);
            #endif
            }
        }

        public void PlayOneShot(int ID, float volumeMultiplier=1) {
            if (clipDicts.ContainsKey(ID)) {
                oneShotAudioSrc.PlayOneShot(clipDicts[ID], volumeMultiplier);
            }
            else {
            #if UNITY_EDITOR
                Debug.LogWarningFormat("Audio '{0}' doesn't exist", ID);
            #endif
            }
        }

        public void PlayOneShot(AudioClip clip, float volumeMultiplier = 1) {
            oneShotAudioSrc.PlayOneShot(clip, volumeMultiplier);
        }
    #endregion

    #region Gameobject's Audio One Shot
        public void PlayOneShotAtPosition(AudioIDEnum enumID, Vector3 position, float volumeMultiplier=1) {
            int ID = (int) enumID;
            if (!clipDicts.ContainsKey(ID)) {
            #if UNITY_EDITOR
                Debug.LogWarningFormat("Audio '{0}' doesn't exist", ID);
            #endif
                return;
            }

            AudioOneShotPlayer player = oneShotPlayerPool.Get();
            player.transform.position = position;
            player.Play(clipDicts[ID], (_player) => oneShotPlayerPool.Put(_player), volumeMultiplier);
        }

        public void PlayOneShotAtPosition(int ID, Vector3 position, float volumeMultiplier=1) {
            if (!clipDicts.ContainsKey(ID)) {
            #if UNITY_EDITOR
                Debug.LogWarningFormat("Audio '{0}' doesn't exist", ID);
            #endif
                return;
            }

            AudioOneShotPlayer player = oneShotPlayerPool.Get();
            player.transform.position = position;
            player.Play(clipDicts[ID], (_player) => oneShotPlayerPool.Put(_player), volumeMultiplier);
        }

        public void PlayOneShotAtPosition(AudioClip clip, Vector3 position, float volumeMultiplier=1) {
            AudioOneShotPlayer player = oneShotPlayerPool.Get();
            player.transform.position = position;
            player.Play(clip, (_player) => oneShotPlayerPool.Put(_player), volumeMultiplier);
        }
    #endregion
    
    #region Background Music
        public void PlayBgm(AudioClip bgmClip, bool overrideCurrentBGM=false) {
            if (bgmAudioSrc.isPlaying) {
                if (!overrideCurrentBGM) return;
                bgmAudioSrc.Stop();
                bgmAudioSrc.clip = null;
            }
            if (secondaryBgmAudioSrc.isPlaying) {
                if (!overrideCurrentBGM) return;
                secondaryBgmAudioSrc.Stop();
                secondaryBgmAudioSrc.clip = null;
            }

            bgmAudioSrc.clip = bgmClip;
            bgmAudioSrc.Play();
        }

        public void BlendNewBgm(AudioClip bgmClip, float fadeOut=0.5f, float fadeOutDelay=0,
                                float fadeIn=0.5f, float fadeInDelay=0.25f) {
            if (!bgmAudioSrc.isPlaying && !secondaryBgmAudioSrc.isPlaying) {
                PlayBgm(bgmClip);
                return;
            }
            // if (bgmAudioSrc.isPlaying && secondaryBgmAudioSrc.isPlaying)
            // TODO: Handle if two bgm audio source both playing

            if (bgmAudioSrc.isPlaying) {
                secondaryBgmAudioSrc.clip = bgmClip;
                StartCoroutine(FadeAudioSource(bgmAudioSrc, 0, fadeOut, fadeOutDelay, stopAfterFade: true));
                secondaryBgmAudioSrc.volume = 0;
                StartCoroutine(FadeAudioSource(secondaryBgmAudioSrc, bgmAudioSrc.volume, fadeIn, fadeInDelay, playerAfterDelay: true, returnVolume: false));
            }
            else {
                bgmAudioSrc.clip = bgmClip;
                StartCoroutine(FadeAudioSource(secondaryBgmAudioSrc, 0, fadeOut, fadeOutDelay, stopAfterFade: true));
                bgmAudioSrc.volume = 0;
                StartCoroutine(FadeAudioSource(bgmAudioSrc, secondaryBgmAudioSrc.volume, fadeIn, fadeInDelay, playerAfterDelay: true, returnVolume: false));
            }
        }
    #endregion

    #region Audio Source Fadeout Control
        public IEnumerator FadeAudioSource(AudioSource src, float targetVolume, float fadeTime,
                                    float delayTime=0, bool returnVolume=true, bool playerAfterDelay=false, bool stopAfterFade=false) {
            if (delayTime > 0) yield return new WaitForSeconds(delayTime);
            if (playerAfterDelay) src.Play();

            float time = 0;
            float originVolume = src.volume;

            while (time < fadeTime) {
                yield return null;
                time += Time.deltaTime;
                src.volume = Mathf.Lerp(originVolume, targetVolume, time / fadeTime);
            }

            if (stopAfterFade) src.Stop();
            if (returnVolume) src.volume = originVolume;
            else src.volume = targetVolume;
        }
    #endregion
    }
}