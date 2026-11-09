// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GLTFast.Addons;
using GLTFast.Loading;
using GLTFast.Logging;
using UnityEngine;

namespace GLTFast
{
    static class ImageImport
    {
        internal static async Task<ImageResult> LoadImageAsync(
            ImportContext context,
            ImportSettings settings,
            int imageIndex,
            bool linear,
            bool readable,
            bool generateMipMaps,
            Task<IReadOnlyDisposableData> dataTask,
            ImportAddonInstanceCollection addons,
            CancellationToken cancellationToken
            )
        {
            using var data = await dataTask;
            if (data == null)
            {
                return ImageResult.Null;
            }

            while (context.DeferAgent.ShouldDefer())
            {
                cancellationToken.ThrowIfCancellationRequestedWithTracking();
                await Task.Yield();
            }

            var addonTask = addons
                ?.First<ITextureImageLoader>(addon => addon.IsAbleToLoad(data.Data.AsReadOnlySpan()))
                ?.LoadImageGovernedAsync(
                    context,
                    data.Data,
                    linear,
                    readable,
                    generateMipMaps,
                    cancellationToken);
            if (addonTask != null)
            {
                return await addonTask;
            }

            if (ImageFormatDetection.IsPngOrJpeg(data.Data.AsReadOnlySpan()))
            {
#if UNITY_IMAGECONVERSION
                Task<ImageResult> task;
                using (context.Scheduler.Install(cancellationToken))
                {
                    task = ImageConversionImageLoader.LoadAsync(
                        context, settings, data.Data, linear, readable, cancellationToken);
                }

                return await task;
#else
                context.Logger?.Error(LogCode.ImageConversionNotEnabled);
                return ImageResult.Null;
#endif
            }

            if (ImageFormatDetection.IsKtx(data.Data.AsReadOnlySpan()))
            {
#if KTX_IS_RECENT
                Task<ImageResult> task;
                using (context.Scheduler.Install(cancellationToken))
                {
                    task = KtxImageLoader.LoadAsync(
                        context, settings, data.Data, linear, readable, cancellationToken);
                }

                return await task;
#else
                context.Logger?.Error(
                    LogCode.PackageMissing, "KTX for Unity", ExtensionName.TextureBasisUniversal);
                return ImageResult.Null;
#endif // KTX_IS_RECENT
            }

            if (ImageFormatDetection.IsWebP(data.Data.AsReadOnlySpan()))
            {
                context.Logger?.Error(
                    LogCode.ImageFormatUnsupported,
                    imageIndex.ToString(),
                    nameof(ImageFormat.WebP)
                    );
                return ImageResult.Null;
            }

            context.Logger?.Error(
                LogCode.ImageFormatUnknown,
                imageIndex.ToString());
            return ImageResult.Null;
        }

        internal static async Task<ImageResult> LoadImageAsync(
            ImportContext context,
            Task<IReadOnlyDisposableData> dataTask,
            bool linear,
            bool readable,
            bool generateMipMaps,
            CancellationToken cancellationToken,
            ITextureImageLoader loader
        )
        {
            using var data = await dataTask;
            if (data == null || !data.Data.IsCreated || data.Data.Length == 0)
            {
                return ImageResult.Null;
            }
            while (context.DeferAgent.ShouldDefer())
            {
                cancellationToken.ThrowIfCancellationRequestedWithTracking();
                await Task.Yield();
            }

            return await loader.LoadImageGovernedAsync(
                context, data.Data, linear, readable, generateMipMaps, cancellationToken);
        }

        internal static async ValueTask<IReadOnlyDisposableData> LoadDataAsync(
            ImportContext context,
            Uri uri,
            CancellationToken cancellationToken
        )
        {
            var download = await context.DownloadProvider.Request(uri);
            if (download == null)
            {
                context.Logger?.Error(LogCode.TextureDownloadFailed, "?", uri.ToString());
                return null;
            }

            if (cancellationToken.IsCancellationRequested)
                return null;

            if (download.Success)
            {
                if (download is INativeDownload nativeDownload)
                {
                    return new ReadOnlyData(nativeDownload.NativeData, download);
                }
                var data = new ReadOnlyNativeArrayFromManagedArray<byte>(download.Data);
                return new ReadOnlyData(data.Array.AsNativeArrayReadOnly(), data);
            }

            context.Logger?.Error(LogCode.TextureDownloadFailed, download.Error, uri.ToString());
            return null;
        }
    }
}
