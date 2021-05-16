using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Audior
{
    [System.Serializable]
    public struct AudioClipInfo
    {
        public AudioClip AudioClip;
        public float Volume;
        public float VolumeVariance;
        public float Pitch;
        public float PitchVariance;
    }
}