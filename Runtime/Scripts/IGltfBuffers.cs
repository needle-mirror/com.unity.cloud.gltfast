// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace GLTFast
{

    using Schema;

    interface IGltfBuffers
    {
        AccessorBase GetAccessor(int index);
        unsafe bool TryGetAccessorData(AccessorBase accessor, int componentCount, out void* data, out int byteStride);
        unsafe bool TryGetAccessorSparseIndices(AccessorBase accessor, out void* data);
        unsafe bool TryGetAccessorSparseValues(AccessorBase accessor, int componentCount, out void* data);
        bool TryGetBufferView(int bufferViewIndex, out ReadOnlyNativeArray<byte> data, int offset = 0, int length = 0);

        ReadOnlyNativeArray<T> GetAccessorData<T>(
            int bufferViewIndex,
            int count,
            int offset = 0
        )
            where T : unmanaged;

        ReadOnlyNativeStridedArray<T> GetStridedAccessorData<T>(
            int bufferViewIndex,
            int count,
            int offset = 0
        )
            where T : unmanaged;
    }
}
