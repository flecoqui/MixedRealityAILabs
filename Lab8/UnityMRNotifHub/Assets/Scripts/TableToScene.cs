using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using UnityEngine;


/// <summary> 
/// This objects is used to serialise and deserialise the Azure Table Entity 
/// </summary> 
[System.Serializable]
public class AzureTableEntity : TableEntity
{
    public AzureTableEntity(string partitionKey, string rowKey)
        : base(partitionKey, rowKey) { }


    public AzureTableEntity() { }


    public string Type { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
}

public class TableToScene : MonoBehaviour {
    /// <summary>          
    /// allows this class to behave like a singleton 
    /// </summary>          
    public static TableToScene instance;

    /// <summary>          
    /// Insert here you Azure Storage name
    /// </summary>          
    private string accountName = "mrazurenothubappstorage";

    /// <summary>          
    /// Insert here you Azure Storage key          
    /// </summary>          
    private string accountKey = "ZrPBvMaXCJ4mzDEYlUud+OcZ+kD5t1+LL9aaKfNEyA6caZQwjcg/Fn4dKi8908tlGB1xVoSYqaBCcAyW7zE44w==";
    /// <summary> 
    /// Triggers before initialization 
    /// </summary> 
    void Awake()
    {
        // static instance of this class 
        instance = this;
    }
    /// <summary> 
    /// Use this for initialization 
    /// </summary> 
    void Start()
    {
        //call method to populate the scene with new objects as  
        //    specified in the Azure Table 
        PopulateSceneFromTableAsync();
    }

     /// <summary>          
     /// Populate the scene with new objects as specified in the Azure Table        
     /// </summary> 
     private async void PopulateSceneFromTableAsync() 
     { 
         // Obtain credentials for the Azure Storage 
         StorageCredentials creds = new StorageCredentials(accountName, accountKey);
        // Storage account 
        CloudStorageAccount account = new CloudStorageAccount(creds, useHttps: true);                  // Storage client 
        CloudTableClient client = account.CreateCloudTableClient();                   // Table reference 
        CloudTable table = client.GetTableReference("SceneObjectsTable");
        TableContinuationToken token = null; 
         // Query the table for every existing Entity 
         do 
         { 
             // Queries the whole table by breaking it into segments 
             // (would happen only if the table had huge number of Entities) 
             TableQuerySegment<AzureTableEntity> queryResult =
                                  await table.ExecuteQuerySegmentedAsync(
                                  new TableQuery<AzureTableEntity>(), token);  
             foreach (AzureTableEntity entity in queryResult.Results) 
             { 
                 GameObject newSceneGameObject = null; 
                 // check for the Entity Type and spawn in the scene the appropriate Primitive 
                 switch (entity.Type) 
                 { 
                     case "Cube": 
                         // Create a Cube in the scene 
                         newSceneGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube); 
                         break; 
                     case "Sphere": 
                         // Create a Sphere in the scene 
                         newSceneGameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere); 
                         break; 
                     case "Cylinder": 
                         // Create a Cylinder in the scene 
                         newSceneGameObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder); 
                         break; 
                 }
newSceneGameObject.name = entity.RowKey; 
                 //check for the Entity X,Y,Z and move the Primitive at those coordinates 
                 newSceneGameObject.transform.position =  
                              new Vector3((float) entity.X, (float) entity.Y, (float) entity.Z); 
             } 
             // if the token is null, it means there are no more segments left to query 
             token = queryResult.ContinuationToken; 
         } 
         while (token != null); 
     } 
}
