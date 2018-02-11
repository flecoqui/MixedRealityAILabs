
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


public class Behaviours : MonoBehaviour
{



    public static Behaviours instance;

    public GameObject sphere;
    public GameObject cylinder;
    public GameObject cube;
    [HideInInspector] public GameObject gazedTarget;
 

    private void Awake()
    {
        instance = this;
    }



    public void ChangeTargetColor(string targetName, string colorName)
    {
        GameObject g = FindTarget(targetName);

        if (g != null)
        {
            Debug.Log("Changing Color " + colorName + " to target " + g.name);
            switch (colorName)
            {
                case "blue":
                    g.GetComponent<Renderer>().material.color = Color.blue;
                    break;
                case "red":
                    g.GetComponent<Renderer>().material.color = Color.red;
                    break;
                case "yellow":
                    g.GetComponent<Renderer>().material.color = Color.yellow;
                    break;
                case "green":
                    g.GetComponent<Renderer>().material.color = Color.green;
                    break;
                case "white":
                    g.GetComponent<Renderer>().material.color = Color.white;
                    break;
                case "black":
                    g.GetComponent<Renderer>().material.color = Color.black;
                    break;

            }

        }

    }

    public void DownSizeTarget(string targetName)
    {
        Debug.Log("DownSize " + targetName);
        GameObject g = FindTarget(targetName);
        g.transform.localScale -= new Vector3(0.5F, 0.5F, 0.5F);
    }
    public void UpSizeTarget(string targetName)
    {
        Debug.Log("UpSize " + targetName);
        GameObject g = FindTarget(targetName);
        g.transform.localScale += new Vector3(0.5F, 0.5F, 0.5F);

    }
    public GameObject FindTarget(string name)
    {
        GameObject  g = null;
        switch(name)
        {
            case "sphere":
                g = sphere;
                break;
            case "cylinder":
                g = cylinder;
                break;
            case "cube":
                g = cube;
                break;
            case "this":
            case "it":
            case "that":
                if(gazedTarget!= null)
                {
                    g = gazedTarget;
                }
                break;

        }
        return g;

    }
 
}
