// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using AOT;
using GLTFast.Vertex;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace GLTFast.Jobs
{

    using Schema;

    [BurstCompile]
    static unsafe class CachedFunction
    {

        public delegate int GetIndexDelegate(void* baseAddress, int index);
        public delegate void GetFloat3Delegate(float3* destination, void* src);

        // Cached function pointers
        static FunctionPointer<GetIndexDelegate> s_GetIndexValueInt8Method;
        static FunctionPointer<GetIndexDelegate> s_GetIndexValueUInt8Method;
        static FunctionPointer<GetIndexDelegate> s_GetIndexValueInt16Method;
        static FunctionPointer<GetIndexDelegate> s_GetIndexValueUInt16Method;
        static FunctionPointer<GetIndexDelegate> s_GetIndexValueUInt32Method;


        static FunctionPointer<GetFloat3Delegate> s_GetFloat3FloatMethod;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3Int8Method;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3UInt8Method;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3Int16Method;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3UInt16Method;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3UInt32Method;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3Int8NormalizedMethod;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3UInt8NormalizedMethod;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3Int16NormalizedMethod;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3UInt16NormalizedMethod;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3UInt32NormalizedMethod;

        /// <summary>
        /// Returns Burst compatible function that retrieves an index value
        /// </summary>
        /// <param name="format">Data type of index</param>
        /// <returns>Burst Function Pointer to correct conversion function</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static FunctionPointer<GetIndexDelegate> GetIndexConverter(GltfComponentType format)
        {
            switch (format)
            {
                case GltfComponentType.UnsignedByte:
                    if (!s_GetIndexValueUInt8Method.IsCreated)
                    {
                        s_GetIndexValueUInt8Method = BurstCompiler.CompileFunctionPointer<GetIndexDelegate>(GetIndexValueUInt8);
                    }
                    return s_GetIndexValueUInt8Method;
                case GltfComponentType.Byte:
                    if (!s_GetIndexValueInt8Method.IsCreated)
                    {
                        s_GetIndexValueInt8Method = BurstCompiler.CompileFunctionPointer<GetIndexDelegate>(GetIndexValueInt8);
                    }
                    return s_GetIndexValueInt8Method;
                case GltfComponentType.UnsignedShort:
                    if (!s_GetIndexValueUInt16Method.IsCreated)
                    {
                        s_GetIndexValueUInt16Method = BurstCompiler.CompileFunctionPointer<GetIndexDelegate>(GetIndexValueUInt16);
                    }
                    return s_GetIndexValueUInt16Method;
                case GltfComponentType.Short:
                    if (!s_GetIndexValueInt16Method.IsCreated)
                    {
                        s_GetIndexValueInt16Method = BurstCompiler.CompileFunctionPointer<GetIndexDelegate>(GetIndexValueInt16);
                    }
                    return s_GetIndexValueInt16Method;
                case GltfComponentType.UnsignedInt:
                    if (!s_GetIndexValueUInt32Method.IsCreated)
                    {
                        s_GetIndexValueUInt32Method = BurstCompiler.CompileFunctionPointer<GetIndexDelegate>(GetIndexValueUInt32);
                    }
                    return s_GetIndexValueUInt32Method;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        public static FunctionPointer<GetFloat3Delegate> GetPositionConverter(
            GltfComponentType format,
            bool normalized
            )
        {
            if (normalized)
            {
                switch (format)
                {
                    case GltfComponentType.Float:
                        // Floats cannot be normalized.
                        // Fall back to non-normalized below
                        break;
                    case GltfComponentType.Byte:
                        if (!s_GetFloat3Int8NormalizedMethod.IsCreated)
                        {
                            s_GetFloat3Int8NormalizedMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3Int8Normalized);
                        }
                        return s_GetFloat3Int8NormalizedMethod;
                    case GltfComponentType.UnsignedByte:
                        if (!s_GetFloat3UInt8NormalizedMethod.IsCreated)
                        {
                            s_GetFloat3UInt8NormalizedMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt8Normalized);
                        }
                        return s_GetFloat3UInt8NormalizedMethod;
                    case GltfComponentType.Short:
                        if (!s_GetFloat3Int16NormalizedMethod.IsCreated)
                        {
                            s_GetFloat3Int16NormalizedMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3Int16Normalized);
                        }
                        return s_GetFloat3Int16NormalizedMethod;
                    case GltfComponentType.UnsignedShort:
                        if (!s_GetFloat3UInt16NormalizedMethod.IsCreated)
                        {
                            s_GetFloat3UInt16NormalizedMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt16Normalized);
                        }
                        return s_GetFloat3UInt16NormalizedMethod;
                    case GltfComponentType.UnsignedInt:
                        if (!s_GetFloat3UInt32NormalizedMethod.IsCreated)
                        {
                            s_GetFloat3UInt32NormalizedMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt32Normalized);
                        }
                        return s_GetFloat3UInt32NormalizedMethod;
                }
            }
            switch (format)
            {
                case GltfComponentType.Float:
                    if (!s_GetFloat3FloatMethod.IsCreated)
                    {
                        s_GetFloat3FloatMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3Float);
                    }
                    return s_GetFloat3FloatMethod;
                case GltfComponentType.Byte:
                    if (!s_GetFloat3Int8Method.IsCreated)
                    {
                        s_GetFloat3Int8Method = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3Int8);
                    }
                    return s_GetFloat3Int8Method;
                case GltfComponentType.UnsignedByte:
                    if (!s_GetFloat3UInt8Method.IsCreated)
                    {
                        s_GetFloat3UInt8Method = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt8);
                    }
                    return s_GetFloat3UInt8Method;
                case GltfComponentType.Short:
                    if (!s_GetFloat3Int16Method.IsCreated)
                    {
                        s_GetFloat3Int16Method = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3Int16);
                    }
                    return s_GetFloat3Int16Method;
                case GltfComponentType.UnsignedShort:
                    if (!s_GetFloat3UInt16Method.IsCreated)
                    {
                        s_GetFloat3UInt16Method = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt16);
                    }
                    return s_GetFloat3UInt16Method;
                case GltfComponentType.UnsignedInt:
                    if (!s_GetFloat3UInt32Method.IsCreated)
                    {
                        s_GetFloat3UInt32Method = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt32);
                    }
                    return s_GetFloat3UInt32Method;
            }
            throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetIndexDelegate))]
        static int GetIndexValueUInt8(void* baseAddress, int index)
        {
            return *((byte*)baseAddress + index);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetIndexDelegate))]
        static int GetIndexValueInt8(void* baseAddress, int index)
        {
            return *(((sbyte*)baseAddress) + index);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetIndexDelegate))]
        static int GetIndexValueUInt16(void* baseAddress, int index)
        {
            return *(((ushort*)baseAddress) + index);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetIndexDelegate))]
        static int GetIndexValueInt16(void* baseAddress, int index)
        {
            return *(((short*)baseAddress) + index);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetIndexDelegate))]
        static int GetIndexValueUInt32(void* baseAddress, int index)
        {
            return (int)*(((uint*)baseAddress) + index);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3Float(float3* destination, void* src)
        {
            destination->x = -*(float*)src;
            destination->y = *((float*)src + 1);
            destination->z = *((float*)src + 2);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3Int8(float3* destination, void* src)
        {
            destination->x = -*(sbyte*)src;
            destination->y = *((sbyte*)src + 1);
            destination->z = *((sbyte*)src + 2);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt8(float3* destination, void* src)
        {
            destination->x = -*(byte*)src;
            destination->y = *((byte*)src + 1);
            destination->z = *((byte*)src + 2);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3Int16(float3* destination, void* src)
        {
            destination->x = -*(short*)src;
            destination->y = *((short*)src + 1);
            destination->z = *((short*)src + 2);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt16(float3* destination, void* src)
        {
            destination->x = -*(ushort*)src;
            destination->y = *((ushort*)src + 1);
            destination->z = *((ushort*)src + 2);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt32(float3* destination, void* src)
        {
            destination->x = -*(uint*)src;
            destination->y = *((uint*)src + 1);
            destination->z = *((uint*)src + 2);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3Int8Normalized(float3* destination, void* src)
        {
            destination->x = -max(*(sbyte*)src / 127f, -1);
            destination->y = max(*((sbyte*)src + 1) / 127f, -1);
            destination->z = max(*((sbyte*)src + 2) / 127f, -1);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt8Normalized(float3* destination, void* src)
        {
            destination->x = -*(byte*)src / 255f;
            destination->y = *((byte*)src + 1) / 255f;
            destination->z = *((byte*)src + 2) / 255f;
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3Int16Normalized(float3* destination, void* src)
        {
            destination->x = -max(*(short*)src / (float)short.MaxValue, -1f);
            destination->y = max(*((short*)src + 1) / (float)short.MaxValue, -1f);
            destination->z = max(*((short*)src + 2) / (float)short.MaxValue, -1f);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt16Normalized(float3* destination, void* src)
        {
            destination->x = -*(ushort*)src / (float)ushort.MaxValue;
            destination->y = *((ushort*)src + 1) / (float)ushort.MaxValue;
            destination->z = *((ushort*)src + 2) / (float)ushort.MaxValue;
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt32Normalized(float3* destination, void* src)
        {
            destination->x = -*(uint*)src / (float)uint.MaxValue;
            destination->y = *((uint*)src + 1) / (float)uint.MaxValue;
            destination->z = *((uint*)src + 2) / (float)uint.MaxValue;
        }
    }

    [BurstCompile]
    struct CreateIndicesInt32Job : IJobParallelFor
    {
        [WriteOnly]
        public NativeArray<int> result;

        public void Execute(int index)
        {
            result[index] = index;
        }
    }

    [BurstCompile]
    struct CreateIndicesInt32FlippedJob : IJobParallelFor
    {
        [WriteOnly]
        public NativeArray<int> result;

        public void Execute(int index)
        {
            result[index] = index - 2 * (index % 3 - 1);
        }
    }

    [BurstCompile]
    struct CreateIndicesForTriangleStripJob : IJobParallelFor
    {
        [WriteOnly]
        public NativeArray<int> result;

        public void Execute(int index)
        {
            // Source https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html
            // Triangle Strips
            // One triangle primitive is defined by each vertex and the two vertices that follow it, according to the equation:
            // pi = { vi, vi + (1 + i % 2), vi + (2 - i % 2)}
            // We change first and second indices for Unity

            var triangleIndex = index / 3;
            result[index] = (index % 3) switch
            {
                0 => triangleIndex + (1 + triangleIndex % 2),
                1 => triangleIndex,
                2 => triangleIndex + (2 - triangleIndex % 2),
                _ => result[index]
            };
        }
    }

    [BurstCompile]
    struct CreateIndicesForTriangleFanJob : IJobParallelFor
    {
        [WriteOnly]
        public NativeArray<int> result;

        public void Execute(int index)
        {
            // Source https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html
            // Triangle Fans
            // Triangle primitives are defined around a shared common vertex, according to the equation:
            // pi = {vi+1, vi+2, v0}
            // We change first and second indices for Unity

            var triangleIndex = index / 3;
            result[index] = (index % 3) switch
            {
                0 => triangleIndex + 2,
                1 => triangleIndex + 1,
                _ => 0
            };
        }
    }

    [BurstCompile]
    struct RecalculateIndicesForTriangleStripJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<int> input;

        [WriteOnly, NativeDisableParallelForRestriction]
        public NativeArray<int> result;

        public void Execute(int index)
        {
            // Source https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html
            // Triangle Strips
            // One triangle primitive is defined by each vertex and the two vertices that follow it, according to the equation:
            // pi = { vi, vi + (1 + i % 2), vi + (2 - i % 2)}
            // We change first and second indices for Unity

            var triangleIndex = index * 3;
            result[triangleIndex + 1] = input[index];
            result[triangleIndex] = input[index + (1 + index % 2)];
            result[triangleIndex + 2] = input[index + (2 - index % 2)];
        }
    }

    [BurstCompile]
    struct RecalculateIndicesForTriangleFanJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<int> input;

        [WriteOnly, NativeDisableParallelForRestriction]
        public NativeArray<int> result;

        public void Execute(int index)
        {
            // Source https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html
            // Triangle Fans
            // Triangle primitives are defined around a shared common vertex, according to the equation:
            // pi = {vi+1, vi+2, v0}
            // We change first and second indices for Unity

            var triangleIndex = index * 3;
            result[triangleIndex + 1] = input[index + 1];
            result[triangleIndex] = input[index + 2];
            result[triangleIndex + 2] = 0;
        }
    }

    [BurstCompile]
    struct ConvertIndicesUInt8ToInt32Job : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<byte>.ReadOnly input;

        [WriteOnly]
        public NativeArray<int> result;

        public void Execute(int index)
        {
            result[index] = input[index];
        }
    }

    [BurstCompile]
    struct ConvertIndicesUInt8ToInt32FlippedJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<byte3>.ReadOnly input;

        [WriteOnly]
        public NativeArray<int3> result;

        public void Execute(int index)
        {
            result[index] = input[index].GltfToUnityTriangleIndies();
        }
    }

    [BurstCompile]
    struct ConvertIndicesUInt16ToInt32FlippedJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<ushort3>.ReadOnly input;

        [WriteOnly]
        public NativeArray<int3> result;

        public void Execute(int index)
        {
            result[index] = input[index].GltfToUnityTriangleIndies();
        }
    }

    [BurstCompile]
    struct ConvertIndicesUInt16ToInt32Job : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<ushort>.ReadOnly input;

        [WriteOnly]
        public NativeArray<int> result;

        public void Execute(int index)
        {
            result[index] = input[index];
        }
    }

    [BurstCompile]
    struct ConvertIndicesUInt32ToInt32Job : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<uint>.ReadOnly input;

        [WriteOnly]
        public NativeArray<int> result;

        public void Execute(int index)
        {
            result[index] = (int)input[index];
        }
    }

    [BurstCompile]
    struct ConvertIndicesUInt32ToInt32FlippedJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<uint3>.ReadOnly input;

        [WriteOnly]
        public NativeArray<int3> result;

        public void Execute(int index)
        {
            var idx = input[index];
            result[index] = new int3((int)idx.x, (int)idx.z, (int)idx.y);
        }
    }

    [BurstCompile]
    unsafe struct ConvertUVsUInt8ToFloatInterleavedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {
        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float2* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float2*)((byte*)result + startIndex * outputByteStride);
            var off = input + (startIndex * inputByteStride);

            for (var index = 0; index < count; index++)
            {
                *resultV = new float2(off[0], 1 - off[1]);
                resultV = (float2*)((byte*)resultV + outputByteStride);
                off += inputByteStride;
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float2*)(((byte*)result) + (index * outputByteStride));
            var off = input + inputByteStride * index;
            *resultV = new float2(off[0], 1 - off[1]);
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertUVsUInt8ToFloatInterleavedNormalizedJob : IJobParallelFor
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float2* result;

        public void Execute(int index)
        {
            var resultV = (float2*)(((byte*)result) + (index * outputByteStride));
            var off = input + inputByteStride * index;
            var tmp = new float2(
                off[0],
                off[1]
                ) / 255f;
            tmp.y = 1 - tmp.y;
            *resultV = tmp;
        }
    }

    [BurstCompile]
    unsafe struct ConvertUVsUInt16ToFloatInterleavedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float2* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float2*)((byte*)result + startIndex * outputByteStride);
            var uv = (ushort*)(input + startIndex * inputByteStride);

            for (var index = 0; index < count; index++)
            {
                *resultV = new float2(uv[0], 1 - uv[1]);
                resultV = (float2*)((byte*)resultV + outputByteStride);
                uv = (ushort*)((byte*)uv + inputByteStride);
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float2*)(((byte*)result) + (index * outputByteStride));
            var uv = (ushort*)(input + inputByteStride * index);
            *resultV = new float2(uv[0], 1 - uv[1]);
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertUVsUInt16ToFloatInterleavedNormalizedJob : IJobParallelFor
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float2* result;

        public void Execute(int index)
        {
            var resultV = (float2*)(((byte*)result) + (index * outputByteStride));
            var uv = (ushort*)(input + inputByteStride * index);
            var tmp = new float2(
                uv[0],
                uv[1]
            ) / ushort.MaxValue;
            tmp.y = 1 - tmp.y;
            *resultV = tmp;
        }
    }

    [BurstCompile]
    unsafe struct ConvertUVsInt16ToFloatInterleavedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public short* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float2* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float2*)((byte*)result + startIndex * outputByteStride);
            var uv = (short*)((byte*)input + startIndex * inputByteStride);

            for (var index = 0; index < count; index++)
            {
                *resultV = new float2(uv[0], 1 - uv[1]);
                resultV = (float2*)((byte*)resultV + outputByteStride);
                uv = (short*)((byte*)uv + inputByteStride);
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float2*)(((byte*)result) + (index * outputByteStride));
            var uv = (short*)((byte*)input + inputByteStride * index);
            *resultV = new float2(uv[0], 1 - uv[1]);
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertUVsInt16ToFloatInterleavedNormalizedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public short* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float2* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float2*)((byte*)result + startIndex * outputByteStride);
            var uv = (short*)((byte*)input + startIndex * inputByteStride);

            for (var index = 0; index < count; index++)
            {
                var tmp = new float2(uv[0], uv[1]) / short.MaxValue;
                var tmp2 = max(tmp, -1f);
                tmp2.y = 1 - tmp2.y;
                *resultV = tmp2;

                resultV = (float2*)((byte*)resultV + outputByteStride);
                uv = (short*)((byte*)uv + inputByteStride);
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float2*)(((byte*)result) + (index * outputByteStride));
            var uv = (short*)((byte*)input + inputByteStride * index);

            var tmp = new float2(uv[0], uv[1]) / short.MaxValue;
            var tmp2 = max(tmp, -1f);
            tmp2.y = 1 - tmp2.y;
            *resultV = tmp2;
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertUVsInt8ToFloatInterleavedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public sbyte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float2* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float2*)((byte*)result + startIndex * outputByteStride);
            var off = input + startIndex * inputByteStride;

            for (var index = 0; index < count; index++)
            {
                *resultV = new float2(off[0], 1 - off[1]);
                resultV = (float2*)((byte*)resultV + outputByteStride);
                off += inputByteStride;
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float2*)(((byte*)result) + (index * outputByteStride));
            var off = input + inputByteStride * index;
            *resultV = new float2(off[0], 1 - off[1]);
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertUVsInt8ToFloatInterleavedNormalizedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public sbyte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float2* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float2*)((byte*)result + startIndex * outputByteStride);
            var off = input + startIndex * inputByteStride;

            for (var index = 0; index < count; index++)
            {
                var tmp = new float2(off[0], off[1]) / 127f;
                var tmp2 = max(tmp, -1f);
                tmp2.y = 1 - tmp2.y;
                *resultV = tmp2;

                resultV = (float2*)((byte*)resultV + outputByteStride);
                off += inputByteStride;
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float2*)(((byte*)result) + (index * outputByteStride));
            var off = input + inputByteStride * index;
            var tmp = new float2(off[0], off[1]) / 127f;
            var tmp2 = max(tmp, -1f);
            tmp2.y = 1 - tmp2.y;
            *resultV = tmp2;
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertColorsRGBFloatToRGBAFloatJob : IJobParallelFor
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [WriteOnly]
        public NativeArray<float4> result;

        public void Execute(int index)
        {
            var src = (float3*)(input + (index * inputByteStride));
            result[index] = new float4(*src, 1f);
        }
    }

    [BurstCompile]
    unsafe struct ConvertColorsRgbUInt8ToRGBAFloatJob : IJobParallelFor
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [WriteOnly]
        public NativeArray<float4> result;

        public void Execute(int index)
        {
            var src = input + (index * inputByteStride);
            result[index] = new float4(
                new float3(src[0], src[1], src[2]) / byte.MaxValue,
                1f
            );
        }
    }

    [BurstCompile]
    unsafe struct ConvertColorsRgbUInt16ToRGBAFloatJob : IJobParallelFor
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public ushort* input;

        [WriteOnly]
        public NativeArray<float4> result;

        public void Execute(int index)
        {
            var src = (ushort*)(((byte*)input) + (index * inputByteStride));
            result[index] = new float4(
                new float3(src[0], src[1], src[2]) / ushort.MaxValue,
                1f
            );
        }
    }

    [BurstCompile]
    unsafe struct ConvertColorsRgbaUInt16ToRGBAFloatJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public ushort* input;

        [WriteOnly]
        public NativeArray<float4> result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var src = (ushort*)((byte*)input + startIndex * inputByteStride);
            var endIndex = startIndex + count;
            for (var index = startIndex; index < endIndex; index++)
            {
                result[index] = new float4(
                    src[0] / (float)ushort.MaxValue,
                    src[1] / (float)ushort.MaxValue,
                    src[2] / (float)ushort.MaxValue,
                    src[3] / (float)ushort.MaxValue
                );
                src = (ushort*)((byte*)src + inputByteStride);
            }
        }
