using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;

    public MessageBlock messageBlockPrefab;
    public Transform messagesParent;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void LogMessage(string message)
    {
        MessageBlock newMessage = Instantiate(messageBlockPrefab, messagesParent);

        string currentTime = DateTime.Now.ToString("HH:mm:ss");
        newMessage.DrawMessage(message, currentTime);
    }
}
