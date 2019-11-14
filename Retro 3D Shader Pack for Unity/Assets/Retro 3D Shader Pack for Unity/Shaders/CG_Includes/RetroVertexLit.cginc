//////////////////////////////////////////////////
// Author:				LEAKYFINGERS
// Date created:		01.09.19
// Date last edited:	14.11.19
// References:			http://wiki.unity3d.com/index.php/CGVertexLit
//////////////////////////////////////////////////
#ifndef _RETRO_3D_VERTEX_LIT_SHADER_
#define _RETRO_3D_VERTEX_LIT_SHADER_

sampler2D _MainTex;
float4 _MainTex_ST; // The four float values used to specify the tiling (x, y) and offset (z, w) values for the textures/maps as set in the Inspector window. 
float4 _Color;
sampler2D _SpecGlossMap;
float4 _SpecColor;
float _Glossiness;
float4 _EmissionColor;
sampler2D _EmissionMap;
float _VertJitter;
float _AffineMapIntensity; 
float _DrawDist;

// A struct containing the fragment data (a collection of values produced by the rasterizer) to be passed into the fragment function.
struct v2f
{
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
	float3 uv_affine : TEXCOORD2;
	fixed3 diffuse : COLOR;
	fixed3 specular : TEXCOORD1; // Stores the specular color of the vertex in an available set of texture coordinates.	
	float drawDistClip : TEXCOORD3; // The value used to pass whether the corresponding fragments of a vertex should be clipped according to the draw distance value: 0 == false, 1 == true.
	UNITY_FOG_COORDS(4)
};

// Quantizes the position of the vertex in screen space according to the _VertJitter value and returns it - vertex pos must be in clip space first.
float4 ScreenSnap(float4 vertex)
{
	float geoRes = _VertJitter * 125.0f + 1.0f;	// A geometric resolution value which transforms the vertex jitter value from a range of 0.0f (no effect) - 1.0f (full effect) into a more useful range of 1.0f (no effect) - 126.0f (full effect).
	float2 pixelPos = round((vertex.xy / vertex.w) * _ScreenParams.xy / geoRes) * geoRes; // Transforms the vertex using the perspective divide to find its pixel location, then quantizes it.
	vertex.xy = pixelPos / _ScreenParams.xy * vertex.w; // Transforms the quantized pixel position back into clip space.
	return vertex;
}

// The vertex function used to transform vertices from world space into clip space.
v2f vert(appdata_full v)
{
	v2f o;	

	// Vertex snapping:	
	#ifdef ENABLE_SCREENSPACE_JITTER
		float4 viewPos = float4(UnityObjectToViewPos(v.vertex.xyz).xyz, 1);
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.vertex = ScreenSnap(o.vertex); 
	#else
		float geoRes = (_VertJitter - 1.0f) * -1000.0f; // A geometric resolution value which transforms the vertex jitter value from a range of 0.0f (no effect) - 1.0f (full effect) into a more useful range of 1000.0f (no effect) - 0.0f (full effect).
		float4 viewPos = float4(UnityObjectToViewPos(v.vertex.xyz).xyz, 1);
		viewPos.xyz = floor(viewPos.xyz * geoRes) / geoRes; // Multiplies the view-space vertex position by the geometric resolution variable, rounds each value to the nearest integer, then divides it by geores in order to quantize it.
		float4 clipPos = mul(UNITY_MATRIX_P, viewPos);
		o.vertex = clipPos;
	#endif	
		   
	o.uv = v.texcoord;

	// Affine texture mapping:
	float wVal = mul(UNITY_MATRIX_P, o.vertex).z;
	o.uv_affine = float3(v.texcoord.xy * wVal, wVal);
	   
	// Draw distance:
	o.drawDistClip = 0;
	float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
	if (distance(worldPos, _WorldSpaceCameraPos) > _DrawDist && _DrawDist != 0)
		o.drawDistClip = 1;

	// Lighting:
	o.diffuse = UNITY_LIGHTMODEL_AMBIENT.xyz; // The initial base diffuse color is set as the global ambient light.
	o.specular = 0;
	//viewPos = (mul(UNITY_MATRIX_MV, v.vertex).xyz, 1); // The vertex transformed into view-space, required because Unity's macros provide the data for each light in view-space.
	fixed3 viewDirObj = normalize(ObjSpaceViewDir(v.vertex)); // The direction from the object/local-space vertex towards the camera. 
	// Adds the colors from four vertex lights to the diffuse and specular values. 
	for (int i = 0; i < 4; i++)
	{
		half3 toLight = unity_LightPosition[i].xyz - viewPos.xyz * unity_LightPosition[i].w; // A vector from the light to the vertex in view space.
		half lengthSq = dot(toLight, toLight);
		half atten = 1.0 / (1.0 + lengthSq * unity_LightAtten[i].z); // The attenuation/falloff of the light over distance.
		fixed3 lightDirObj = mul((float3x3)UNITY_MATRIX_T_MV, toLight);	// The light direction converted from view to object/local-space.
		lightDirObj = normalize(lightDirObj);

		fixed diffuse = max(0, dot(v.normal, lightDirObj));
		o.diffuse += unity_LightColor[i].rgb * (diffuse * atten);

		fixed3 h = normalize(viewDirObj + lightDirObj);
		fixed nh = max(0, dot(v.normal, h));
		fixed specular = pow(nh, _Glossiness * 128.0) * 0.5;
		o.specular += specular * unity_LightColor[i].rgb * atten;
	}

	float4 emissionParameter = float4(0, 0, 0, 0);
	#ifdef EMISSION_ENABLED
		emissionParameter = _EmissionColor;
		#ifdef USING_EMISSION_MAP
			emissionParameter *= tex2Dlod(_EmissionMap, float4(v.texcoord.xy, 0, 0));
		#endif
	#endif
	o.diffuse = (o.diffuse * _Color + emissionParameter.rgb) * 2;
	   
	float4 specularParameter = _SpecColor;
	#ifdef USING_SPECULAR_MAP
		specularParameter = tex2Dlod(_SpecGlossMap, float4(v.texcoord.xy, 0, 0));
	#endif
	o.specular *= specularParameter * 2;	

	UNITY_TRANSFER_FOG(o, o.vertex);

	return o;
}

// The fragment function used to transform fragments into pixels.
fixed4 frag(v2f i) : COLOR
{	
	// Affine texture mapping:
	float2 correctUV = TRANSFORM_TEX(i.uv, _MainTex);
	float2 affineUV = TRANSFORM_TEX((i.uv_affine / i.uv_affine.z).xy, _MainTex);
	float2 finalUV = lerp(correctUV, affineUV, _AffineMapIntensity);

	fixed4 col = tex2D(_MainTex, finalUV);
	col.rgb = (col.rgb * i.diffuse + i.specular);
	col.a = col.a * _Color.a;
	
	UNITY_APPLY_FOG(i.fogCoord, col);

	// Draw distance:
	if (i.drawDistClip != 0)
		clip(-1);

	return col;
}

#endif