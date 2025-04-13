using System.Collections;
using System.Collections.Generic;
using Cdm.XR.Extensions;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class Alignment : MonoBehaviour
{
    [SerializeField]
    GameObject body;

    [SerializeField]
    ARDensePointCloudManager pointCloudManager;

    [SerializeField]
    GameObject showDuringScan;

    [SerializeField]
    GameObject showDuringAlignment;

    [SerializeField]
    GameObject errorText;

    [SerializeField]
    private string serverUrl = "http://127.0.0.1:5000/gedi";

    GameObject startButton = null;

    public void beginAlignment()
    {
        pointCloudManager.enabled = true;

        // Update UI
        startButton = GameObject.Find("StartButton");
        startButton.SetActive(false);
        showDuringScan.SetActive(true);

        StartCoroutine(align());
    }

    IEnumerator align()
    {
        // Wait for scan
        yield return new WaitForSeconds(15f);

        // Update UI
        showDuringScan.SetActive(false);
        showDuringAlignment.SetActive(true);
        pointCloudManager.DestroyAllPointClouds();
        pointCloudManager.enabled = false;

        // Get point cloud as a list
        NativeArray<Vector3> points = pointCloudManager.pointCloud.points;

        List<float[]> pointList = new List<float[]>();
        foreach (Vector3 point in points)
        {
            pointList.Add(new float[] { point.x, point.y, point.z });
        }

        // Convert to JSON
        string jsonData = JsonConvert.SerializeObject(new { points = pointList }, Formatting.Indented);

        // Send data to Flask server
        UnityWebRequest webRequest = new UnityWebRequest(serverUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-type", "application/json");

        yield return webRequest.SendWebRequest();

        // Update UI
        showDuringAlignment.SetActive(false);
        startButton.SetActive(true);

        // Handle response
        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("pcd successfully sent to server yayyyy");
            Debug.Log("Server response: " + webRequest.downloadHandler.text);


            string jsonResponse = webRequest.downloadHandler.text;
            TransformationResponse response = JsonConvert.DeserializeObject<TransformationResponse>(jsonResponse);

            // Convert response transformation to float[,] array
            float[,] transformationMatrix = response.GetMatrix();
            Debug.Log(transformationMatrix);

            // Apply transformation after receiving it from the server
            applyTransformationMatrix(transformationMatrix);
        }
        else
        {
            errorText.SetActive(true);
            Debug.Log("Error contacting server: " + webRequest.error);
        }
    }

    // Applies the received transformation matrix to body
    public void applyTransformationMatrix(float[,] transform)
    {
        if (body == null || transform.GetLength(0) != 4 || transform.GetLength(1) != 4)
        {
            Debug.Log("invalid object or transformation matrix");
        }

        // Build matrix out of the transform array
        Matrix4x4 matrix = new Matrix4x4();
        matrix.SetColumn(0, new Vector4(transform[0, 0], transform[1, 0], transform[2, 0], transform[3, 0]));
        matrix.SetColumn(1, new Vector4(transform[0, 1], transform[1, 1], transform[2, 1], transform[3, 1]));
        matrix.SetColumn(2, new Vector4(transform[0, 2], transform[1, 2], transform[2, 2], transform[3, 2]));
        matrix.SetColumn(3, new Vector4(transform[0, 3], transform[1, 3], transform[2, 3], transform[3, 3]));

        // Get translation from the matrix, then invert x
        Vector3 translation = new Vector3(transform[0, 3], transform[1, 3], transform[2, 3]);
        translation.x *= -1;

        // Convert rotation matrix to Quaternion, then invert it
        Quaternion rotation = matrix.rotation;
        rotation = Quaternion.Inverse(rotation);

        // Apply transformation and make body visible
        body.transform.position += translation;
        body.transform.Rotate(rotation.eulerAngles);
        body.SetActive(true);

        Debug.Log("Transformation applied.");
    }

    // Holds the response from the server
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
}