#else
        public void Execute(int index)
        {
            var src = (ushort*)(((byte*)input) + (index * inputByteStride));
            result[index] = new float4(
                src[0] / (float)ushort.MaxValue,
                src[1] / (float)ushort.MaxValue,
                src[2] / (float)ushort.MaxValue,
                src[3] / (float)ushort.MaxValue
            );
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertColorsRGBAFloatToRGBAFloatJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [WriteOnly]
        public NativeArray<float4> result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var src = (float4*)(input + startIndex * inputByteStride);
            var endIndex = startIndex + count;
            for (var index = startIndex; index < endIndex; index++)
            {
                result[index] = *src;
                src = (float4*)((byte*)src + inputByteStride);
            }
        }
#else
        public void Execute(int index)
        {
            var src = (float4*)(input + (index * inputByteStride));
            result[index] = *src;
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertColorsRgbaUInt8ToRGBAFloatJob : IJobParallelFor
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [WriteOnly]
        public NativeArray<float4> result;

        public void Execute(int index)
        {
            var src = input + (index * inputByteStride);
            result[index] = new float4(
                src[0] / (float)byte.MaxValue,
                src[1] / (float)byte.MaxValue,
                src[2] / (float)byte.MaxValue,
                src[3] / (float)byte.MaxValue
            );
        }
    }

    [BurstCompile]
    unsafe struct MemCopyJob : IJob
    {

        [ReadOnly]
        public long bufferSize;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public void* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public void* result;

        public void Execute()
        {
            UnsafeUtility.MemCpy(result, input, bufferSize);
        }
    }

    /// <summary>
    /// General purpose vector 3 (position or normal) conversion
    /// </summary>
    [BurstCompile]
    struct ConvertVector3FloatToFloatJob : IJobParallelFor
    {
        [ReadOnly]
        public ReadOnlyNativeStridedArray<float3> input;

        [WriteOnly]
        public NativeArray<float3> result;

        public void Execute(int index)
        {
            var tmp = input[index];
            tmp.x *= -1;
            result[index] = tmp;
        }
    }

    [BurstCompile]
    struct ConvertRotationsFloatToFloatJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<float4>.ReadOnly input;

        [WriteOnly]
        public NativeArray<quaternion> result;

        public void Execute(int index)
        {
            var tmp = input[index];
            tmp.y *= -1;
            tmp.z *= -1;
            result[index] = tmp;
        }
    }

    [BurstCompile]
    struct ConvertRotationsInt16ToFloatJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<short4>.ReadOnly input;

        [WriteOnly]
        public NativeArray<quaternion> result;

        public void Execute(int index)
        {
            result[index] = input[index].GltfToUnityRotation();
        }
    }

    /// <summary>
    /// Converts an array of glTF space quaternions (normalized, signed bytes) to
    /// Quaternions in Unity space (floats).
    /// </summary>
    [BurstCompile]
    struct ConvertRotationsInt8ToFloatJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<sbyte4>.ReadOnly input;

        [WriteOnly]
        public NativeArray<quaternion> result;

        public void Execute(int index)
        {
            result[index] = input[index].GltfToUnityRotation();
        }
    }

    [BurstCompile]
    unsafe struct ConvertUVsFloatToFloatInterleavedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float2* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float2*)((byte*)result + startIndex * outputByteStride);
            var off = (float2*)(input + startIndex * inputByteStride);

            for (var index = 0; index < count; index++)
            {
                var tmp = *off;
                tmp.y = 1 - tmp.y;
                *resultV = tmp;

                resultV = (float2*)((byte*)resultV + outputByteStride);
                off = (float2*)((byte*)off + inputByteStride);
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float2*)(((byte*)result) + (index * outputByteStride));
            var off = (float2*)(input + (index * inputByteStride));
            var tmp = *off;
            tmp.y = 1 - tmp.y;
            *resultV = tmp;
        }
