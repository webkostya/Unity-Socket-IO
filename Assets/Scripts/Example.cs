using UnityEngine;

public class Example: MonoBehaviour
{
    public struct Data
    {
        public string name;
        public string type;
    }

    private SocketIO socket;

    private void Awake ()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start ()
    {
        socket = new SocketIO("127.0.0.1", 3000);

        socket.On("connection", Connected);
        socket.On("disconnect", OnDisconnect);
        socket.On("error", OnError);
        socket.On("message", OnMessage);

        socket.Connect();
    }

    private void Connected (string response)
    {
        Debug.Log("Connected: " + response);
    }

    private void OnDisconnect (string response)
    {
        Debug.Log("Disconnect: " + response);
    }

    private void OnError (string response)
    {
        Debug.Log("Error: " + response);
    }

    private void OnMessage (string response)
    {
        Debug.Log("Message: " + response);
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
                name = "webkostya",
                type = "man"
            };

            socket.Emit("message", JsonUtility.ToJson(data));
        } 

        if (GUI.Button(new Rect(15, 60, 80, 30), "Connect"))
            socket.Connect();
        
        if (GUI.Button(new Rect(15, 105, 80, 30), "Close"))
            socket.Close();
    }
}