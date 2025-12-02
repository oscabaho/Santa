using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Santa.Core.Utils
{
    public static class AsyncUtils
    {
        /// <summary>
        /// Waits for a specified duration in seconds, respecting Unity's Time.timeScale.
        /// Optimized with UniTask to avoid allocations.
        /// </summary>
        public static async UniTask Wait(float seconds, CancellationToken token = default)
        {
            if (seconds <= 0) return;

            // UniTask.Delay with DelayType.DeltaTime respects Time.timeScale
            // and is allocation-free, unlike Task.Delay
            await UniTask.Delay(
                TimeSpan.FromSeconds(seconds), 
                delayTiming: PlayerLoopTiming.Update,
                cancellationToken: token
            );
        }

        /// <summary>
        /// Waits for a specified duration in real seconds, ignoring Time.timeScale.
        /// Optimized with UniTask to avoid allocations.
        /// </summary>
        public static async UniTask WaitRealtime(float seconds, CancellationToken token = default)
        {
            if (seconds <= 0) return;
            
            // UniTask.Delay with UnscaledDeltaTime ignores Time.timeScale
            // and is allocation-free
            await UniTask.Delay(
                TimeSpan.FromSeconds(seconds),
                ignoreTimeScale: true,
                cancellationToken: token
            );
        }
    }
}
