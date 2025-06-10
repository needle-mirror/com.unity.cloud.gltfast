// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_ANIMATION
namespace GLTFast.Documentation.Examples
{
#region CustomGltfImportPlayables
    using System;
    using UnityEngine;

    public class CustomGltfImportPlayables : MonoBehaviour
    {
        public string Uri;

        async void Start()
        {
            try
            {
                var gltfImport = new GltfImport();
                var instantiator = new CustomGameObjectInstantiator(gltfImport, transform);
                var importSettings = new ImportSettings { AnimationMethod = AnimationMethod.Playables };

                await gltfImport.Load(Uri, importSettings);
                await gltfImport.InstantiateMainSceneAsync(instantiator);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
#endregion
}
#endif
