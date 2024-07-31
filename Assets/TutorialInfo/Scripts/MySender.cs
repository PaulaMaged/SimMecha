using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;

public class MySender : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;

    public GameObject obj;
    private ObjectData objectData;

    public GameObject pythonRun;
    private RunPythonScript runPythonScript;

    public static MySender Instance;

    void Awake()
    {
        // Ensure there's only one instance of ReceiverScript
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartSimulation()
    {
        objectData = obj.GetComponent<ObjectData>();
        runPythonScript = pythonRun.GetComponent<RunPythonScript>();

        runPythonScript.RunPython();

        Debug.Log("Hellooo");
        Connect("127.0.0.1", 65432); // Replace with your server IP and port

        objectData.sendObjectData();
    }

    private void Update()
    {

    }

    void Connect(string server, int port)
    {
        try
        {
            client = new TcpClient(server, port);
            stream = client.GetStream();
            Debug.Log("Connected to server");
        }
        catch (Exception e)
        {
            Debug.LogError("Error: " + e.Message);
        }
    }

    void SendMessage(string message)  // sends messages to python
    {
        if (stream != null)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            Debug.Log("Sent: " + message);
        }
    }

    // receives data about gameObjects from ObjectData script
    public void ReceiveData(string name, Vector3 position, Quaternion orientation)
    {
        // Process the received data
        Debug.Log(position.ToString() + " " + orientation.ToString());

        SendMessage(position.ToString() + " " + orientation.ToString());

        Debug.Log("e7m");
    }

    void OnApplicationQuit()
    {
        stream.Close();
        client.Close();
    }

    async Task Delayed(int milli)
    {
        await Task.Delay(milli); // Wait for 2000 milliseconds (2 seconds)
        Debug.Log("Async function executed after 2 seconds");
    }
}