#endif
    }

    /// <summary>
    /// General purpose vector 3 (position or normal) conversion
    /// </summary>
    [BurstCompile]
    unsafe struct ConvertVector3FloatToFloatInterleavedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {
        [ReadOnly]
        public ReadOnlyNativeStridedArray<float3> input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);

            var end = startIndex + count;
            for (var index = startIndex; index < end; index++)
            {
                var tmp = input[index];
                tmp.x *= -1;
                *resultV = tmp;

                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float3*)(((byte*)result) + (index * outputByteStride));
            var tmp = input[index];
            tmp.x *= -1;
            *resultV = tmp;
        }
#endif
    }

    /// <summary>
    /// General purpose sparse vector 3 (position or normal) conversion
    /// </summary>
    [BurstCompile]
    unsafe struct ConvertVector3SparseJob : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public void* indexBuffer;

        public FunctionPointer<CachedFunction.GetIndexDelegate> indexConverter;

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public void* input;

        public FunctionPointer<CachedFunction.GetFloat3Delegate> valueConverter;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

        public void Execute(int index)
        {
            var resultIndex = indexConverter.Invoke(indexBuffer, index);
            var resultV = (float3*)(((byte*)result) + (resultIndex * outputByteStride));
            valueConverter.Invoke(resultV, (byte*)input + index * inputByteStride);
        }
    }

    [BurstCompile]
    unsafe struct ConvertTangentsFloatToFloatInterleavedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float4*)((byte*)result + startIndex * outputByteStride);
            var off = (float4*)(input + startIndex * inputByteStride);

            for (var index = 0; index < count; index++)
            {
                var tmp = *off;
                tmp.z *= -1;
                *resultV = tmp;

                resultV = (float4*)((byte*)resultV + outputByteStride);
                off = (float4*)((byte*)off + inputByteStride);
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float4*)(((byte*)result) + (index * outputByteStride));
            var off = input + (index * inputByteStride);
            var tmp = *((float4*)off);
            tmp.z *= -1;
            *resultV = tmp;
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertBoneWeightsFloatToFloatInterleavedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float4*)((byte*)result + startIndex * outputByteStride);
            var off = (float4*)(input + startIndex * inputByteStride);

            for (var index = 0; index < count; index++)
            {
                *resultV = *off;
                resultV = (float4*)((byte*)resultV + outputByteStride);
                off = (float4*)((byte*)off + inputByteStride);
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float4*)(((byte*)result) + (index * outputByteStride));
            var off = input + (index * inputByteStride);
            *resultV = *((float4*)off);
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertBoneWeightsUInt8ToFloatInterleavedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float4*)((byte*)result + startIndex * outputByteStride);
            var off = input + startIndex * inputByteStride;

            for (var index = 0; index < count; index++)
            {
                *resultV = new float4(
                    off[0] / 255f,
                    off[1] / 255f,
                    off[2] / 255f,
                    off[3] / 255f
                    );
                resultV = (float4*)((byte*)resultV + outputByteStride);
                off += inputByteStride;
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float4*)(((byte*)result) + (index * outputByteStride));
            var off = input + (index * inputByteStride);
            *resultV = new float4(
                off[0] / 255f,
                off[1] / 255f,
                off[2] / 255f,
                off[3] / 255f
            );
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertBoneWeightsUInt16ToFloatInterleavedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float4*)((byte*)result + startIndex * outputByteStride);
            var off = (ushort*)(input + startIndex * inputByteStride);

            for (var index = 0; index < count; index++)
            {
                *resultV = new float4(
                    off[0] / (float)ushort.MaxValue,
                    off[1] / (float)ushort.MaxValue,
                    off[2] / (float)ushort.MaxValue,
                    off[3] / (float)ushort.MaxValue
                    );
                resultV = (float4*)((byte*)resultV + outputByteStride);
                off = (ushort*)((byte*)off + inputByteStride);
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float4*)(((byte*)result) + (index * outputByteStride));
            var off = (ushort*)(input + index * inputByteStride);
            *resultV = new float4(
                off[0] / (float)ushort.MaxValue,
                off[1] / (float)ushort.MaxValue,
                off[2] / (float)ushort.MaxValue,
                off[3] / (float)ushort.MaxValue
            );
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertTangentsInt16ToFloatInterleavedNormalizedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public short* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float4*)((byte*)result + startIndex * outputByteStride);
            var off = (short*)((byte*)input + startIndex * inputByteStride);

            for (var index = 0; index < count; index++)
            {
                var tmp = new float4(off[0], off[1], off[2], off[3]) / short.MaxValue;
                var tmp2 = max(tmp, -1f);
                tmp2.z *= -1;
                *resultV = normalize(tmp2);

                resultV = (float4*)((byte*)resultV + outputByteStride);
                off = (short*)((byte*)off + inputByteStride);
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float4*)(((byte*)result) + (index * outputByteStride));
            var off = (short*)(((byte*)input) + (index * inputByteStride));
            var tmp = new float4(off[0], off[1], off[2], off[3]) / short.MaxValue;
            var tmp2 = max(tmp, -1f);
            tmp2.z *= -1;
            *resultV = normalize(tmp2);
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertTangentsInt8ToFloatInterleavedNormalizedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public sbyte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float4*)((byte*)result + startIndex * outputByteStride);
            var off = input + startIndex * inputByteStride;

            for (var index = 0; index < count; index++)
            {
                var tmp = new float4(off[0], off[1], off[2], off[3]) / 127f;
                var tmp2 = max(tmp, -1f);
                tmp2.z *= -1;
                *resultV = normalize(tmp2);

                resultV = (float4*)((byte*)resultV + outputByteStride);
                off += inputByteStride;
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float4*)(((byte*)result) + (index * outputByteStride));
            var off = input + (index * inputByteStride);
            var tmp = new float4(off[0], off[1], off[2], off[3]) / 127f;
            var tmp2 = max(tmp, -1f);
            tmp2.z *= -1;
            *resultV = normalize(tmp2);
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertPositionsUInt16ToFloatInterleavedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {
        [ReadOnly]
        public ReadOnlyNativeStridedArray<ushort3> input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);
            var end = startIndex + count;

            for (var index = startIndex; index < end; index++)
            {
                *resultV = input[index].GltfToUnityFloat3();
                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float3*)(((byte*)result) + (index * outputByteStride));
            *resultV = input[index].GltfToUnityFloat3();
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertPositionsUInt16ToFloatInterleavedNormalizedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {
        [ReadOnly]
        public ReadOnlyNativeStridedArray<ushort3> input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);
            var end = startIndex + count;

            for (var index = startIndex; index < end; index++)
            {
                *resultV = input[index].GltfToUnityNormalizedFloat3();
                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float3*)(((byte*)result) + (index * outputByteStride));
            *resultV = input[index].GltfToUnityNormalizedFloat3();
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertPositionsInt16ToFloatInterleavedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {
        [ReadOnly]
        public ReadOnlyNativeStridedArray<short3> input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

        [ReadOnly]
        public int outputByteStride;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);
            var end = startIndex + count;

            for (var index = startIndex; index < end; index++)
            {
                *resultV = input[index].GltfToUnityFloat3();
                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float3*)(((byte*)result) + (index * outputByteStride));
            *resultV = input[index].GltfToUnityFloat3();
        }
