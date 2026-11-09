// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GLTFast.Logging;
using GLTFast.Schema;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using Mesh = UnityEngine.Mesh;

namespace GLTFast
{

    class MorphTargetsGenerator : IDisposable
    {
        readonly string[] m_MorphTargetNames;
        readonly IGltfBuffers m_Buffers;
        readonly IDeferAgent m_DeferAgent;

        MorphTargetGenerator[] m_Contexts;
        NativeArray<JobHandle> m_Handles;

        public MorphTargetsGenerator(
            int vertexCount,
            int subMeshCount,
            int morphTargetCount,
            string[] morphTargetNames,
            bool hasNormals,
            bool hasTangents,
            IGltfBuffers buffers,
            IDeferAgent deferAgent
            )
        {
            m_MorphTargetNames = morphTargetNames;
            m_Buffers = buffers;
            m_DeferAgent = deferAgent;

            m_Contexts = new MorphTargetGenerator[morphTargetCount];
            for (var i = 0; i < morphTargetCount; i++)
            {
                m_Contexts[i] = new MorphTargetGenerator(vertexCount, hasNormals, hasTangents);
            }
            m_Handles = new NativeArray<JobHandle>(morphTargetCount * subMeshCount, VertexBufferGeneratorBase.defaultAllocator);
        }

        public bool AddMorphTarget(
            int offset,
            int vertexCount,
            int subMesh,
            int morphTargetIndex,
            MorphTarget morphTarget,
            ICodeLogger logger
            )
        {
            var morphTargetGenerator = m_Contexts[morphTargetIndex];
            var jobHandle = morphTargetGenerator.ScheduleMorphTargetJobs(
                morphTarget,
                offset,
                vertexCount,
                m_Buffers,
                logger
                );
            if (jobHandle.HasValue)
            {
                m_Handles[subMesh * m_Contexts.Length + morphTargetIndex] = jobHandle.Value;
            }
            else
            {
                return false;
            }
            return true;
        }

        public JobHandle GetJobHandle()
        {
            var handle = m_Handles.Length > 1 ? JobHandle.CombineDependencies(m_Handles) : m_Handles[0];
            m_Handles.Dispose();
            return handle;
        }

        public async Task ApplyOnMeshAndDispose(Mesh mesh)
        {
            for (var index = 0; index < m_Contexts.Length; index++)
            {
                var context = m_Contexts[index];
                context.AddToMesh(mesh, m_MorphTargetNames?[index] ?? index.ToString());
                context.Dispose();
                m_Contexts[index] = null;
                await m_DeferAgent.BreakPoint();
            }
            m_Contexts = null;
        }

        public void Dispose()
        {
            if (m_Contexts != null)
            {
                foreach (var context in m_Contexts)
                {
                    context?.Dispose();
                }

                m_Contexts = null;
            }

            if (m_Handles.IsCreated)
            {
                m_Handles.Dispose();
            }
        }
    }

    sealed class MorphTargetGenerator : IDisposable
    {
        Vector3[] m_Positions;
        Vector3[] m_Normals;
        Vector3[] m_Tangents;

        GCHandle m_PositionsHandle;
        GCHandle m_NormalsHandle;
        GCHandle m_TangentsHandle;

        public MorphTargetGenerator(int vertexCount, bool hasNormals, bool hasTangents)
        {
            m_Positions = new Vector3[vertexCount];
            m_PositionsHandle = GCHandle.Alloc(m_Positions, GCHandleType.Pinned);

            if (hasNormals)
            {
                m_Normals = new Vector3[vertexCount];
                m_NormalsHandle = GCHandle.Alloc(m_Normals, GCHandleType.Pinned);
            }

            if (hasTangents)
            {
                m_Tangents = new Vector3[vertexCount];
                m_TangentsHandle = GCHandle.Alloc(m_Tangents, GCHandleType.Pinned);
            }
        }

