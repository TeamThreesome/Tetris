using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{

    private static AudioManager instance = null;
    public AudioSource audioMgr;
    private AudioClip ac;
    private string curMusicName = "";

    public static AudioManager Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this.gameObject);
        else
            instance = this;

        DontDestroyOnLoad(this.gameObject);
        Play("Fireflies.mp3");
    }

    public void Play(string fileName)
    {
        if (!fileName.Equals(curMusicName))
        {
            ac = Resources.Load("Audio/" + fileName) as AudioClip;
            audioMgr.clip = ac;
            audioMgr.Play();
            curMusicName = fileName;
        }
    }

    public void Stop()
    {
        audioMgr.Stop();
        curMusicName = "";
        Debug.Log("Stop background music");
    }
}