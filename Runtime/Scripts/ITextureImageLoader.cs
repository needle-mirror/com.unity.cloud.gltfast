// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using System.Threading.Tasks;
using GLTFast.Schema;
using Unity.Collections;
using UnityEngine;

namespace GLTFast
{
    /// <summary>
    /// Creates a <see cref="UnityEngine.Texture2D"/> from raw, usually compressed image data.
    /// </summary>
    public interface ITextureImageLoader
    {
        /// <summary>
        /// Creates an image from the input data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// glTFast governs where this method's <c>await</c> continuations resume, keeping the main thread responsive
        /// without the loader consulting the <see cref="IDeferAgent"/>. Write plain asynchronous code: decode on a
        /// worker thread with <c>Task.Run</c> and create the texture after awaiting the result. The method is also
        /// entered with main-thread time available, so short work needs no yielding at all.
        /// </para>
        /// <para>
        /// Two rules apply. Do not use <c>ConfigureAwait(false)</c>, which escapes to the thread pool where Unity's
        /// API is unavailable; and never block the main thread on a task with <c>Task.Result</c> or <c>Task.Wait</c>,
        /// which deadlocks.
        /// </para>
        /// <para>
        /// Split long stretches of synchronous main-thread work with <c>await Task.Yield()</c>; each yield resumes once
        /// main-thread time is available. Unity's <c>Awaitable</c> and <c>AsyncOperation</c> bypass this: code after
        /// awaiting one runs ungoverned for the rest of the method, so prefer <c>Task</c>-based APIs.
        /// </para>
        /// </remarks>
        /// <param name="data">Raw, compressed image data.</param>
        /// <param name="linear">If true, the texture being created is in linear space. If false, it is in sRGB space.</param>
        /// <param name="readable">If true, the resulting texture should remain readable (<see cref="UnityEngine.Texture2D.isReadable"/>).</param>
        /// <param name="generateMipMaps">If true, mipmap levels should get generated.</param>
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        /// <returns>An image texture result</returns>
        Task<ImageResult> LoadImage(
            NativeArray<byte>.ReadOnly data,
            bool linear,
            bool readable,
            bool generateMipMaps,
            CancellationToken cancellationToken
        );

        /// <summary>
        /// Determines if this loader can load the given texture, and if so, returns the corresponding image index.
        /// The typical use-case is a glTF texture extension that adds support for a new image format,
        /// e.g. EXT_texture_webp.
        /// The loader may also update the image index to point to a different image, e.g. if the extension adds support
        /// for a new image format that is stored in a different image than the one referenced by the texture.
        /// </summary>
        /// <param name="texture">glTF texture.</param>
        /// <param name="imageIndex">Corresponding, potentially updated glTF image index.</param>
        /// <returns>True if the texture image loader supports loading that texture, false otherwise.</returns>
        bool IsAbleToLoad(TextureBase texture, out int imageIndex);

        /// <summary>
        /// Implement this to support content-based image format detection.
        /// This is optional, but fixes loading of glTF images
        /// where the type could not be derived from the mime type or file extension.
        /// </summary>
        /// <param name="data">Image data.</param>
        /// <returns>True if the image loader supports loading that data, false otherwise.</returns>
        bool IsAbleToLoad(ReadOnlySpan<byte> data) => false;
    }
}
