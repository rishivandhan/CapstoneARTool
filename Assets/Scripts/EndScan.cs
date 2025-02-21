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
    

        string jsonData = JsonConvert.SerializeObject(pointList, Formatting.Indented);


           
    }

    

    

}
