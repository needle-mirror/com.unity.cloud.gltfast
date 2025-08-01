// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANIMATION
using UnityEngine.Playables;
#endif

namespace GLTFast
{
    /// <summary>
    /// Descriptor of a glTF scene instance
    /// </summary>
    public class GameObjectSceneInstance
    {

        /// <summary>
        /// List of instantiated cameras
        /// </summary>
        public IReadOnlyList<Camera> Cameras => m_Cameras;
        /// <summary>
        /// List of instantiated lights
        /// </summary>
        public IReadOnlyList<Light> Lights => m_Lights;

        /// <summary>
        /// Enables controlling and applying materials variants.
        /// </summary>
        public MaterialsVariantsControl MaterialsVariantsControl { get; private set; }

#if UNITY_ANIMATION
        /// <summary>
        /// <see cref="Animation" /> component. Is null if scene has no
        /// animation clips.
        /// Only available if the built-in Animation module is enabled.
        /// </summary>
        public Animation LegacyAnimation { get; private set; }

        /// <summary>
        /// <a href="https://docs.unity3d.com/Manual/Playables.html">Playables</a> support has been removed since
        /// it was not usable in builds. Use LegacyAnimation instead.
        /// See: <a href="https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@6.13/manual/UseCaseCustomPlayablesAnimation.html">UseCaseCustomPlayablesAnimation</a>
        /// </summary>
        [Obsolete("Playables support has been removed since it was not usable in builds. Use LegacyAnimation instead. " +
            "See: <a href=\"https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@6.13/manual/UseCaseCustomPlayablesAnimation.html\">UseCaseCustomPlayablesAnimation</a>")]
        public Playable? Playable { get; internal set; }
#endif

        List<Camera> m_Cameras;
        List<Light> m_Lights;

        /// <summary>
        /// Adds a camera
        /// </summary>
        /// <param name="camera">Camera to be added</param>
        internal void AddCamera(Camera camera)
        {
            if (m_Cameras == null)
            {
                m_Cameras = new List<Camera>();
            }
            m_Cameras.Add(camera);
        }

        internal void AddLight(Light light)
        {
            if (m_Lights == null)
            {
                m_Lights = new List<Light>();
            }
            m_Lights.Add(light);
        }

        internal void SetMaterialsVariantsControl(MaterialsVariantsControl control)
        {
            MaterialsVariantsControl = control;
        }

#if UNITY_ANIMATION
        internal void SetLegacyAnimation(Animation animation) {
            LegacyAnimation = animation;
        }
#endif
    }
}
