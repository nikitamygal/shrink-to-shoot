Shader "UnblockMyCar/Muted Environment"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)

        _Saturation("Saturation", Range(0, 1)) = 0.72
        _Brightness("Brightness", Range(0, 1.5)) = 0.92

        _Smoothness("Smoothness", Range(0, 1)) = 0.25
        _SpecularStrength("Specular Strength", Range(0, 1)) = 0.15
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Back
        ZWrite On
        ZTest LEqual

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_SCREEN

            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Saturation;
                half _Brightness;
                half _Smoothness;
                half _SpecularStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                half3 normalWS     : TEXCOORD1;
                float2 uv          : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                half fogFactor     : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            half3 ApplySaturation(half3 color, half saturation)
            {
                // Rec.709 luminance weights.
                half luminance = dot(color, half3(0.2126h, 0.7152h, 0.0722h));
                return lerp(luminance.xxx, color, saturation);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                output.positionHCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.shadowCoord = GetShadowCoord(positionInputs);
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);

                half3 albedo = tex.rgb * _BaseColor.rgb;

                // Muting controls.
                albedo = ApplySaturation(albedo, _Saturation);
                albedo *= _Brightness;

                half3 normalWS = NormalizeNormalPerPixel(input.normalWS);
                half3 viewDirWS = SafeNormalize(GetWorldSpaceViewDir(input.positionWS));

                // Ambient / environment lighting.
                half3 lighting = SampleSH(normalWS);

                // Main light.
                Light mainLight = GetMainLight(input.shadowCoord);
                half NdotL = saturate(dot(normalWS, mainLight.direction));

                lighting += mainLight.color
                          * NdotL
                          * mainLight.distanceAttenuation
                          * mainLight.shadowAttenuation;

                // Simple specular to keep a Lit-like look.
                half3 halfDir = SafeNormalize(mainLight.direction + viewDirWS);
                half NdotH = saturate(dot(normalWS, halfDir));
                half specPower = lerp(8.0h, 128.0h, _Smoothness);
                half specular = pow(NdotH, specPower) * _SpecularStrength
                              * mainLight.distanceAttenuation
                              * mainLight.shadowAttenuation;

                half3 specularColor = mainLight.color * specular;

                #if defined(_ADDITIONAL_LIGHTS)
                uint lightCount = GetAdditionalLightsCount();

                LIGHT_LOOP_BEGIN(lightCount)
                    Light light = GetAdditionalLight(lightIndex, input.positionWS);

                    half addNdotL = saturate(dot(normalWS, light.direction));

                    lighting += light.color
                              * addNdotL
                              * light.distanceAttenuation
                              * light.shadowAttenuation;

                    half3 addHalfDir = SafeNormalize(light.direction + viewDirWS);
                    half addNdotH = saturate(dot(normalWS, addHalfDir));
                    half addSpecular = pow(addNdotH, specPower) * _SpecularStrength
                                     * light.distanceAttenuation
                                     * light.shadowAttenuation;

                    specularColor += light.color * addSpecular;
                LIGHT_LOOP_END
                #endif

                half3 color = albedo * lighting + specularColor;
                color = MixFog(color, input.fogFactor);

                return half4(color, tex.a * _BaseColor.a);
            }

            ENDHLSL
        }

        // Reuse URP Lit passes so the material still participates in shadows/depth.
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/DepthNormals"
    }

    FallBack Off
}
