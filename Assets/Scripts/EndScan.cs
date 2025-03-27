using System.Collections;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Networking;
using Cdm.XR.Extensions;
using Newtonsoft.Json;


public class EndScan : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    ARDensePointCloudManager densePointCloudManager;

    private NativeArray<Vector3> points;
    [SerializeField]
    GameObject body;

    [SerializeField]
    private string serverUrl = "http://127.0.0.1:5000/gedi";

    public void onClick()
    {

        if (densePointCloudManager == null)
        {
            Debug.Log("Densepointcloud manager is not assinged");
            return;
        }

        ARDensePointCloud pointCloud = densePointCloudManager.pointCloud;
        NativeArray<Vector3> points = pointCloud.points;

        if (points == null)
        {
            Debug.Log("there are no points, hit the start button");
            return;
        }



        for (int i = 0; i < Mathf.Min(points.Length, 5); i++)
        {
            Debug.Log($"Point {i}: {points[i]}");
        }



        List<float[]> pointList = new List<float[]>();
        foreach(Vector3 point in points)
        {
            pointList.Add(new float[] { point.x, point.y, point.z });

        }


        string jsonData = JsonConvert.SerializeObject(new { points = pointList }, Formatting.Indented);
        Debug.Log(jsonData);

        body.SetActive(true);
        Debug.Log("Tunnel is spawned");


        StartCoroutine(sendPCDToFlask(jsonData, transformationMatrix =>
        {
            applyTransformationMatrix(body, transformationMatrix);
        }));

        Debug.Log("Coroutine started");


    }



    IEnumerator sendPCDToFlask(string jsonData, System.Action<float[,]> onComplete)
    {
        UnityWebRequest webRequest = new UnityWebRequest(serverUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-type", "application/json");


        yield return webRequest.SendWebRequest();


        if(webRequest.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("pcd successfully sent to server yayyyy");
            Debug.Log("Server response: " + webRequest.downloadHandler.text);


            string jsonResponse = webRequest.downloadHandler.text;
            TransformationResponse response = JsonConvert.DeserializeObject<TransformationResponse>(jsonResponse);

            // Convert response transformation to float[,] array
            float[,] transformationMatrix = response.GetMatrix();
            Debug.Log(transformationMatrix);

            // Apply transformation after receiving it from the server
            onComplete?.Invoke(transformationMatrix);

        } else
        {
            Debug.Log("Error sending point cloud fix it " + webRequest.error);
        }



    }



    public void applyTransformationMatrix(GameObject obj, float[,] transform)
    {
        if(obj == null || transform.GetLength(0) !=4 || transform.GetLength(1) != 4)
        {
            Debug.Log("invalid object or transformation matrix");
        }

        Matrix4x4 matrix = new Matrix4x4();
        matrix.SetColumn(0, new Vector4(transform[0, 0], transform[1, 0], transform[2, 0], transform[3, 0]));
        matrix.SetColumn(1, new Vector4(transform[0, 1], transform[1, 1], transform[2, 1], transform[3, 1]));
        matrix.SetColumn(2, new Vector4(transform[0, 2], transform[1, 2], transform[2, 2], transform[3, 2]));
        matrix.SetColumn(2, new Vector4(transform[0, 3], transform[1, 3], transform[2, 3], transform[3, 3]));

        matrix = mirror_transform(matrix);

        // NOTE: Translation is mirrored across the x axis and inverted
        //Vector3 position = new Vector3(matrix[0, 3] * -1, matrix[1, 3], matrix[2, 3]);
        Vector3 position = new Vector3(transform[0, 3] * -1, transform[1, 3], transform[2, 3]);

        // Extract rotation matrix (3x3 part of the transformation matrix)
        Matrix4x4 rotationMatrix = new Matrix4x4();
        rotationMatrix.SetColumn(0, new Vector4(matrix[0, 0], matrix[1, 0], matrix[2, 0], 0));
        rotationMatrix.SetColumn(1, new Vector4(matrix[0, 1], matrix[1, 1], matrix[2, 1], 0));
        rotationMatrix.SetColumn(2, new Vector4(matrix[0, 2], matrix[1, 2], matrix[2, 2], 0));
        rotationMatrix.SetColumn(3, new Vector4(0, 0, 0, 1));

        // Convert rotation matrix to Quaternion
        Quaternion rotation = rotationMatrix.rotation;

        // Mirror across the x axis and reverse it
        //rotation.y *= -1;
        //rotation.z *= -1;
        //rotation = Quaternion.Inverse(rotation);

        // Apply transformation
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        Debug.Log("transformation applied ");
    }


    [System.Serializable]
    public class TransformationResponse
    {
        public List<List<float>> transformation; // JSON structure expects a list of lists

        public float[,] GetMatrix()
        {
            if (transformation == null || transformation.Count != 4 || transformation[0].Count != 4)
            {
                Debug.LogError("Invalid transformation matrix received.");
                return new float[4, 4]; // Return empty matrix to prevent errors
            }

            float[,] matrix = new float[4, 4];

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    matrix[i, j] = transformation[i][j];
                }
            }

            return matrix;
        }
    }



    private Matrix4x4 mirror_transform(Matrix4x4 transform)
    {
        // Define the reflection matrix that flips the z-axis.
        Matrix4x4 reflection = new Matrix4x4();
        reflection.SetRow(0, new Vector4(1, 0, 0, 0));
        reflection.SetRow(1, new Vector4(0, 1, 0, 0));
        reflection.SetRow(2, new Vector4(0, 0, -1, 0));
        reflection.SetRow(3, new Vector4(0, 0, 0, 1));


        // Multiply: reflection * rightHandMatrix * reflection
        Matrix4x4 leftHandMatrix = reflection * transform * reflection;
        return leftHandMatrix;
    }


}
