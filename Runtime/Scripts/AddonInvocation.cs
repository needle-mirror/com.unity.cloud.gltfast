// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;

namespace GLTFast
{
    /// <summary>
    /// Calls into asynchronous add-on code with glTFast governing where its continuations resume.
    /// </summary>
    /// <remarks>
    /// The add-on runs up to its first <c>await</c> under the import's <see cref="DeferringSynchronizationContext"/>,
    /// which its continuations capture. The returned task is awaited by the caller outside that context.
    /// </remarks>
    static class AddonInvocation
    {
        internal static Task<ImageResult> LoadImageGovernedAsync(
            this ITextureImageLoader loader,
            ImportContext context,
            NativeArray<byte>.ReadOnly data,
            bool linear,
            bool readable,
            bool generateMipMaps,
            CancellationToken cancellationToken
            )
        {
            using (context.Scheduler.Install(cancellationToken))
                return loader.LoadImage(data, linear, readable, generateMipMaps, cancellationToken);
        }
    }
}
