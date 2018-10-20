using System.Collections.Generic;
using WebSocketSharp;
using System.Dynamic;
using UnityEngine;
using System;

public class SocketIO: MonoBehaviour
{
    public int port = 3000;
    public string host = "ws://127.0.0.1";

    private struct EventMessage
    {
        public string[] data;
    }
    
    private bool connected;
    private WebSocket socket;
    private Dictionary<string, Action<string>> events;
    private Queue<Action> execute = new Queue<Action>();

    #region Public Properties

    public void On (string name, Action<string> action)
    {
        if (events.ContainsKey(name))
            events[name] = action;
        else
            events.Add(name, action);
    }

    public void Off (string name)
    {
        events.Remove(name);
    }

    public void Emit (string name, string data)
    {
        if (!connected) return;

        socket.SendAsync($"42[\"{name}\", {data}]", null);
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

    private void OnEnable ()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start ()
    {
        socket = new WebSocket($"{host}:{port}/socket.io/?EIO=4&transport=websocket");
        events = new Dictionary<string, Action<string>>();

        socket.OnOpen += OnOpen;
        socket.OnClose += OnClose;
        socket.OnError += OnError;
        socket.OnMessage += OnMessage;
    }

    private void Update ()
    {
        while (execute.Count > 0)
        {
            execute.Dequeue().Invoke();
        }
    }

    private void OnOpen (object sender, EventArgs args)
    {
        connected = true;

        if (events.ContainsKey("connection"))
        {
            execute.Enqueue(delegate {
                events["connection"].Invoke(args.ToString());
            });
        }
    }

    private void OnClose (object sender, EventArgs args)
    {
        connected = false;

        if (events.ContainsKey("disconnect"))
        {
            execute.Enqueue(delegate {
                events["disconnect"].Invoke(args.ToString());
            });
        }
    }

    private void OnError (object sender, ErrorEventArgs args)
    {
        connected = false;

        if (events.ContainsKey("error"))
        {
            execute.Enqueue(delegate {
                events["error"].Invoke(args.Message);
            });
        }
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
                    {
                        execute.Enqueue(delegate {
                            events[decoder.name](decoder.data);
                        });
                    }
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