//////////////////////////////////////////////////
// Author:              LEAKYFINGERS
// Date created:        27.10.19
// Date last edited:    07.11.19
//////////////////////////////////////////////////
Shader "Retro 3D Shader Pack/Post-Process"
{
	HLSLINCLUDE	
		
	#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl" // Holds pre-configured vertex shaders, varying structs, and most of the data required to create common effects.
			
	sampler2D _MainTex;			

	int _PixelScale; // The value used to scale every pixel in the image if the shader isn't using 'fixed resolution' mode (_FixedVerticalResolution set to -1.0f).	
	int _FixedVerticalResolution; // The fixed vertical resolution used to pixelate the image if the shader is using 'fixed resolution' mode (_PixelScale set to -1.0f).	
	float _SourceRenderWidth; // The width of the source rendered image in texels.	
	float _SourceRenderHeight; // The height of the source rendered image in texels.		

	float _ColorDepth; // The value which determines the range of the color palette applied to the image.
	
	sampler2D _DitherPattern; // The pattern used to implement ordered dithering.
	float4 _DitherPattern_TexelSize; // Four float values containing the texel dimensions of the dither pattern texture (1.0 / width, 1.0 / height, width, height).
	int _DitherPatternScale; // The value used to scale the dither pattern.
	float _DitherThreshold; // The threshold used to control the range of colors that are affected by the dithering effect.
	float _DitherIntensity; 

	float4 Frag(VaryingsDefault i) : SV_Target
	{
		// Pixelation:
		// If using fixed resolution mode, calulates the appropriate pixel scale value using the fixed vertical resolution value. 
		float finalPixelScale = _PixelScale; 
		if(_FixedVerticalResolution != -1)
			finalPixelScale = _SourceRenderHeight / _FixedVerticalResolution;
		float4 outputColor = tex2D(_MainTex, i.texcoord);
		if (finalPixelScale > 1)
		{
			// The width and height percentage of each scaled pixel within the overall image (e.g. a scaled width of 0.25f is one-quarter the width of the image).
			float dx = finalPixelScale * (1.0f / _SourceRenderWidth);
			float dy = finalPixelScale * (1.0f / _SourceRenderHeight);
			float2 coord = float2(dx * floor(i.texcoord.x / dx), dy * floor(i.texcoord.y / dy)); // The texture coordinate from which to sample the original source image color for this pixel.		
			outputColor = tex2D(_MainTex, coord);
		}		
		 
		// Dithering:		
		float2 screenPos = i.texcoord.xy * _ScreenParams.xy; // The position of the current fragment/pixel on the screen.
		float2 ditherCoordinate = screenPos * _DitherPattern_TexelSize.xy / (_DitherPatternScale * finalPixelScale); // The associated texture coordinate of the dither pattern texture to sample.
		float ditherValue = tex2D(_DitherPattern, ditherCoordinate).r; 
		outputColor = outputColor + ditherValue * _DitherThreshold *_DitherIntensity;

		// Posterization:
		outputColor.rgb = floor(outputColor.rgb * (_ColorDepth * 256)) / (_ColorDepth * 256); // Applies the effect by multiplying the initial color by the color depth value * 100, rounding it to the nearest whole integer, and re-dividing it to quantize the color value.			   	

		return outputColor;
	}

	ENDHLSL

	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDefault // Specifies that the default vertex function will be used as a custom version isn't required for post-processing.
			#pragma fragment Frag

			ENDHLSL
		}
	}
}