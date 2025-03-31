// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GLTFast
{
    class AnimationLoopPlayable : PlayableBehaviour
    {
        int m_Index;
        bool m_AutoSequence;

        internal AnimationMixerPlayable Mixer { get; private set; }
        internal float Time { get; set; }

        public void Init(Playable owner, PlayableGraph graph, bool autoSequence, params AnimationClip[] clips)
        {
            if (clips is null)
                throw new ArgumentNullException(nameof(clips));

            if (clips.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(clips));

            m_AutoSequence = autoSequence;

            owner.SetInputCount(1);
            owner.SetInputWeight(0, 1);

            Mixer = AnimationMixerPlayable.Create(graph, clips.Length);
            graph.Connect(Mixer, 0, owner, 0);

            for (var i = 0; i < clips.Length; ++i)
            {
                graph.Connect(AnimationClipPlayable.Create(graph, clips[i]), 0, Mixer, i);
                Mixer.SetInputWeight(i, i == 0 ? 1f : 0f);
            }
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            Time -= info.deltaTime;

            if (Time > 0f)
                return;

            if (m_AutoSequence)
            {
                Mixer.SetInputWeight(m_Index, 0f);
                m_Index = ++m_Index % Mixer.GetInputCount();
                Mixer.SetInputWeight(m_Index, 1f);
            }

            var clip = (AnimationClipPlayable)Mixer.GetInput(m_Index);
            clip.SetTime(0);
            Time = clip.GetAnimationClip().length;
        }
    }
}
