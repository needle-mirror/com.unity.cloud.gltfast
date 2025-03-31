// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GLTFast
{
    [RequireComponent(typeof(Animator))]
    class AnimationPlayableComponent : MonoBehaviour
    {
        internal static readonly string k_NotInitializedMessage = $"{nameof(AnimationPlayableComponent)} was not initialized.";
        internal static readonly string k_InvalidGraphMessage = $"{nameof(PlayableGraph)} must still be valid before destroying.";

        [SerializeField]
        AnimationClip[] m_Clips;

        PlayableGraph m_PlayableGraph;

        public Playable? Playable { get; private set; }

        void Start()
        {
            Assert.IsTrue(m_PlayableGraph.IsValid(), k_NotInitializedMessage);
        }

        void OnDestroy()
        {
            Assert.IsTrue(m_PlayableGraph.IsValid(), k_InvalidGraphMessage);
            m_PlayableGraph.Destroy();
        }

        public void Init(AnimationClip[] clips, bool autoSequence)
        {
            m_Clips = clips;

            var animator = GetComponent<Animator>();
            m_PlayableGraph = PlayableGraph.Create("GltfPlayableGraph");
            m_PlayableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            var scriptPlayable = ScriptPlayable<AnimationLoopPlayable>.Create(m_PlayableGraph);
            scriptPlayable.GetBehaviour().Init(scriptPlayable, m_PlayableGraph, autoSequence, m_Clips);

            var playableOutput = AnimationPlayableOutput.Create(m_PlayableGraph, "GltfAnimationPlayableOutput", animator);
            playableOutput.SetSourcePlayable(scriptPlayable, 0);

            Playable = scriptPlayable;
        }
    }
}
