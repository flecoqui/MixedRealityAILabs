using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelfKeeper : MonoBehaviour {

    /// <summary> 
    /// Provides this class Singleton-like behaviour 
    /// </summary> 
    public static ShelfKeeper instance;
    /// <summary> 
    /// Unity Inspector accessible Reference to the Text Mesh object needed for data 
    /// </summary> 
    public TextMesh dateText;
    /// <summary> 
    /// Unity Inspector accessible Reference to the Text Mesh object needed for time 
    /// </summary> 
    public TextMesh timeText;
    /// <summary> 
    /// Provides references to the spawn locations for the products prefabs 
    /// </summary> 
    public Transform[] spawnPoint; private void Awake()
    {
        instance = this;
    }
    /// <summary> 
    /// Set the text of the date in the scene 
    /// </summary> 
    public void SetDate(string day, string month)
    {
        dateText.text = day + " " + month;
    }
    /// <summary> 
    /// Set the text of the time in the scene 
    /// </summary> 
    public void SetTime(string hour)
    {
        timeText.text = hour + ":00";
    }
    /// <summary> 
    /// Spawn a product on the shelf by providing the name and selling grade 
    /// </summary> 
    /// <param name="name"></param> 
    /// <param name="sellingGrade">0 being the best seller</param> 
    public void SpawnProduct(string name, int sellingGrade)
    {
        Instantiate(Resources.Load(name),
            spawnPoint[sellingGrade].transform.position, Quaternion.identity);
    }
}
