using System.Collections;
using UnityEngine;

/// <summary>
/// An abstract ScriptableObject representing a single step in a transition sequence.
/// </summary>
public abstract class TransitionTask : ScriptableObject
{
    /// <summary>
    /// Executes the task as a coroutine.
    /// </summary>
    /// <param name="context">The context providing access to scene objects.</param>
    /// <returns>An IEnumerator to allow the task to run over multiple frames.</returns>
    public abstract IEnumerator Execute(TransitionContext context);
}
