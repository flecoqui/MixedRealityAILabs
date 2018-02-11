using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultsLabel : MonoBehaviour {
    public static ResultsLabel instance;
    public GameObject cursor;
    public Transform labelPrefab;

    [HideInInspector] public Transform lastLabelPlaced;
    [HideInInspector] public TextMesh lastLabelPlacedText;

    private void Awake()
    {
        instance = this;
    }
    public void CreateLabel()
    {
        lastLabelPlaced = Instantiate(labelPrefab, cursor.transform.position, transform.rotation);
        lastLabelPlacedText = lastLabelPlaced.GetComponent<TextMesh>();
        lastLabelPlacedText.text = "Analysing...";
    }
    public void SetTagsToLastLabel(Dictionary<string, float> tagsDictionary)
    {
        lastLabelPlacedText = lastLabelPlaced.GetComponent<TextMesh>();
        lastLabelPlacedText.text = "I see: \n";
        string outputString = string.Empty;
        foreach(var tag in tagsDictionary)
        {
            lastLabelPlacedText.text += tag.Key + ", Confidence: " + tag.Value.ToString("0.00 \n");
            outputString += tag.Key + ", Confidence: " + tag.Value.ToString("0.00 ");
        }
        Debug.Log("Photo information: " + outputString);
    }

}
