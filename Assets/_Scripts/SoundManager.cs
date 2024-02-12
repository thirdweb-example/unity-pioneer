using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundName
{
    None,
    Charlotte_TinyFlames,
    Charlotte_MediumFlames,
    Charlotte_LargeFlames,
    Charlotte_TrailOfSteam,
    Charlotte_Fireflies,
    Charlotte_Spotlight,
    Charlotte_Encouraging,
    Charlotte_Inspiring,
}

[System.Serializable]
public class Sound
{
    public SoundName soundName;
    public AudioClip audioClip;
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public List<Sound> sounds;

    private AudioSource _audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(SoundName soundName)
    {
        if (soundName == SoundName.None)
            return;

        var sound = sounds.Find(s => s.soundName == soundName);
        if (sound != null)
        {
            _audioSource.clip = sound.audioClip;
            _audioSource.Play();
        }
    }

    public void StopAllSounds()
    {
        _audioSource.Stop();
    }
}
