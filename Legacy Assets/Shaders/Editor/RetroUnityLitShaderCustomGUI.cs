//////////////////////////////////////////////////
// Author:				LEAKYFINGERS
// Date created:		06.10.19
// Date last edited:	03.11.19
// References:          https://docs.unity3d.com/Manual/SL-CustomShaderGUI.html
//                      https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/StandardShaderGUI.cs
//////////////////////////////////////////////////
using System;
using UnityEngine;
using UnityEditor;

// The class which defines a custom GUI for materials using the Retro 3D unity lighting shader.
public class RetroUnityLitShaderCustomGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        FindProperties(properties);
        this.materialEditor = materialEditor;
        Material material = materialEditor.target as Material;

        ShaderPropertiesGUI(material);
    }


    // The static class containing the strings used for the GUI labels and tooltips.
    private static class Styles
    {
        public static string MapsText = "Maps";
        public static GUIContent AlbedoText = new GUIContent("Albedo", "Albedo (RGB) and Transparency (A)");
        public static GUIContent SpecularText = new GUIContent("Specular", "Specular (RGB) and Smoothness (A)");
        public static GUIContent SmoothnessText = EditorGUIUtility.TrTextContent("Smoothness", "Smoothness value");
        public static GUIContent NormalText = new GUIContent("Normal", "Normal Map");
        public static GUIContent EmissionText = new GUIContent("Color", "Emission (RGB)");

        public static string RetroText = "Retro Properties";
        public static string VertexJitterIntensityText = "Vertex Jitter Intensity";
        public static GUIContent AffineMapText = new GUIContent("Affine Texture Mapping", "Disable for default perspective-correct texture mapping");
        public static string DrawDistanceText = "Vertex Draw Distance";
    }

    private MaterialEditor materialEditor;
    private MaterialProperty albedoMap = null;
    private MaterialProperty albedoColor = null;
    private MaterialProperty specularMap = null;
    private MaterialProperty specularColor = null;
    private MaterialProperty smoothness = null;
    private MaterialProperty emissionColorForRendering = null;
    private MaterialProperty emissionMap = null;
    private MaterialProperty normalMap = null;
    private MaterialProperty vertexJitter = null;
    private MaterialProperty drawDistance = null;

    // Gets the properties used by the shader and uses them to update the corresponding member variables.
    private void FindProperties(MaterialProperty[] properties)
    {
        albedoMap = FindProperty("_AlbedoTex", properties);
        albedoColor = FindProperty("_AlbedoColorTint", properties);
        specularMap = FindProperty("_SpecularMap", properties);
        specularColor = FindProperty("_SpecularColor", properties);
        smoothness = FindProperty("_Smoothness", properties);
        normalMap = FindProperty("_NormalMap", properties);
        emissionColorForRendering = FindProperty("_EmissionColor", properties);
        emissionMap = FindProperty("_EmissionMap", properties);
        vertexJitter = FindProperty("_VertJitter", properties);
        drawDistance = FindProperty("_DrawDist", properties);
    }

    // Displays the shader properties on the material GUI.
    private void ShaderPropertiesGUI(Material material)
    {
        EditorGUIUtility.labelWidth = 0.0f; // Use default label width.

        EditorGUI.BeginChangeCheck();
        {
            // The 'Maps' section:
            GUILayout.Label(Styles.MapsText, EditorStyles.boldLabel);
            DoAlbedoArea(material);
            DoSpecularArea(material);
            DoNormalArea(material);
            DoEmissionArea(material);
            materialEditor.TextureScaleOffsetProperty(albedoMap); // Displays the offset and tiling values to be used for all the maps.

            EditorGUILayout.Space();

            // The 'Retro Properties' section:
            GUILayout.Label(Styles.RetroText, EditorStyles.boldLabel);
            DoVertexJitterArea(material);
            DoAffineMappingArea(material);
            DoDrawDistanceArea(material);
        }
        if (EditorGUI.EndChangeCheck())        
            MaterialChanged(material);        
    }

    private void DoAlbedoArea(Material material)
    {
        materialEditor.TexturePropertySingleLine(Styles.AlbedoText, albedoMap, albedoColor);
    }

    private void DoSpecularArea(Material material)
    {
        bool hasSpecularMap = specularMap.textureValue != null;
        materialEditor.TexturePropertySingleLine(Styles.SpecularText, specularMap, hasSpecularMap ? null : specularColor); // Displays the specular color picker if the specular map value is 'none', else just displays the specular map.
        if (hasSpecularMap)
            material.EnableKeyword("USING_SPECULAR_MAP");
        else
            material.DisableKeyword("USING_SPECULAR_MAP");

        int indentation = 2;
        materialEditor.ShaderProperty(smoothness, Styles.SmoothnessText, indentation);
    }

    private void DoNormalArea(Material material)
    {
        materialEditor.TexturePropertySingleLine(Styles.NormalText, normalMap);
    }

    private void DoEmissionArea(Material material)
    {
        if (materialEditor.EmissionEnabledProperty())
        {
            bool hasEmissionTexture = emissionMap.textureValue != null;

            materialEditor.TexturePropertyWithHDRColor(Styles.EmissionText, emissionMap, emissionColorForRendering, false);

            float brightness = emissionColorForRendering.colorValue.maxColorComponent;
            if (emissionMap.textureValue != null && !hasEmissionTexture && brightness <= 0.0f)
                emissionColorForRendering.colorValue = Color.white;

            if (hasEmissionTexture)
                material.EnableKeyword("USING_EMISSION_MAP");
            else
                material.DisableKeyword("USING_EMISSION_MAP");

            materialEditor.LightmapEmissionFlagsProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel, true);
        }
    }

    private void DoVertexJitterArea(Material material)
    {
        materialEditor.RangeProperty(vertexJitter, Styles.VertexJitterIntensityText);
    }

    private void DoAffineMappingArea(Material material)
    {
        bool affineMappingEnabled = Array.IndexOf(material.shaderKeywords, "ENABLE_AFFINE_TEXTURE_MAPPING") != -1;

        EditorGUI.BeginChangeCheck();

        affineMappingEnabled = EditorGUILayout.Toggle(Styles.AffineMapText, affineMappingEnabled);

        if (EditorGUI.EndChangeCheck())
        {
            if (affineMappingEnabled)
                material.EnableKeyword("ENABLE_AFFINE_TEXTURE_MAPPING");
            else
                material.DisableKeyword("ENABLE_AFFINE_TEXTURE_MAPPING");
        }
    }

    private void DoDrawDistanceArea(Material material)
    {
        materialEditor.FloatProperty(drawDistance, Styles.DrawDistanceText);
    }

    private static void SetMaterialKeywords(Material material)
    {
        MaterialEditor.FixupEmissiveFlag(material);
        bool shouldEmissionBeEnabled = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;

        if (shouldEmissionBeEnabled)
            material.EnableKeyword("EMISSION_ENABLED");
        else
            material.DisableKeyword("EMISSION_ENABLED");
    }

    private static void MaterialChanged(Material material)
    {
        SetMaterialKeywords(material);
    }
}