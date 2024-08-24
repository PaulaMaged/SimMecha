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

    public static int x = 0;
    public static bool started = false;
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

        //runPythonScript.RunPython();

        Debug.Log("Hellooo");
        Connect("127.0.0.1", 300); // Replace with your server IP and port

        objectData.sendObjectData();
        objectData.SendMotorData();
        
        //started = true;
    }

    private void Update()
    {
        if (started)
        {
            SendMessage(x++.ToString());
            Delayed(200);
        }
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

    public void SendMessage(string message)  // sends messages to python
    {
        if (stream != null)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            Debug.Log("Sent: " + message);
        }
    }


    void OnApplicationQuit()
    {
        stream.Close();
        client.Close();
    }

    async Task Delayed(int milli)
    {
        await Task.Delay(milli);
    }
}