using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/ZRenderPipeline")]
public class ZRenderPipelineAsset : RenderPipelineAsset
{
    [SerializeField]
    bool useDynamicBatching = true;
    [SerializeField]
    bool useGPUInstancing = true;
    [SerializeField]
    bool useSRPBatcher = true;
    protected override RenderPipeline CreatePipeline()
    {
        return new ZRenderPipeline(
            useDynamicBatching, useGPUInstancing, useSRPBatcher
        );
    }
}
