using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ZRenderPipeline : RenderPipeline
{
    ZForwardRenderer renderer = new ZForwardRenderer();
    bool useDynamicBatching, useGPUInstancing;
    ZShadowSettings shadowSettings;

    public ZRenderPipeline(
        bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher,
        ZShadowSettings shadowSettings
    ) {
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        this.shadowSettings = shadowSettings;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        GraphicsSettings.lightsUseLinearIntensity = true;
    }
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach(Camera camera in cameras)
        {
            renderer.Render(
                context, camera, useDynamicBatching, useGPUInstancing, shadowSettings
            );
        }
    }
}
