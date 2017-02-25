Shader "Custom/NormalMapShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_NormalTex ("Normal Map", 2D) = "bump" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _NormalTex;

		struct Input {
			float2 uv_NormalTex;
		};

		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutput o) {
            float3 normalMap = UnpackNormal(tex2D(_NormalTex, IN.uv_NormalTex));
            o.Normal = normalMap.rgb;
            o.Albedo = _Color.rgb;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
