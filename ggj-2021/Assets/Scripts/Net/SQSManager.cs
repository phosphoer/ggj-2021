using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class SQSManager : Singleton<SQSManager>
{
    [System.Serializable]
    public class GamePayload
    {
        public string message;
    }

    [System.Serializable]
    public class SQS_Message
    {
        public string Body;
        public string MD5OfBody;
        public string MessageId;
        public string ReceiptHandle;
    }

    [System.Serializable]
    public class SQS_ReceiveMessageResult
    {
        public List<SQS_Message> messages;
    }

    [System.Serializable]
    public class SQS_ResponseMetadata
    {
        public string RequestId;
    }

    [System.Serializable]
    public class SQS_ReceiveMessageResponse
    {
        public SQS_ReceiveMessageResult ReceiveMessageResult;
        public SQS_ResponseMetadata ResponseMetadata;
    }

    [System.Serializable]
    public class SQS_GetResponse
    {
        public SQS_ReceiveMessageResponse ReceiveMessageResponse;
    }

    public enum MessageResultType{
        MessageReceived,
        NoMessages,
        FailedToClaimMessage,
        NetworkError
    };
    public delegate void OnMessageReceived(MessageResultType mesgType, string message);

    public string simpleQueingServiceURL= "https://xbiih0vg3c.execute-api.us-west-2.amazonaws.com/prod/";
    public string apiKey= "9bUJSVyao69ysDMW3Bqu16QliiHV7A9U9Iz9Kexz";
    public bool enableLogging= false;

    private bool hasPendingFetch= false;    

    private void Awake()
    {
        SQSManager.Instance = this;
    }

    public void FetchNewSQSMessages(OnMessageReceived mesgRecvCallback)
    {
        if (!hasPendingFetch)
        {
            hasPendingFetch= true;
            StartCoroutine(SQSMessageGetAsync(mesgRecvCallback));
        }
    }

    public void SendSQSMessage(string message)
    {
        GamePayload payload = new GamePayload();
        payload.message = message;

        StartCoroutine(SQSMessagePostAsync(payload));
    }

    IEnumerator SQSMessageGetAsync(OnMessageReceived mesgRecvCallback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(simpleQueingServiceURL))
        {
            request.SetRequestHeader("X-Api-Key", apiKey);

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Log("[ERROR]: "+request.error);

                if (mesgRecvCallback != null)
                {
                    mesgRecvCallback(MessageResultType.NetworkError, request.error);
                }
            }
            else if (request.isDone)
            {
                string bodyJsonString= System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
                Log(bodyJsonString);

                SQS_GetResponse response = JsonUtility.FromJson<SQS_GetResponse>(bodyJsonString);
                List<SQS_Message> messages= response.ReceiveMessageResponse.ReceiveMessageResult.messages;

                if (messages != null)
                {
                    Log(string.Format("Processing {0} messages: ", messages.Count));

                    foreach (SQS_Message sqs_message in messages)
                    {
                        // Try and remove each received message from the SQS.
                        // Only once this succeedes do we then post the message to the game
                        yield return StartCoroutine(SQSMessageDeleteAsync(sqs_message.ReceiptHandle, sqs_message.Body, mesgRecvCallback));
                    }
                }
                else
                {
                    if (mesgRecvCallback != null)
                    {
                        mesgRecvCallback(MessageResultType.NoMessages, "");
                    }
                }
            }
        }

        hasPendingFetch= false;    
    }

    IEnumerator SQSMessageDeleteAsync(string receiptHandle, string payloadString, OnMessageReceived mesgRecvCallback)
    {
        Log(string.Format("Deleting SQS message"));
        
        string requestURL=  string.Format("{0}message?receiptHandle={1}", simpleQueingServiceURL, UnityWebRequest.EscapeURL(receiptHandle));
        var request = new UnityWebRequest(requestURL, "DELETE");
        request.SetRequestHeader("X-Api-Key", apiKey);
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        MessageResultType messageResultType= MessageResultType.MessageReceived;
        string message= "";

        if (request.isNetworkError)
        {
            Log("SQS DELETE Net Error: " + request.error);

            messageResultType= MessageResultType.NetworkError;
            message= request.error;
        }
        else if (request.isHttpError)
        {
            Log("SQS DELETE Http Error: " + request.error);
            messageResultType= MessageResultType.FailedToClaimMessage;
        }
        else
        {
            if (enableLogging)
            {
                string bodyJsonString= System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
                Log(bodyJsonString);                
            }

            messageResultType= MessageResultType.MessageReceived;

            if (payloadString != null)
            {
                GamePayload gamePayload = JsonUtility.FromJson<GamePayload>(payloadString);

                if (gamePayload != null)
                {
                    message= gamePayload.message;
                }
                else
                {
                    message= payloadString;
                }
            }          
        }   

        if (mesgRecvCallback != null)
        {
            mesgRecvCallback(messageResultType, message);
        }          
    }    

    IEnumerator SQSMessagePostAsync(GamePayload payload)
    {
        var request = new UnityWebRequest(simpleQueingServiceURL, "POST");
        string bodyJsonString = JsonUtility.ToJson(payload);
        Log(bodyJsonString);

        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        request.SetRequestHeader("X-Api-Key", apiKey);
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Log("SQS POST Error: " + request.error);
        }
        else
        {
            Log("SQS POST Resonse Code: " + request.responseCode);
            Log("SQS POST Result: " + request.downloadHandler.text);
        }                
    }

    private void Log(string message)
    {
        if (enableLogging)
        {
            Debug.Log(message);
        }
    }
}
