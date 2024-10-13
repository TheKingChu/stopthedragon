using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioClip titleMusic;
    public AudioClip gameMusic;

    private AudioSource audioSource;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
    }
    // Start is called before the first frame update
    void Start()
    {
        PlayTitleMusic();
    }

    public void PlayTitleMusic()
    {
        PlayMusic(titleMusic);
    }

    public void PlayGameMusic()
    {
        PlayMusic(gameMusic);
    }

    private void PlayMusic(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }
}
