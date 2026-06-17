Shader "UI/URP_PerfectMaskFade"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0,0,0,1) // フェードの色
        
        //背景透過画像をここにセット
        _MaskTex ("Alpha Mask (Sprite)", 2D) = "white" {}
        
        //DOTweenから送られてくる 0.0 ～ 1.5 の値（変更不要）
        _FadeRadius ("Fade Progress", Range(0, 1.5)) = 0.0
        
        //フェードが「1.5（最大値）」に達した時の最大大きさ
        _MaxScale ("Max Scale at End", Range(1, 10)) = 4.0
        
        //拡大したときの輪郭のガビガビを滑らかにするフィルターの強さ
        _EdgeSmoothness ("Edge Smoothness", Range(0.001, 0.5)) = 0.02

        //UIシステム連携用の設定
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [ZTest] Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _FadeRadius;
                float _MaxScale;
                float _EdgeSmoothness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                //【歪み修正】画面のアスペクト比（縦横比）を自動計算して補正
                // _ScreenParams.x = 画面の横ピクセル数、_ScreenParams.y = 縦ピクセル数
                float aspect = _ScreenParams.x / _ScreenParams.y; 
                
                //画面中央 (0.5, 0.5) を原点とした座標系に変換
                float2 uvOffset = input.uv - float2(0.5, 0.5);
                
                //縦画面（横 < 縦）の場合、横方向のUVを縮めることで、スライムの縦長潰れを相殺する
                if (aspect < 1.0)
                {
                    uvOffset.x *= aspect;
                }
                else //横画面の場合の互換性も維持
                {
                    uvOffset.y /= aspect;
                }

                //【速度同期】DOTweenの秒数とアニメーションの見た目を完全一致させる
                //C#から送られてくる 0.0 ～ 1.5 を、0.0 ～ 1.0 の「進捗率（Progress）」にマッピングし直す
                float progress = _FadeRadius / 1.5;
                
                //演出時間をフルに使って、0 から指定した最大サイズ（_MaxScale）まで均等に大きくする
                float scale = progress * _MaxScale;
                
                //完全に閉じている（scaleがほぼ0）ときは真っ黒を返す
                if (scale < 0.001)
                {
                    return _Color;
                }
                
                //補正・拡大を適用した最終的なマスクUV座標
                float2 scaledUV = uvOffset / scale + float2(0.5, 0.5);
                
                half maskAlpha = 0.0;
                
                //UVが画像の内側（0～1）にある時だけスライムの透明度を読み込む
                if (scaledUV.x >= 0.0 && scaledUV.x <= 1.0 && scaledUV.y >= 0.0 && scaledUV.y <= 1.0)
                {
                    maskAlpha = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, scaledUV).a;
                }
                
                //画像がある部分（1）を「くり抜き（0）」に反転
                float invertedAlpha = 1.0 - maskAlpha;
                
                //アンチエイリアス（ボケ）処理。拡大しても輪郭を滑らかに保つ
                float fadeAlpha = smoothstep(0.5 - _EdgeSmoothness, 0.5 + _EdgeSmoothness, invertedAlpha);
                
                //最終カラー合成
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
                color.a *= fadeAlpha;
                
                return color;
            }
            ENDHLSL
        }
    }
}