using UnityEngine;

[System.Serializable]
public class Sound
{
    public AudioType audioType;
    
    [Tooltip("Danh sách các audio clips. Nếu có nhiều hơn 1 clip, sẽ phát ngẫu nhiên")]
    public AudioClip[] clips;
    
    [Range(0f, 1f)]
    public float volume = 1f;
    
    [Range(0.1f, 3f)]
    public float pitch = 1f;
    
    [Tooltip("Randomize pitch mỗi lần phát (±pitchVariation)")]
    [Range(0f, 0.5f)]
    public float pitchVariation = 0f;
    
    public bool loop = false;
    public bool isMusic = false;
    
    [HideInInspector]
    public AudioSource source;
    
    /// <summary>
    /// Lấy một AudioClip ngẫu nhiên từ danh sách
    /// </summary>
    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Length == 0)
            return null;
            
        if (clips.Length == 1)
            return clips[0];
            
        return clips[Random.Range(0, clips.Length)];
    }
    
    /// <summary>
    /// Lấy pitch với variation ngẫu nhiên
    /// </summary>
    public float GetRandomPitch()
    {
        if (pitchVariation == 0f)
            return pitch;
            
        return pitch + Random.Range(-pitchVariation, pitchVariation);
    }
}
