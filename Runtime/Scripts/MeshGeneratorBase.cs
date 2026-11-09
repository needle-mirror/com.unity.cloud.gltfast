// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace GLTFast
{

    abstract class MeshGeneratorBase : IDisposable
    {

        public const MeshUpdateFlags defaultMeshUpdateFlags =
            MeshUpdateFlags.DontNotifyMeshUsers
            | MeshUpdateFlags.DontRecalculateBounds
            | MeshUpdateFlags.DontResetBoneBounds
            | MeshUpdateFlags.DontValidateIndices;

        protected Task<UnityEngine.Mesh> m_CreationTask;

        public virtual bool IsCompleted => m_CreationTask == null || m_CreationTask.IsCompleted;

        protected MorphTargetsGenerator m_MorphTargetsGenerator;

        protected string m_MeshName;

        bool m_ResultClaimed;
        bool m_Disposed;

        protected MeshGeneratorBase(string meshName)
        {
            m_MeshName = meshName;
        }

        public async Task<UnityEngine.Mesh> CreateMeshResult(CancellationToken cancellationToken)
        {
            while (!IsCompleted)
            {
                cancellationToken.ThrowIfCancellationRequestedWithTracking();
                await Task.Yield();
            }

            m_ResultClaimed = true;
            return m_CreationTask?.Result;
        }

        public void Dispose()
        {
            if (m_Disposed)
            {
                return;
            }

            m_Disposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!m_ResultClaimed && m_CreationTask?.Status == TaskStatus.RanToCompletion)
                {
                    DestroyUtils.SafeDestroy(m_CreationTask.Result);
                }

                m_CreationTask?.Dispose();
                m_MorphTargetsGenerator?.Dispose();
            }
        }
    }
}
