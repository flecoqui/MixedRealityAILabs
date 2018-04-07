using Newtonsoft.Json;
using System;
using System.Collections;
using UnityEngine;
#if UNITY_WSA_10_0 && !UNITY_EDITOR
 using Windows.Networking.PushNotifications; 
 
#endif

public class NotificationReceiver : MonoBehaviour {
    /// <summary> 
    /// allows this class to behave like a singleton 
    /// </summary> 
    public static NotificationReceiver instance;
    /// <summary> 
    /// Value set by the notification, new object position 
    /// </summary> 
    Vector3 newObjPosition;
    /// <summary> 
    /// Value set by the notification, object name 
    /// </summary> 
    string gameObjectName;

    /// <summary> 
    /// Value set by the notification, new object position 
    /// </summary> 
    bool notifReceived;
    /// <summary> 
    /// Insert here your Notification Hub Service name  
    /// </summary> 
    private string hubName = " -- Insert the name of your service -- ";
    /// <summary> 
    /// Insert here your Notification Hub Service "Listen endpoint" 
    /// </summary> 
    private string hubListenEndpoint = "-Insert your Notification Hub Service Listen endpoint-";
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
        // Register the App at launch 
        InitNotificationsAsync();
        // Begin listening for notifications 
        StartCoroutine(WaitForNotification());
    }

    /// <summary> 
    /// This notification listener is necessary to avoid clashes  
    /// between the notification hub and the main thread    
    /// </summary> 
    private IEnumerator WaitForNotification()
    {
        while (true)
        {
            // Checks for notifications each second 
            yield return new WaitForSeconds(1f);
            if (notifReceived)
            {
                // If a notification is arrived, moved the appropriate object to the new position 
                GameObject.Find(gameObjectName).transform.position = newObjPosition;
                // Reset the flag 
                notifReceived = false;
            }
        }
    }

    /// <summary> 
    /// Register this application to the Notification Hub Service 
    /// </summary> 
    private async void InitNotificationsAsync()
    {

        // PushNotificationChannel channel =  
        //     await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync(); 
        // NotificationHub hub = new NotificationHub(hubName, hubListenEndpoint); 
        // Registration result = await hub.RegisterNativeAsync(channel.Uri); 
        // If registration was successful, subscribe to Push Notifications 
        // if (result.RegistrationId != null) 
        // { 
        //     Debug.Log($"Registration Successful: {result.RegistrationId}"); 
        //     channel.PushNotificationReceived += Channel_PushNotificationReceived; 
        // } 
    }
    /* 


/// <summary> 
 /// Handler called when a Push Notification is received 
 /// </summary> 
 private async void Channel_PushNotificationReceived(         PushNotificationChannel sender,  
    PushNotificationReceivedEventArgs args) 

     { 

      Debug.Log("New Push Notification Received"); 
     if (args.NotificationType == PushNotificationType.Raw) 
     { 
         //  Raw content of the Notification 
         string jsonContent = args.RawNotification.Content; 
         // Deserialise the Raw content into an AzureTableEntity object 
         AzureTableEntity ate = JsonConvert.DeserializeObject<AzureTableEntity>(jsonContent); 
         // The name of the Game Object to be moved 
         gameObjectName = ate.RowKey;           
         // The position where the Game Object has to be moved 
         newObjPosition = new Vector3((float)ate.X, (float)ate.Y, (float)ate.Z); 
         // Flag thats a notification has been received 
         notifReceived = true; 
     } 
 } 
     */
}
