Shader "Custom/ClearBlowShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        [HDR]_EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _EmissionTex ("Emission Texture", 2D) = "white"{}
        _EmisionIntensity("Emission Intensity",Range(0,10)) = 1.0
        _Alpha("Alpha",Range(0,1)) =0.9
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent"}
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _EmissionTex;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_EmissionTex;
        };

        fixed4 _Color;
        fixed4 _EmissionColor;
        half _EmisionIntensity;
        half _Alpha;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;

            o.Alpha = _Alpha * c.a;

            fixed4 e =tex2D(_EmissionTex ,IN.uv_EmissionTex) *_EmissionColor * _EmisionIntensity;
            o.Emission = e.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