#endif
    }

    /// <summary>
    /// General purpose (position / morph target delta normal)
    /// Result is not normalized (scaled to unit length)
    /// </summary>
    [BurstCompile]
    unsafe struct ConvertVector3Int16ToFloatInterleavedNormalizedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {

        [ReadOnly]
        public ReadOnlyNativeStridedArray<short3> input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);
            var end = startIndex + count;

            for (var index = startIndex; index < end; index++)
            {
                *resultV = input[index].GltfToUnityNormalizedFloat3();
                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float3*)(((byte*)result) + (index * outputByteStride));
            *resultV = input[index].GltfToUnityNormalizedFloat3();
        }
#endif
    }

    /// <summary>
    /// Normal conversion
    /// Result is normalized (scaled to unit length)
    /// </summary>
    [BurstCompile]
    unsafe struct ConvertNormalsInt16ToFloatInterleavedNormalizedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {

        [ReadOnly]
        public ReadOnlyNativeStridedArray<short3> input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);
            var end = startIndex + count;

            for (var index = startIndex; index < end; index++)
            {
                *resultV = input[index].GltfNormalToUnityFloat3();
                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float3*)(((byte*)result) + (index * outputByteStride));
            *resultV = input[index].GltfNormalToUnityFloat3();
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertPositionsInt8ToFloatInterleavedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {
        [ReadOnly]
        public ReadOnlyNativeStridedArray<sbyte3> input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);
            var end = startIndex + count;

            for (var index = startIndex; index < end; index++)
            {
                *resultV = input[index].GltfToUnityFloat3();
                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float3*)(((byte*)result) + (index * outputByteStride));
            *resultV = input[index].GltfToUnityFloat3();
        }
