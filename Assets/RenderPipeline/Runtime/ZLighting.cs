using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class ZLighting
{
	const int maxDirLightCount = 4;

	const string bufferName = "Lighting";
	static int dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
	static int dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
	static int dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");
	static int dirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData");

	static Vector4[] dirLightColors = new Vector4[maxDirLightCount];
	static Vector4[] dirLightDirections = new Vector4[maxDirLightCount];
	static Vector4[] dirLightShadowData = new Vector4[maxDirLightCount];
	CullingResults cullingResults;
	ZShadows shadows = new ZShadows();

	CommandBuffer buffer = new CommandBuffer
	{
		name = bufferName
	};

	public void Setup(ScriptableRenderContext context, CullingResults cullingResults,
		ZShadowSettings shadowSettings)
	{
		this.cullingResults = cullingResults;
		buffer.BeginSample(bufferName);
		shadows.Setup(context, cullingResults, shadowSettings);
		SetupLights();
		shadows.Render();
		buffer.EndSample(bufferName);
		context.ExecuteCommandBuffer(buffer);
		buffer.Clear();
	}
	public void Cleanup()
	{
		shadows.Cleanup();
	}

	void SetupLights()
	{
		NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
		int dirLightCount = 0;
		for (int i = 0; i < visibleLights.Length; i++)
		{
			VisibleLight visibleLight = visibleLights[i];
			if (visibleLight.lightType == LightType.Directional && visibleLight.light.lightmapBakeType != LightmapBakeType.Baked)
			{
				SetupDirectionalLight(dirLightCount++, ref visibleLight);
				if (dirLightCount >= maxDirLightCount)
				{
					break;
				}
			}
		}

		buffer.SetGlobalInt(dirLightCountId, dirLightCount);
		buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
		buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
		buffer.SetGlobalVectorArray(dirLightShadowDataId, dirLightShadowData);

		//Light light = RenderSettings.sun;
		//buffer.SetGlobalVector(dirLightColorsId, light.color.linear * light.intensity);
		//buffer.SetGlobalVector(dirLightDirectionsId, -light.transform.forward);
	}

	void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
	{
		dirLightColors[index] = visibleLight.finalColor;
		dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
		dirLightShadowData[index] = shadows.ReserveDirectionalShadows(visibleLight.light, index);
	}
}
