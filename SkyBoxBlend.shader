Shader "Skybox/360Blend"
{
    Properties
    {
        _Tex1 ("Skybox 1 (Equirectangular)", 2D) = "white" {}
        _Tex2 ("Skybox 2 (Equirectangular)", 2D) = "black" {}
        _Blend ("Blend", Range(0,1)) = 0.0
        _Rotation1 ("Rotation 1 (Degrees)", Range(0,360)) = 0.0
        _Rotation2 ("Rotation 2 (Degrees)", Range(0,360)) = 0.0
    }

    SubShader
    {
        Tags { "Queue"="Background" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM 
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _Tex1;
            sampler2D _Tex2;
            float _Blend;
            float _Rotation1;
            float _Rotation2;

            struct v2f {
                float4 pos : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            v2f vert (float4 v : POSITION) {
                v2f o;
                o.pos = UnityObjectToClipPos(v);
                o.dir = normalize(mul(unity_ObjectToWorld, v).xyz);
                return o;
            }

            half4 frag (v2f i) : SV_Target {
                float3 dir = normalize(i.dir);

                float lon = atan2(dir.x, dir.z);
                float lat = asin(dir.y);

                // Convert degrees to radians
                float rot1 = radians(_Rotation1);
                float rot2 = radians(_Rotation2);

                // Apply individual rotations
                float2 uv1;
                uv1.x = (lon + rot1) / (2.0 * UNITY_PI) + 0.5;
                uv1.y = lat / UNITY_PI + 0.5;

                float2 uv2;
                uv2.x = (lon + rot2) / (2.0 * UNITY_PI) + 0.5;
                uv2.y = uv1.y;

                // Wrap horizontal UV to [0,1]
                uv1.x = frac(uv1.x);
                uv2.x = frac(uv2.x);

                half4 col1 = tex2D(_Tex1, uv1);
                half4 col2 = tex2D(_Tex2, uv2);

                return lerp(col1, col2, _Blend);
            }
            ENDCG
        }
    }
}
