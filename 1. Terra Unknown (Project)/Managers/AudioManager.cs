using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("#BGM")] 
    public AudioClip[] bgmClips;
    public float bgmVolume;
    private AudioSource bgmPlayer;
    
    [Header("#SFX")] 
    public AudioClip[] sfxClips;
    public float sfxVolume;
    public int channels;
    private AudioSource[] sfxPlayers;
    private int channelIndex;

    public enum Sfx
    {
        Dead,
        Hit,
        Select,
    }
    
    public enum BattleSfx
    {
        ArrowSound,
        ExplosionSound,
        FireSound,
        WaterDropSound,
        HealMagicSound,
        HitSound,
        SwingSound,
        SmokeSound,
        BuffSound,
        MagicHitSound
    }

    public enum Bgm
    {
        Main,
        FootStep,
    }
    
    private void Awake()
    {
        instance = this;
        Init();
    }

    private void Init()
    {
        // bgm player init
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;

        // sfx player init
        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            sfxPlayers[i] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[i].playOnAwake = false;
            sfxPlayers[i].volume = sfxVolume;
        }
    }

    public void PlayBgm(bool isPlay, Bgm bgm)
    {
        if (isPlay)
        {
            bgmPlayer.clip = bgmClips[(int)bgm];
            bgmPlayer.Play();
        }
        else
        {
            bgmPlayer.Stop();
        }
    }

    public void PlaySfx(Sfx sfx)
    {
        for (int i = 0; i < channels; i++)
        {
            int loopIndex = (i + channelIndex) % channels;
            
            if(sfxPlayers[loopIndex].isPlaying) continue;

            channelIndex = loopIndex;
            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx];
            sfxPlayers[loopIndex].Play();
            break;
        }
    }
    
    public void PlayBattleSfx(BattleSfx battleSfx)
    {
        for (int i = 0; i < channels; i++)
        {
            int loopIndex = (i + channelIndex) % channels;
            
            if(sfxPlayers[loopIndex].isPlaying) continue;

            channelIndex = loopIndex;
            sfxPlayers[loopIndex].clip = sfxClips[(int)battleSfx];
            sfxPlayers[loopIndex].Play();
            break;
        }
    }
}
