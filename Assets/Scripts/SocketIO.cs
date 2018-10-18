using System.Collections.Generic;
using WebSocketSharp;
using System.Dynamic;
using UnityEngine;
using System;

public class SocketIO
{
    private struct EventMessage
    {
        public string[] data;
    }

    private int type = 4;
    private bool connected;
    private string protocol = "ws";
    private string transport = "websocket";

    private WebSocket socket;
    private Dictionary<string, Action<string>> events;

    public SocketIO (string host, int port)
    {
        socket = new WebSocket($"{protocol}://{host}:{port}/socket.io/?EIO={type}&transport={transport}");
        events = new Dictionary<string, Action<string>>();

        socket.OnOpen += OnOpen;
        socket.OnClose += OnClose;
        socket.OnError += OnError;
        socket.OnMessage += OnMessage;
    }

    #region Public Properties

    public void On (string name, Action<string> action)
    {
        events.Add(name, action);
    }

    public void Off (string name)
    {
        events.Remove(name);
    }

    public void Emit (string name, string data)
    {
        if (!connected) return;

        socket.Send($"42[\"{name}\", {data}]");
    }

    public void Connect ()
    {
        if (!socket.IsAlive)
            socket.ConnectAsync();
    }

    public void Close ()
    {
        connected = false;

        if (socket.IsAlive)
            socket.CloseAsync();
    }

    #endregion

    #region Private Properties

    private void OnOpen (object sender, EventArgs args)
    {
        connected = true;
        events["connection"]?.Invoke(args.ToString());
    }

    private void OnClose (object sender, EventArgs args)
    {
        connected = false;
        events["disconnect"]?.Invoke(args.ToString());
    }

    private void OnError (object sender, ErrorEventArgs args)
    {
        connected = false;
        events["error"]?.Invoke(args.Message);
    }

    private void OnMessage (object sender, MessageEventArgs response)
    {
        int state;
        int status;

        if (int.TryParse(response.Data.Substring(0, 1), out state))
        {
            if (int.TryParse(response.Data.Substring(1, 1), out status))
            {
                if (state == 4 && status == 2) {
                    var decoder = Decoder(response.Data);

                    if (events.ContainsKey(decoder.name))
                        events[decoder.name]?.Invoke(decoder.data);   
                }
            }
        }
    }

    private dynamic Decoder (string response)
    {
        string data = response.Substring(2);
        data = $"{{\"data\": {data}}}";

        dynamic output = new ExpandoObject();
        output.name = String.Empty;
        output.data = String.Empty;

        try
        {
            EventMessage message = JsonUtility.FromJson<EventMessage>(data);
            output.name = message.data[0];
            output.data = message.data[1];
        }
        catch (Exception)
        {
            return output;
        }
        
        return output;
    }

    #endregion
}