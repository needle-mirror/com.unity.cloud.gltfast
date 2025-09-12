// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if KTX_IS_RECENT
#define KTX_IS_ENABLED
#endif

#if KTX_IS_ENABLED

using System.Threading.Tasks;
using KtxUnity;
using Unity.Collections;

namespace GLTFast {
    class KtxLoadContext : KtxLoadContextBase {
        NativeArray<byte>.ReadOnly m_Data;

        public KtxLoadContext(int index, NativeArray<byte>.ReadOnly data) {
            imageIndex = index;
            m_Data = data;
            m_KtxTexture = new KtxTexture();
        }

        public override async Task<TextureResult> LoadTexture2D(bool linear) {
            // TODO: Wait for KTX for Unity to offer a non-slice API and avoid slice here.
            var errorCode = m_KtxTexture.Open(m_Data);
            if (errorCode != ErrorCode.Success) {
                return new TextureResult(errorCode);
            }

            var result = await m_KtxTexture.LoadTexture2D(linear);

            m_KtxTexture.Dispose();
            return result;
        }
    }
}
#endif // KTX_IS_INSTALLED
