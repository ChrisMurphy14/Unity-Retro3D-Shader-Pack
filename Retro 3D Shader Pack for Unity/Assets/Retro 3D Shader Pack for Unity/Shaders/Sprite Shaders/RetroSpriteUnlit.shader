//////////////////////////////////////////////////
// Author:				LEAKYFINGERS
// Date created:		24.10.19
// Date last edited:	14.11.19
//////////////////////////////////////////////////
Shader "Retro 3D Shader Pack/Sprite (Unlit)"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_Color("Color Tint", Color) = ( 1, 1, 1, 1 )

		_VertJitter("Vertex Jitter", Range(0.0, 0.999)) = 0.95 // The range used to set the geometric resolution of each vertex position value in order to create a vertex jittering/snapping effect.		
		_AffineMapIntensity("Affine Texture Mapping Intensity", Range(0.0, 1.0)) = 1.0 // The intensity of the affine texture mapping effect - set to 0.0 for perspective-correct texture mapping.
		_DrawDist("Draw Distance", Float) = 0 // The max draw distance from the camera to each vertex, with all vertices outside this range being clipped - set to 0 for infinite range.
	}
			
    SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }		
		Blend SrcAlpha OneMinusSrcAlpha // Specifies the equation used for blending overlapping transparent surfaces.
		Cull Off 
		ZWrite Off

		Pass
		{
			CGPROGRAM

			#pragma vertex vert 
			#pragma fragment frag			
			#pragma multi_compile_fog
			#pragma shader_feature_local ENABLE_SCREENSPACE_JITTER 
			#include "UnityCG.cginc"			
			
			// A struct containing the vertex data to be passed into the vertex function.
			struct appdata
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			// A struct containing the fragment data (a collection of values produced by the rasterizer) to be passed into the fragment function.
			struct v2f
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
				float3 uv_affine : TEXCOORD2;
				float drawDistClip : TEXCOORD3; // The value used to pass whether the corresponding fragments of a vertex should be clipped according to the draw distance value: 0 == false, 1 == true.
				UNITY_FOG_COORDS(1)
			};

			sampler2D _MainTex; // The sampler used to store the sprite texture set in the Inspector window.
			float4 _MainTex_ST; // The four float values used to specify the tiling (x, y) and offset (z, w) values for the albedo texture as set in the Inspector window. 		
			fixed4 _Color;
			float _VertJitter;
			float _AffineMapIntensity;
			float _DrawDist;

			// Quantizes the position of the vertex in screen space according to the _VertJitter value and returns it - vertex pos must be in clip space first.
			float4 ScreenSnap(float4 vertex)
			{
				float geoRes = _VertJitter * 125.0f + 1.0f;	// A geometric resolution value which transforms the vertex jitter value from a range of 0.0f (no effect) - 1.0f (full effect) into a more useful range of 1.0f (no effect) - 126.0f (full effect).
				float2 pixelPos = round((vertex.xy / vertex.w) * _ScreenParams.xy / geoRes) * geoRes; // Transforms the vertex using the perspective divide to find its pixel location, then quantizes it.
				vertex.xy = pixelPos / _ScreenParams.xy * vertex.w; // Transforms the quantized pixel position back into clip space.
				return vertex;
			}

			// The vertex function used to transform vertices from world space into clip space.
			v2f vert(appdata v)
			{
				v2f o;

				// Vertex snapping:	
				#ifdef ENABLE_SCREENSPACE_JITTER
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.vertex = ScreenSnap(o.vertex);
				#else
					float geoRes = (_VertJitter - 1.0f) * -1000.0f; // A geometric resolution value which transforms the vertex jitter value from a range of 0.0f (no effect) - 1.0f (full effect) into a more useful range of 1000.0f (no effect) - 0.0f (full effect).
					float4 viewPos = float4(UnityObjectToViewPos(v.vertex.xyz).xyz, 1);
					viewPos.xyz = floor(viewPos.xyz * geoRes) / geoRes; // Multiplies the view-space vertex position by the geometric resolution variable, rounds each value to the nearest integer, then divides it by geores in order to quantize it.
					float4 clipPos = mul(UNITY_MATRIX_P, viewPos);
					o.vertex = clipPos;
				#endif	

				o.color = v.color;

				// Texture mapping:
				o.uv = v.uv;

				// Affine texture mapping:
				float wVal = mul(UNITY_MATRIX_P, o.vertex).z;
				o.uv_affine = float3(v.uv.xy * wVal, wVal);

				// Draw distance:
				o.drawDistClip = 0;
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
				if (distance(worldPos, _WorldSpaceCameraPos) > _DrawDist && _DrawDist != 0)
					o.drawDistClip = 1;

				UNITY_TRANSFER_FOG(o, o.vertex);

				return o;
			}

			// The fragment function used to transform fragments into pixels.
			fixed4 frag(v2f i) : SV_Target
			{
				// Affine texture mapping:
				float2 correctUV = TRANSFORM_TEX(i.uv, _MainTex);
				float2 affineUV = TRANSFORM_TEX((i.uv_affine / i.uv_affine.z).xy, _MainTex);
				float2 finalUV = lerp(correctUV, affineUV, _AffineMapIntensity);

				fixed4 col = tex2D(_MainTex, finalUV) * i.color * _Color;

				UNITY_APPLY_FOG(i.fogCoord, col);

				// Draw distance:
				if (i.drawDistClip != 0)
					clip(-1);

				return col;
			}

			ENDCG
		} 
	}

	FallBack "Unlit-Normal"
	CustomEditor "RetroSpriteUnlitShaderCustomGUI"
}