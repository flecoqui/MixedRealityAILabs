using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gaze : MonoBehaviour {
    /// <summary> 
    /// Provides Singleton-like behaviour to this class. 
    /// </summary> 
    public static Gaze instance;
    /// <summary> 
    /// Provides a reference to the object the user is currently looking at. 
    /// </summary> 
    public GameObject FocusedGameObject { get; private set; }
    /// <summary> 
    /// Provides a reference to compare whether the user is still looking at  
    /// the same object (and has not looked away). 
    /// </summary> 
    private GameObject oldFocusedObject = null;
    /// <summary> 
    /// Max Ray Distance 
    /// </summary> 
    float gazeMaxDistance = 300;
    /// <summary> 
    /// Provides when the gaze is ready to start working (based upon whether  
    /// Azure connects successfully). 
    /// </summary> 
    [HideInInspector]
    public bool enableGaze;
    /// <summary> 
    /// Provides whether an object has been successfully hit by the raycast. 
    /// </summary> 
    public bool Hit { get; private set; }

    private void Awake()
    {
        // Set this class to behave similar to singleton 
        instance = this;
    }

    void Start()
    {
        FocusedGameObject = null;
    }
    void Update()
    {
        // Only allow raycasting if AzureServices has flagged it ready to go. 
        if (enableGaze)
        {
            // Set the old focused gameobject. 
            oldFocusedObject = FocusedGameObject;
            RaycastHit hitInfo;
            // Initialise Raycasting. 
            Hit = Physics.Raycast(Camera.main.transform.position,
                Camera.main.transform.forward,
                out hitInfo,
                gazeMaxDistance);
            // Check whether raycast has hit. 
            if (Hit == true)
            {
                // Check whether the hit has a collider. 
                if (hitInfo.collider != null)
                {
                    // Set the focused object with what the user just looked at. 
                    FocusedGameObject = hitInfo.collider.gameObject;
                }
                else
                {
                    // Object looked on is not valid, set focused gameobject to null. 
                    FocusedGameObject = null;
                }
            }
            else
            {
                // No object looked upon, set focused gameobject to null. 
                FocusedGameObject = null;
            }
            // Check whether the previous focused object is this same object (so to stop spamming of function). 
            if (FocusedGameObject != oldFocusedObject)
            {
                // Reset the old focused object back to its original state. 
                ResetGaze();
                // Compare whether the new Focused Object has the desired tag we set previously. 
                if (FocusedGameObject != null &&
                    FocusedGameObject.CompareTag("GazeButton"))
                {
                    // Set the Focused object to green - success! 
                    FocusedGameObject.GetComponent<Renderer>().material.color = Color.green;
                    // Start the Azure Function, to provide the next shape! 
                    AzureServices.instance.CallAzureFunctionForNextShape();
                }
            }
        }
    }     /// <summary> 
          /// Reset the Raycast target reference 
          /// </summary> 
    public void ResetGaze()
    {
        // Ensure the old focused object is not null. 
        if (oldFocusedObject != null)
        {
            if (oldFocusedObject.CompareTag("GazeButton"))
            {
                // Set the old focused object to red - its original state. 
                oldFocusedObject.GetComponent<Renderer>().material.color = Color.red;

            }
        }
    }
}
