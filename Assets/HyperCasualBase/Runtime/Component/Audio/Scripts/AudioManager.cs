using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Settings")]
    [SerializeField] private Sound[] sounds;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;

    [Header("Audio Pool Settings")]
    [SerializeField] private int maxAudioSourcesPerSound = 5; // Số AudioSource tối đa cho mỗi loại âm thanh

    private Dictionary<AudioType, Sound> soundDictionary = new Dictionary<AudioType, Sound>();
    private Dictionary<AudioType, List<AudioSource>> audioSourcePools = new Dictionary<AudioType, List<AudioSource>>();
    private List<AudioSource> activeMusicSources = new List<AudioSource>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSounds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSounds()
    {
        soundDictionary.Clear();
        audioSourcePools.Clear();

        foreach (Sound sound in sounds)
        {
            if (sound.clips == null || sound.clips.Length == 0)
            {
                Debug.LogWarning($"Audio clips are missing for {sound.audioType}");
                continue;
            }

            soundDictionary[sound.audioType] = sound;

            // Tạo pool AudioSource cho mỗi loại âm thanh
            if (!sound.isMusic)
            {
                // SFX: Tạo pool để có thể phát nhiều âm thanh cùng lúc
                List<AudioSource> pool = new List<AudioSource>();
                for (int i = 0; i < maxAudioSourcesPerSound; i++)
                {
                    AudioSource source = gameObject.AddComponent<AudioSource>();
                    source.volume = sound.volume * masterVolume;
                    source.pitch = sound.pitch;
                    source.loop = false; // SFX không loop
                    source.playOnAwake = false;
                    pool.Add(source);
                }
                audioSourcePools[sound.audioType] = pool;
            }
            else
            {
                // Music: Chỉ cần 1 AudioSource
                sound.source = gameObject.AddComponent<AudioSource>();
                sound.source.volume = sound.volume * masterVolume;
                sound.source.pitch = sound.pitch;
                sound.source.loop = sound.loop;
                sound.source.playOnAwake = false;
                activeMusicSources.Add(sound.source);
            }
        }
    }

    /// <summary>
    /// Lấy AudioSource có thể dùng từ pool (không đang phát)
    /// </summary>
    private AudioSource GetAvailableAudioSource(AudioType audioType)
    {
        if (!audioSourcePools.TryGetValue(audioType, out List<AudioSource> pool))
            return null;

        // Tìm AudioSource không đang phát
        foreach (AudioSource source in pool)
        {
            if (!source.isPlaying)
                return source;
        }

        // Nếu tất cả đang phát, dùng AudioSource đầu tiên (ghi đè)
        return pool[0];
    }

    /// <summary>
    /// Phát âm thanh theo loại (ngẫu nhiên nếu có nhiều clips)
    /// </summary>
    public void Play(AudioType audioType)
    {
        if (!soundDictionary.TryGetValue(audioType, out Sound sound))
        {
            Debug.LogWarning($"Sound {audioType} not found in dictionary!");
            return;
        }

        // Kiểm tra settings
        if (SettingsManager.Instance != null)
        {
            if (sound.isMusic && !SettingsManager.Instance.Data.isMusicOn)
                return;

            if (!sound.isMusic && !SettingsManager.Instance.Data.isSoundOn)
                return;
        }

        // Lấy clip ngẫu nhiên
        AudioClip clip = sound.GetRandomClip();
        if (clip == null)
        {
            Debug.LogWarning($"No clip found for {audioType}. Assign clips in Inspector!");
            return;
        }

        if (sound.isMusic)
        {
            // Music: Dùng AudioSource riêng
            if (sound.source != null)
            {
                sound.source.clip = clip;
                sound.source.pitch = sound.GetRandomPitch();
                sound.source.Play();
            }
            else
            {
                Debug.LogError($"Music {audioType} has NO AudioSource!");
            }
        }
        else
        {
            // SFX: Dùng pool để có thể phát nhiều cùng lúc
            AudioSource source = GetAvailableAudioSource(audioType);
            if (source != null)
            {
                source.clip = clip;
                source.pitch = sound.GetRandomPitch();
                source.volume = sound.volume * masterVolume;
                source.Play();
            }
        }
    }

    /// <summary>
    /// Phát âm thanh một lần (không lặp lại, ngẫu nhiên nếu có nhiều clips)
    /// Tốt nhất cho SFX vì có thể phát nhiều cùng lúc
    /// </summary>
    public void PlayOneShot(AudioType audioType)
    {
        if (!soundDictionary.TryGetValue(audioType, out Sound sound))
        {
            Debug.LogWarning($"Sound {audioType} not found!");
            return;
        }

        // Kiểm tra settings
        if (SettingsManager.Instance != null)
        {
            if (sound.isMusic && !SettingsManager.Instance.Data.isMusicOn)
                return;

            if (!sound.isMusic && !SettingsManager.Instance.Data.isSoundOn)
                return;
        }

        // Lấy clip ngẫu nhiên
        AudioClip clip = sound.GetRandomClip();
        if (clip == null)
        {
            Debug.LogWarning($"No valid clip found for {audioType}");
            return;
        }

        if (sound.isMusic)
        {
            // Music không nên dùng OneShot
            if (sound.source != null)
            {
                sound.source.pitch = sound.GetRandomPitch();
                sound.source.PlayOneShot(clip, sound.volume * masterVolume);
            }
        }
        else
        {
            // SFX: Lấy AudioSource từ pool
            AudioSource source = GetAvailableAudioSource(audioType);
            if (source != null)
            {
                source.pitch = sound.GetRandomPitch();
                source.volume = sound.volume * masterVolume;
                source.PlayOneShot(clip, sound.volume * masterVolume);
            }
        }
    }

    /// <summary>
    /// Phát âm thanh với volume tùy chỉnh (ngẫu nhiên nếu có nhiều clips)
    /// </summary>
    public void PlayWithVolume(AudioType audioType, float volumeScale)
    {
        if (!soundDictionary.TryGetValue(audioType, out Sound sound))
        {
            Debug.LogWarning($"Sound {audioType} not found!");
            return;
        }

        // Kiểm tra settings
        if (SettingsManager.Instance != null)
        {
            if (sound.isMusic && !SettingsManager.Instance.Data.isMusicOn)
                return;

            if (!sound.isMusic && !SettingsManager.Instance.Data.isSoundOn)
                return;
        }

        // Lấy clip ngẫu nhiên
        AudioClip clip = sound.GetRandomClip();
        if (clip == null)
        {
            Debug.LogWarning($"No valid clip found for {audioType}");
            return;
        }

        if (sound.isMusic)
        {
            if (sound.source != null)
            {
                sound.source.pitch = sound.GetRandomPitch();
                sound.source.PlayOneShot(clip, sound.volume * masterVolume * volumeScale);
            }
        }
        else
        {
            // SFX: Lấy AudioSource từ pool
            AudioSource source = GetAvailableAudioSource(audioType);
            if (source != null)
            {
                source.pitch = sound.GetRandomPitch();
                source.PlayOneShot(clip, sound.volume * masterVolume * volumeScale);
            }
        }
    }

    /// <summary>
    /// Dừng âm thanh
    /// </summary>
    public void Stop(AudioType audioType)
    {
        if (!soundDictionary.TryGetValue(audioType, out Sound sound))
            return;

        if (sound.isMusic)
        {
            if (sound.source != null)
                sound.source.Stop();
        }
        else
        {
            // Dừng tất cả AudioSource trong pool
            if (audioSourcePools.TryGetValue(audioType, out List<AudioSource> pool))
            {
                foreach (AudioSource source in pool)
                {
                    if (source.isPlaying)
                        source.Stop();
                }
            }
        }
    }

    /// <summary>
    /// Tạm dừng âm thanh
    /// </summary>
    public void Pause(AudioType audioType)
    {
        if (!soundDictionary.TryGetValue(audioType, out Sound sound))
            return;

        if (sound.isMusic)
        {
            if (sound.source != null)
                sound.source.Pause();
        }
        else
        {
            // Tạm dừng tất cả AudioSource trong pool
            if (audioSourcePools.TryGetValue(audioType, out List<AudioSource> pool))
            {
                foreach (AudioSource source in pool)
                {
                    if (source.isPlaying)
                        source.Pause();
                }
            }
        }
    }

    /// <summary>
    /// Tiếp tục phát âm thanh
    /// </summary>
    public void Resume(AudioType audioType)
    {
        if (!soundDictionary.TryGetValue(audioType, out Sound sound))
            return;

        if (sound.isMusic)
        {
            if (sound.source != null)
                sound.source.UnPause();
        }
        else
        {
            // Resume tất cả AudioSource trong pool
            if (audioSourcePools.TryGetValue(audioType, out List<AudioSource> pool))
            {
                foreach (AudioSource source in pool)
                {
                    source.UnPause();
                }
            }
        }
    }

    /// <summary>
    /// Kiểm tra xem âm thanh có đang phát không
    /// </summary>
    public bool IsPlaying(AudioType audioType)
    {
        if (!soundDictionary.TryGetValue(audioType, out Sound sound))
            return false;

        if (sound.isMusic)
        {
            return sound.source != null && sound.source.isPlaying;
        }
        else
        {
            // Kiểm tra có AudioSource nào đang phát không
            if (audioSourcePools.TryGetValue(audioType, out List<AudioSource> pool))
            {
                foreach (AudioSource source in pool)
                {
                    if (source.isPlaying)
                        return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Dừng tất cả âm thanh (không bao gồm nhạc)
    /// </summary>
    public void StopAllSounds()
    {
        foreach (var pool in audioSourcePools.Values)
        {
            foreach (var source in pool)
            {
                if (source != null && source.isPlaying)
                    source.Stop();
            }
        }
    }

    /// <summary>
    /// Dừng tất cả nhạc nền
    /// </summary>
    public void StopAllMusic()
    {
        foreach (var source in activeMusicSources)
        {
            if (source != null)
                source.Stop();
        }
    }

    /// <summary>
    /// Dừng tất cả âm thanh và nhạc
    /// </summary>
    public void StopAll()
    {
        StopAllSounds();
        StopAllMusic();
    }

    /// <summary>
    /// Cập nhật volume cho tất cả âm thanh
    /// </summary>
    public void UpdateAllVolumes()
    {
        foreach (var kvp in soundDictionary)
        {
            Sound sound = kvp.Value;
            AudioType audioType = kvp.Key;

            if (SettingsManager.Instance != null)
            {
                float targetVolume = sound.volume * masterVolume;

                if (sound.isMusic)
                {
                    targetVolume = SettingsManager.Instance.Data.isMusicOn ? targetVolume : 0f;
                    if (sound.source != null)
                        sound.source.volume = targetVolume;
                }
                else
                {
                    targetVolume = SettingsManager.Instance.Data.isSoundOn ? targetVolume : 0f;

                    // Cập nhật volume cho tất cả AudioSource trong pool
                    if (audioSourcePools.TryGetValue(audioType, out List<AudioSource> pool))
                    {
                        foreach (AudioSource source in pool)
                        {
                            if (source != null)
                                source.volume = targetVolume;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Đặt master volume
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    /// <summary>
    /// Fade in âm thanh
    /// </summary>
    public void FadeIn(AudioType audioType, float duration)
    {
        if (soundDictionary.TryGetValue(audioType, out Sound sound))
        {
            if (sound.source != null)
            {
                // Lấy clip ngẫu nhiên và gán vào AudioSource
                AudioClip clip = sound.GetRandomClip();
                if (clip == null)
                {
                    Debug.LogWarning($"FadeIn: No clip found for {audioType}. Assign clips in Inspector!");
                    return;
                }

                sound.source.clip = clip;
                sound.source.pitch = sound.GetRandomPitch();

                StartCoroutine(FadeInCoroutine(sound.source, sound.volume * masterVolume, duration));
            }
        }
    }

    private System.Collections.IEnumerator FadeInCoroutine(AudioSource source, float targetVolume, float duration)
    {
        source.volume = 0f;
        source.Play();

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, elapsedTime / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }

    /// <summary>
    /// Fade out âm thanh
    /// </summary>
    public void FadeOut(AudioType audioType, float duration)
    {
        if (soundDictionary.TryGetValue(audioType, out Sound sound))
        {
            if (sound.source != null)
            {
                StartCoroutine(FadeOutCoroutine(sound.source, duration));
            }
        }
    }

    private System.Collections.IEnumerator FadeOutCoroutine(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);
            yield return null;
        }

        source.volume = 0f;
        source.Stop();
    }

    /// <summary>
    /// Phát nhạc nền
    /// </summary>
    public void PlayMusic(AudioType musicType, bool fadeIn = false, float fadeDuration = 1f)
    {
        StopAllMusic();

        if (fadeIn)
        {
            FadeIn(musicType, fadeDuration);
        }
        else
        {
            Play(musicType);
        }
    }

    /// <summary>
    /// Chuyển đổi nhạc nền
    /// </summary>
    public void CrossfadeMusic(AudioType fromMusic, AudioType toMusic, float duration = 1f)
    {
        if (IsPlaying(fromMusic))
        {
            FadeOut(fromMusic, duration);
        }

        FadeIn(toMusic, duration);
    }
}
