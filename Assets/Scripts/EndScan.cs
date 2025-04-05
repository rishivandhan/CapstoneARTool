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
            Debug.Log("Tunnel is spawned");
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

        // Things we've tried:
        //
        // - position.x, rotation.w, rotation.z
        //
        Vector3 position = new Vector3(transform[0, 3], transform[1, 3], transform[2, 3]); //grabbing position from the transformation matrix. 
        position.x *= -1;

        Debug.Log("Postion x,y,z: " + position.ToString());

        Matrix4x4 matrix = new Matrix4x4();

        // Set matrix
        for (int i = 0; i < 4; i++)
        {
            matrix.SetRow(i, new Vector4(transform[i, 0], transform[i, 1], transform[i, 2], transform[i, 3]));
        }

        Quaternion rotation = matrix.rotation;
        Debug.Log("before inverse of w and z" + rotation.ToString()); 
        rotation.w *= -1;
        rotation.z *= -1;
        Debug.Log("after inverse of w and z" + rotation.ToString());


        Debug.Log("Full Matrix " + matrix.ToString());
        Debug.Log("Rotation Matrix -- " + rotation.ToString());
        Debug.Log("Position -- " + position.ToString());

        obj.transform.position = position;
      //obj.transform.rotation = rotation;
        


        if (obj == null || transform.GetLength(0) != 4 || transform.GetLength(1) != 4)
        {
            Debug.Log("Invalid object or transformation matrix");
            return;
        }

        //// Correct translation without unnecessary inversion
        //Vector3 position = new Vector3(transform[0, 3], transform[1, 3], transform[2, 3]);

        //// Extract rotation matrix
        //Matrix4x4 rotationMatrix = new Matrix4x4();
        //rotationMatrix.SetColumn(0, new Vector4(transform[0, 0], transform[1, 0], transform[2, 0], 0));
        //rotationMatrix.SetColumn(1, new Vector4(transform[0, 1], transform[1, 1], transform[2, 1], 0));
        //rotationMatrix.SetColumn(2, new Vector4(transform[0, 2], transform[1, 2], transform[2, 2], 0));
        //rotationMatrix.SetColumn(3, new Vector4(0, 0, 0, 1));

        //// Convert rotation matrix to Quaternion using LookRotation
        //Quaternion rotation = Quaternion.LookRotation(rotationMatrix.GetColumn(2), rotationMatrix.GetColumn(1));

        //rotation *= Quaternion.Euler(0, , 0);

        //// Apply transformation considering parent object
        //if (obj.transform.parent != null)
        //{
        //    obj.transform.SetPositionAndRotation(obj.transform.parent.TransformPoint(position), obj.transform.parent.rotation * rotation);
        //}
        //else
        //{
        //    obj.transform.SetPositionAndRotation(position, rotation);
        //}

        //Debug.Log("Transformation applied");







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
        Matrix4x4 leftHandMatrix = reflection * transform;
        return leftHandMatrix;
    }


}
