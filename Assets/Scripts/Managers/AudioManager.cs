using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class AudioManager
{
    public GameObject Root
    {
        get
        {
            GameObject root = GameObject.Find("@Audio");
            if (root == null)
                root = new GameObject { name = "@Audio" };
            return root;
        }
    }

    AudioClip[] bgmClip;
    const float originBGMVolume = 0.2f;
    float bgmVolume = 0.2f;
    AudioSource bgmPlayer;
    const int bgmPlayerIndex = 5;

    AudioClip[] sfxClips;
    const float originSFXVolume = 0.5f;
    float sfxVolume = 0.5f;
    AudioSource[] sfxPlayers;
    const int sfxPlayerIndex = 4;
    int channelIndex = 0;

    bool isSFXPlayerReady = false;

    public void Init()
    {
        bgmClip = new AudioClip[bgmPlayerIndex];
        bgmClip[0] = Managers.Resource.Load<AudioClip>("Audio/BGM/Login");
        bgmClip[1] = Managers.Resource.Load<AudioClip>("Audio/BGM/Lobby");
        bgmClip[2] = Managers.Resource.Load<AudioClip>("Audio/BGM/Friend");
        bgmClip[3] = Managers.Resource.Load<AudioClip>("Audio/BGM/PlayerInfo");
        bgmClip[4] = Managers.Resource.Load<AudioClip>("Audio/BGM/Game");

        sfxClips = new AudioClip[sfxPlayerIndex];
        sfxClips[0] = Managers.Resource.Load<AudioClip>("Audio/SFX/Win");
        sfxClips[1] = Managers.Resource.Load<AudioClip>("Audio/SFX/Lose");
        sfxClips[2] = Managers.Resource.Load<AudioClip>("Audio/SFX/Button");
        sfxClips[3] = Managers.Resource.Load<AudioClip>("Audio/SFX/Card");
    }

    public void PlayBGM(Define.BGM bgm)
    {
        GameObject bgmObject = new GameObject("BGM");
        bgmObject.transform.parent = Root.transform;
        bgmPlayer = bgmObject.GetOrAddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;

        bgmPlayer.clip = bgmClip[(int)bgm];
        bgmPlayer.Play();
    }

    public void StopBGM()
    {
        if (bgmPlayer == null) return;
        if (bgmPlayer.isPlaying)
            bgmPlayer.Stop();
    }

    public float GetBGMVolume()
    {
        return bgmVolume;
    }

    void SFXPlayerReady()
    {
        if (isSFXPlayerReady) return;
        isSFXPlayerReady = true;

        GameObject sfxObject = new GameObject("SFX");
        sfxObject.transform.parent = Root.transform;
        sfxPlayers = new AudioSource[10];

        for(int i = 0; i < sfxPlayers.Length; i++)
        {
            sfxPlayers[i] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[i].playOnAwake = false;
            sfxPlayers[i].volume = sfxVolume;
        }
    }

    public void PlaySFX(Define.SFX sfx, float pitch = 1.0f)
    {
        SFXPlayerReady();

        for(int i = 0; i < sfxPlayers.Length; i++)
        {
            int loopIndex = (i + channelIndex) % sfxPlayers.Length;

            if (sfxPlayers[i].isPlaying) continue;

            channelIndex = loopIndex;
            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx];
            sfxPlayers[loopIndex].pitch = pitch;
            sfxPlayers[loopIndex].Play();
            break;
        }
    }

    float minPitch = 0.95f;
    float maxPitch = 1.05f;
    public void PlayCardSFX()
    {
        float pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        PlaySFX(Define.SFX.Card, pitch);
    }

    public void PlayCardSFX(string justFormat = "")
    {
        PlayCardSFX();
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    void StopSFX()
    {
        if (sfxPlayers == null) return;

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            if (sfxPlayers[i] == null) continue;
            if (sfxPlayers[i].isPlaying)
                sfxPlayers[i].Stop();
        }
    }

    public float BGMSoundOnOff()
    {
        if (bgmVolume == originBGMVolume) bgmVolume = 0f;
        else bgmVolume = originBGMVolume;

        if (bgmPlayer == null) return 0f;
        bgmPlayer.volume = bgmVolume;
        return bgmVolume;
    }

    public float SFXSoudOnOff()
    {
        if(sfxVolume == originSFXVolume) sfxVolume = 0f;
        else sfxVolume = originSFXVolume;

        if (sfxPlayers == null) return 0f;
        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            sfxPlayers[i].volume = sfxVolume;
        }
        return sfxVolume;
    }

    public void Claer()
    {
        StopBGM();
        StopSFX();
        isSFXPlayerReady = false;
    }
}