using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class placeTrackedImages : MonoBehaviour
{
    //reference to AR tracked image manager component
    private ARTrackedImageManager _trackedImagesManager;
    // List of prefabs of instatiate - these should be named the same
    // as their corresponding 2D images in the refrence image library
    public GameObject[] ArPrefabs;
    // Keep dictionary array of created prefabs
    private readonly Dictionary<string, GameObject> _instantiatedPrefabs = new Dictionary<string, GameObject>();


    void Awake()
    {
        // cache a ref to the tracked image manager component
        _trackedImagesManager = GetComponent<ARTrackedImageManager>();
    }
    void OnEnable()
    {
        //attach event handler when tracked images change
        _trackedImagesManager.trackedImagesChanged += OnTrackedImagesChanged;
    }
    void OnDisable()
    {
        //remove event handler
        _trackedImagesManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    //event handler
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        //loop through all new tracked images that have been detected
        foreach (var trackedImage in eventArgs.added)
        {

            //get name of the reference image
            var imageName = trackedImage.referenceImage.name;
            //now loop over the array of prefabs

            foreach (var curPrefab in ArPrefabs)
            {
                // check whether this prefab matches the tracked image name,
                // and that the prefab has not already been created
                if (string.Compare(curPrefab.name, imageName,
                    StringComparison.OrdinalIgnoreCase) == 0 && !_instantiatedPrefabs.ContainsKey(imageName))
                {
                    //instantiate the prefab, parenting it to the ARTrackedImage
                    var newPrefab = Instantiate(curPrefab, trackedImage.transform);
                    //add the created prefab to the array
                    _instantiatedPrefabs[imageName] = newPrefab;
                }
            }
        }

        // For all prefabs that have been created so far, set them active or not depending
        //on whether their corresponding image is currently being tracked.
        foreach (var trackedImage in eventArgs.updated)
        {
            _instantiatedPrefabs[trackedImage.referenceImage.name]
                .SetActive(trackedImage.trackingState == TrackingState.Tracking);
        }

        //if the AR subsytem has given up looking for a tracked image,
        foreach (var trackedImage in eventArgs.removed)
        {
            // Destroy its prefab
            Destroy(_instantiatedPrefabs[trackedImage.referenceImage.name]);
            // Also remove the instance from our array
            _instantiatedPrefabs.Remove(trackedImage.referenceImage.name);
            // Or, simply set the prefab instance to inactive
            //_instantiatedPrefabs[trackedImage.referenceImage.name].SetActive(false);
        }
    }
}

