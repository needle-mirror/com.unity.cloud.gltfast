// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_ANIMATION
namespace GLTFast.Documentation.Examples
{
#region PlayAnimationUtilitiesSample
    using UnityEngine;
    using UnityEngine.Playables;

    [RequireComponent(typeof(Animator))]
    public class PlayAnimationUtilitiesSample : MonoBehaviour
    {
        public AnimationClip clip;
        PlayableGraph playableGraph;

        void Start()
        {
            AnimationPlayableUtilities.PlayClip(GetComponent<Animator>(), clip, out playableGraph);
        }

        void OnDisable()
        {
            // Destroys all Playables and Outputs created by the graph.
            playableGraph.Destroy();
        }
    }
#endregion
}
#endif
