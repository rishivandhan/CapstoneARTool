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

    private string serverUrl = "http://172.20.136.222:5000/fpfh";

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
        StartCoroutine(sendPCDToFlask(jsonData));
           
    }



    IEnumerator sendPCDToFlask(string jsonData)
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

        } else
        {
            Debug.Log("Error sending point cloud fix your shit: " + webRequest.error);
        }



    }





}