        public unsafe JobHandle? ScheduleMorphTargetJobs(
            MorphTarget morphTarget,
            int offset,
            int vertexCount,
            IGltfBuffers buffers,
            ICodeLogger logger
        )
        {
            Profiler.BeginSample("ScheduleMorphTargetJobs");

            if (!TryGetAccessor(buffers, morphTarget.POSITION, vertexCount, logger, out var posAcc))
            {
                Profiler.EndSample();
                return null;
            }

            var jobCount = 1;
            if (posAcc.IsSparse && posAcc.bufferView >= 0)
                jobCount++;

            AccessorBase nrmAcc = null;

            if (morphTarget.NORMAL >= 0)
            {
                if (!TryGetAccessor(buffers, morphTarget.NORMAL, vertexCount, logger, out nrmAcc))
                {
                    Profiler.EndSample();
                    return null;
                }
                jobCount += nrmAcc.IsSparse && nrmAcc.bufferView >= 0 ? 2 : 1;
            }

            AccessorBase tanAcc = null;

            if (morphTarget.TANGENT >= 0)
            {
                if (!TryGetAccessor(buffers, morphTarget.TANGENT, vertexCount, logger, out tanAcc))
                {
                    Profiler.EndSample();
                    return null;
                }
                jobCount += tanAcc.IsSparse && tanAcc.bufferView >= 0 ? 2 : 1;
            }

            var handles = new NativeArray<JobHandle>(jobCount, VertexBufferGeneratorBase.defaultAllocator);
            var handleIndex = 0;

            if (!SchedulePositionsJobs(offset, buffers, morphTarget.POSITION, posAcc, logger, handles, ref handleIndex)
                || (nrmAcc != null
                    && !ScheduleNormalsJobs(offset, buffers, morphTarget.NORMAL, nrmAcc, logger, handles, ref handleIndex))
                || (tanAcc != null
                    && !ScheduleTangentsJobs(offset, buffers, morphTarget.TANGENT, tanAcc, logger, handles, handleIndex)))
            {
                VertexBufferGeneratorBase.CompleteAndDispose(handles);
                Profiler.EndSample();
                return null;
            }

            var handle = jobCount > 1 ? JobHandle.CombineDependencies(handles) : handles[0];
            handles.Dispose();
            Profiler.EndSample();
            return handle;
        }

        static bool TryGetAccessor(
            IGltfBuffers buffers,
            int index,
            int vertexCount,
            ICodeLogger logger,
            out AccessorBase accessor
            )
        {
            accessor = buffers.GetAccessor(index);
            if (accessor == null || accessor.count > vertexCount)
            {
                logger?.Error(LogCode.AccessorAccessFailed, index.ToString());
                return false;
            }
            return true;
        }

