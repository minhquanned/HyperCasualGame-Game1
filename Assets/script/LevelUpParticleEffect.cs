using UnityEngine;

/// <summary>
/// Script quản lý hiệu ứng particle khi tower lên cấp
/// Tự động destroy sau khi particle kết thúc
/// </summary>
public class LevelUpParticleEffect : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lifetime = 2f; // Thời gian tồn tại của particle effect
    [SerializeField] private ParticleSystem[] particleSystems; // Các particle systems

    private void Start()
    {
        // Tự động tìm tất cả particle systems nếu không được gán
        if (particleSystems == null || particleSystems.Length == 0)
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>();
        }

        // Cấu hình và play tất cả particle systems - chỉ chạy 1 lần
        float maxDuration = 0f;
        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps != null)
            {
                // Tắt loop để particle chỉ chạy 1 lần
                var main = ps.main;
                main.loop = false;

                // Play particle
                ps.Play();

                // Tính thời gian tồn tại tối đa
                float duration = main.duration + main.startLifetime.constantMax;
                if (duration > maxDuration)
                {
                    maxDuration = duration;
                }
            }
        }

        // Tự động destroy sau khi particle kết thúc (sử dụng thời gian thực tế hoặc lifetime)
        float destroyTime = maxDuration > 0 ? maxDuration : lifetime;
        Destroy(gameObject, destroyTime);
    }

    /// <summary>
    /// Spawn particle effect tại vị trí cụ thể
    /// </summary>
    public static void SpawnAt(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return;

        GameObject effect = Instantiate(prefab, position, Quaternion.identity);

        // Đảm bảo effect có component LevelUpParticleEffect
        if (effect.GetComponent<LevelUpParticleEffect>() == null)
        {
            LevelUpParticleEffect comp = effect.AddComponent<LevelUpParticleEffect>();
            comp.lifetime = 2f;
        }
    }

    /// <summary>
    /// Spawn particle effect tại vị trí cụ thể với parent
    /// </summary>
    public static void SpawnAt(GameObject prefab, Vector3 position, Transform parent)
    {
        if (prefab == null) return;

        GameObject effect = Instantiate(prefab, position, Quaternion.identity, parent);

        // Đảm bảo effect có component LevelUpParticleEffect
        if (effect.GetComponent<LevelUpParticleEffect>() == null)
        {
            LevelUpParticleEffect comp = effect.AddComponent<LevelUpParticleEffect>();
            comp.lifetime = 2f;
        }
    }
}
