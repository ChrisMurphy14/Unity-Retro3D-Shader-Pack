//////////////////////////////////////////////////
// Author:				LEAKYFINGERS
// Date created:		09.07.19
// Date last edited:	08.11.19
//////////////////////////////////////////////////
Shader "Retro 3D Shader Pack/Unlit"
{	
    Properties
    {		 
        _AlbedoTex ("Albedo Texture", 2D) = "white" {} 
		_AlbedoColorTint("Albedo Color Tint", Color) = (1, 1, 1, 1)  
		
		_VertJitter("Vertex Jitter", Range(0.0, 0.999)) = 0.95 // The range used to set the geometric resolution of each vertex position value in order to create a vertex jittering/snapping effect.		
		_DrawDist("Draw Distance", Float) = 0 // The max draw distance from the camera to each vertex, with all vertices outside this range being clipped - set to 0 for infinite range.
    } 

    SubShader
    {	 
		Tags { "RenderType" = "Opaque" }	
		
        Pass
        {
            CGPROGRAM 
						
            #pragma vertex vert 
            #pragma fragment frag			
			#pragma multi_compile_fog
			#pragma shader_feature_local ENABLE_SCREENSPACE_JITTER
			#pragma shader_feature_local ENABLE_AFFINE_TEXTURE_MAPPING 
            #include "UnityCG.cginc"			
			#include "./CG_Includes/RetroUnlit.cginc" // The include file containing the majority of the shader code which is shared between the transparent and non-transparent variants of the shader. 			

            ENDCG
        }
    }

	FallBack "Unlit-Normal"
	CustomEditor "RetroUnlitShaderCustomGUI"
}