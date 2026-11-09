// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace GLTFast
{

    using Logging;
    using Schema;

    sealed class VertexBufferTexCoords : IDisposable
    {
        readonly ICodeLogger m_Logger;

        NativeArray<byte> m_Data;
        readonly bool m_OwnsData;
        readonly int m_ByteStride;
        readonly int m_ByteOffset;

        public unsafe VertexBufferTexCoords(int uvSetCount, int vertexCount, ICodeLogger logger)
        {
            m_Logger = logger;
            UVSetCount = uvSetCount;
            m_ByteStride = uvSetCount * sizeof(float2);
            m_Data = new NativeArray<byte>(
                vertexCount * m_ByteStride,
                VertexBufferGeneratorBase.defaultAllocator
            );
            m_OwnsData = true;
        }

        public VertexBufferTexCoords(
            int uvSetCount,
            NativeArray<byte> data,
            int byteStride,
            int byteOffset,
            ICodeLogger logger
            )
        {
            m_Logger = logger;
            UVSetCount = uvSetCount;
            m_Data = data;
            m_ByteStride = byteStride;
            m_ByteOffset = byteOffset;
        }

        public int UVSetCount { get; }

        public unsafe bool ScheduleVertexUVJobs(
            int offset,
            int vertexCount,
            int[] uvAccessorIndices,
            NativeArray<JobHandle> handles,
            IGltfBuffers buffers
            )
        {
            Profiler.BeginSample("ScheduleVertexUVJobs");
            Profiler.BeginSample("AllocateNativeArray");

            var vDataPtr = (byte*)m_Data.GetUnsafeReadOnlyPtr() + m_ByteOffset;
            Profiler.EndSample();

            for (var uvSet = 0; uvSet < UVSetCount; uvSet++)
            {
                var accIndex = uvAccessorIndices[uvSet];
                var uvAcc = buffers.GetAccessor(accIndex);
                if (uvAcc is { IsSparse: true })
                {
                    m_Logger?.Error(LogCode.SparseAccessor, "UVs");
                    continue;
                }
                if (!buffers.TryGetAccessorData(uvAcc, 2, out var data, out var byteStride)
                    || uvAcc.count > vertexCount)
                {
                    m_Logger?.Error(LogCode.AccessorAccessFailed, accIndex.ToString());
                    Profiler.EndSample();
                    return false;
                }
                var h = GetUvsJob(
                    data,
                    uvAcc.count,
                    uvAcc.componentType,
                    byteStride,
                    (float2*)(vDataPtr + m_ByteStride * offset + uvSet * sizeof(float2)),
                    m_ByteStride,
                    uvAcc.normalized
                );
                if (h.HasValue)
                {
                    handles[uvSet] = h.Value;
                }
                else
                {
                    Profiler.EndSample();
                    return false;
                }
            }
            Profiler.EndSample();
            return true;
        }

        public void AddDescriptors(VertexAttributeDescriptor[] dst, ref int offset, int stream)
        {
            for (int i = 0; i < UVSetCount; i++)
            {
                var vertexAttribute = (VertexAttribute)((int)VertexAttribute.TexCoord0 + i);
                dst[offset] = new VertexAttributeDescriptor(vertexAttribute, VertexAttributeFormat.Float32, 2, stream);
                offset++;
            }
        }

        public void ApplyOnMesh(
            UnityEngine.Mesh msh,
            int stream,
            MeshUpdateFlags flags = MeshGeneratorBase.defaultMeshUpdateFlags
            )
        {
            Profiler.BeginSample("ApplyUVs");
            msh.SetVertexBufferData(m_Data, 0, 0, m_Data.Length, stream, flags);
            Profiler.EndSample();
        }

        public void Dispose()
        {
            if (m_OwnsData && m_Data.IsCreated)
            {
                m_Data.Dispose();
            }
        }

        unsafe JobHandle? GetUvsJob(
            void* input,
            int count,
            GltfComponentType inputType,
            int inputByteStride,
            float2* output,
            int outputByteStride,
            bool normalized = false
            )
        {
            Profiler.BeginSample("PrepareUVs");
            JobHandle? jobHandle = null;

            switch (inputType)
            {
                case GltfComponentType.Float:
                {
                    var jobUv = new Jobs.ConvertUVsFloatToFloatInterleavedJob
                    {
                        inputByteStride = (inputByteStride > 0) ? inputByteStride : sizeof(float2),
                        input = (byte*)input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
                    jobHandle = jobUv.ScheduleBatch(count, GltfImport.DefaultBatchCount);
                }
                break;
                case GltfComponentType.UnsignedByte:
                    if (normalized)
                    {
                        var jobUv = new Jobs.ConvertUVsUInt8ToFloatInterleavedNormalizedJob
                        {
                            inputByteStride = (inputByteStride > 0) ? inputByteStride : 2,
                            input = (byte*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
                        jobHandle = jobUv.Schedule(count, GltfImport.DefaultBatchCount);
                    }
                    else
                    {
                        var jobUv = new Jobs.ConvertUVsUInt8ToFloatInterleavedJob
                        {
                            inputByteStride = (inputByteStride > 0) ? inputByteStride : 2,
                            input = (byte*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
                        jobHandle = jobUv.ScheduleBatch(count, GltfImport.DefaultBatchCount);
                    }
                    break;
                case GltfComponentType.UnsignedShort:
                    if (normalized)
                    {
                        var jobUv = new Jobs.ConvertUVsUInt16ToFloatInterleavedNormalizedJob
                        {
                            inputByteStride = (inputByteStride > 0) ? inputByteStride : 4,
                            input = (byte*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
                        jobHandle = jobUv.Schedule(count, GltfImport.DefaultBatchCount);
                    }
                    else
                    {
                        var jobUv = new Jobs.ConvertUVsUInt16ToFloatInterleavedJob
                        {
                            inputByteStride = (inputByteStride > 0) ? inputByteStride : 4,
                            input = (byte*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
                        jobHandle = jobUv.ScheduleBatch(count, GltfImport.DefaultBatchCount);
                    }
                    break;
                case GltfComponentType.Short:
                    if (normalized)
                    {
                        var job = new Jobs.ConvertUVsInt16ToFloatInterleavedNormalizedJob
                        {
                            inputByteStride = inputByteStride > 0 ? inputByteStride : 4,
                            input = (short*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
                        jobHandle = job.ScheduleBatch(count, GltfImport.DefaultBatchCount);
                    }
                    else
                    {
                        var job = new Jobs.ConvertUVsInt16ToFloatInterleavedJob
                        {
                            inputByteStride = inputByteStride > 0 ? inputByteStride : 4,
                            input = (short*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
                        jobHandle = job.ScheduleBatch(count, GltfImport.DefaultBatchCount);
                    }
                    break;
                case GltfComponentType.Byte:
                    if (normalized)
                    {
                        var jobInt8 = new Jobs.ConvertUVsInt8ToFloatInterleavedNormalizedJob
                        {
                            inputByteStride = inputByteStride > 0 ? inputByteStride : 2,
                            input = (sbyte*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
                        jobHandle = jobInt8.ScheduleBatch(count, GltfImport.DefaultBatchCount);
                    }
                    else
                    {
                        var jobInt8 = new Jobs.ConvertUVsInt8ToFloatInterleavedJob
                        {
                            inputByteStride = inputByteStride > 0 ? inputByteStride : 2,
                            input = (sbyte*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
                        jobHandle = jobInt8.ScheduleBatch(count, GltfImport.DefaultBatchCount);
                    }
                    break;
                default:
                    m_Logger?.Error(LogCode.TypeUnsupported, "UV", inputType.ToString());
                    break;
            }
            Profiler.EndSample();
            return jobHandle;
        }
    }
}
