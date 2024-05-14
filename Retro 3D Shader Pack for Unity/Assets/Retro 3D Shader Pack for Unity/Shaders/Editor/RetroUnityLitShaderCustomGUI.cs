//////////////////////////////////////////////////
// Author:				Chris Murphy
// Date created:		06.10.19
// Date last edited:	14.05.24
// Notes:               Any file containing a class derived from 'ShaderGUI' must be placed in a folder named 'Editor' to function correctly.
// References:          https://docs.unity3d.com/Manual/SL-CustomShaderGUI.html
//                      https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/StandardShaderGUI.cs
//////////////////////////////////////////////////
using System;
using UnityEngine;
using UnityEditor;

// Defines a custom GUI for materials using the Retro 3D unity lighting shader.
public class RetroUnityLitShaderCustomGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        FindProperties(properties);
        this._materialEditor = materialEditor;
        Material material = materialEditor.target as Material;

        ShaderPropertiesGUI(material);
    }

    // The static class containing the strings used for the GUI labels and tooltips.
    private static class Styles
    {
        public static string MapsText = "Maps";
        public static GUIContent AlbedoText = new GUIContent("Albedo", "Albedo (RGB) and Transparency (A)");
        public static GUIContent SpecularText = new GUIContent("Specular", "Specular color (RGB)");
        public static GUIContent SmoothnessText = EditorGUIUtility.TrTextContent("Smoothness", "Smoothness value");
        public static GUIContent NormalText = new GUIContent("Normal", "Normal Map");
        public static GUIContent EmissionText = new GUIContent("Color", "Emission (RGB)");

        public static string RetroText = "Retro Properties";
        public static string VertexJitterIntensityText = "Vertex Jitter Intensity";
        public static string AffineMapText = "Affine Texture Mapping Intensity";
        public static string DrawDistanceText = "Vertex Draw Distance";
        public static string DrawDistanceTipText = "(Set to '0' for infinite draw distance)";
    }

    private MaterialEditor _materialEditor;
    private MaterialProperty _albedoMap = null;
    private MaterialProperty _albedoColor = null;
    private MaterialProperty _specularMap = null;
    private MaterialProperty _specularColor = null;
    private MaterialProperty _smoothness = null;
    private MaterialProperty _emissionColorForRendering = null;
    private MaterialProperty _emissionMap = null;
    private MaterialProperty _normalMap = null;
    private MaterialProperty _vertexJitter = null;
    private MaterialProperty _affineMapIntensity = null;
    private MaterialProperty _drawDistance = null;

    // Gets the properties used by the shader and uses them to update the corresponding member variables.
    private void FindProperties(MaterialProperty[] properties)
    {
        _albedoMap = FindProperty("_MainTex", properties);
        _albedoColor = FindProperty("_Color", properties);
        _specularMap = FindProperty("_SpecGlossMap", properties);
        _specularColor = FindProperty("_SpecularColor", properties);
        _smoothness = FindProperty("_Glossiness", properties);
        _normalMap = FindProperty("_BumpMap", properties);
        _emissionColorForRendering = FindProperty("_EmissionColor", properties);
        _emissionMap = FindProperty("_EmissionMap", properties);
        _vertexJitter = FindProperty("_VertJitter", properties);
        _affineMapIntensity = FindProperty("_AffineMapIntensity", properties);
        _drawDistance = FindProperty("_DrawDist", properties);
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
            _materialEditor.TextureScaleOffsetProperty(_albedoMap); // Displays the offset and tiling values to be used for all the maps.

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
        _materialEditor.TexturePropertySingleLine(Styles.AlbedoText, _albedoMap, _albedoColor);
    }

    private void DoSpecularArea(Material material)
    {
        bool hasSpecularMap = _specularMap.textureValue != null;
        _materialEditor.TexturePropertySingleLine(Styles.SpecularText, _specularMap, hasSpecularMap ? null : _specularColor); // Displays the specular color picker if the specular map value is 'none', else just displays the specular map.
        if (hasSpecularMap)
            material.EnableKeyword("USING_SPECULAR_MAP");
        else
            material.DisableKeyword("USING_SPECULAR_MAP");

        int indentation = 2;
        _materialEditor.ShaderProperty(_smoothness, Styles.SmoothnessText, indentation);
    }

    private void DoNormalArea(Material material)
    {
        _materialEditor.TexturePropertySingleLine(Styles.NormalText, _normalMap);
    }

    private void DoEmissionArea(Material material)
    {
        if (_materialEditor.EmissionEnabledProperty())
        {
            bool hasEmissionTexture = _emissionMap.textureValue != null;

            _materialEditor.TexturePropertyWithHDRColor(Styles.EmissionText, _emissionMap, _emissionColorForRendering, false);

            float brightness = _emissionColorForRendering.colorValue.maxColorComponent;
            if (_emissionMap.textureValue != null && !hasEmissionTexture && brightness <= 0.0f)
                _emissionColorForRendering.colorValue = Color.white;

            if (hasEmissionTexture)
                material.EnableKeyword("USING_EMISSION_MAP");
            else
                material.DisableKeyword("USING_EMISSION_MAP");

            _materialEditor.LightmapEmissionFlagsProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel, true);
        }
    }

    private void DoVertexJitterArea(Material material)
    {
        _materialEditor.RangeProperty(_vertexJitter, Styles.VertexJitterIntensityText);
    }

    private void DoAffineMappingArea(Material material)
    {
        _materialEditor.RangeProperty(_affineMapIntensity, Styles.AffineMapText);
    }

    private void DoDrawDistanceArea(Material material)
    {        
        _materialEditor.FloatProperty(_drawDistance, Styles.DrawDistanceText);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Space(30);
            GUILayout.Label(Styles.DrawDistanceTipText);
        }          
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