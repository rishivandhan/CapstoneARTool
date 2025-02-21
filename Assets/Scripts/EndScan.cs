using System.Collections;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Networking;
using Cdm.XR.Extensions;

public class EndScan : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    ARDensePointCloudManager densePointCloudManager;

    private NativeArray<Vector3> points;

    void Start()
    {
        // densePointCloudManager.OnPointCloudUpdated += getPointsByUpdate;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void onClick()
    {
        ARDensePointCloud pointCloud = densePointCloudManager.pointCloud;
        NativeArray<Vector3> points = pointCloud.points;

        for (int i = 0; i < Mathf.Min(points.Length, 5); i++)
        {
            Debug.Log($"Point {i}: {points[i]}");
        }
           
    }

    // UNUSED
    public void getPointsByUpdate(ARDensePointCloud densePointCloud)
    {
        points = densePointCloud.points;
    }

}
