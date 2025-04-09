using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] private float duration = 10f;
    [SerializeField] private EndScan endScan;
    [SerializeField] private GameObject DuringAlignmentText;
    [SerializeField] private GameObject StartButton;


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
            endScan.EndScanning();
            Debug.Log("EndScan invoked after timer finished!");
        }
    }

    // Call this from your Start button's onClick
    public void StartTimer()
    {


        elapsedTime = 0f;
        isRunning = true;
        Debug.Log("Timer started.");

        if(StartButton != null)
        {
            StartButton.SetActive(false);
        }

        if(DuringAlignmentText != null)
        {
            DuringAlignmentText.SetActive(true);
        }
        


    }
}
