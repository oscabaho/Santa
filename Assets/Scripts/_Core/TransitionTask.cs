using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// An abstract ScriptableObject representing a single step in a transition sequence.
/// </summary>
public abstract class TransitionTask : ScriptableObject
{
    /// <summary>
    /// Executes the task as an async UniTask.
    /// </summary>
    /// <param name="context">The context providing access to scene objects.</param>
    /// <returns>A UniTask to allow the task to run asynchronously.</returns>
    public abstract UniTask Execute(TransitionContext context);
}