#endif
    }

    /// <summary>
    /// General purpose conversion (positions or morph target delta normals)
    /// Result is not normalized (scaled to unit length)
    /// </summary>
    [BurstCompile]
    unsafe struct ConvertVector3Int8ToFloatInterleavedNormalizedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {
        [ReadOnly]
        public ReadOnlyNativeStridedArray<sbyte3> input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);
            var end = startIndex + count;

            for (var index = startIndex; index < end; index++)
            {
                *resultV = input[index].GltfToUnityNormalizedFloat3();
                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float3*)(((byte*)result) + (index * outputByteStride));
            *resultV = input[index].GltfToUnityNormalizedFloat3();
        }
#endif
    }

    /// <summary>
    /// Normal conversion
    /// Result is normalized (scaled to unit length)
    /// </summary>
    [BurstCompile]
    unsafe struct ConvertNormalsInt8ToFloatInterleavedNormalizedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {
        [ReadOnly]
        public ReadOnlyNativeStridedArray<sbyte3> input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);
            var end = startIndex + count;

            for (var index = startIndex; index < end; index++)
            {
                *resultV = input[index].GltfNormalToUnityFloat3();
                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float3*)(((byte*)result) + (index * outputByteStride));
            *resultV = input[index].GltfNormalToUnityFloat3();
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertPositionsUInt8ToFloatInterleavedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {
        [ReadOnly]
        public ReadOnlyNativeStridedArray<byte3> input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);
            var end = startIndex + count;

            for (var index = startIndex; index < end; index++)
            {
                *resultV = input[index].GltfToUnityFloat3();
                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float3*)(((byte*)result) + (index * outputByteStride));
            *resultV = input[index].GltfToUnityFloat3();
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertPositionsUInt8ToFloatInterleavedNormalizedJob :
#if UNITY_COLLECTIONS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {
        [ReadOnly]
        public ReadOnlyNativeStridedArray<byte3> input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

