using System;
using UnityEngine;

public interface IGameplayUIService
{
    // Signal when gameplay UI is fully ready for interaction (e.g., action button registered)
    bool IsReady { get; }
    event Action Ready;

    void ShowActionButton(bool show);
    void RegisterActionButton(GameObject button);
    void UnregisterActionButton(GameObject button);
}
