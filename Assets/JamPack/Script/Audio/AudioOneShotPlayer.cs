using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPJamPack {
    public class AudioOneShotPlayer : MonoBehaviour, IPoolableObj
    {
        AudioSource audioSource;

        public System.Action<AudioOneShotPlayer> PlayEndCall;

        public void Instantiate() {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.spatialBlend = 1;
        }
        public void DeactivateObj(Transform collectionTransform) {
            if (collectionTransform != null) transform.SetParent(collectionTransform);
            gameObject.SetActive(false);
        }
        public void Reinstantiate() {
            transform.SetParent(null);
            gameObject.SetActive(true);
            enabled = true;
        }

        public void Play(AudioClip clip, System.Action<AudioOneShotPlayer> playEndCall=null, float volumeMultiplier=1) {
            audioSource.clip = clip;
            audioSource.volume = volumeMultiplier;
            audioSource.Play();
            audioSource.loop = false;

            PlayEndCall = playEndCall;
        }

        private void Update() {
            if (!audioSource.isPlaying) {
                if (PlayEndCall != null) PlayEndCall(this);
                enabled = false;
            }
        }
    }
}