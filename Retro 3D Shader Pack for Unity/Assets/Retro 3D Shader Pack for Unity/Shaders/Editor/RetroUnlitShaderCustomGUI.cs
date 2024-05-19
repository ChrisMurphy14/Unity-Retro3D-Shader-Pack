//////////////////////////////////////////////////
// Author:				Chris Murphy
// Date created:		02.10.19
// Date last edited:	19.05.24
// Notes:               Any file containing a class derived from 'ShaderGUI' must be placed in a folder named 'Editor' to function correctly.
// References:          https://docs.unity3d.com/Manual/SL-CustomShaderGUI.html
//                      https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/StandardShaderGUI.cs
//////////////////////////////////////////////////
using System;
using UnityEngine;
using UnityEditor;

namespace Retro3DShaderPack
{
    // Defines a custom GUI for materials using the Retro 3D Unlit shaders (opaque and transparent).
    public class RetroUnlitShaderCustomGUI : ShaderGUI
    {
        public enum VertexJitterSpace
        {
            World,
            Screen
        }

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
            public static GUIContent AlbedoText = new GUIContent("Albedo", "Albedo (RGB) and Transparency (A)"); // The text and tooltip displayed for the albedo map.

            public static string RetroText = "Retro Properties";
            public static string VertexJitterIntensityText = "Vertex Jitter Intensity";
            public static GUIContent VertexJitterSpaceText = new GUIContent("Vertex Jitter Space");
            public static string AffineMapText = "Affine Texture Mapping Intensity";
            public static string DrawDistanceText = "Vertex Draw Distance";
            public static string DrawDistanceTipText = "(Set to '0' for infinite draw distance)";
        }

        private MaterialEditor _materialEditor;
        private MaterialProperty _albedoMap = null;
        private MaterialProperty _albedoColor = null;
        private MaterialProperty _vertexJitter = null;
        private MaterialProperty _affineMapIntensity = null;
        private MaterialProperty _drawDistance = null;

        // Gets the properties used by the shader and uses them to update the corresponding member variables.
        private void FindProperties(MaterialProperty[] properties)
        {
            _albedoMap = FindProperty("_MainTex", properties);
            _albedoColor = FindProperty("_Color", properties);
            _vertexJitter = FindProperty("_VertJitter", properties);
            _affineMapIntensity = FindProperty("_AffineMapIntensity", properties);
            _drawDistance = FindProperty("_DrawDist", properties);
        }

        // Displays the shader properties on the material GUI.
        private void ShaderPropertiesGUI(Material material)
        {
            EditorGUIUtility.labelWidth = 0.0f; // Use default label width.

            // The 'Maps' section:
            GUILayout.Label(Styles.MapsText, EditorStyles.boldLabel);
            DoAlbedoArea(material);
            _materialEditor.TextureScaleOffsetProperty(_albedoMap); // Displays the offset and tiling values to be used for all the maps.

            EditorGUILayout.Space();

            // The 'Retro Properties' section:
            GUILayout.Label(Styles.RetroText, EditorStyles.boldLabel);
            DoVertexJitterArea(material);
            DoAffineMappingArea(material);
            DoDrawDistanceArea(material);
        }

        private void DoAlbedoArea(Material material)
        {
            _materialEditor.TexturePropertySingleLine(Styles.AlbedoText, _albedoMap, _albedoColor);
        }

        private void DoVertexJitterArea(Material material)
        {
            _materialEditor.RangeProperty(_vertexJitter, Styles.VertexJitterIntensityText);

            VertexJitterSpace vertexJitterSpace = Array.IndexOf(material.shaderKeywords, "ENABLE_SCREENSPACE_JITTER") != -1 ? VertexJitterSpace.Screen : VertexJitterSpace.World;
            EditorGUI.BeginChangeCheck();
            vertexJitterSpace = (VertexJitterSpace)EditorGUILayout.EnumPopup(Styles.VertexJitterSpaceText, vertexJitterSpace);
            if (EditorGUI.EndChangeCheck())
            {
                if (vertexJitterSpace == VertexJitterSpace.Screen)
                    material.EnableKeyword("ENABLE_SCREENSPACE_JITTER");
                else if (vertexJitterSpace == VertexJitterSpace.World)
                    material.DisableKeyword("ENABLE_SCREENSPACE_JITTER");
            }
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
    }
}