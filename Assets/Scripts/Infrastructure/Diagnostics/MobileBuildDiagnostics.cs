using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Santa.Core;
using Santa.Infrastructure.Input;
using Santa.Presentation.HUD;

namespace Santa.Infrastructure.Diagnostics
{
    /// <summary>
    /// Mobile Build Diagnostics - Run this in your build to verify all systems are working.
    /// 
    /// Attach this to a GameObject in your main scene and check the logs when you build for mobile.
    /// It will tell you exactly what's broken (if anything).
    /// </summary>
    public class MobileBuildDiagnostics : MonoBehaviour
    {
        private void Start()
        {
            RunDiagnostics();
        }

        public void RunDiagnostics()
        {
            GameLog.Log("=== MOBILE BUILD DIAGNOSTICS START ===", this);

            DiagnoseEventSystem();
            DiagnoseInput();
            DiagnoseCamera();
            DiagnoseUI();
            DiagnoseButton();

            GameLog.Log("=== MOBILE BUILD DIAGNOSTICS END ===", this);
        }

        private void DiagnoseEventSystem()
        {
            GameLog.Log("\n[1] EVENTSYSTEM DIAGNOSTICS", this);

            var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                GameLog.LogError("  ✗ CRITICAL: EventSystem NOT FOUND in scene!", this);
                return;
            }

            GameLog.Log("  ✓ EventSystem found", this);

            var module = eventSystem.currentInputModule;
            if (module == null)
            {
                GameLog.LogError("  ✗ CRITICAL: currentInputModule is NULL!", this);
                return;
            }

            string moduleName = module.GetType().Name;
            GameLog.Log($"  ✓ Input Module: {moduleName}", this);

            if (moduleName != "InputSystemUIInputModule")
            {
                GameLog.LogError($"  ✗ WARNING: Using {moduleName} instead of InputSystemUIInputModule (New Input System).", this);
            }
        }

        private void DiagnoseInput()
        {
            GameLog.Log("\n[2] INPUT SYSTEM DIAGNOSTICS", this);

            var readers = Resources.FindObjectsOfTypeAll<InputReader>();
            if (readers == null || readers.Length == 0)
            {
                GameLog.LogError("  ✗ CRITICAL: InputReader NOT FOUND in Resources!", this);
                return;
            }

            GameLog.Log($"  ✓ Found {readers.Length} InputReader(s)", this);

            var inputReader = readers[0];
            GameLog.Log($"  ✓ Primary InputReader: '{inputReader.name}'", this);

            // Check if action maps exist (they will if OnEnable has been called)
            var moveEvent = inputReader.GetType().GetField("MoveEvent", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var interactEvent = inputReader.GetType().GetField("InteractEvent", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (interactEvent != null)
            {
                GameLog.Log("  ✓ InteractEvent found", this);
            }
            else
            {
                GameLog.LogWarning("  ? InteractEvent not found (may not be initialized yet)", this);
            }
        }

        private void DiagnoseCamera()
        {
            GameLog.Log("\n[3] CAMERA DIAGNOSTICS", this);

            var mainCam = UnityEngine.Camera.main;
            if (mainCam == null)
            {
                GameLog.LogError("  ✗ CRITICAL: Camera.main is NULL! Check that main camera is tagged 'MainCamera'.", this);
                return;
            }

            GameLog.Log($"  ✓ Camera.main found: {mainCam.gameObject.name}", this);
            GameLog.Log($"  ✓ Camera enabled: {mainCam.enabled}", this);
        }

        private void DiagnoseUI()
        {
            GameLog.Log("\n[4] UI SYSTEM DIAGNOSTICS", this);

            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameLog.LogError("  ✗ WARNING: No Canvas found in scene!", this);
                return;
            }

            GameLog.Log($"  ✓ Canvas found: {canvas.gameObject.name}", this);
            GameLog.Log($"  ✓ Canvas RenderMode: {canvas.renderMode}", this);

            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                if (canvas.worldCamera == null)
                {
                    GameLog.LogError("  ✗ CRITICAL: Canvas is ScreenSpaceCamera but worldCamera is NULL!", this);
                }
                else
                {
                    GameLog.Log($"  ✓ Canvas.worldCamera: {canvas.worldCamera.name}", this);
                }
            }

            var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster == null)
            {
                GameLog.LogError("  ✗ CRITICAL: Canvas missing GraphicRaycaster!", this);
            }
            else
            {
                GameLog.Log("  ✓ GraphicRaycaster found", this);
                GameLog.Log($"  ✓ GraphicRaycaster enabled: {raycaster.enabled}", this);
            }

            // Check CanvasGroups
            var canvasGroups = FindObjectsByType<CanvasGroup>(FindObjectsSortMode.None);
            if (canvasGroups.Length > 0)
            {
                GameLog.Log($"  ℹ Found {canvasGroups.Length} CanvasGroup(s) - checking for blocks:", this);
                foreach (var cg in canvasGroups)
                {
                    if (!cg.interactable)
                    {
                        GameLog.LogError($"    ✗ CanvasGroup '{cg.gameObject.name}' has interactable=false (BLOCKS INPUT)", this);
                    }
                    if (!cg.blocksRaycasts)
                    {
                        GameLog.LogWarning($"    ? CanvasGroup '{cg.gameObject.name}' has blocksRaycasts=false", this);
                    }
                }
            }
        }

        private void DiagnoseButton()
        {
            GameLog.Log("\n[5] ACTION BUTTON DIAGNOSTICS", this);

            var button = FindFirstObjectByType<Santa.Presentation.HUD.ActionButtonController>();
            if (button == null)
            {
                GameLog.LogWarning("  ? ActionButtonController not found (may not be instantiated yet)", this);
                return;
            }

            GameLog.Log($"  ✓ ActionButtonController found: {button.gameObject.name}", this);
            GameLog.Log($"  ✓ Button active: {button.gameObject.activeInHierarchy}", this);

            var uiButton = button.GetComponent<UnityEngine.UI.Button>();
            if (uiButton == null)
            {
                GameLog.LogError("  ✗ CRITICAL: Button component missing!", this);
            }
            else
            {
                GameLog.Log($"  ✓ Button component found", this);
                GameLog.Log($"  ✓ Button interactable: {uiButton.interactable}", this);
            }
        }
    }
}
