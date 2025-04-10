using System.Collections;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Networking;
using Cdm.XR.Extensions;
using Newtonsoft.Json;
using System;
using TMPro;


public class EndScan : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    ARDensePointCloudManager densePointCloudManager;

    [SerializeField]
    ARDensePointCloudMeshVisualizer ARDensePointCloudMeshVisualizer;

    [SerializeField]
    GameObject body;

    [SerializeField]
    GameObject GediRenameText;

    [SerializeField]
    GameObject DuringAlignmentText;

    [SerializeField]
    GameObject AfterAlignmentText;


    private NativeArray<Vector3> points;
   


    

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



        //for (int i = 0; i < Mathf.Min(points.Length, 5); i++)
        //{
        //    Debug.Log($"Point {i}: {points[i]}");
        //}



        List<float[]> pointList = new List<float[]>();
        foreach(Vector3 point in points)
        {
            pointList.Add(new float[] { point.x, point.y, point.z });

        }


        string jsonData = JsonConvert.SerializeObject(new { points = pointList }, Formatting.Indented);
        //Debug.Log(jsonData);

        // body.SetActive(true);
        // Debug.Log("Tunnel is spawned");


        StartCoroutine(sendPCDToFlask(jsonData, transformationMatrix =>
        {
            applyTransformationMatrix(body, transformationMatrix);
            body.SetActive(true);
            Debug.Log("Resonse Recieved performing allignment");
            GediRenameText.SetActive(false);
            AfterAlignmentText.SetActive(true);


            
        }));

        Debug.Log("Coroutine started");


    }



    IEnumerator sendPCDToFlask(string jsonData, System.Action<float[,]> onComplete)
    {
        DuringAlignmentText.SetActive(false);
        GediRenameText.SetActive(true);


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

        // Apply transformation
        obj.transform.position += translation;
        obj.transform.Rotate(rotation.eulerAngles);

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



    public void EndScanning()
    {
        // Stop scanning
        //densePointCloudManager.enabled = false;


        Debug.Log("ending scan");

        //if (ARDensePointCloudMeshVisualizer != null)
        //{
        //    ARDensePointCloudMeshVisualizer.ClearMesh();
        //}


        densePointCloudManager.DestroyAllPointClouds();
        Debug.Log("point cloud destroyed");



        //// Remove all accumulated point clouds
        //densePointCloudManager.DestroyAllPointClouds();

        //// Disable the visualized mesh
        //var visualizer = FindObjectOfType<ARDensePointCloudMeshVisualizer>();
        //if (visualizer != null)
        //{
        //    visualizer.gameObject.SetActive(false);
        //}

        //Debug.Log("Scan ended and point cloud visualizer disabled.");
    }



    public void RestartScan()
    {
        // Reactivate scanning logic
        densePointCloudManager.enabled = true;

        // Reactivate the visualizer GameObject
        var visualizer = FindObjectOfType<ARDensePointCloudMeshVisualizer>();
        if (visualizer != null)
        {
            visualizer.gameObject.SetActive(true);
        }

    }

}
