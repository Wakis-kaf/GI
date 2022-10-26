using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace UnitFramework.Runtime
{
    public class AudioComponent : ComponentUnit
    {
        public override string ComponentUnitName
        {
            get => "Audio";
        }

        private Dictionary<string, Sound> _id2Sound = new Dictionary<string, Sound>(50);
        private List<Sound> _sounds = new List<Sound>(50);


        public GameObject rootGO { get; private set; }

        public override void OnUnitAwake()
        {
            base.OnUnitAwake();
            rootGO = new GameObject("Audio Root");
            rootGO.transform.SetParent(transform);
        }


        public Sound FindFreeSound()
        {
            foreach (var sound in _sounds)
            {
                if (sound.isFree) return sound;
            }

            return null;
        }

        public Sound FindSound(string name)
        {
            if (_id2Sound.ContainsKey(name))
            {
                return _id2Sound[name];
            }

            return null;
        }

        public Sound CreateSound()
        {
            string name = "New Sound" + Sound.soundInstanceCount;
            return CreateSound(name);
        }

        public Sound CreateSound(string name)
        {
            int count = 1;
            //if (_id2Sound.ContainsKey(name)) name = "New Sound" + Sound.soundInstanceCount;
            while (_id2Sound.ContainsKey(name))
            {
                name = "New Sound" + count;
                count++;
            }

            GameObject soundModle = new GameObject(name);
            soundModle.transform.SetParent(rootGO.transform);
            Sound sound = soundModle.AddComponent<Sound>();
            sound.SetTag(name);
            _sounds.Add(sound);
            _id2Sound.Add(sound.soundID, sound);
            return sound;
        }

        public Sound GetSound(string name)
        {
            Sound find = FindSound(name);
            if (find == null)
            {
                return CreateSound(name);
            }

            return find;
        }

        public Sound GetFreeSound()
        {
            Sound get = FindFreeSound();
            if (get == null)
                get = CreateSound();
            return get;
        }

        public void DestroySound(Sound sound)
        {
            if (sound != null)
            {
                sound.Destroy();
            }
        }

        public void DestroySound(string tag)
        {
            if (_id2Sound.ContainsKey(tag))
            {
                DestroySound(_id2Sound[tag]);
            }
        }


        [RequireComponent(typeof(AudioSource))]
        public class Sound : MonoBehaviour
        {
            public static int soundInstanceCount { get; private set; } = 0;

            public Sound()
            {
                soundInstanceCount++;
                soundStatus = SoundStatus.Free;
            }

            public string soundID { get; private set; } // 标签

            public string SetTag(string tag)
            {
                this.soundID = tag;
                return tag;
            }

            public GameObject attachGO { get; private set; }
            public AudioSource attahAS { get; private set; }
            public AudioClip targetClip { get; private set; }
            public SoundOption targetSoundOption { get; private set; } = new SoundOption();
            public SoundStatus soundStatus { get; private set; }
            public bool isFree => soundStatus == SoundStatus.Free;
            private Coroutine _stopCoroutine;

            private void Awake()
            {
                attachGO = gameObject;
                attahAS = GetComponent<AudioSource>();
                Disable();
            }

            public void Play()
            {
                Enable();
                InitOption(targetSoundOption);
                attahAS.Play();
                soundStatus = SoundStatus.Playing;
            }

            public void Stop()
            {
                attahAS.Stop();
                soundStatus = SoundStatus.Free;
                Disable();
            }

            public void Pause()
            {
                attahAS.Pause();
            }

            public void Resume()
            {
                attahAS.UnPause();
            }

            public void Disable()
            {
                attahAS.gameObject.SetActive(false);
            }

            public void Enable()
            {
                attahAS.gameObject.SetActive(true);
            }

            private void InitOption(SoundOption option)
            {
                targetSoundOption = option;
                attahAS.clip = option.audioClip;
                attahAS.outputAudioMixerGroup = option.output;
                attahAS.mute = option.mute;
                attahAS.bypassEffects = option.byPassEffects;
                attahAS.bypassListenerEffects = option.bypassListenerEffects;
                attahAS.bypassReverbZones = option.byPassReverbZones;
                attahAS.playOnAwake = option.playOnAwake;
                attahAS.loop = option.loop;
                attahAS.priority = option.priority;
                attahAS.volume = option.volume;
                attahAS.pitch = option.pitch;
                attahAS.panStereo = option.panStereo;
                attahAS.spatialBlend = option.spatialBlend;
                attahAS.reverbZoneMix = option.reverbZoneMix;
                // 3D settings 
                attahAS.dopplerLevel = option.dopplerLevel;
                attahAS.spread = option.spread;
                attahAS.rolloffMode = option.rolloffMode;
                attahAS.minDistance = option.minDistance;
                attahAS.maxDistance = option.maxDistance;

                // position 
                gameObject.transform.position = option.worldPosition;
            }

            public void Play(SoundOption option)
            {
                InitOption(option);
                if (attahAS.clip != null)
                {
                    Play();
                    if (!attahAS.loop)
                    {
                        if (_stopCoroutine != null)
                        {
                            StopCoroutine(_stopCoroutine);
                        }

                        _stopCoroutine = StartCoroutine(Stop_Timeout(attahAS.clip.length));
                    }
                }
            }

            IEnumerator Stop_Timeout(float time)
            {
                yield return new WaitForSeconds(time);
                Stop();
            }

            public void Destroy()
            {
                Stop();
                Destroy(gameObject);
            }
        }

        public class SoundOption
        {
            public AudioClip audioClip;
            public AudioMixerGroup output;
            public Vector3 worldPosition;
            public bool mute;
            public bool byPassEffects;
            public bool bypassListenerEffects;
            public bool byPassReverbZones;
            public bool playOnAwake;
            public bool loop;
            public int priority = 128;
            public float volume = 1;
            public float pitch = 1;
            public float panStereo = 0;
            public float spatialBlend = 0;
            public float reverbZoneMix = 0;
            public float dopplerLevel = 1;
            public float spread = 1;
            public float minDistance = 1;
            public float maxDistance = 500;

            public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
        }


        public enum SoundStatus
        {
            Disabled, // 禁用状态
            Free, // 空闲状态
            Playing // 播放状态
        }
    }
}