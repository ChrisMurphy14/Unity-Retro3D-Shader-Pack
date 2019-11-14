//////////////////////////////////////////////////
// Author:				LEAKYFINGERS
// Date created:		18.08.19
// Date last edited:	14.11.19
//////////////////////////////////////////////////
Shader "Retro 3D Shader Pack/Flat Lit (Transparent)"
{	
	Properties
	{ 
		_MainTex("Albedo Texture", 2D) = "white" {}
		_Color("Albedo Color Tint", Color) = (1, 1, 1, 1)
		_SpecGlossMap("Specular Map", 2D) = "white" {}
		_SpecularColor("Specular Color", Color) = (0, 0, 0, 1) 
		_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.0
		[HDR] _EmissionColor("Emission Color", Color) = (0, 0, 0, 1)
		[HDR] _EmissionMap("Emission Map", 2D) = "black" {}

		_VertJitter("Vertex Jitter", Range(0.0, 0.999)) = 0.95 // The range used to set the geometric resolution of each vertex position value in order to create a vertex jittering/snapping effect.		
		_AffineMapIntensity("Affine Texture Mapping Intensity", Range(0.0, 1.0)) = 1.0 // The intensity of the affine texture mapping effect - set to 0.0 for perspective-correct texture mapping.
		_DrawDist("Draw Distance", Float) = 0 // The max draw distance from the camera to each vertex, with all vertices outside this range being clipped - set to 0 for infinite range.
	}
		
	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "ForceNoShadowCasting" = "True" }
		ZWrite Off

		CGPROGRAM

		#pragma surface surf StandardSpecular alpha:fade vertex:vert 
		#pragma target 3.0		
		#pragma shader_feature_local ENABLE_SCREENSPACE_JITTER			
		#pragma shader_feature_local USING_SPECULAR_MAP // Whether the shader is using the specular map or specular color.
		#pragma shader_feature_local EMISSION_ENABLED 
		#pragma shader_feature_local USING_EMISSION_MAP 		
		#include "./CG_Includes/RetroFlatLit.cginc" // The include file that contains the majority of the shader code which is shared between the transparent and non-transparent variants of the shader. 
		
		ENDCG 
	}

	FallBack "Diffuse" 
	CustomEditor "RetroFlatLitShaderCustomGUI"
}