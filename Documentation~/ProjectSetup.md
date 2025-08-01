---
uid: doc-project-setup
---

# Project Setup

This page explains how projects can be setup to fit your needs and tweaked in detail. As a prerequisite you first need to [install *glTFast*](installation.md).

## Materials and Shader Variants

For runtime import *Unity glTFast* uses custom shader graphs or shaders for rendering glTF&trade; materials. Depending on the properties of a glTF material (and extensions it relies on), a specific [shader variant][shader-variants] will get used. In the Editor this shader variant will be built on-demand, but in order for materials to work in your build, you **have** to make sure all shader variants that you're going to need are included.

Including all possible variants is the safest approach, but can make your build very big. There's another way to find the right subset, if you already know what files you'll expect:

- Run your scene that loads all glTFs you expect in the editor.
- Go to Edit->Project Settings->Graphics
- At the bottom end you'll see the "Shader Preloading" section
- Save the currently tracked shaders/variants to an asset
- Take this ShaderVariantCollection asset and add it to the "Preloaded Shaders" list

An alternative way is to create placeholder materials for all feature combinations you expect and put them in a "Resource" folder in your project.

Read the documentation about [`Shader.Find`](https://docs.unity3d.com/ScriptReference/Shader.Find.html) for details how to include shaders in builds. It's also recommended to learning more about [shader variants][shader-variants].

Depending on the Unity version and render pipeline in use, different shader graphs or shaders will be used.

- Shader graphs under `Runtime/Shader` for
  - Universal render pipe 12 or newer
  - High-Definition render pipe 10 or newer
  - Built-in render pipe (experimental opt-in; see below)
- Shader graphs in folder `Runtime/Shader/HDRP` for HDRP-specific material types
- Shader graphs in folder `Runtime/Shader/Legacy` for older Universal / High-Definition render pipe versions
- Shaders in folder `Runtime/Shader/Built-In` for the built-in render pipeline

### Shader Graphs and the Built-In Render Pipeline

> This approach is experimental and has know shading issues

Built-In render pipe projects can optionally use the shader graphs instead of the Built-In shaders by:

- Installing Shader Graph version 12 or newer
- Adding `GLTFAST_BUILTIN_SHADER_GRAPH` to the list of scripting define symbols in the project settings

## Optional Packages

*glTFast* has soft-dependencies on some [optional packages](installation.md#optional-packages). By not installing those packages you might be able to reduce your final build size, so consider doing that.

For example, if you don't need PNG/Jpeg support (because you use only KTX&trade; 2.0 textures or no textures at all), you can disable the *Image Conversion* and *UnityWebRequestTexture* modules.

## Performance Increase via Collections Package

> [!CAUTION]
> Don't apply this if your project or one of its packages (e.g. [Polyspatial 1.3]) has a dependency on [Collections] 1.4 or older. Unfortunately [Collections] version 1.5.0 introduced breaking changes that might lead to compiler errors otherwise.

Projects running Editor version 2021 can increase the performance of certain low-level C# jobs by upgrading the [Collections] package to version 1.5.1 (or newer). *glTFast* then utilizes [IJobParallelForBatch] for increased loading speed.

> [!NOTE]
> Performance measurements showed that in Unity 2022 and later the Burst compiler has gotten so advanced that performance is excellent out-of-the-box and adjusting the [Collections] package versions makes little to no difference.

## Readable Mesh Data

By default *Unity glTFast* discards mesh data after it was uploaded to the GPU to free up main memory (see [`markNoLongerReadable`](https://docs.unity3d.com/ScriptReference/Mesh.UploadMeshData.html)). You can disable this globally by using the scripting define `GLTFAST_KEEP_MESH_DATA`.

Motivations for this might be using meshes as physics colliders amongst [other cases](https://docs.unity3d.com/ScriptReference/Mesh-isReadable.html).

## Safe Mode

Arbitrary (and potentially broken) input data is a challenge to software's robustness and safety. Some measurements to make *Unity glTFast* more robust have a negative impact on its performance though.

For this reason some pedantic safety checks in *Unity glTFast* are not performed by default. You can enable safe-mode by adding the scripting define `GLTFAST_SAFE` to your project.

Enable safe-mode if you are not in control over what content your application may end up loading and you cannot test up front.

## Disable Editor Import

By default, *Unity glTFast* provides Editor import for all files ending with `.gltf` or `.glb` via a `ScriptedImporter`.
If you experience conflicts with other packages that are offering `.gltf`/`.glb` import as well (e.g. [MixedRealityToolkit-Unity][MRTK]) or you simply want to disable Editor import,
add `GLTFAST_EDITOR_IMPORT_OFF` to the *Scripting Define Symbols* in the *Player Settings* and this feature will be turned off.

## Trademarks

*Unity&reg;* is a registered trademark of [Unity Technologies][Unity].

*Khronos&reg;* is a registered trademark and *glTF&trade;* is a trademark of [The Khronos Group Inc][Khronos].

*KTX&trade;* and the KTX logo are trademarks of the [The Khronos Group Inc][khronos].

[Collections]: https://docs.unity3d.com/Packages/com.unity.collections@1.5/manual/index.html
[IJobParallelForBatch]: https://docs.unity3d.com/Packages/com.unity.collections@1.5/api/Unity.Jobs.IJobParallelForBatch.html?q=IJobParallelForBatch
[Khronos]: https://www.khronos.org
[MRTK]: https://github.com/microsoft/MixedRealityToolkit-Unity
[Polyspatial 1.3]: https://docs.unity3d.com/Packages/com.unity.polyspatial.visionos@1.3/manual/index.html
[shader-variants]: https://docs.unity3d.com/Manual/shader-variants.html
[Unity]: https://unity.com
