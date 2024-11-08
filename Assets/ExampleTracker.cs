using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;




public class ExampleTracker : MonoBehaviour
{
    [SerializeField] TMP_Text debugText;
    [SerializeField] ARTrackedImageManager trackedImageManager;
    [SerializeField] ARAnchorManager arAnchorManager;
    [SerializeField] Transform cube;



    //stores all the anchors that are being placed as a dictionary.
    private Dictionary<string, ARAnchor> anchors = new Dictionary<string, ARAnchor>();

    // Start is called before the first frame update


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
            Debug.Log($"new tracked image added: { trackedImage.referenceImage.name}");
            debugText.text = $"Tracked Image Added: {trackedImage.referenceImage.name}\n" + debugText.text;


            //rendering the cube
            cube.position = trackedImage.transform.position;
            cube.rotation = trackedImage.transform.rotation;

            //calling creating anchor function --> creating anchor
            CreateAnchor(trackedImage);
            Debug.Log("anchor created");

        }
        foreach (var trackedImage in eventArgs.updated)
        {


        }
        foreach(var trackedImage in eventArgs.removed)
        {
            RemoveAnchor(trackedImage);
            Debug.Log($"Anchor Removed");
        }
    }



    //ui component
    public void handleButtonPress()
    {
        debugText.text = "here" + "\n" + debugText.text;
    }


    //createAnchor Function
    public void CreateAnchor(ARTrackedImage trackedImage)
    {
        GameObject anchorObject = new GameObject($"Anchor for {trackedImage.referenceImage.name}");
        anchorObject.transform.position = trackedImage.transform.position;
        anchorObject.transform.rotation = trackedImage.transform.rotation;

        ARAnchor anchor = anchorObject.AddComponent<ARAnchor>();

        if(anchor != null)
        {
            anchors[trackedImage.referenceImage.name] = anchor;
            Debug.Log($"Anchor succesfully created for image: {trackedImage.referenceImage.name}");
            debugText.text = $"Anchor Created: { trackedImage.referenceImage.name}\n" + debugText.text;
        } else
        {
            Debug.LogWarning($"Failed to create anchor for image: {trackedImage.referenceImage.name}");
            debugText.text = $"Anchot Creation Failed:  {trackedImage.referenceImage.name}\n" + debugText.text;
        }
    }



    //remove anchor function
    private void RemoveAnchor(ARTrackedImage trackedImage)
    {
        if (anchors.TryGetValue(trackedImage.referenceImage.name, out ARAnchor anchor))
        {
            Destroy(anchor.gameObject);
            anchors.Remove(trackedImage.referenceImage.name);
        }
    }


}



