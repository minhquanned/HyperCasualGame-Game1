using UnityEngine;

/// <summary>
/// Ví dụ về cách sử dụng AudioManager với multiple clips và randomization
/// </summary>
public class AudioExample : MonoBehaviour
{
    [Header("Examples")]
    [Tooltip("Test phát âm thanh với nhiều biến thể")]
    public bool testMultipleShots = false;

    private void Update()
    {
        if (testMultipleShots)
        {
            testMultipleShots = false;
            TestMultipleShots();
        }
    }

    /// <summary>
    /// Ví dụ: Phát nhiều âm thanh shoot liên tiếp
    /// Mỗi lần phát sẽ random một clip khác nhau và pitch khác nhau
    /// </summary>
    private void TestMultipleShots()
    {
        StartCoroutine(PlayMultipleShotsCoroutine());
    }

    private System.Collections.IEnumerator PlayMultipleShotsCoroutine()
    {
        for (int i = 0; i < 5; i++)
        {
            // Mỗi lần gọi sẽ phát một clip ngẫu nhiên với pitch ngẫu nhiên
            AudioManager.Instance.PlayOneShot(AudioType.Shoot);
            yield return new WaitForSeconds(0.3f);
        }
    }

    // ========== VÍ DỤ SỬ DỤNG TRONG GAME ==========

    /// <summary>
    /// Ví dụ: Khi nhân vật bắn
    /// </summary>
    public void OnPlayerShoot()
    {
        // Mỗi lần bắn sẽ phát một trong các âm thanh shoot ngẫu nhiên
        AudioManager.Instance.PlayOneShot(AudioType.Shoot);
    }

    /// <summary>
    /// Ví dụ: Khi enemy bị hit
    /// Nếu có nhiều âm thanh hit khác nhau, sẽ random phát
    /// </summary>
    public void OnEnemyHit()
    {
        AudioManager.Instance.PlayOneShot(AudioType.Hit);
    }

    /// <summary>
    /// Ví dụ: UI button click
    /// Có thể có nhiều âm thanh click khác nhau để tạo sự đa dạng
    /// </summary>
    public void OnButtonClick()
    {
        AudioManager.Instance.PlayOneShot(AudioType.UITap);
    }

    /// <summary>
    /// Ví dụ: Thu thập coin
    /// Mỗi coin có thể phát âm thanh hơi khác nhau
    /// </summary>
    public void OnCollectCoin()
    {
        AudioManager.Instance.PlayOneShot(AudioType.Collect);
    }

    /// <summary>
    /// Ví dụ: Nhiều enemy cùng chết
    /// </summary>
    public void OnMultipleEnemiesDie(int count)
    {
        StartCoroutine(PlayMultipleExplosions(count));
    }

    private System.Collections.IEnumerator PlayMultipleExplosions(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Mỗi explosion sẽ có âm thanh hơi khác nhau
            AudioManager.Instance.PlayOneShot(AudioType.Explosion);
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// Ví dụ: Phát âm thanh với volume tùy chỉnh dựa trên khoảng cách
    /// </summary>
    public void OnDistantExplosion(float distance)
    {
        // Volume giảm dần theo khoảng cách
        float volumeScale = 1f / (1f + distance * 0.1f);
        AudioManager.Instance.PlayWithVolume(AudioType.Explosion, volumeScale);
    }
}
