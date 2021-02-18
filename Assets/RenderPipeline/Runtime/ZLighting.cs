using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ZLighting
{
	const string bufferName = "Lighting";
	static int dirLightColorId = Shader.PropertyToID("_DirectionalLightColor");
	static int dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection");

	CommandBuffer buffer = new CommandBuffer
	{
		name = bufferName
	};

	public void Setup(ScriptableRenderContext context)
	{
		buffer.BeginSample(bufferName);
		SetupDirectionalLight();
		buffer.EndSample(bufferName);
		context.ExecuteCommandBuffer(buffer);
		buffer.Clear();
	}

	void SetupDirectionalLight()
	{
		Light light = RenderSettings.sun;
		buffer.SetGlobalVector(dirLightColorId, light.color.linear);
		buffer.SetGlobalVector(dirLightDirectionId, -light.transform.forward);
	}
}
