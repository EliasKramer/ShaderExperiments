using System.Collections;
using UnityEngine;

public class CubeGrid : MonoBehaviour
{
    public Transform CubePrefab;
    public ComputeShader CubeShader;

    private ComputeBuffer _cubesPositionBuffer;

    // Grid size
    public int CubesPerAxis = 80;
    public int Repetitions = 1000;
    // Cube objects
    private Transform[] _cubes;

    // Array containing all y positions of cubes.
    // Will be put on the compute buffer
    private float[] _cubesPositions;

    public bool UseGPU;

    private void Awake()
    {
        _cubesPositionBuffer = new ComputeBuffer(CubesPerAxis * CubesPerAxis, sizeof(float));
    }
    private void OnDestroy()
    {
        _cubesPositionBuffer.Release();
    }
    private void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        _cubes = new Transform[CubesPerAxis * CubesPerAxis];
        _cubesPositions = new float[CubesPerAxis * CubesPerAxis];
        for (int x = 0, i = 0; x < CubesPerAxis; x++)
        {
            for (int z = 0; z < CubesPerAxis; z++, i++)
            {
                _cubes[i] = Instantiate(CubePrefab);
                _cubes[i].transform.position = new Vector3(x, 0, z);
            }
        }
        StartCoroutine(UpdateCubeGrid());
    }
    IEnumerator UpdateCubeGrid()
    {
        while (true)
        {
            if (UseGPU)
            {
                UpdatePositionsGPU();
            }
            else
            {
                UpdatePositionsCPU();
            }

            for (int i = 0; i < _cubes.Length; i++)
            {
                _cubes[i].localPosition = new Vector3(_cubes[i].localPosition.x, _cubesPositions[i], _cubes[i].localPosition.z);
            }
            yield return new WaitForSeconds(1);
        }
    }
    void UpdatePositionsCPU()
    {
        for (int i = 0; i < _cubes.Length; i++)
        {
            for (int j = 0; j < Repetitions; j++)
            {
                _cubesPositions[i] = Random.Range(-1f, 1f);
            }
        }
    }
    void UpdatePositionsGPU()
    {
        CubeShader.SetBuffer(0, "_Positions", _cubesPositionBuffer);

        CubeShader.SetInt("_CubesPerAxis", CubesPerAxis);
        CubeShader.SetInt("_Repetitions", Repetitions);
        CubeShader.SetFloat("_Time", Time.deltaTime);

        int workgroups = Mathf.CeilToInt(CubesPerAxis / 8.0f);
        CubeShader.Dispatch(0, workgroups, workgroups, 1);

        _cubesPositionBuffer.GetData(_cubesPositions);
    }
}