# Use case: Use Playables API for animation

This use case describes the steps to use the Playables API to customize animation behaviour at runtime.

To accomplish this use case, do the following:

1. Create scripts for CustomGltfImport, CustomGameObjectInstantiator and PlayAnimationUtilitiesSample
2. Set up a new scene
3. Import the glTF asset in runtime

## Before you start

Before you start, you must add the following package dependencies to your project.

* In the `manifest.json` file, add the following dependencies:

```json
  {
    "dependencies": {
      // Add these lines:
      // Replace "<x.y.z>" with the version you wish to install
      "com.unity.cloud.gltfast": "<x.y.z>",
      "com.unity.modules.animation": "<x.y.z>"
      // Other dependencies...
    }
  }
```

## How do I...?

### Create custom scripts

To create the custom scripts, follow these steps:

1. Open your Unity&reg; Project.
2. Go to the **Assets** folder in the Project window.
3. Select and hold **Create**.
4. Select **C# Script**.
5. Rename the new script as `PlayAnimationUtilitiesSample`.
6. Open the `PlayAnimationUtilitiesSample` script and replace the content with the following:
   [!code-cs [play-animation-utilities-sample](../DocExamples/PlayAnimationUtilitiesSample.cs#PlayAnimationUtilitiesSample)]
7. Repeat step 2-4 to create another new script.
8. Rename the new script as `CustomGameObjectInstantiator`.
9. Open the `CustomGameObjectInstantiator` script and replace the content with the following:
   [!code-cs [custom-game-object-instantiator](../DocExamples/CustomGameObjectInstantiator.cs#CustomGameObjectInstantiator)]
10. Repeat step 2-4 to create another new script.
11. Rename the new script as `CustomGltfImportPlayables`.
12. Open the `CustomGltfImportPlayables` script and replace the content with the following:
   [!code-cs [custom-gltf-import-playables](../DocExamples/CustomGltfImportPlayables.cs#CustomGltfImportPlayables)]

### Set up a new scene

To set up a new scene, follow these steps:

1. Create a new scene.
2. Create a GameObject called **GltfImport**.
3. Select **Add Component** in the Inspector window and add the **Custom Gltf Import Playables** component.
4. In the **Uri** field, set the path to point to where the glTF asset is stored.

### Import the glTF asset in runtime

Select **Play**, the glTF asset should be loaded and displayed along with its animation at runtime.

You can then replace the contents of `PlayAnimationUtilitiesSample` with your own custom behaviour using the [Playables API](https://docs.unity3d.com/Manual/Playables.html).

## Trademarks

*Unity&reg;* is a registered trademark of [Unity Technologies][Unity].

*glTF&trade;* is a trademark of [The Khronos Group Inc][Khronos].

[Khronos]: https://www.khronos.org
[Unity]: https://unity.com
