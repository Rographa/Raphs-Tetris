using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private AudioClip[] musics;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private float fadeInTime = 5;

    private readonly List<AudioClip> _notPlayedMusics = new List<AudioClip>();
    public enum Clips { LINE_FINISHED, TETRO_POPUP};

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        foreach (var music in musics)
        {
            _notPlayedMusics.Add(music);
        }
        StartCoroutine(FadeInMusic());
        StartCoroutine(LoopMusic());
    }

    private void OnEnable()
    {
        GameManager.OnLineCompleted += OnLineFinishedFeedback;
    }

    private void OnDisable()
    {
        GameManager.OnLineCompleted -= OnLineFinishedFeedback;
    }

    private void OnLineFinishedFeedback()
    {
        PlayClip(AudioManager.Clips.LINE_FINISHED, true);
    }

    public void PlayClip(Clips clip, bool pitchVariation = false)
    {
        var index = (int)clip;
        sfxSource.pitch = pitchVariation ? Random.Range(0.8f, 1.2f) : 1;
        sfxSource.PlayOneShot(clips[index]);        
    }
    private IEnumerator FadeInMusic()
    {
        var desiredVolume = musicSource.volume;
        for (var i = 0f; i < 1; i += Time.deltaTime / fadeInTime)
        {
            musicSource.volume = Mathf.Lerp(0, desiredVolume, i);
            yield return new WaitForEndOfFrame();
        }
        musicSource.volume = desiredVolume;
    }
    private IEnumerator LoopMusic()
    {
        while (true)
        {
            var music = RandomizeMusic();
            musicSource.clip = music;
            musicSource.Play();
            yield return new WaitForSecondsRealtime(music.length);
        }
    }
    private AudioClip RandomizeMusic()
    {
        AudioClip music;
        if (_notPlayedMusics.Count > 0)
        {
            music = _notPlayedMusics[Random.Range(0, _notPlayedMusics.Count)];
            _notPlayedMusics.Remove(music);
        }
        else
        {
            music = musics[Random.Range(0, musics.Length)];
        }
        return music;
    }

}
