using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager
{
    AudioSource[] _audioSources = new AudioSource[(int)Define.Sound.MaxCount];
    Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

    // MP3 Player   -> AudioSource
    // MP3 음원     -> AudioClip
    // 관객(귀)     -> AudioListener

    public void Init()
    {
        GameObject root = GameObject.Find("@Sound");
        if (root == null)
        {
            root = new GameObject { name = "@Sound" };
            Object.DontDestroyOnLoad(root);

            string[] soundNames = System.Enum.GetNames(typeof(Define.Sound));
            for (int i = 0; i < soundNames.Length - 1; i++)
            {
                GameObject go = new GameObject { name = soundNames[i] };
                _audioSources[i] = go.AddComponent<AudioSource>();
                go.transform.parent = root.transform;
            }

            _audioSources[(int)Define.Sound.Bgm].loop = true;
        }
    }

    public void Clear()
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            audioSource.clip = null;
            audioSource.Stop();
        }
        _audioClips.Clear();
    }

    public void Play(string path, int loop = 0, Define.Sound type = Define.Sound.Effect, float pitch = 1.0f)
    {
        AudioClip audioClip = GetOrAddAudioClip(path, type);
        Play(audioClip, loop, type, pitch);
    }

	public void Play(AudioClip audioClip, int loop = 0, Define.Sound type = Define.Sound.Effect, float pitch = 1.0f)
	{
        if (audioClip == null)
            return;

		if (type == Define.Sound.Bgm)
		{
			AudioSource audioSource = _audioSources[(int)Define.Sound.Bgm];
			if (audioSource.isPlaying)
				audioSource.Stop();

            audioSource.mute = false;
            audioSource.pitch = pitch;
			audioSource.clip = audioClip;
			audioSource.Play();
		}
		else
		{
			AudioSource audioSource = _audioSources[(int)Define.Sound.Effect];
			audioSource.pitch = pitch;
            if(loop == 0)
    			audioSource.PlayOneShot(audioClip);
            else
            {
                audioSource.clip = audioClip;
                audioSource.Play();
                _audioSources[(int)Define.Sound.Effect].loop = true;
            }
        }
	}

    public void SoundPlayFromCharacter(GameObject go, string path, AudioSource source)
    {
        AudioClip audioClip = GetOrAddAudioClip(path);
        SoundPlayFromCharacter(go, audioClip, source);
    }

    public void SoundPlayFromCharacter(GameObject go, AudioClip audioClip, AudioSource source)
    {
        if (audioClip == null)
            return;

        source.clip = audioClip;
        source.Play();
    }

    public void MuteBgm()
    {
        AudioSource audioSource = _audioSources[(int)Define.Sound.Bgm];

        audioSource.mute = true;
    }

    public void TurnOnBgm()
    {
        AudioSource audioSource = _audioSources[(int)Define.Sound.Bgm];
        audioSource.mute = false;
    }

    public void RemoveBgm()
    {
        AudioSource audioSource = _audioSources[(int)Define.Sound.Bgm];
        audioSource.clip = null;

    }

    public IEnumerator IBgmSlowDown()
    {
        AudioSource audioSource = _audioSources[(int)Define.Sound.Bgm];

        while (true)
        {
            audioSource.volume -= Time.deltaTime;

            if(audioSource.volume <= 0)
            {
                audioSource.clip = null;
                audioSource.volume = 1;

                yield break;
            }

            yield return null;
        }
    }

	AudioClip GetOrAddAudioClip(string path, Define.Sound type = Define.Sound.Effect)
    {
		if (path.Contains("Sounds/") == false)
			path = $"Sounds/{type}/{path}";

		AudioClip audioClip = null;

		if (type == Define.Sound.Bgm)
		{
            audioClip = Managers.Resource.Load<AudioClip>(path);
		}
		else
		{
			if (_audioClips.TryGetValue(path, out audioClip) == false)
            {
				audioClip = Managers.Resource.Load<AudioClip>(path);
				_audioClips.Add(path, audioClip);
			}
		}

		if (audioClip == null)
			Debug.Log($"AudioClip Missing ! {path}");

		return audioClip;
    }
}
