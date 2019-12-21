using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;

public class DoremiPlayer : MonoBehaviour
{
    
    public DoremiMusic currentMusic;

    public PlayerSetting playerSetting = PlayerSetting.Stopped;

    private int currentChannel;
    public int currentPosition;

    public AudioMixerGroup mixerGroup;

    public AudioSource masterChannel;

    public List<AudioSource> mainChannels;
    public List<AudioSource> loopChannels;


    public double dspTimeAtSongStart;

    public float blendDuration = 0f;


    public DoremiMusic CurrentMusic {
        get {
            return currentMusic;
        }

        set {
            mainChannels.Clear();
            loopChannels.Clear();
            currentMusic = value;

            if (currentMusic.loopEndSamples == -1) {
                currentMusic.loopEndSamples = currentMusic.audio[0].samples;
            }

            // Clear the old music players
            foreach (Transform child in transform) {
                Destroy(child.gameObject);
            }

            // The master channel will help keep track of timing n stuff
            // GameObject masterGO = new GameObject("Master Audio");
            // masterChannel = masterGO.AddComponent<AudioSource>();
            // masterChannel.clip = currentMusic.audio[0];
            // masterChannel.volume = 0f;
            // masterChannel.playOnAwake = false;

            // Create a new clip for each subtrack
            foreach (AudioClip clip in currentMusic.audio) {
                GameObject newObj = new GameObject();


                if (currentMusic.loopStartSamples > 0) {
                    AudioSource newSource = newObj.AddComponent<AudioSource>();
                    newSource.clip = SnipAudioClip(clip, currentMusic.loopStartSamples, currentMusic.loopEndSamples, SnipSetting.Intro);
                    newSource.loop = false;
                    newSource.volume = 1f;

                    newSource.outputAudioMixerGroup = mixerGroup;

                    mainChannels.Add(newSource);
                    Debug.Log("Intro clip has been created and should be playing");
                }


                GameObject loopObj = new GameObject();
                
                AudioSource loopSource = loopObj.AddComponent<AudioSource>();
                loopSource.clip = SnipAudioClip(clip, currentMusic.loopStartSamples, currentMusic.loopEndSamples, SnipSetting.Loop);
                loopSource.loop = true;
                loopSource.volume = 1f;
                loopSource.outputAudioMixerGroup = mixerGroup;
                loopSource.playOnAwake = false;

                loopChannels.Add(loopSource);


                loopObj.transform.SetParent(transform, false);
                newObj.transform.SetParent(loopObj.transform, false);
            }

        }
    }

    public int CurrentPosition {
        get {
            if (dspTimeAtSongStart == 0) return -2;

            int output = -1;

            // First let's find out if we're in the loop part yet...
            if (!InLoopPortion) {
                //Debug.Log("INTRO");
                if (mainChannels.Count > 0 && mainChannels[0] != null) {
                    output = mainChannels[0].timeSamples;
                } else {
                    Debug.LogErrorFormat("Doremi Error: There are {0} main channels; there should be at least one!", mainChannels.Count);
                    output = 69;
                }
            } else {
                //Debug.Log("CURRENT POSITION IS IN LOOP");
                if (mainChannels.Count > 0 && loopChannels.Count > 0 && mainChannels[0] != null && loopChannels[0] != null) {
                    output = mainChannels[0].clip.samples + loopChannels[0].timeSamples;
                } else {
                    Debug.LogErrorFormat("Doremi Error: There are {0} main channels and {1} loop channels; there should be at least one of each!", mainChannels.Count, loopChannels.Count);
                    output = 69;
                }
            }
            return output;
        }


        set {
            Debug.LogWarning("Doremi Warning: You're trying to set the current position of the song. This doesn't work quite right.");
            // input is sample number

            Pause();

            // If we're in the intro portion, just adjust
            // the samples of the intro players
            if (value < currentMusic.loopStartSamples) {
                dspTimeAtSongStart = (float)AudioSettings.dspTime - (mainChannels[0].time);
                foreach (AudioSource s in mainChannels) {
                    s.timeSamples = value;
                }
            }

            // If we're in the loop portion, just adjust
            // the samples of the loop players
            else if (value >= currentMusic.loopStartSamples) {
                
                foreach (AudioSource s in loopChannels) {
                    s.timeSamples = value - currentMusic.loopStartSamples;
                }
            }
            Resume(remainPaused: true);

            if (playerSetting == PlayerSetting.Paused) Pause();
        }
    }

