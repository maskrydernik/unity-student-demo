using UnityEngine;

namespace MiniWoW
{
    /// <summary>
    /// Debug helper to verify scene setup
    /// Add this to any GameObject to see what's in the scene
    /// </summary>
    public class SceneDebugger : MonoBehaviour
    {
        [Header("Auto-run on Start")]
        public bool debugOnStart = true;

        private void Start()
        {
            if (debugOnStart)
            {
                DebugScene();
            }
        }

        [ContextMenu("Debug Scene Setup")]
        public void DebugScene()
        {
            Debug.Log("=== SCENE DEBUG INFO ===");
            
            // Check cameras
            var cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            Debug.Log($"Cameras in scene: {cameras.Length}");
            foreach (var cam in cameras)
            {
                Debug.Log($"  - {cam.name} (Tag: {cam.tag}, Enabled: {cam.enabled}, Depth: {cam.depth})");
            }

            // Check EventSystem
            var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            Debug.Log($"EventSystem: {(eventSystem ? "Found" : "MISSING!")}");

            // Check UI Canvases
            var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            Debug.Log($"Canvases in scene: {canvases.Length}");
            foreach (var canvas in canvases)
            {
                Debug.Log($"  - {canvas.name} (Render Mode: {canvas.renderMode}, Sort Order: {canvas.sortingOrder})");
            }

            // Check UI components
            var playerFrameUI = FindFirstObjectByType<PlayerFrameUI>();
            Debug.Log($"PlayerFrameUI: {(playerFrameUI ? $"Found on {playerFrameUI.gameObject.name}" : "MISSING!")}");

            var targetFrameUI = FindFirstObjectByType<TargetFrameUI>();
            Debug.Log($"TargetFrameUI: {(targetFrameUI ? $"Found on {targetFrameUI.gameObject.name}" : "MISSING!")}");

            var abilityBarUI = FindFirstObjectByType<AbilityBarUI>();
            Debug.Log($"AbilityBarUI: {(abilityBarUI ? $"Found on {abilityBarUI.gameObject.name}" : "MISSING!")}");

            // Check spawner
            var spawner = FindFirstObjectByType<SimplePlayerSpawner>();
            if (spawner)
            {
                Debug.Log($"SimplePlayerSpawner: Found on {spawner.gameObject.name}");
                Debug.Log("  - Mode: ClassTemplates (auto-discovered)");
            }
            else
            {
                Debug.Log("SimplePlayerSpawner: MISSING!");
            }

            // Check player
            var player = GameObject.Find("Player");
            if (player)
            {
                Debug.Log($"Player: Found");
                Debug.Log($"  - Has Health: {player.GetComponent<Health>() != null}");
                Debug.Log($"  - Has AbilitySystem: {player.GetComponent<AbilitySystem>() != null}");
                Debug.Log($"  - Has TargetingSystem: {player.GetComponent<TargetingSystem>() != null}");
                Debug.Log($"  - Has PlayerMotor: {player.GetComponent<PlayerMotor>() != null}");
            }
            else
            {
                Debug.Log("Player: Not spawned yet (this is normal before selection)");
            }

            // Check training dummies
            var dummies = FindObjectsByType<TrainingDummy>(FindObjectsSortMode.None);
            Debug.Log($"Training Dummies: {dummies.Length}");
            foreach (var dummy in dummies)
            {
                var health = dummy.GetComponent<Health>();
                Debug.Log($"  - {dummy.name} (Faction: {health?.Faction}, HP: {health?.Current}/{health?.Max})");
            }

            Debug.Log("=== END DEBUG INFO ===");
        }
    }
}
