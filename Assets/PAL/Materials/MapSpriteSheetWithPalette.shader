Shader "ayy/PAL/MapSpriteSheetWithPalette"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SpriteSheetTex("SpriteSheet",2D) = "white" {}
        _PaletteTex("PaletteTex",2D) = "white" {}
    }
    SubShader
    {
        Tags {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _SpriteSheetTex;
            sampler2D _PaletteTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_SpriteSheetTex, i.uv);
                float hasData = col.a;
                float index = col.r * 255.0f;
                //index = floor(index);
                int y = index / 16;
                int x = index % 16;
                float2 lutUV = float2(x/16.0f,y/16.0f);
                fixed4 lut = tex2D(_PaletteTex,lutUV);
                return float4(lut.rgb,step(0.5,hasData));
            }
            ENDCG
        }
    }
}
