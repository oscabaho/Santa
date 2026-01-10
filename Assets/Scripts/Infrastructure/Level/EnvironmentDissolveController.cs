using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Santa.Infrastructure.Level
{
    /// <summary>
    /// Controls the global shader variables for the World Space Dissolve effect.
    /// Drives the _GlobalDissolveRadius parameter to creating the liberation wave.
    /// </summary>
    public class EnvironmentDissolveController : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private float debugMaxRadius = 100f;
        [SerializeField] private float debugDuration = 5f;

        private static readonly int CenterID = Shader.PropertyToID("_GlobalDissolveCenter");
        private static readonly int RadiusID = Shader.PropertyToID("_GlobalDissolveRadius");

        private CancellationTokenSource _dissolveCancellation;

        private void OnDestroy()
        {
            ResetShaders();
        }

        private void OnApplicationQuit()
        {
            ResetShaders();
        }

        /// <summary>
        /// Resets the shader globals so objects appear normal (Gentrified visible, Liberated hidden).
        /// </summary>
        public void ResetShaders()
        {
            _dissolveCancellation?.Cancel();
            _dissolveCancellation?.Dispose();
            _dissolveCancellation = null;

            // Radius 0 means:
            // Gentrified (Clip outside radius): Visible everywhere (since distance > 0 is always true if radius is -1 or 0 effectively)
            // Wait, logic check:
            // Gentrified Clip: transition = dist - radius. Clip if transition < 0.
            // If Radius = 0. Dist is always > 0. Transition > 0. NO CLIP. Visible. Correct.
            // Liberated Clip: Clip if -transition < 0  => transition > 0.
            // If Radius = 0. Dist > 0. Transition > 0. CLIP. Invisible. Correct.
            
            Shader.SetGlobalFloat(RadiusID, 0f);
            Shader.SetGlobalVector(CenterID, Vector3.zero);
        }

        /// <summary>
        /// Starts the dissolve effect animation.
        /// </summary>
        public async UniTask AnimateDissolveAsync(Vector3 center, float maxRadius, float duration)
        {
            // Cancel previous
            _dissolveCancellation?.Cancel();
            _dissolveCancellation?.Dispose();
            _dissolveCancellation = new CancellationTokenSource();
            var ct = _dissolveCancellation.Token;

            Shader.SetGlobalVector(CenterID, center);
            
            float time = 0f;
            while (time < duration)
            {
                if (ct.IsCancellationRequested) return;

                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                
                // Optional: Add easing here
                float currentRadius = Mathf.Lerp(0f, maxRadius, t);
                
                Shader.SetGlobalFloat(RadiusID, currentRadius);
                
                await UniTask.Yield();
            }

            // Ensure final state
            Shader.SetGlobalFloat(RadiusID, maxRadius);
        }

        [ContextMenu("Test Dissolve Here")]
        private void DebugTestDissolve()
        {
            AnimateDissolveAsync(transform.position, debugMaxRadius, debugDuration).Forget();
        }

        [ContextMenu("Reset Dissolve")]
        private void DebugReset()
        {
            ResetShaders();
        }
    }
}
