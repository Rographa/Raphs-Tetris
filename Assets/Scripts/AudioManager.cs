using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField] AudioClip[] clips;
    [SerializeField] AudioClip[] musics;
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] float fadeInTime = 5;

    List<AudioClip> notPlayedMusics = new List<AudioClip>();
    public enum Clips { LINE_FINISHED, TETRO_POPUP};

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
        foreach (AudioClip music in musics)
        {
            notPlayedMusics.Add(music);
        }
        StartCoroutine(FadeInMusic());
        StartCoroutine(LoopMusic());
    }
    public void PlayClip(Clips index, bool pitchVariation = false)
    {
        int _index = (int)index;
        sfxSource.pitch = pitchVariation ? Random.Range(0.8f, 1.2f) : 1;
        sfxSource.PlayOneShot(clips[_index]);        
    }
    IEnumerator FadeInMusic()
    {
        float desiredVolume = musicSource.volume;
        for (float i = 0; i < 1; i += Time.deltaTime / fadeInTime)
        {
            musicSource.volume = Mathf.Lerp(0, desiredVolume, i);
            yield return new WaitForEndOfFrame();
        }
        musicSource.volume = desiredVolume;
    }
    IEnumerator LoopMusic()
    {
        while (true)
        {
            AudioClip music = RandomizeMusic();
            musicSource.clip = music;
            musicSource.Play();
            yield return new WaitForSecondsRealtime(music.length);
        }
    }
    AudioClip RandomizeMusic()
    {
        AudioClip music;
        if (notPlayedMusics.Count > 0)
        {
            music = notPlayedMusics[Random.Range(0, notPlayedMusics.Count)];
            notPlayedMusics.Remove(music);
        }
        else
        {
            music = musics[Random.Range(0, musics.Length)];
        }
        return music;
    }

}
