// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if KTX_IS_RECENT
#define KTX_IS_ENABLED
#endif

#if KTX_IS_ENABLED

using System.Threading.Tasks;
using KtxUnity;
using UnityEngine;

namespace GLTFast {
    abstract class KtxLoadContextBase {
        public int imageIndex;
        protected KtxTexture m_KtxTexture;

        public abstract Task<TextureResult> LoadTexture2D(bool linear);
    }
}
#endif // KTX_IS_INSTALLED
