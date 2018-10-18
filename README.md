## Usage ##

### WebSocket Client ###

```csharp
using UnityEngine;

public class Example: MonoBehaviour
{
    private void Start ()
    {
        // New Instance
        socket = new SocketIO("127.0.0.1", 3000);

        // Event Listeners
        socket.On("connection", EventHandler);
        socket.On("disconnect", EventHandler);
        socket.On("error", EventHandler);
        socket.On("event name", EventHandler);

        // Connect
        socket.Connect();

        // Emit
        socket.Emit("event name", "{json string}");

        // Close Connection
        socket.Close();
    }

    private void EventHandler (string response) 
    {
        // JsonUtility.FromJson<Class>(response);
    }
}
```