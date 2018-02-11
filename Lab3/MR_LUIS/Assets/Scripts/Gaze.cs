
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.XR;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.WSA.Input;



public class Gaze : MonoBehaviour
{

    public GameObject gazedObject;
    public float gazedMaxDistance = 300;


 


    // Use this for initialization
    void ResetGaze() {
        if(gazedObject != null)
        {
            Behaviours.instance.gazedTarget = null;
            gazedObject = null;
            Debug.Log("Current gazed object is null" );
        }
    }

    // Update is called once per frame
    void Update() {
        Vector3 fwd = gameObject.transform.TransformDirection(Vector3.forward);
        Ray ray = new Ray(Camera.main.transform.position,fwd);
        RaycastHit hit;
        Debug.DrawRay(Camera.main.transform.position, fwd);

        if (Physics.Raycast(ray, out hit, gazedMaxDistance) && hit.collider != null)
        {
            if(gazedObject == null)
            {
                gazedObject = hit.transform.gameObject;
                Behaviours.instance.gazedTarget = gazedObject;

                Debug.Log("Current gazed object:" + gazedObject.name);
            }
        }
        else
            ResetGaze();
    }

   
 
}
