using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class ZForwardRenderer
{
    const string bufferName = "Render Camera";
    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    static ShaderTagId litShaderTagId = new ShaderTagId("ZRPLit");


    #region Runtime Params
    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };
    ScriptableRenderContext context;
    Camera camera;
    ZLighting lighting = new ZLighting();
    CullingResults cullingResults;
    #endregion

    public void Render(ScriptableRenderContext context, Camera camera,
        bool useDynamicBatching, bool useGPUInstancing,
        ZShadowSettings shadowSettings)
    {
        this.context = context;
        this.camera = camera;

#if UNITY_EDITOR
        PrepareBuffer();
        PrepareForSceneWindow();
#endif

        if (!Cull(shadowSettings.maxDistance))
        {
            return;
        }

        // Shadow
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
        lighting.Setup(context, cullingResults, shadowSettings);
        buffer.EndSample(SampleName);

        // Geometry
        Setup();
        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
#if UNITY_EDITOR
        DrawUnsupportedShaders();
        DrawGizmos();
#endif
        lighting.Cleanup();
        Submit();
    }

    #region Draw Call Functions
    void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing
        };
        drawingSettings.SetShaderPassName(1, litShaderTagId);
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );

        context.DrawSkybox(camera);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;

        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );
    }
    #endregion

    #region Tool Functions
    void Setup()
    {
        context.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags;
        buffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth,
            flags == CameraClearFlags.Color,
            flags == CameraClearFlags.Color ?
                camera.backgroundColor.linear : Color.clear);
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
    }
    bool Cull(float maxShadowDistance)
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }
    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
    void Submit()
    {
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }
    #endregion
}