#if UNITY_COLLECTIONS
        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);
            var end = startIndex + count;

            for (var index = startIndex; index < end; index++)
            {
                *resultV = input[index].GltfToUnityNormalizedFloat3();
                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
#else
        public void Execute(int index)
        {
            var resultV = (float3*)(((byte*)result) + (index * outputByteStride));
            *resultV = input[index].GltfToUnityNormalizedFloat3();
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertBoneJointsUInt8ToUInt32Job : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int inputByteStride;

        [WriteOnly]
        [NativeDisableUnsafePtrRestriction]
        public uint4* result;

        [ReadOnly]
        public int outputByteStride;

        public void Execute(int index)
        {
            var resultV = (uint4*)(((byte*)result) + (index * outputByteStride));
            var off = input + (index * inputByteStride);
            *resultV = new uint4(off[0], off[1], off[2], off[3]);
        }
    }

    [BurstCompile]
    unsafe struct ConvertBoneJointsUInt16ToUInt32Job : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int inputByteStride;

        [WriteOnly]
        [NativeDisableUnsafePtrRestriction]
        public uint4* result;

        [ReadOnly]
        public int outputByteStride;

        public void Execute(int index)
        {
            var resultV = (uint4*)(((byte*)result) + (index * outputByteStride));
            var off = (ushort*)(input + (index * inputByteStride));
            *resultV = new uint4(off[0], off[1], off[2], off[3]);
        }
    }

    [BurstCompile]
    struct SortAndNormalizeBoneWeightsJob : IJobParallelFor
    {

        public NativeArray<VBones> bones;

        /// <summary>
        /// Number of skin weights that are taken into account (project quality setting)
        /// </summary>
        public int skinWeights;


        public unsafe void Execute(int index)
        {
            var v = bones[index];

            // Most joints/weights are already sorted by weight
            // Detect and early return if true
            var sortedAndNormalized = true;
            for (var i = 0; i < 3; i++)
            {
                var a = v.weights[i];
                var b = v.weights[i + 1];
                if (a < b)
                {
                    sortedAndNormalized = false;
                    break;
                }
            }

            // Sort otherwise
            if (!sortedAndNormalized)
            {
                for (var i = 0; i < skinWeights; i++)
                {
                    var max = v.weights[i];
                    var maxI = i;

                    for (var j = i + 1; j < 4; j++)
                    {
                        var value = v.weights[j];
                        if (v.weights[j] > max)
                        {
                            max = value;
                            maxI = j;
                        }
                    }

                    if (maxI > i)
                    {
                        Swap(ref v, maxI, i);
                    }
                }
            }

            // Calculate the sum of weights
            var weightSum = 0f;
            for (var i = 0; i < skinWeights; i++)
            {
                weightSum += v.weights[i];
            }
            if (abs(weightSum - 1.0f) > 2e-7f && weightSum > 0)
            {
                sortedAndNormalized = false;
                // Re-normalize the weight sum
                for (var i = 0; i < skinWeights; i++)
                {
                    v.weights[i] /= weightSum;
                }
            }

            if (!sortedAndNormalized)
            {
                bones[index] = v;
            }
        }

        static unsafe void Swap(ref VBones v, int a, int b)
        {
            (v.weights[a], v.weights[b]) = (v.weights[b], v.weights[a]);
            (v.joints[a], v.joints[b]) = (v.joints[b], v.joints[a]);
        }
    }

