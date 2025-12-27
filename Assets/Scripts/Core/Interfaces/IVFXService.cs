using UnityEngine;

namespace Santa.Core
{
    /// <summary>
    /// Interface for visual effects (VFX) service.
    /// Handles playing particle effects and visual feedback.
    /// </summary>
    public interface IVFXService
    {
        /// <summary>
        /// Plays a visual effect at a specified position.
        /// </summary>
        /// <param name="key">The Addressable key or identifier for the effect to play.</param>
        /// <param name="position">The world position where the effect should appear.</param>
        /// <param name="rotation">Optional rotation for the effect. Uses identity if null.</param>
        /// <returns>The instantiated effect GameObject.</returns>
        GameObject PlayEffect(string key, Vector3 position, Quaternion? rotation = null);
        
        /// <summary>
        /// Plays a fade-out effect on a GameObject and destroys it after completion.
        /// </summary>
        /// <param name="targetObject">The GameObject to fade out and destroy.</param>
        /// <param name="duration">The duration of the fade effect in seconds.</param>
        void PlayFadeAndDestroyEffect(GameObject targetObject, float duration);
    }
}
