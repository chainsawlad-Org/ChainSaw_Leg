Shader "Custom/2D/Stripes"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _StripeColor ("Stripe Color", Color) = (1,0,0,1)
        _BgColor ("Background Color", Color) = (1,1,1,1)
        _StripeCount ("Stripe Count", Float) = 10.0
        _LineThickness ("Line Thickness", Range(0.01, 1.0)) = 0.5
        _Rotation ("Rotation (Degrees)", Range(0, 360)) = 45.0
        
        // --- NEW ANIMATION PROPERTIES ---
        _Speed ("Animation Speed", Float) = 1.0
        [Toggle] _Reverse ("Reverse Direction", Float) = 0
        
        // Required for UI/Sprite masking support
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            fixed4 _StripeColor;
            fixed4 _BgColor;
            float _StripeCount;
            float _LineThickness;
            float _Rotation;
            float _Speed;       // New
            float _Reverse;     // New

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. Calculate Rotation
                float rad = radians(_Rotation);
                float cosA = cos(rad);
                float sinA = sin(rad);

                // 2. Rotate UVs around the center (0.5, 0.5)
                float2 centeredUV = IN.texcoord - 0.5;
                float2 rotatedUV;
                rotatedUV.x = centeredUV.x * cosA - centeredUV.y * sinA;
                rotatedUV.y = centeredUV.x * sinA + centeredUV.y * cosA;

                // 3. Create the repeating pattern base
                float patternValue = (rotatedUV.x + rotatedUV.y) * _StripeCount;

                // 4. ANIMATION: Add time to shift the pattern
                // We use _Time.y which is the time in seconds.
                // The _Reverse toggle flips the speed multiplier between 1 and -1.
                float direction = lerp(1.0, -1.0, _Reverse);
                patternValue += _Time.y * _Speed * direction;

                // 5. Create the stripe mask
                float mask = step(frac(patternValue), _LineThickness);

                // 6. Mix Colors
                fixed4 finalColor = lerp(_BgColor, _StripeColor, mask);

                // 7. Apply Sprite Tint and Alpha
                finalColor *= IN.color;
                finalColor.rgb *= finalColor.a;

                return finalColor;
            }
            ENDCG
        }
    }
}