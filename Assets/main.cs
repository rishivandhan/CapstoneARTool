using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class PlaceTrackedImages : MonoBehaviour
{
    private ARTrackedImageManager _trackedImagesManager;
    public GameObject[] ArPrefabs;
    private readonly Dictionary<string, GameObject> _instantiatedPrefabs = new Dictionary<string, GameObject>();

    void Awake()
    {
        _trackedImagesManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        _trackedImagesManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        _trackedImagesManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
    //function is being called here
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.updated)
        {
            var imageName = trackedImage.referenceImage.name;
            if (!_instantiatedPrefabs.ContainsKey(imageName))
            {
                GameObject prefabToInstantiate = Array.Find(ArPrefabs, prefab => prefab.name.Equals(imageName, StringComparison.OrdinalIgnoreCase));
                if (prefabToInstantiate != null)
                {
                    var newPrefab = Instantiate(prefabToInstantiate, trackedImage.transform);
                    _instantiatedPrefabs[imageName] = newPrefab;
                }
                else
                {
                    Debug.LogWarning($"No prefab found for image: {imageName}");
                    continue;
                }
            }

            _instantiatedPrefabs[imageName].SetActive(trackedImage.trackingState == TrackingState.Tracking);
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            var imageName = trackedImage.referenceImage.name;
            if (_instantiatedPrefabs.ContainsKey(imageName))
            {
                Destroy(_instantiatedPrefabs[imageName]);
                _instantiatedPrefabs.Remove(imageName);
            }
        }
    }
}