    public int CurrentChannel {
        get {
            return currentChannel;
        }

        set {
            currentChannel = value;

            for (int i = 0; i < mainChannels.Count; i++) {
                Debug.LogFormat("Setting volume of channel {0}", i);
                if (i == currentChannel) {
                    Debug.Log("This is the channel");
                    if (blendDuration <= 0) {
                        mainChannels[i].volume = 1f;
                        loopChannels[i].volume = 1f;
                    } else {
                        mainChannels[i].DOFade(1f, blendDuration);
                        loopChannels[i].DOFade(1f, blendDuration);
                    }
                } else {
                    if (blendDuration <= 0) {
                        mainChannels[i].volume = 0f;
                        loopChannels[i].volume = 0f;
                    } else {
                        mainChannels[i].DOFade(0f, blendDuration);
                        loopChannels[i].DOFade(0f, blendDuration);
                    }
                }
            }
        }
    }
    public float CurrentPositionFraction {
        get {
            if (dspTimeAtSongStart == 0) return 0f;
            int cPos = CurrentPosition;
            int finalPos = currentMusic.loopEndSamples;

            float frac = (float)cPos / (float)finalPos;
            return frac;
        }

        set {
            int samples = (int)(value * currentMusic.loopEndSamples);
            CurrentPosition = samples;
        }
    }

    public bool InLoopPortion {
        get {
            bool inLoop = loopChannels[0].time > 0;
            return inLoop;
        }
    }

    private float secondsToLoopStart {
        get {
            return currentMusic.loopStartSamples / (float)AudioSettings.outputSampleRate;
        }
    }

    private float timeWhereLoopStarts {
        get {
            return (float)dspTimeAtSongStart + secondsToLoopStart;
        }
    }

    public void Update() {
        currentPosition = CurrentPosition;
        //if (mainChannels[0].isPlaying) Debug.LogError("AAAAAA");
    }

    // public void Start() {
    //     CurrentMusic = currentMusic;
    //     //Play();
    // }

    public void Stop() {
        playerSetting = PlayerSetting.Stopped;
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }     

        mainChannels.Clear();
        loopChannels.Clear();

        dspTimeAtSongStart = 0;
    }

    public void Play(DoremiMusic m) {
        playerSetting = PlayerSetting.Playing;
        CurrentMusic = m;
        

        int i = 0;
        foreach (AudioSource a in mainChannels) {
            a.Play();
        }

        i = 0;
        foreach (AudioSource b in loopChannels) {
            dspTimeAtSongStart = AudioSettings.dspTime;
            b.PlayScheduled(timeWhereLoopStarts);
        }

        CurrentChannel = 0;

        Debug.Log("Doremi - Playing music");

        
    }

    public void Pause() {
        foreach (AudioSource a in mainChannels) {
            a.Pause();
        }

        foreach (AudioSource l in loopChannels) {
            if (InLoopPortion) {
                Debug.Log("Pausing while in the loop");
                l.Pause();
            }
            else {
                Debug.Log("Pausing while not in the loop");
                l.Stop();
            }
        }

        playerSetting = PlayerSetting.Paused;
    }

    public void Resume(bool remainPaused = false) {
        if (!remainPaused) playerSetting = PlayerSetting.Playing;

        if (InLoopPortion) {
            foreach (AudioSource a in loopChannels) {
                a.UnPause();
            }
        } else {
            // We're in the intro, so play the intro channels
            foreach (AudioSource a in mainChannels) {
                a.UnPause();
            }

            float currentDspTime = (float)AudioSettings.dspTime;

            dspTimeAtSongStart = (float)AudioSettings.dspTime - (mainChannels[0].time);

            // We need to reschedule the loop channels, so let's first cancel the current one...
            foreach (AudioSource l in loopChannels) {
                l.Stop();
                l.PlayScheduled(timeWhereLoopStarts);
            }
        }
    }

    public void SetChannelVolume(int channel, float volume, float duration = 0f) {
        mainChannels[channel].DOFade(volume, duration);
        loopChannels[channel].DOFade(volume, duration);
    }

    public void FadeOut(float duration) {
        foreach (AudioSource a in mainChannels) {
            a.DOFade(0f, duration);
        }

        foreach (AudioSource b in loopChannels) {
            b.DOFade(0f, duration);
        }
    }


    public void StopMusic() {}
    public void StartMusic() {}
    private AudioClip SnipAudioClip(AudioClip clipIn, int startSamples, int endSamples, SnipSetting setting) {
        int clipOutSamples;
        if (setting == SnipSetting.Loop) {clipOutSamples = endSamples - startSamples;}
        else {clipOutSamples = startSamples;}


        AudioClip clipOut = AudioClip.Create("Look Segment", clipOutSamples, clipIn.channels, clipIn.frequency, false);
        float[] samplesToCopy = new float[clipOut.samples * clipOut.channels];

        int pointToGetData;

        if (setting == SnipSetting.Loop) {pointToGetData = startSamples;}
        else {pointToGetData = 0;}
        clipIn.GetData(samplesToCopy, pointToGetData);
        clipOut.SetData(samplesToCopy, 0);

        return clipOut;
    }
}

public enum SnipSetting {
    Intro = 0,
    Loop = 1
}


public enum PlayerSetting {
    Stopped = 0,
    Playing = 1,
    Paused = 2
}