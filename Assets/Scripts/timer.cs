using Cdm.XR.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] private float duration = 10f;
    [SerializeField] private EndScan endScan;
    [SerializeField] private GameObject DuringAlignmentText;
    [SerializeField] private GameObject StartButton;
    [SerializeField] private ARDensePointCloudManager ARDensePointCloud;
    //[SerializeField] private TMP_Text startButtonText;


    private float elapsedTime = 0f;
    private bool isRunning = false;


    void Update()
    {
        if (!isRunning) return;

        elapsedTime += Time.deltaTime;
        Debug.Log($"Timer running: {elapsedTime:F2} seconds");

        if (elapsedTime >= duration)
        {
            Debug.Log("Timer Finished");
            isRunning = false;
            endScan.onClick();


            //destroys pointcloud after timer is done
            ARDensePointCloud.DestroyAllPointClouds();


            Debug.Log("EndScan invoked after timer finished!");




        }
    }

    // Call this from your Start button's onClick
    public void StartTimer()
    {


        elapsedTime = 0f;
        isRunning = true;
        Debug.Log("Timer started.");

        if (StartButton != null)
        {
            StartButton.GetComponent<Button>().interactable = false;
            DuringAlignmentText.SetActive(true);
        }





    }
}
