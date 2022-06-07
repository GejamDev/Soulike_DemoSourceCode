using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public UniversalManager um;


    public List<Sound> soundList = new List<Sound>();
    public List<AudioSource> musicList = new List<AudioSource>();

    Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();

    public string currentlyPlayingMusic;

    public float masterVolume;
    public float sfxVolume;
    public float voiceVolume;
    public float musicVolume;

    void Awake()
    {
        StopMusic();
        foreach (Sound s in soundList)
        {
            soundDictionary.Add(s.audio.name, s);
        }
    }
    

    public void PlaySound(string soundName, float vol)
    {
        if (!soundDictionary.ContainsKey(soundName))
        {
            Debug.Log(soundName + ", sound doesn't exist");
            return;
        }
        Sound playingSound = soundDictionary[soundName];

        float volume = masterVolume;
        volume *= playingSound.defaultVolume;
        volume *= vol;
        switch (playingSound.type)
        {
            case SoundType.sfx:
                volume *= sfxVolume;
                break;
            case SoundType.voice:
                volume *= voiceVolume;
                break;
            default:
                break;
        }
        GameObject prefab = new GameObject("Sound : " + soundName);
        AudioSource source = prefab.AddComponent<AudioSource>();
        source.volume = volume;

        source.clip = playingSound.audio;
        source.Play();

        Destroy(prefab, playingSound.audio.length);

    }

    public void ChangeMusic(string musicName)
    {
        currentlyPlayingMusic = musicName;
        foreach(AudioSource m in musicList)
        {
            m.gameObject.SetActive(m.clip.name == musicName);
        }
    }
    public void StopMusic()
    {
        currentlyPlayingMusic = "";
        foreach (AudioSource m in musicList)
        {
            m.gameObject.SetActive(false);
        }
    }
}

public enum SoundType
{
    sfx,
    voice,
    music
}

[System.Serializable]
public class Sound
{
    public AudioClip audio;
    [Range(0, 1)]
    public float defaultVolume = 1;
    public SoundType type;
}

[System.Serializable]
public class BGM
{
    public AudioClip audio;
    [Range(0, 1)]
    public float defaultVolume = 1;
}
