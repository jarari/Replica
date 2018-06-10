// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:1,cusa:True,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:True,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:True,atwp:True,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:1873,x:33482,y:32698,varname:node_1873,prsc:2|emission-5448-OUT,alpha-603-OUT;n:type:ShaderForge.SFN_Tex2d,id:4805,x:32551,y:32729,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:True,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:1086,x:32812,y:32818,cmnt:RGB,varname:node_1086,prsc:2|A-4805-RGB,B-5983-RGB,C-5376-RGB;n:type:ShaderForge.SFN_Color,id:5983,x:32551,y:32915,ptovrint:False,ptlb:MainTex_Color,ptin:_MainTex_Color,varname:_Color,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_VertexColor,id:5376,x:32551,y:33079,varname:node_5376,prsc:2;n:type:ShaderForge.SFN_Multiply,id:1749,x:33025,y:32818,cmnt:Premultiply Alpha,varname:node_1749,prsc:2|A-1086-OUT,B-603-OUT;n:type:ShaderForge.SFN_Multiply,id:603,x:32812,y:32992,cmnt:A,varname:node_603,prsc:2|A-4805-A,B-5983-A,C-5376-A;n:type:ShaderForge.SFN_Tex2d,id:1935,x:32232,y:32692,varname:_N,prsc:2,tex:d7160d7e20d2f8d489b5d0478724f293,ntxv:0,isnm:False|UVIN-3964-OUT,TEX-1322-TEX;n:type:ShaderForge.SFN_TexCoord,id:9066,x:31612,y:32688,varname:node_9066,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Time,id:7017,x:31412,y:32655,varname:node_7017,prsc:2;n:type:ShaderForge.SFN_Append,id:3964,x:32049,y:32672,varname:node_3964,prsc:2|A-4422-OUT,B-6742-OUT;n:type:ShaderForge.SFN_Add,id:4422,x:31833,y:32630,varname:node_4422,prsc:2|A-4213-OUT,B-9066-U;n:type:ShaderForge.SFN_Multiply,id:4213,x:31646,y:32508,varname:node_4213,prsc:2|A-6007-OUT,B-7017-T;n:type:ShaderForge.SFN_ValueProperty,id:6007,x:31423,y:32449,ptovrint:False,ptlb:U_Speed,ptin:_U_Speed,varname:_U,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.5;n:type:ShaderForge.SFN_Lerp,id:6921,x:32551,y:32537,varname:node_6921,prsc:2|A-5261-RGB,B-4274-RGB,T-1935-RGB;n:type:ShaderForge.SFN_Color,id:5261,x:32232,y:32325,ptovrint:False,ptlb:color_1,ptin:_color_1,varname:_node_2278,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0.006896496,c3:1,c4:1;n:type:ShaderForge.SFN_Color,id:4274,x:32232,y:32500,ptovrint:False,ptlb:Color_2,ptin:_Color_2,varname:_node_6133,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:0.01671309,c3:0,c4:1;n:type:ShaderForge.SFN_Add,id:6742,x:31833,y:32779,varname:node_6742,prsc:2|A-9066-V,B-2955-OUT;n:type:ShaderForge.SFN_Multiply,id:2955,x:31625,y:32870,varname:node_2955,prsc:2|A-7017-T,B-4879-OUT;n:type:ShaderForge.SFN_ValueProperty,id:4879,x:31412,y:32904,ptovrint:False,ptlb:V_Speed,ptin:_V_Speed,varname:_V,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:-1;n:type:ShaderForge.SFN_Tex2d,id:8903,x:32551,y:32325,ptovrint:False,ptlb:Mask,ptin:_Mask,varname:_node_8903,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Lerp,id:5448,x:33265,y:32775,varname:node_5448,prsc:2|A-6921-OUT,B-1749-OUT,T-6829-OUT;n:type:ShaderForge.SFN_Tex2dAsset,id:1322,x:31994,y:32909,ptovrint:False,ptlb:Noise,ptin:_Noise,varname:node_1322,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:d7160d7e20d2f8d489b5d0478724f293,ntxv:0,isnm:False;n:type:ShaderForge.SFN_OneMinus,id:6829,x:32734,y:32325,varname:node_6829,prsc:2|IN-8903-RGB;proporder:4805-8903-5983-5261-4274-6007-4879-1322;pass:END;sub:END;*/

Shader "Shader Forge/1" {
    Properties {
        [PerRendererData]_MainTex ("MainTex", 2D) = "white" {}
        _Mask ("Mask", 2D) = "white" {}
        _MainTex_Color ("MainTex_Color", Color) = (1,1,1,1)
        [HDR]_color_1 ("color_1", Color) = (0,0.006896496,1,1)
        [HDR]_Color_2 ("Color_2", Color) = (1,0.01671309,0,1)
        _U_Speed ("U_Speed", Float ) = 0.5
        _V_Speed ("V_Speed", Float ) = -1
        _Noise ("Noise", 2D) = "white" {}
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        _Stencil ("Stencil ID", Float) = 0
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilComp ("Stencil Comparison", Float) = 8
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilOpFail ("Stencil Fail Operation", Float) = 0
        _StencilOpZFail ("Stencil Z-Fail Operation", Float) = 0
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
            "PreviewType"="Plane"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            
            Stencil {
                Ref [_Stencil]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
                Comp [_StencilComp]
                Pass [_StencilOp]
                Fail [_StencilOpFail]
                ZFail [_StencilOpZFail]
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _MainTex_Color;
            uniform float _U_Speed;
            uniform float4 _color_1;
            uniform float4 _Color_2;
            uniform float _V_Speed;
            uniform sampler2D _Mask; uniform float4 _Mask_ST;
            uniform sampler2D _Noise; uniform float4 _Noise_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );
                #ifdef PIXELSNAP_ON
                    o.pos = UnityPixelSnap(o.pos);
                #endif
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float4 node_7017 = _Time;
                float2 node_3964 = float2(((_U_Speed*node_7017.g)+i.uv0.r),(i.uv0.g+(node_7017.g*_V_Speed)));
                float4 _N = tex2D(_Noise,TRANSFORM_TEX(node_3964, _Noise));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float node_603 = (_MainTex_var.a*_MainTex_Color.a*i.vertexColor.a); // A
                float4 _Mask_var = tex2D(_Mask,TRANSFORM_TEX(i.uv0, _Mask));
                float3 emissive = lerp(lerp(_color_1.rgb,_Color_2.rgb,_N.rgb),((_MainTex_var.rgb*_MainTex_Color.rgb*i.vertexColor.rgb)*node_603),(1.0 - _Mask_var.rgb));
                float3 finalColor = emissive;
                return fixed4(finalColor,node_603);
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            struct VertexInput {
                float4 vertex : POSITION;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.pos = UnityObjectToClipPos( v.vertex );
                #ifdef PIXELSNAP_ON
                    o.pos = UnityPixelSnap(o.pos);
                #endif
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
