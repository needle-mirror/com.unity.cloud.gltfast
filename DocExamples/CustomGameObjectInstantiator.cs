// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_ANIMATION
namespace GLTFast.Documentation.Examples
{
#region CustomGameObjectInstantiator
    using UnityEngine;

    public class CustomGameObjectInstantiator : GameObjectInstantiator
    {
        public CustomGameObjectInstantiator(IGltfReadable gltf, Transform parent) : base(gltf, parent) { }

        public override void AddAnimation(AnimationClip[] animationClips)
        {
            // add an Animator component to the root object of the imported scene
            SceneTransform.gameObject.AddComponent<Animator>();

            // add a custom component containing your Playables API implementation
            var playAnimation = SceneTransform.gameObject.AddComponent<PlayAnimationUtilitiesSample>();
            playAnimation.clip = animationClips[0];
        }
    }
#endregion
}
#endif