#if GLTFAST_SAFE
    [BurstCompile]
    struct RenormalizeBoneWeightsJob : IJobParallelFor {

        public NativeArray<VBones> bones;

        public unsafe void Execute(int index) {
            var v = bones[index];

            // Calculate the sum of weights
            var weightSum = v.weights[0] + v.weights[1] + v.weights[2] + v.weights[3];
            if (abs(weightSum - 1.0f) > 2e-7f && weightSum > 0) {
                // Re-normalize the weight sum
                for (var i = 0; i < 4; i++) {
                    v.weights[i] /= weightSum;
                }
            }

            bones[index] = v;
        }
    }
#endif

    [BurstCompile]
    struct ConvertMatricesJob : IJobParallelFor
    {

        [ReadOnly]
        public NativeArray<float4x4>.ReadOnly input;

        [WriteOnly]
        public NativeArray<float4x4> result;

        public void Execute(int index)
        {
            var tmp = input[index];
            result[index] = new float4x4(
                tmp.c0.x, -tmp.c1.x, -tmp.c2.x, -tmp.c3.x,
                -tmp.c0.y, tmp.c1.y, tmp.c2.y, tmp.c3.y,
                -tmp.c0.z, tmp.c1.z, tmp.c2.z, tmp.c3.z,
                tmp.c0.w, tmp.c1.w, tmp.c2.w, tmp.c3.w
                );
        }
    }

    [BurstCompile]
    struct ConvertScalarInt8ToFloatNormalizedJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<sbyte>.ReadOnly input;

        [WriteOnly]
        public NativeArray<float> result;

        public void Execute(int index)
        {
            result[index] = max(input[index] / 127f, -1.0f);
        }
    }

    [BurstCompile]
    struct ConvertScalarUInt8ToFloatNormalizedJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<byte>.ReadOnly input;

        [WriteOnly]
        public NativeArray<float> result;

        public void Execute(int index)
        {
            result[index] = input[index] / 255f;
        }
    }

    [BurstCompile]
    struct ConvertScalarInt16ToFloatNormalizedJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<short>.ReadOnly input;

        [WriteOnly]
        public NativeArray<float> result;

        public void Execute(int index)
        {
            result[index] = max(input[index] / (float)short.MaxValue, -1.0f);
        }
    }

    [BurstCompile]
    struct ConvertScalarUInt16ToFloatNormalizedJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<ushort>.ReadOnly input;

        [WriteOnly]
        public NativeArray<float> result;

        public void Execute(int index)
        {
            result[index] = input[index] / (float)ushort.MaxValue;
        }
    }
}
