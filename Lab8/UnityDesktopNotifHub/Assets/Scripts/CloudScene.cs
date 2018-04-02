using Newtonsoft.Json;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class CloudScene : MonoBehaviour {
    /// <summary> 
    /// Allows this class to behave like a singleton 
    /// </summary> 
    public static CloudScene instance;
    /// <summary> 
    /// Insert here you Azure Function Url 
    /// </summary> 
    private string azureFunctionEndpoint = "--Insert here you Azure Function Endpoint--";
    /// <summary> 
    /// Flag for object being moved 
    /// </summary> 
    private bool gameObjHasMoved;
    /// <summary> 
    /// Transform of the object being dragged by the mouse 
    /// </summary> 
    private Transform gameObjHeld;
    /// <summary> 
    /// Class hosted in the TableToScene script 
    /// </summary> 
    AzureTableEntity azureTableEntity;
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
        // initialise an AzureTableEntity 
        azureTableEntity = new AzureTableEntity();
    }

    /// <summary> 
    /// Update is called once per frame 
    /// </summary> 
    void Update()
    {
        //Enable Drag if button is held down 
        if (Input.GetMouseButton(0))
        {
            // Get the mouse position 
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
            Vector3 objPos = Camera.main.ScreenToWorldPoint(mousePosition);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            // Raycast from the current mouse position to the object overlapped by the mouse 
            if (Physics.Raycast(ray, out hit))
            {
                // update the position of the object "hit" by the mouse 
                hit.transform.position = objPos;
                gameObjHasMoved = true;
                gameObjHeld = hit.transform;
            }
        }
        // check if the left button mouse is released while holding an object 
        if (Input.GetMouseButtonUp(0) && gameObjHasMoved)
        {
            gameObjHasMoved = false;
            // Call the Azure Function that will update the appropriate Entity in the Azure Table 
            // and send a Notification to all subrscribed Apps 
            Debug.Log("Calling Azure Function");
            StartCoroutine(UpdateCloudScene(gameObjHeld.name,
                      gameObjHeld.position.x, gameObjHeld.position.y, gameObjHeld.position.z));
        }
    }

    private IEnumerator UpdateCloudScene(string objName, double xPos, double yPos, double zPos)
    {
        WWWForm form = new WWWForm();
        // set the properties of the AzureTableEntity 
        azureTableEntity.RowKey = objName;
        azureTableEntity.X = xPos;
        azureTableEntity.Y = yPos;
        azureTableEntity.Z = zPos;
        // Serialise the AzureTableEntity object to be sent to Azure 
        string jsonObject = JsonConvert.SerializeObject(azureTableEntity);
        using (UnityWebRequest www = UnityWebRequest.Post(azureFunctionEndpoint, jsonObject))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonObject);
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.uploadHandler.contentType = "application/json";
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();
            string response = www.responseCode.ToString();
        }

    }
}
