using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Santa.Core.Transitions
{
    /// <summary>
    /// A ScriptableObject that holds a series of TransitionTasks to be executed in order as a UniTask.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTransitionSequence", menuName = "Transitions/Transition Sequence")]
    public class TransitionSequence : ScriptableObject
    {
        [SerializeReference] // Use SerializeReference to allow polymorphism for the abstract TransitionTask
        public List<TransitionTask> Tasks = new List<TransitionTask>();

        public async UniTask Execute(TransitionContext context)
        {
            foreach (var task in Tasks)
            {
                if (task != null)
                {
                    await task.Execute(context);
                }
            }
        }
    }
}