        unsafe bool SchedulePositionsJobs(
            int offset,
            IGltfBuffers buffers,
            int posAccIndex,
            AccessorBase posAcc,
            ICodeLogger logger,
            NativeArray<JobHandle> handles,
            ref int handleIndex
            )
        {
            void* posIndexData = null;
            void* posValueData = null;
            if (posAcc.IsSparse
                && !VertexBufferGeneratorBase.TryGetVector3SparseData(
                    buffers, posAccIndex, posAcc, logger, out posIndexData, out posValueData))
            {
                return false;
            }

            fixed (void* dest = &m_Positions[offset])
            {
                JobHandle? h = null;
                if (posAcc.bufferView >= 0)
                {
                    h = VertexBufferGeneratorBase.GetVector3Job(
                        buffers,
                        posAccIndex,
                        posAcc,
                        (float3*)dest,
                        12,
                        logger,
                        posAcc.normalized,
                        false // positional data never needs to be normalized
                    );
                    if (h.HasValue)
                    {
                        handles[handleIndex] = h.Value;
                        handleIndex++;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (posAcc.IsSparse)
                {
                    var sparseJobHandle = VertexBufferGeneratorBase.GetVector3SparseJob(
                        posIndexData,
                        posValueData,
                        posAcc.Sparse.count,
                        posAcc.Sparse.Indices.componentType,
                        posAcc.componentType,
                        (float3*)dest,
                        12,
                        dependsOn: ref h,
                        posAcc.normalized
                    );
                    if (sparseJobHandle.HasValue)
                    {
                        handles[handleIndex] = sparseJobHandle.Value;
                        handleIndex++;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        unsafe bool ScheduleNormalsJobs(
            int offset,
            IGltfBuffers buffers,
            int nrmAccIndex,
            AccessorBase nrmAcc,
            ICodeLogger logger,
            NativeArray<JobHandle> handles,
            ref int handleIndex
            )
        {
            void* indexData = null;
            void* valueData = null;
            if (nrmAcc.IsSparse
                && !VertexBufferGeneratorBase.TryGetVector3SparseData(
                    buffers, nrmAccIndex, nrmAcc, logger, out indexData, out valueData))
            {
                return false;
            }

            fixed (void* dest = &(m_Normals[offset]))
            {
                JobHandle? h = null;
                if (nrmAcc.bufferView >= 0)
                {
                    h = VertexBufferGeneratorBase.GetVector3Job(
                        buffers,
                        nrmAccIndex,
                        nrmAcc,
                        (float3*)dest,
                        12,
                        logger,
                        nrmAcc.normalized,
                        false // morph target normals are deltas -> don't normalize
                    );
                    if (h.HasValue)
                    {
                        handles[handleIndex] = h.Value;
                        handleIndex++;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (nrmAcc.IsSparse)
                {
                    var sparseJobHandle = VertexBufferGeneratorBase.GetVector3SparseJob(
                        indexData,
                        valueData,
                        nrmAcc.Sparse.count,
                        nrmAcc.Sparse.Indices.componentType,
                        nrmAcc.componentType,
                        (float3*)dest,
                        12,
                        dependsOn: ref h,
                        nrmAcc.normalized
                    );
                    if (sparseJobHandle.HasValue)
                    {
                        handles[handleIndex] = sparseJobHandle.Value;
                        handleIndex++;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        unsafe bool ScheduleTangentsJobs(
            int offset,
            IGltfBuffers buffers,
            int tanAccIndex,
            AccessorBase tanAcc,
            ICodeLogger logger,
            NativeArray<JobHandle> handles,
            int handleIndex
            )
        {
            void* indexData = null;
            void* valueData = null;
            if (tanAcc.IsSparse
                && !VertexBufferGeneratorBase.TryGetVector3SparseData(
                    buffers, tanAccIndex, tanAcc, logger, out indexData, out valueData))
            {
                return false;
            }

            fixed (void* dest = &(m_Tangents[offset]))
            {
                JobHandle? h = null;
                if (tanAcc.bufferView >= 0)
                {
                    h = VertexBufferGeneratorBase.GetVector3Job(
                        buffers,
                        tanAccIndex,
                        tanAcc,
                        (float3*)dest,
                        12,
                        logger,
                        tanAcc.normalized,
                        false // morph target tangents are deltas -> don't normalize
                    );
                    if (h.HasValue)
                    {
                        handles[handleIndex] = h.Value;
                        handleIndex++;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (tanAcc.IsSparse)
                {
                    var sparseJobHandle = VertexBufferGeneratorBase.GetVector3SparseJob(
                        indexData,
                        valueData,
                        tanAcc.Sparse.count,
                        tanAcc.Sparse.Indices.componentType,
                        tanAcc.componentType,
                        (float3*)dest,
                        12,
                        dependsOn: ref h,
                        tanAcc.normalized
                    );
                    if (sparseJobHandle.HasValue)
                    {
                        handles[handleIndex] = sparseJobHandle.Value;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void AddToMesh(Mesh mesh, string name)
        {
            Profiler.BeginSample("AddBlendShapeFrame");
            mesh.AddBlendShapeFrame(name, 1f, m_Positions, m_Normals, m_Tangents);
            Profiler.EndSample();
        }

        public void Dispose()
        {
            m_PositionsHandle.Free();
            m_Positions = null;
            if (m_Normals != null)
            {
                m_NormalsHandle.Free();
                m_Normals = null;
            }
            if (m_Tangents != null)
            {
                m_TangentsHandle.Free();
                m_Tangents = null;
            }
        }
    }
}
