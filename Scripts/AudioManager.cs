using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilr;
using Configr;

namespace Audior
{
    public class AudioManager : Singleton<AudioManager>
    {
        #region Serialized Fields
        [SerializeField]
        private int m_bgmPoolCount = 8;

        [SerializeField]
        private int m_oneShotPoolCount = 64;
        #endregion // Serialized Fields

        #region Private Fields
        private ObjectPool<AudioSourceController> m_oneShotPool = null;
        private ObjectPool<AudioSourceController> m_bgmPool = null;
        #endregion // Private Fields

        #region Public Methods
        /// <summary>
        /// Play one shot audio like SFX
        /// </summary>
        /// <param name="audioClipInfo"></param>
        public void PlayOneShot(AudioClipInfo audioClipInfo)
        {
            var (src, index) = m_oneShotPool.GetNextAvailable();
            src.Play(audioClipInfo, () => { m_oneShotPool.SetAvailable(index); });
        }

        /// <summary>
        /// Play loop-ing audio like BGM
        /// </summary>
        /// <param name="audioClipInfo"></param>
        /// <returns>the handle to the bgm audio source</returns>
        public int PlayBGM(AudioClipInfo audioClipInfo)
        {
            var (src, index) = m_bgmPool.GetNextAvailable();
            src.Play(audioClipInfo);
            return index;
        }

        /// <summary>
        /// Pauses the given handle
        /// </summary>
        /// <param name="index"></param>
        public void PauseBGM(int index)
        {
            m_bgmPool.Get(index).Pause();
        }

        /// <summary>
        /// Resumes the given handle
        /// </summary>
        /// <param name="index"></param>
        public void ResumeBGM(int index)
        {
            m_bgmPool.Get(index).Resume();
        }

        /// <summary>
        /// Stops the given handle
        /// </summary>
        /// <param name="index"></param>
        public void StopBGM(int index)
        {
            m_bgmPool.Get(index).Stop();
            m_bgmPool.SetAvailable(index);
        }
        #endregion // Public Methods

        #region Unity Messages
        /// <summary>
        /// Start, haha who documents this
        /// </summary>
        private void Start()
        {
            m_oneShotPool = CreateNewObjectPool(m_oneShotPoolCount, AudioSourceController.Type.kOneShot);
            m_bgmPool = CreateNewObjectPool(m_bgmPoolCount, AudioSourceController.Type.kLooping);

            var playerConfig = ConfigManager.Instance.PlayerConfig;
            playerConfig.AddOnBGMVolumeChangedListener((vol) => { UpdateAudioSources(m_bgmPool, vol); });
            playerConfig.AddOnSFXVolumeChangedListener((vol) => { UpdateAudioSources(m_oneShotPool, vol); });
        }
        #endregion // Unity Messages

        #region Private Methods
        /// <summary>
        /// Helper function for creating audio source controller object pools
        /// </summary>
        /// <param name="count"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private ObjectPool<AudioSourceController> CreateNewObjectPool(int count, AudioSourceController.Type type)
        {
            List<AudioSourceController> tempList = new List<AudioSourceController>();
            for (int i = 0; i < count; i++)
            {
                var gameObj = new GameObject("Audio Source " + i);
                gameObj.AddComponent(typeof(AudioSource));
                var component = (AudioSourceController)gameObj.AddComponent(typeof(AudioSourceController));
                component.Configure(type);
                component.gameObject.name = type.ToString() + component.gameObject.name;
                tempList.Add(component);
            }

            return new ObjectPool<AudioSourceController>(tempList);
        }

        /// <summary>
        /// Helper function for updating audio source pool's volume
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="volume"></param>
        private void UpdateAudioSources(ObjectPool<AudioSourceController> pool, float volume)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                pool.Get(i).VolumeModifier = volume;
            }
        }
        #endregion // Private Methods
    }
}