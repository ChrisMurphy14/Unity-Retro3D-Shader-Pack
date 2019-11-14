//////////////////////////////////////////////////
// Author:				LEAKYFINGERS
// Date created:		18.07.19
// Date last edited:	14.11.19
//////////////////////////////////////////////////
#ifndef _RETRO_3D_UNITY_LIGHTING_SHADER_
#define _RETRO_3D_UNITY_LIGHTING_SHADER_

// The structure used to pass data into the surface function.
 struct Input
{	
	float2 texCoords : TEXCOORD0;
	float3 affineTexCoords;
	bool distClip; // Whether a vertex should be clipped according to the draw distance value.
};

sampler2D _MainTex; 
float4 _MainTex_ST; // The four float values used to specify the tiling (x, y) and offset (z, w) values for the texture/maps as set in the Inspector window. 
float4 _Color;
sampler2D _SpecGlossMap;
float4 _SpecularColor;
float _Glossiness;
sampler2D _BumpMap;
float4 _EmissionColor;
sampler2D _EmissionMap;
float _VertJitter;
float _AffineMapIntensity;
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
	
	// Affine texture mapping:
	float2 correctUV = TRANSFORM_TEX(IN.texCoords, _MainTex);
	float2 affineUV = TRANSFORM_TEX((IN.affineTexCoords / IN.affineTexCoords.z).xy, _MainTex);
	float2 finalUV = lerp(correctUV, affineUV, _AffineMapIntensity);

	fixed4 c = tex2D(_MainTex, finalUV);
	o.Albedo = c.rgb * _Color.rgb;	
	o.Alpha = c.a * _Color.a;

	o.Specular = _SpecularColor;
	#ifdef USING_SPECULAR_MAP 
		o.Specular = tex2D(_SpecGlossMap, finalUV);		
	#endif
	o.Smoothness = _Glossiness; 
	
	o.Normal = UnpackNormal(tex2D(_BumpMap, finalUV));

	#ifdef EMISSION_ENABLED 
		o.Emission = _EmissionColor; 	
		#ifdef USING_EMISSION_MAP 
			o.Emission *= tex2D(_EmissionMap, finalUV);
		#endif
	#endif
}

#endif