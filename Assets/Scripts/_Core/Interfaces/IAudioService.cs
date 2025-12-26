using UnityEngine;
using Santa.Domain.Combat;

namespace Santa.Core
{
    /// <summary>
    /// Interface for audio playback service.
    /// Handles 2D/3D sound effects, looping sounds, and music playback.
    /// </summary>
    public interface IAudioService
    {
        /// <summary>
        /// Plays a 2D sound effect that is not positional.
        /// </summary>
        /// <param name="audioData">The audio data configuration to play.</param>
        void PlaySound2D(AudioData audioData);
        
        /// <summary>
        /// Plays a 3D positional sound effect at a specific world position.
        /// </summary>
        /// <param name="audioData">The audio data configuration to play.</param>
        /// <param name="position">The world position where the sound should play.</param>
        void PlaySound3D(AudioData audioData, Vector3 position);
        
        /// <summary>
        /// Plays a looping sound attached to a specific GameObject.
        /// </summary>
        /// <param name="audioData">The audio data configuration to loop.</param>
        /// <param name="target">The GameObject to attach the looping sound to.</param>
        void PlayLoopingSoundOnObject(AudioData audioData, GameObject target);
        
        /// <summary>
        /// Stops any looping sound currently playing on the specified GameObject.
        /// </summary>
        /// <param name="target">The GameObject whose looping sound should stop.</param>
        void StopLoopingSoundOnObject(GameObject target);
        
        /// <summary>
        /// Plays background music.
        /// </summary>
        /// <param name="musicClip">The music clip to play.</param>
        /// <param name="loop">Whether the music should loop. Defaults to true.</param>
        void PlayMusic(AudioClip musicClip, bool loop = true);
    }
}
