﻿//////////////////////////////////////////////////
// Author:				LEAKYFINGERS
// Date created:		02.10.19
// Date last edited:	18.10.19
// References:			http://wiki.unity3d.com/index.php/CGVertexLit
//////////////////////////////////////////////////
Shader "Retro 3D Shader Pack/Vertex Lit (Transparent)"
{  
	Properties  
	{
		_AlbedoTex("Albedo Texture", 2D) = "white" {}
		_AlbedoColorTint("Albedo Color Tint", Color) = (1, 1, 1, 1)
		_SpecularMap("Specular Map", 2D) = "white" {}
		_SpecularColor("Specular Color", Color) = (0, 0, 0, 1)
		_Smoothness("Smoothness", Range(0.01, 1.0)) = 0.5
		_NormalMap("Normal Map", 2D) = "bump" {}
		[HDR] _EmissionColor("Emission Color", Color) = (0, 0, 0, 1)
		[HDR] _EmissionMap("Emission Map", 2D) = "black" {}

		_VertJitter("Vertex Jitter", Range(0.0, 0.999)) = 0.95 // The range used to set the geometric resolution of each vertex position value in order to create a vertex jittering/snapping effect.		
		_DrawDist("Draw Distance", Float) = 0 // The max draw distance from the camera to each vertex, with all vertices outside this range being clipped - set to 0 for infinite range.
	}

	SubShader  
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "ForceNoShadowCasting" = "True"}
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha // Specifies the equation used for blending overlapping transparent surfaces.
		ColorMask RGB
		
		Pass
		{
			Tags { LightMode = Vertex }

			CGPROGRAM
			 
			#pragma vertex vert  
			#pragma fragment frag	
			#pragma multi_compile_fog
			#pragma shader_feature_local ENABLE_SCREENSPACE_JITTER
			#pragma shader_feature_local ENABLE_AFFINE_TEXTURE_MAPPING 
			#pragma shader_feature_local USING_SPECULAR_MAP // Whether the shader is using the specular map or specular color.
			#pragma shader_feature_local EMISSION_ENABLED 
			#pragma shader_feature_local USING_EMISSION_MAP 
			#include "UnityCG.cginc"
			#include "./CG_Includes/RetroVertexLit.cginc" // The include file containing the majority of the shader code which is shared between the transparent and non-transparent variants of the shader. 	

			ENDCG
		}
	}

	Fallback "VertexLit"
	CustomEditor "RetroVertexLitShaderCustomGUI"
}