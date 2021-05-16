using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Audior
{
    public class AudioSourceController : MonoBehaviour
    {
        public enum Type
        {
            kInvalid = 0,
            kOneShot = 1,
            kLooping = 2,
        }

        AudioSource m_audioSource = null;
        UnityAction m_onPlayFinishCb = null;
        float m_volumeModifier = 1.0f;

        private void Awake()
        {
            TryGetComponent(out m_audioSource);
        }

        public void Configure(Type type)
        {
            switch (type)
            {
                case Type.kOneShot:
                    m_audioSource.loop = false;
                    break;
                case Type.kLooping:
                    m_audioSource.loop = true;
                    break;
                default:
                    Debug.LogErrorFormat("Trying to configure {0} with type {1}", name, type);
                    break;
            }
        }

        public void Play(AudioClipInfo clip)
        {
            if (clip.AudioClip == null)
            {
                Debug.LogWarning("Audio clip unexpectedly null");
                TriggerFinishCb();
                return;
            }

            m_audioSource.volume = m_volumeModifier * clip.Volume
                + Random.Range(-clip.VolumeVariance, clip.VolumeVariance);
            m_audioSource.pitch = clip.Pitch
                + Random.Range(-clip.PitchVariance, clip.PitchVariance);

            m_audioSource.clip = clip.AudioClip;
            m_audioSource.Play();
        }

        public void Play(AudioClipInfo clip, UnityAction onPlayFinishCb)
        {

            m_onPlayFinishCb = onPlayFinishCb;
            Play(clip);
            if (!m_audioSource.loop && clip.AudioClip != null)
                Invoke(kTriggerFinishFuncName, clip.AudioClip.length);
        }

        public void Pause()
        {
            m_audioSource.Pause();
        }

        public void Resume()
        {
            m_audioSource.UnPause();
        }

        public void Stop()
        {
            m_audioSource.Stop();
            TriggerFinishCb();
        }

        public float VolumeModifier { set { m_volumeModifier = value; } get { return m_volumeModifier; } }

        const string kTriggerFinishFuncName = "TriggerFinishCb";
        private void TriggerFinishCb()
        {
            m_onPlayFinishCb.Invoke();
            ResetComponent();
        }

        private void ResetComponent()
        {
            m_onPlayFinishCb = null;
            m_audioSource.clip = null;
        }
    }
}
