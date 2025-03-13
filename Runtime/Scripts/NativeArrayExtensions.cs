// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;

namespace GLTFast
{
    static class NativeArrayExtensions
    {
        internal static unsafe UnmanagedMemoryStream ToUnmanagedMemoryStream(this NativeArray<byte> data)
        {
            return new UnmanagedMemoryStream(
                (byte*)data.GetUnsafePtr(),
                data.Length,
                data.Length,
                FileAccess.Write
            );
        }

        internal static unsafe UnmanagedMemoryStream ToUnmanagedMemoryStream(this NativeArray<byte>.ReadOnly data, uint start, uint count)
        {
            Assert.IsTrue(start + count <= data.Length);
            return new UnmanagedMemoryStream(
                (byte*)data.GetUnsafeReadOnlyPtr() + start,
                count,
                count,
                FileAccess.Read
            );
        }

        internal static unsafe uint ReadUInt32(this NativeArray<byte>.ReadOnly data, int offset)
        {
            var ptr = (uint*)((byte*)data.GetUnsafeReadOnlyPtr() + offset);
            return *ptr;
        }

        // TODO: Return NativeArray<byte>.ReadOnly
        internal static unsafe NativeSlice<byte> Slice(this NativeArray<byte>.ReadOnly data, int start, int length)
        {
            var bufferAddress = (byte*)data.GetUnsafeReadOnlyPtr() + start;
            var subArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(bufferAddress, length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var safetyHandle = AtomicSafetyHandle.Create();
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(array: ref subArray, safetyHandle);
#endif
            return subArray;
        }
    }
}
