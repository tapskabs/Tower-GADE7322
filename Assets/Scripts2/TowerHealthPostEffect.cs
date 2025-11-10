using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Custom/TowerHealthPostEffect")]
public class TowerHealthPostEffect : VolumeComponent, IPostProcessComponent
{
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
    public ColorParameter tintColor = new ColorParameter(Color.red);

    public bool IsActive() => intensity.value > 0f;
    public bool IsTileCompatible() => true;
}

public class TowerHealthPostEffectRenderer : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        private Material material;
        private RTHandle tempTexture;
        private RTHandle source;

        public CustomRenderPass(Material mat)
        {
            material = mat;
        }

        public void Setup(RTHandle src)
        {
            source = src;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            RenderingUtils.ReAllocateIfNeeded(ref tempTexture, descriptor, name: "_TempTowerHealthTex");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null) return;

            var stack = VolumeManager.instance.stack;
            var effect = stack.GetComponent<TowerHealthPostEffect>();
            if (!effect.IsActive()) return;

            CommandBuffer cmd = CommandBufferPool.Get("TowerHealthPostEffect");

            material.SetFloat("_Intensity", effect.intensity.value);
            material.SetColor("_TintColor", effect.tintColor.value);

            Blit(cmd, source, tempTexture, material);
            Blit(cmd, tempTexture, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (tempTexture != null)
                tempTexture.Release();
        }
    }

    public Shader shader;
    private Material material;
    private CustomRenderPass pass;

    public override void Create()
    {
        if (shader == null)
            shader = Shader.Find("Hidden/TowerHealthPostEffect");

        material = CoreUtils.CreateEngineMaterial(shader);

        pass = new CustomRenderPass(material)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        pass.Setup(renderer.cameraColorTargetHandle);
        renderer.EnqueuePass(pass);
    }
}
