using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SoundEffect
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool loop = false;
    [Range(0f, 1f)] public float spatialBlend = 0f; // 0 = 2D, 1 = 3D
}

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("Audio Settings")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private int maxConcurrentSFX = 10;

    [Header("Battle SFX")]
    [SerializeField] private List<SoundEffect> soundEffects = new List<SoundEffect>();

    private Queue<AudioSource> availableSources = new Queue<AudioSource>();
    private List<AudioSource> activeSources = new List<AudioSource>();

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
            return;
        }

        InitializeAudioPool();
    }

    private void InitializeAudioPool()
    {
        // Create additional audio sources for concurrent SFX
        for (int i = 0; i < maxConcurrentSFX; i++)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            newSource.loop = false;
            availableSources.Enqueue(newSource);
        }
    }

    // Play a sound by name (uses dedicated SFX source)
    public void PlaySFX(string name, float volumeMultiplier = 1f)
    {
        SoundEffect sfx = GetSoundEffect(name);
        if (sfx == null)
        {
            Debug.LogWarning($"Sound effect '{name}' not found!");
            return;
        }

        if (availableSources.Count > 0)
        {
            AudioSource source = availableSources.Dequeue();
            SetupAudioSource(source, sfx, volumeMultiplier);
            source.Play();
            activeSources.Add(source);
            StartCoroutine(ReturnToPool(source, sfx.clip.length));
        }
        else
        {
            // Fallback to main SFX source
            sfxSource.volume = sfx.volume * volumeMultiplier;
            sfxSource.pitch = sfx.pitch;
            sfxSource.spatialBlend = sfx.spatialBlend;
            sfxSource.PlayOneShot(sfx.clip);
        }
    }

    // Play UI sound (uses dedicated UI source)
    public void PlayUISound(string name, float volumeMultiplier = 1f)
    {
        SoundEffect sfx = GetSoundEffect(name);
        if (sfx == null)
        {
            Debug.LogWarning($"Sound effect '{name}' not found!");
            return;
        }

        uiSource.volume = sfx.volume * volumeMultiplier;
        uiSource.pitch = sfx.pitch;
        uiSource.PlayOneShot(sfx.clip);
    }

    // Play music (uses dedicated music source)
    public void PlayMusic(string name, bool loop = true, float fadeTime = 1f)
    {
        SoundEffect sfx = GetSoundEffect(name);
        if (sfx == null)
        {
            Debug.LogWarning($"Music '{name}' not found!");
            return;
        }

        StartCoroutine(CrossfadeMusic(sfx.clip, sfx.volume, loop, fadeTime));
    }

    private SoundEffect GetSoundEffect(string name)
    {
        foreach (SoundEffect sfx in soundEffects)
        {
            if (sfx.name == name)
            {
                return sfx;
            }
        }
        return null;
    }

    private void SetupAudioSource(AudioSource source, SoundEffect sfx, float volumeMultiplier)
    {
        source.clip = sfx.clip;
        source.volume = sfx.volume * volumeMultiplier;
        source.pitch = sfx.pitch;
        source.loop = sfx.loop;
        source.spatialBlend = sfx.spatialBlend;
        source.playOnAwake = false;
    }

    private IEnumerator ReturnToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);

        source.Stop();
        source.clip = null;
        activeSources.Remove(source);
        availableSources.Enqueue(source);
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip, float targetVolume, bool loop, float fadeTime)
    {
        // Fade out current music
        float startVolume = musicSource.volume;
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeTime);
            yield return null;
        }

        // Switch clip and fade in
        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.Play();

        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0, targetVolume, t / fadeTime);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }

    // Stop all SFX
    public void StopAllSFX()
    {
        sfxSource.Stop();
        uiSource.Stop();

        foreach (AudioSource source in activeSources)
        {
            source.Stop();
        }

        // Return all active sources to pool
        while (activeSources.Count > 0)
        {
            AudioSource source = activeSources[0];
            source.Stop();
            source.clip = null;
            activeSources.RemoveAt(0);
            availableSources.Enqueue(source);
        }
    }

    // Set music volume
    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    // Set SFX volume
    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
        uiSource.volume = volume;

        foreach (AudioSource source in GetComponents<AudioSource>())
        {
            if (source != musicSource && source != sfxSource && source != uiSource)
            {
                source.volume = volume;
            }
        }
    }
}