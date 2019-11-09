//////////////////////////////////////////////////
// Author:              LEAKYFINGERS
// Date created:        27.10.19
// Date last edited:    09.11.19
//////////////////////////////////////////////////
using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;


[Serializable]
[PostProcess(typeof(RetroPostProcessEffectRenderer), PostProcessEvent.AfterStack, "Retro Effects")] // Tells Unity that this class holds the data for a post-processing effect.
public sealed class RetroPostProcessEffect : PostProcessEffectSettings
{   
    [Tooltip("Whether the pixelation effect uses a fixed vertical resolution or a pixel size multiplier value.")]
    public BoolParameter UsesFixedResolution = new BoolParameter { value = false };
    [UnityEngine.Min(1.0f), Tooltip("The value used to scale the width and height of each pixel")]
    public IntParameter PixelScale = new IntParameter { value = 4 };
    [UnityEngine.Min(1), Tooltip("The vertical resolution of the image output by the pixelation effect.")]
    public IntParameter FixedVerticalResolution = new IntParameter { value = 480 };
    
    [Range(0.0f, 1.0f), Tooltip("Determines the range of the color palette applied to the image.")]
    public FloatParameter ColorDepth = new FloatParameter { value = 0.15f };

    [DisplayName("Dither Pattern"), Tooltip("The pattern used to implement ordered dithering.")]
    public TextureParameter DitherPattern = new TextureParameter { value = null, defaultState = TextureParameterDefault.None };
    [Tooltip("The scale multiplier for the dither pattern (the dither pattern size is also automatically scaled according to the pixelation effect scaling).")]
    public IntParameter DitherPatternScale = new IntParameter { value = 1 };
    [Range(0.0f, 1.0f), Tooltip("The threshold used to control the range of colors that are affected by dithering.")]
    public FloatParameter DitherThreshold = new FloatParameter { value = 0.75f };
    [Range(0.0f, 1.0f), Tooltip("The intensity of the dithering effect.")]
    public FloatParameter DitherIntensity = new FloatParameter { value = 0.15f };    
}


public sealed class RetroPostProcessEffectRenderer : PostProcessEffectRenderer<RetroPostProcessEffect> // The class used to handle the C# side of the post-process effect rendering.
{
    // Renders the image output by the camera using the post-process shader and outputs it to the appropriate destination.
    public override void Render(PostProcessRenderContext context)
    {
        // The property sheet used to handle the post-process shader code.
        PropertySheet sheet = context.propertySheets.Get(Shader.Find("Retro 3D Shader Pack/Post-Process"));

        // Depending on whether fixed resolution mode is being used, passes either the resolution or scaling value and sets the other to -1.
        if (settings.UsesFixedResolution)
        {
            sheet.properties.SetInt("_PixelScale", -1);
            sheet.properties.SetInt("_FixedVerticalResolution", settings.FixedVerticalResolution);
        }
        else
        {
            sheet.properties.SetInt("_PixelScale", settings.PixelScale);
            sheet.properties.SetInt("_FixedVerticalResolution", -1);
        }
        sheet.properties.SetFloat("_SourceRenderWidth", context.width);
        sheet.properties.SetFloat("_SourceRenderHeight", context.height);

        sheet.properties.SetFloat("_ColorDepth", settings.ColorDepth);

        if (settings.DitherPattern.value)
        {
            sheet.properties.SetTexture("_DitherPattern", settings.DitherPattern);
            sheet.properties.SetInt("_DitherPatternScale", settings.DitherPatternScale);
            sheet.properties.SetFloat("_DitherThreshold", settings.DitherThreshold);
            sheet.properties.SetFloat("_DitherIntensity", settings.DitherIntensity);            
        }          
        
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0); // Applies the shader to the source image and outputs the result to the appropriate destination.
    }
}