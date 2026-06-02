using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PowerCrackRendererFeature : ScriptableRendererFeature
{
    [SerializeField]
    private LayerMask layerMask;
    [SerializeField]
    private Material overrideMaterial;
    [SerializeField]
    private ComputeShader computeShader;
    [SerializeField]
    private int textureResolution = 64;
    [SerializeField]
    private int numPoints = 10;
    [SerializeField]
    private int seed = 1;

    class PowerCrackRenderPass : ScriptableRenderPass
    {
        private ProfilingSampler m_profilingSampler;
        private FilteringSettings m_filteringSettings;
        private List<ShaderTagId> m_shaderTagIds = new List<ShaderTagId>();
        private Material m_overrideMaterial;
        private RenderTexture m_crackTexture;
        private ComputeShader m_computeShader;
        private Vector3[] m_points;

        public PowerCrackRenderPass(ComputeShader computeShader, LayerMask layerMask, Material overrideMaterial, int textureResolution, int numPoints, int seed)
        {
            m_overrideMaterial = overrideMaterial;
            m_computeShader = computeShader;

            m_profilingSampler = new ProfilingSampler("Power Crack");
            m_filteringSettings = new FilteringSettings(RenderQueueRange.opaque, layerMask);

            m_shaderTagIds.Add(new ShaderTagId("SRPDefaultUnlit"));
            m_shaderTagIds.Add(new ShaderTagId("UniversalForward"));
            m_shaderTagIds.Add(new ShaderTagId("UniversalForwardOnly"));

            ConfigureInput(ScriptableRenderPassInput.Depth);

            // create procedural texture
            m_crackTexture = new RenderTexture(textureResolution, textureResolution, 0);
            m_crackTexture.enableRandomWrite = true;
            m_crackTexture.dimension = TextureDimension.Tex3D;
            m_crackTexture.volumeDepth = textureResolution;
            m_crackTexture.wrapMode = TextureWrapMode.Repeat;
            m_crackTexture.filterMode = FilterMode.Trilinear;
            m_crackTexture.format = RenderTextureFormat.ARGBFloat;
            m_crackTexture.Create();

            m_points = new Vector3[numPoints];

            Random.InitState(seed);

            for (int i = 0; i < m_points.Length; i++)
            {
                m_points[i].x = Random.Range(0.0f, 1.0f);
                m_points[i].y = Random.Range(0.0f, 1.0f);
                m_points[i].z = Random.Range(0.0f, 1.0f);
            }

            ComputeBuffer pointsBuffer = new ComputeBuffer(m_points.Length, sizeof(float) * 3);
            pointsBuffer.SetData(m_points);

            int numThreadGroups = textureResolution / 8;

            m_computeShader.SetTexture(0, "result", m_crackTexture);
            m_computeShader.SetInt("resolution", textureResolution);
            m_computeShader.SetInt("numPoints", m_points.Length);
            m_computeShader.SetBuffer(0, "points", pointsBuffer);

            m_computeShader.Dispatch(0, numThreadGroups, numThreadGroups, numThreadGroups);

            pointsBuffer.Dispose();
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // nothing needed
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, m_profilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
                DrawingSettings drawingSettings = CreateDrawingSettings(m_shaderTagIds, ref renderingData, sortingCriteria);

                drawingSettings.overrideMaterial = m_overrideMaterial;
                if (m_overrideMaterial != null && m_crackTexture != null)
                    m_overrideMaterial.SetTexture("_MainTex", m_crackTexture);

                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_filteringSettings);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Release();
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }

        public void Dispose()
        {
            if (m_crackTexture != null)
            {
                if (m_crackTexture.IsCreated())
                    m_crackTexture.Release();
                UnityEngine.Object.DestroyImmediate(m_crackTexture);
                m_crackTexture = null;
            }
        }
    }

    PowerCrackRenderPass m_renderPass;

    public override void Create()
    {
        m_renderPass = new PowerCrackRenderPass(computeShader, layerMask, overrideMaterial, textureResolution, numPoints, seed);
        m_renderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (m_renderPass != null)
            renderer.EnqueuePass(m_renderPass);
    }

    protected override void Dispose(bool disposing)
    {
        if (m_renderPass != null)
        {
            m_renderPass.Dispose();
            m_renderPass = null;
        }
        base.Dispose(disposing);
    }
}