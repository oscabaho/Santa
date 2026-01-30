using System;
using UnityEngine;

namespace Santa.Core
{
    /// <summary>
    /// Interface for managing gameplay UI elements during exploration.
    /// Handles action button visibility and registration.
    /// </summary>
    public interface IGameplayUIService
    {
        /// <summary>
        /// Gets whether the gameplay UI is fully ready for interaction.
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// Event fired when the gameplay UI becomes ready for interaction.
        /// </summary>
        event Action Ready;

        /// <summary>
        /// Shows or hides the action button.
        /// </summary>
        /// <param name="show">True to show the action button, false to hide it.</param>
        /// <param name="onAction">The callback to execute when the button is clicked (optional).</param>
        void ShowActionButton(bool show, Action onAction = null);

        /// <summary>
        /// Registers an action button GameObject with the service.
        /// </summary>
        /// <param name="button">The button GameObject to register.</param>
        void RegisterActionButton(GameObject button);

        /// <summary>
        /// Unregisters a previously registered action button.
        /// </summary>
        /// <param name="button">The button GameObject to unregister.</param>
        void UnregisterActionButton(GameObject button);
    }
}
