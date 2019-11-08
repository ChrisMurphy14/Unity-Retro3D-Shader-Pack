//////////////////////////////////////////////////
// Author:              LEAKYFINGERS
// Date created:        27.10.19
// Date last edited:    27.10.19
//////////////////////////////////////////////////
using UnityEditor;
using UnityEditor.Rendering.PostProcessing;
using UnityEngine.Rendering.PostProcessing;


[PostProcessEditor(typeof(RetroPostProcessEffect))]
public sealed class RetroPostProcessEditor : PostProcessEffectEditor<RetroPostProcessEffect>
{
    public SerializedParameterOverride UsesFixedResolutionParameter; 
    public SerializedParameterOverride PixelScaleParameter;
    public SerializedParameterOverride FixedVerticalResolutionParameter;

    public SerializedParameterOverride ColorDepthParameter;

    public SerializedParameterOverride DitherPatternParameter;
    public SerializedParameterOverride DitherPatternScaleParameter;
    public SerializedParameterOverride DitherThresholdParameter;
    public SerializedParameterOverride DitherIntensityParameter;

    // Called when the editor becomes active.
    public override void OnEnable()
    {
        // Connects the editor parameters to the appropriate variables in the RetroPostProcessEffect script.
        UsesFixedResolutionParameter = FindParameterOverride(x => x.UsesFixedResolution);
        PixelScaleParameter = FindParameterOverride(x => x.PixelScale);
        FixedVerticalResolutionParameter = FindParameterOverride(x => x.FixedVerticalResolution);

        ColorDepthParameter = FindParameterOverride(x => x.ColorDepth);

        DitherPatternParameter = FindParameterOverride(x => x.DitherPattern);
        DitherPatternScaleParameter = FindParameterOverride(x => x.DitherPatternScale);
        DitherThresholdParameter = FindParameterOverride(x => x.DitherThreshold);
        DitherIntensityParameter = FindParameterOverride(x => x.DitherIntensity);
    }

    // Called each time the inspector GUI is updated.
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Pixelation", EditorStyles.boldLabel);
        PropertyField(UsesFixedResolutionParameter);
        bool usingFixedResolution = UsesFixedResolutionParameter.value.boolValue;                                  
        // Displays the parameter that is currently enabled according to the 'fixed resolution mode' checkbox.
        if (usingFixedResolution)
            PropertyField(FixedVerticalResolutionParameter);
        else
            PropertyField(PixelScaleParameter);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Posterization", EditorStyles.boldLabel);
        PropertyField(ColorDepthParameter);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Dithering", EditorStyles.boldLabel);
        PropertyField(DitherPatternParameter);
        PropertyField(DitherPatternScaleParameter);
        PropertyField(DitherThresholdParameter);
        PropertyField(DitherIntensityParameter);
    }
}