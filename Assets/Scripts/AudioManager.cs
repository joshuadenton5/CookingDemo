using System.Collections.Generic;
using UnityEngine;

//(Brackeys, 2017)
public class AudioManager : MonoBehaviour
{
    public List<Sound> sounds;
    private List<AudioSource> sources = new List<AudioSource>();

    void Awake()
    {
        foreach (Sound s in sounds) //adding and setting varibales for each sound object
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            sources.Add(s.source);
        }
    }

    public void PlaySound(string _name) //playing a sound in the sound list
    {
        Sound s = sounds.Find(sound => sound.name == _name);
        if (s == null)
        {
            Debug.Log("NotFound");
            return;
        }
        s.source.Play();
    }

    public void AddFoodSound(GameObject obj) //for when food goes on the griddle
    {
        AudioSource source = obj.AddComponent<AudioSource>();
        sources.Add(source);
        Sound s = GetSound("Sizzle");
        source.clip = s.clip;
        source.volume = s.volume;
        source.spatialBlend = 1;
        source.maxDistance = 7;
        source.loop = s.loop;
        source.Play();
    }

    public Sound GetSound(string _name) //finding a particular sound
    {
        Sound s = sounds.Find(sound => sound.name == _name);
        if (s == null)
        {
            Debug.Log("NotFound");
            return null;
        }
        return s;
    }

    public void MuteAll(bool toMute) //muting all sounds
    {
        foreach (Sound s in sounds)
            s.source.mute = toMute;
    }

    public void PauseAll() //pausing all sounds, used when the game is paused 
    {
        foreach (AudioSource s in sources)
            s.Pause();
    }
    public void UnPauseAll() //when the game is resumed 
    {
        foreach (AudioSource s in sources)
            s.UnPause();
    }

    public void RemoveSource(AudioSource source)
    {
        sources.Remove(source);
    }
}

