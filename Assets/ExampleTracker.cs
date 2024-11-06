using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ExampleTracker : MonoBehaviour
{
    [SerializeField]
    TMP_Text debugText;
    [SerializeField]
    ARTrackedImageManager trackedImageManager;
    // Start is called before the first frame update

    [SerializeField]
    Transform cube;
    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }
    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach(var trackedImage in eventArgs.added)
        {

        }
        foreach (var trackedImage in eventArgs.updated)
        {
            Debug.Log(trackedImage.transform.localScale);
            cube.position = trackedImage.transform.position;
            cube.rotation = trackedImage.transform.rotation;
            
        }
        foreach(var trackedImage in eventArgs.removed)
        {

        }
    }
    public void     handleButtonPress()
    {
        debugText.text = "here" + "\n" + debugText.text;
    }

    
}
