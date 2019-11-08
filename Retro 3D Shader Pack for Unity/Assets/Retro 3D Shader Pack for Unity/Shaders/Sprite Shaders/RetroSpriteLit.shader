//////////////////////////////////////////////////
// Author:				LEAKYFINGERS
// Date created:		24.10.19
// Date last edited:	03.11.19
//////////////////////////////////////////////////
Shader "Retro 3D Shader Pack/Sprite (Lit)" 
{	
	Properties       
	{		           
		_MainTex("Main Texture", 2D) = "white" {}
		_ColorTint("Color Tint", Color) = (1, 1, 1, 1)
		_SpecularMap("Specular Map", 2D) = "white" {} 
		_SpecularColor("Specular Color", Color) = (0, 0, 0, 1)  
		_Smoothness("Smoothness", Range(0.0, 1.0)) = 0.0  
		_NormalMap("Normal Map", 2D) = "bump" {}	
		[HDR] _EmissionColor("Emission Color", Color) = (0, 0, 0, 1)
		[HDR] _EmissionMap("Emission Map", 2D) = "black" {}

		_VertJitter("Vertex Jitter", Range(0.0, 0.999)) = 0.95 // The range used to set the geometric resolution of each vertex position value in order to create a vertex jittering/snapping effect.		
		_DrawDist("Draw Distance", Float) = 0 // The max draw distance from the camera to each vertex, with all vertices outside this range being clipped - set to 0 for infinite range.
	} 
			
	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "ForceNoShadowCasting" = "True" }
		ZWrite Off
		Cull Off
  		 		
		CGPROGRAM
					
		#pragma surface surf StandardSpecular alpha:fade vertex:vert 
		#pragma target 3.0		
		#pragma shader_feature_local ENABLE_AFFINE_TEXTURE_MAPPING  			
		#pragma shader_feature_local USING_SPECULAR_MAP // Whether the shader is using the specular map or specular color.
		#pragma shader_feature_local EMISSION_ENABLED 
		#pragma shader_feature_local USING_EMISSION_MAP 
		
		// The structure used to pass data into the surface function.
		 struct Input
		{
			float2 texCoords : TEXCOORD0;
			fixed4 color : COLOR;
			float3 affineTexCoords;
			bool distClip; // Whether a vertex should be clipped according to the draw distance value.
		};

		sampler2D _MainTex;
		float4 _MainTex_ST; // The four float values used to specify the tiling (x, y) and offset (z, w) values for the texture/maps as set in the Inspector window. 
		float4 _ColorTint;
		sampler2D _SpecularMap;
		float4 _SpecularColor;
		float _Smoothness;
		sampler2D _NormalMap;
		float4 _EmissionColor;
		sampler2D _EmissionMap;
		float _VertJitter;
		float _DrawDist;

		// A vertex modifier function that alters the incoming vertex data before it reaches the generated vertex function.
		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			// Vertex snapping:				
			float geoRes = (_VertJitter - 1.0f) * -1000.0f; // A geometric resolution value which transforms the vertex jitter value from a range of 0.0f (no effect) - 1.0f (full effect) into a more useful range of 1000.0f (no effect) - 0.0f (full effect).
			float4 viewPos = float4(UnityObjectToViewPos(v.vertex.xyz).xyz, 1);
			viewPos.xyz = floor(viewPos.xyz * geoRes) / geoRes; // Multiplies the view-space vertex position by the geometric resolution variable, rounds each value to the nearest integer, then divides it by geores in order to quantize it.	
			float4 clipPos = mul(UNITY_MATRIX_P, viewPos);
			v.vertex = mul(viewPos, UNITY_MATRIX_IT_MV);			

			o.color = v.color;

			o.texCoords = v.texcoord;

			// Affine texture mapping:
			float wVal = mul(UNITY_MATRIX_P, clipPos).z;
			o.affineTexCoords = float3(o.texCoords * wVal, wVal);

			// Draw distance:
			o.distClip = false;
			float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
			if (distance(worldPos, _WorldSpaceCameraPos) > _DrawDist && _DrawDist != 0)
				o.distClip = true;
		}

		// The function which fills and outputs data that describes the properties of the mesh surface and is used to generate vertex and fragment functions as well as the passes used for rendering.
		void surf(Input IN, inout SurfaceOutputStandardSpecular o)
		{
			// Draw distance:
			if (IN.distClip)
				clip(-1);

			float2 finalUV = TRANSFORM_TEX(IN.texCoords, _MainTex);
			// Affine texture mapping:
			#ifdef ENABLE_AFFINE_TEXTURE_MAPPING	
				finalUV = TRANSFORM_TEX((IN.affineTexCoords / IN.affineTexCoords.z).xy, _MainTex);
			#endif

			fixed4 c = tex2D(_MainTex, finalUV);
			o.Albedo = c.rgb * IN.color.rgb * _ColorTint.rgb;
			o.Alpha = c.a  * IN.color.a * _ColorTint.a;

			o.Specular = _SpecularColor;
			#ifdef USING_SPECULAR_MAP 
				o.Specular = tex2D(_SpecularMap, finalUV);
			#endif
			o.Smoothness = _Smoothness;

			o.Normal = UnpackNormal(tex2D(_NormalMap, finalUV));

			#ifdef EMISSION_ENABLED 
				o.Emission = _EmissionColor;
				#ifdef USING_EMISSION_MAP 
					o.Emission *= tex2D(_EmissionMap, finalUV);
				#endif
			#endif
		}

		ENDCG
	}

	FallBack "Diffuse"
	CustomEditor "RetroSpriteLitShaderCustomGUI"
}
