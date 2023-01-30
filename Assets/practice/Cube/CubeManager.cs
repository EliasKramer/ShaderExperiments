using System;
using UnityEngine;

public class CubeManager : MonoBehaviour
{
    [SerializeField]
    public GameObject cam;
    [SerializeField]
    public ComputeShader computeShader;

    private DateTime lastTimeUpdated = DateTime.UtcNow;
    private GameObject[] objects;
    public struct Cube
    {
        public Vector3 position;
        public Color color;
    }
    private Cube[] data;
    void Start()
    {
        var camera = cam.GetComponent<Camera>();
        float height = camera.orthographicSize * 2;
        float width = height * camera.aspect;

        Vector2 camSize = new Vector2(width, height);
        Vector2 camCenter = cam.transform.position;

        Debug.Log("camSize: " + camSize);

        Vector3 startPos = camCenter - camSize / 2;

        data = new Cube[100];
        objects = new GameObject[100];
        for (float x = 0; x < 10; x++)
        {
            for (float y = 0; y < 10; y++)
            {
                GameObject cubeGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cubeGameObject.transform.position = new Vector3(x + startPos.x, y + startPos.y, UnityEngine.Random.Range(0f, 1f));
                Color color = UnityEngine.Random.ColorHSV();
                cubeGameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
                Cube cubeStruct = new Cube();
                cubeStruct.position = cubeGameObject.transform.position;
                cubeStruct.color = color;
                data[(int)(x * 10 + y)] = cubeStruct;
                objects[(int)(x * 10 + y)] = cubeGameObject;
            }
        }
    }
    public void Update()
    {
        if (Input.GetMouseButtonDown(0) && (DateTime.UtcNow - lastTimeUpdated).TotalSeconds > 1)
        {
            Debug.Log("updated now " + DateTime.UtcNow);
            lastTimeUpdated = DateTime.UtcNow;
            OnRandomizeGpu();
        }
    }
    public void OnRandomizeGpu()
    {
        Debug.Log("OnRandomizeGpu");

        int colorSize = sizeof(float) * 4;
        int vector3Size = sizeof(float) * 3;
        int totalSize = colorSize + vector3Size;

        ComputeBuffer cubesBuffer = new ComputeBuffer(data.Length, totalSize);
        cubesBuffer.SetData(data);

        int kernelHandle = computeShader.FindKernel("RandomCubes");
        computeShader.SetBuffer(kernelHandle, "cubes", cubesBuffer);
        computeShader.SetFloat("resolution", data.Length);
        computeShader.Dispatch(kernelHandle, data.Length / 10, 1, 1);

        cubesBuffer.GetData(data);

        for (int i = 0; i < objects.Length; i++)
        {
            GameObject currObj = objects[i];
            Cube cube = data[i];
            //currObj.transform.position = cube.position;
            currObj.GetComponent<MeshRenderer>().material.SetColor("_Color", cube.color);
        }

        cubesBuffer.Dispose();
    }
}
