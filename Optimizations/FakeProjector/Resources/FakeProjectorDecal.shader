// Unlit decal shader for FakeProjector's baked quads.
//
// Blend factors are material properties rather than hardcoded passes, so a single shader covers every render
// mode and FakeProjectorRenderModes can build all of them from one Shader.Find. Instancing is what the whole
// FakeProjector optimization rests on, so the vertex stage is instancing-ready.
//
// Lives under Resources/ deliberately: nothing references it from a scene, so without that it would be
// stripped from builds and every decal would render magenta in a player but fine in the editor.
Shader "K10/Fake Projector Decal"
{
    Properties
    {
        _MainTex ("Decal", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // Driven by FakeProjectorRenderModes, not meant to be set by hand.
        [HideInInspector] _SrcBlend ("Src Blend", Float) = 5
        [HideInInspector] _DstBlend ("Dst Blend", Float) = 10
        [HideInInspector] _MultiplyMode ("Multiply Mode", Float) = 0
    }

    SubShader
    {
        // IgnoreProjector so real Projectors never re-project onto the quads that replaced them.
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" }
        LOD 100

        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
            ZWrite Off
            Cull Back
            // Pulls the quad towards the camera in depth. _surfaceOffset already lifts it off the surface
            // geometrically; this covers the shallow viewing angles where that lift projects to nearly nothing.
            Offset -1, -1

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _MultiplyMode;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;

                // Multiply blends against the destination, where "no decal" has to mean white, not black —
                // otherwise transparent texels would darken the surface instead of leaving it alone.
                col.rgb = lerp(col.rgb, lerp(fixed3(1, 1, 1), col.rgb, col.a), _MultiplyMode);
                return col;
            }
            ENDCG
        }
    }

    Fallback Off
}
