using UnityEngine;
using UnityEditor;

/// <summary>
/// Tool để test GameManager trong Unity Editor
/// </summary>
public class GameManagerTester : EditorWindow
{
    private GameManager gameManager;

    [MenuItem("Tools/Game Manager Tester")]
    public static void ShowWindow()
    {
        GetWindow<GameManagerTester>("Game Manager Test");
    }

    private void OnGUI()
    {
        GUILayout.Label("Game Manager Testing Tool", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Tìm GameManager
        if (GUILayout.Button("Find GameManager"))
        {
            gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                EditorGUIUtility.PingObject(gameManager);
                Debug.Log("✓ Đã tìm thấy GameManager!");
            }
            else
            {
                Debug.LogWarning("✗ Không tìm thấy GameManager trong scene!");
            }
        }

        GUILayout.Space(10);
        EditorGUI.BeginDisabledGroup(gameManager == null || !Application.isPlaying);

        GUILayout.Label("Win/Lose Testing:", EditorStyles.boldLabel);

        if (GUILayout.Button("🏆 Test WIN", GUILayout.Height(30)))
        {
            if (gameManager != null)
            {
                // Gọi WinGame thông qua reflection vì nó là private
                var method = gameManager.GetType().GetMethod("WinGame",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(gameManager, null);
                    Debug.Log("✓ Test: Đã gọi WinGame()");
                }
            }
        }

        if (GUILayout.Button("💀 Test LOSE", GUILayout.Height(30)))
        {
            if (gameManager != null)
            {
                // Gọi LoseGame thông qua reflection vì nó là private
                var method = gameManager.GetType().GetMethod("LoseGame",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(gameManager, null);
                    Debug.Log("✓ Test: Đã gọi LoseGame()");
                }
            }
        }

        EditorGUI.EndDisabledGroup();

        GUILayout.Space(10);
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Chạy game (Play Mode) để test Win/Lose", MessageType.Info);
        }

        if (gameManager == null)
        {
            EditorGUILayout.HelpBox("Click 'Find GameManager' để tìm GameManager trong scene", MessageType.Warning);
        }
    }
}
