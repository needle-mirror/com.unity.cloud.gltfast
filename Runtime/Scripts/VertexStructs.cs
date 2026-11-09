// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace GLTFast.Vertex
{

    // Most struct members are never accessed, but keeping them public makes still sense for future use.
    // ReSharper disable MemberCanBePrivate.Global

    [StructLayout(LayoutKind.Sequential)]
    struct VPosNormTan
    {
        public float3 position;
        public float3 normal;
        public float4 tangent;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VPosNorm
    {
        public float3 position;
        public float3 normal;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VPos
    {
        public float3 position;
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe struct VBones
    {
        public fixed float weights[4];
        public fixed uint joints[4];

#if DEBUG
        public override string ToString()
        {
            return $"{joints[0]}/{weights[0]}, {joints[1]}/{weights[1]}, {joints[2]}/{weights[2]}, {joints[3]}/{weights[3]}";
        }
#endif
    }

    // ReSharper restore MemberCanBePrivate.Global
}
