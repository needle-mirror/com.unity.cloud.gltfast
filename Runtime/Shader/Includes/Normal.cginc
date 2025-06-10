//UNITY_SHADER_NO_UPGRADE
#ifndef GLTF_HLSL_INCLUDE_NORMAL
#define GLTF_HLSL_INCLUDE_NORMAL

/// This is a replacement for UnityStandardUtils UnpackScaleNormal to use XYZ normals even with DXT5nm enabled
void NormalInTangentSpace_float(UnityTexture2D normal_texture, float2 uv, float normal_scale, out float3 normal)
{
    float4 packed_normal = tex2D(normal_texture, uv);
    packed_normal.x *= packed_normal.w;

    normal.xy = packed_normal.xy * 2 - 1;
#if (SHADER_TARGET >= 30)
    // SM2.0: instruction count limitation
    // SM2.0: normal scaler is not supported
    normal.xy *= normal_scale;
#endif
    normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
}

#endif
