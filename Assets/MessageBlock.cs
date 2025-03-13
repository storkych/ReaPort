using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessageBlock : MonoBehaviour
{
    public TMP_Text logMsg;
    public TMP_Text timeTxt;

    public void DrawMessage(string message, string time)
    {
        logMsg.text = message;
        timeTxt.text = time;
    }
}
