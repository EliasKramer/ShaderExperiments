using Assets.SlimeSim;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DripRenderer : MonoBehaviour
{
    public ComputeShader TextureShader;
    private RenderTexture _rTexture;

    //between 0 and 1
    [SerializeField]
    public float feremonIntensity = 0.4f;
    [SerializeField]
    public float blurrMultiplier = .95f;
    [SerializeField]
    public float randomMult = 1;
    [SerializeField]
    public float randomAngleRangle = 12f;
    [SerializeField]
    public float speed = 2f;
    [SerializeField]
    public int iterations = 1;
    private struct Agent
    {
        public Vector2 position; //2 float size
        public float directionAngle; //1 float size
        public Color color; //4 float size
    }
    private const int threads = 1024;
    private int numberOfAgents = threads * 128;
    private Agent[] _agents;
    private ComputeBuffer _agentBuffer;
    private ColorRange[] _colorsForAgents;
    public void Awake()
    {
        _rTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        _rTexture.enableRandomWrite = true;
        _rTexture.Create();
    }
    private void Start()
    {
        //setRandom();
        initColorsForAgents();
        generateAgents();
        TextureShader.SetBuffer(TextureShader.FindKernel("UpdateAgents"), "Agents", _agentBuffer);
    }
    public void FixedUpdate()
    {
        UpdateAgents();
        Blurr();
    }
    private void generateAgents()
    {
        _agents = new Agent[numberOfAgents];
        for (int i = 0; i < _agents.Length; i++)
        {
            _agents[i].position = new Vector2(Screen.width / 2, Screen.height / 2);//   new Vector2(UnityEngine.Random.Range(0, Screen.width), UnityEngine.Random.Range(0, Screen.height));// 
            //_agents[i].position = new Vector2(UnityEngine.Random.Range(0, Screen.width), UnityEngine.Random.Range(0, Screen.height));// 
            _agents[i].directionAngle = UnityEngine.Random.Range(0f, Mathf.PI * 2);
            _agents[i].color = getRandomColor();
        }
        _agentBuffer = new ComputeBuffer(numberOfAgents, sizeof(float) * 7);
        _agentBuffer.SetData(_agents);
    }
    private void initColorsForAgents()
    {
        List<ColorRange> colorList = new List<ColorRange>
        {
            new ColorRange(
                new Color(1.0f, 0.0f, 0.0f, 0.0f),
                new Color(1.0f, 5.0f, 0.0f, 0.0f)
                ),
            new ColorRange(
                new Color(0.0f, 1.0f, 0.0f, 0.0f),
                new Color(0.0f, 1.0f, 5.0f, 0.0f)
                ),
        };

        _colorsForAgents = colorList.ToArray();
    }
    private Color getRandomColor()
    {
        return _colorsForAgents.Length == 0 ? 
            _colorsForAgents[0].getRandomColor() :
            _colorsForAgents[UnityEngine.Random.Range(0, _colorsForAgents.Length)].getRandomColor();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(_rTexture, destination);
    }
    private void setRandom()
    {
        int kernel = TextureShader.FindKernel("Random");
        TextureShader.SetTexture(kernel, "Result", _rTexture);
        TextureShader.SetFloat("time", Time.deltaTime);
        int workgroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int workgroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        TextureShader.Dispatch(kernel, workgroupsX, workgroupsY, 1);
    }
    private void Blurr()
    {
        int kernel = TextureShader.FindKernel("Blurr");
        TextureShader.SetTexture(kernel, "Result", _rTexture);
        int workgroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int workgroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        TextureShader.Dispatch(kernel, workgroupsX, workgroupsY, 1);
    }
    private void UpdateAgents()
    {
        int kernel = TextureShader.FindKernel("UpdateAgents");
        TextureShader.SetFloat("sizeX", Screen.width);
        TextureShader.SetFloat("sizeY", Screen.height);
        TextureShader.SetFloat("time", Time.deltaTime);
        TextureShader.SetFloat("feremonIntensity", feremonIntensity);
        TextureShader.SetFloat("blurrMultiplier", blurrMultiplier);
        TextureShader.SetFloat("randomMult", randomMult);
        TextureShader.SetFloat("speed", speed);
        TextureShader.SetInt("iterations", iterations);
        TextureShader.SetFloat("randomRadRange", randomAngleRangle * Mathf.Deg2Rad);
        TextureShader.SetTexture(kernel, "Result", _rTexture);
        int workgroups = Mathf.CeilToInt(numberOfAgents / threads);
        TextureShader.Dispatch(kernel, workgroups, 1, 1);
    }
    private void OnDestroy()
    {
        _rTexture.Release();
    }
}
