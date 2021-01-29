using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SQSTester : MonoBehaviour
{
    private SQSManager sqsManager;

    // Start is called before the first frame update
    void Start()
    {
        sqsManager= this.gameObject.GetComponent<SQSManager>();

        Debug.Log("Sending scream");
        sqsManager.SendSQSMessage("AAAAAAhhhhhh!!!!");

        StartCoroutine(DelayedFetchMessage());
    }

    IEnumerator DelayedFetchMessage()
    {
        //yield on a new YieldInstruction that waits for 3 seconds.
        yield return new WaitForSeconds(3);

        Debug.Log("Fetching latest message");
        sqsManager.FetchNewSQSMessages(OnReceivedSQSMessage);
    }    

    void OnReceivedSQSMessage(SQSManager.MessageResultType messageType, string message)
    {
        switch(messageType)
        {
        case SQSManager.MessageResultType.MessageReceived:
            Debug.Log("Receiving message: "+message);
            break;
        case SQSManager.MessageResultType.FailedToClaimMessage:
            Debug.Log("Failed to claim message (try again later?)");
            break;
        case SQSManager.MessageResultType.NetworkError:
            Debug.Log("Network Error: "+message);
            break;
        case SQSManager.MessageResultType.NoMessages:
            Debug.Log("No Messages");
            break;

        }
    }
}
