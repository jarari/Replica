Shader "Unlit/Overlay"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		Zwrite off
		LOD 100

		Pass
	{
		Blend SrcAlpha One

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

		fixed getAddValue(fixed value) {
		fixed v = value;

		if (v<0.5)v = 0.5;

		return 2 * (v - 0.5);
	}

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

	sampler2D _MainTex; float4 _MainTex_ST;

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		fixed4 texCol = tex2D(_MainTex, i.uv);

	fixed4 col = fixed4(getAddValue(texCol.r),getAddValue(texCol.g),getAddValue(texCol.b),getAddValue(texCol.a));
	return col;
	}
		ENDCG
	}

		Pass
	{
		Blend DstColor Zero

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

		fixed getAddValue(fixed value) {
		fixed v = value;

		if (v>0.5)v = 0.5;

		return 2 * v;
	}

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

	sampler2D _MainTex; float4 _MainTex_ST;

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		fixed4 texCol = tex2D(_MainTex, i.uv);

	fixed4 col = fixed4(getAddValue(texCol.r),getAddValue(texCol.g),getAddValue(texCol.b),getAddValue(texCol.a));
	return col;
	}
		ENDCG
	}
	}
}
