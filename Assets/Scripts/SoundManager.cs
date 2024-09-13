using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public AudioClip crincleAudioClip;

    // crinkle sound found here: http://freesound.org/people/volivieri/sounds/37171/
    private AudioSource _crinkleSFX;

    private void Awake()
    {
        _crinkleSFX = AddAudio(crincleAudioClip);
    }

    private AudioSource AddAudio( AudioClip audioClip)
    {
        var audioSource = this.gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = audioClip;
        return audioSource;
    }

    public void PlayCrinkleSfx()
    {
        _crinkleSFX.Play();
    }
}
