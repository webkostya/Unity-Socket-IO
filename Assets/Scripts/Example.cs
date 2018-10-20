using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;

public class Example: MonoBehaviour
{
    public struct Data
    {
        public string username;
        public string password;
    }

    private SocketIO socket;
    private Text content;

    private void OnEnable ()
    {
        var canvas = GameObject.Find("Canvas");
        var status = canvas.transform.Find("Status");

        content = status.GetComponent<Text>();
    }

    private void Start ()
    {
        socket = GetComponent<SocketIO>();

        socket.On("connection", Connected);
        socket.On("disconnect", OnDisconnect);
        socket.On("error", OnError);
        socket.On("message", OnMessage);

        socket.Connect();
    }

    private void Connected (string response)
    {
        content.text = "Connected";
    }

    private void OnDisconnect (string response)
    {
        content.text = "Disconnect";
    }

    private void OnError (string response)
    {
        content.text = "Error: " + response;
    }

    private void OnMessage (string response)
    {
        content.text = "Message: " + response;
    }

    private void OnApplicationQuit ()
    {
        socket.Close();
    }

    private void OnGUI ()
    {
        if (GUI.Button(new Rect(15, 15, 80, 30), "Emit"))
        {
            var data = new Data {
                username = "username",
                password = "password"
            };

            socket.Emit("message", JsonUtility.ToJson(data));
        } 

        if (GUI.Button(new Rect(15, 60, 80, 30), "Connect"))
            socket.Connect();
        
        if (GUI.Button(new Rect(15, 105, 80, 30), "Close"))
            socket.Close();
    }
}