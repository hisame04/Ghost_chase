Shader "Custom/Outline" {
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", float) = 0.1
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {

            // 前面をカリング
            Cull Front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                half4 vertex : POSITION;
                half3 normal : NORMAL;
            };

            struct v2f
            {
                half4 pos : SV_POSITION;
            };

            half _OutlineWidth;
            half4 _OutlineColor;
            
            v2f vert (appdata v)
            {
                v2f o = (v2f)0;

                // アウトラインの分だけ法線方向に拡大する
                o.pos = UnityObjectToClipPos(v.vertex + v.normal * _OutlineWidth);

                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }

        // 2パス目は好きなようにレンダリングする
        Pass
        {
            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                half4 vertex : POSITION;
                half3 normal : NORMAL;
            };

            struct v2f
            {
                half4 pos : SV_POSITION;
                half3 normal: TEXCOORD1;
            };

            half4 _Color;
            half4 _LightColor0;
            
            v2f vert (appdata v)
            {
                v2f o = (v2f)0;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);

                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                half3 diff = max(0, dot(i.normal, _WorldSpaceLightPos0.xyz)) * _LightColor0;

                fixed4 col;
                col.rgb = _Color * diff;
                return col;
            }
            ENDCG
        }
    }
}