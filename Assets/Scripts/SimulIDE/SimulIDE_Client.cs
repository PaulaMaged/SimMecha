using RuntimeInspectorNamespace;
using System;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

public class SimulIDE_Client : MonoBehaviour
{
    // Start is called before the first frame update
    NamedPipeClientStream pipeClient;
    StreamString ss;
    const int connectionTimeOutMs = 10000;
    Thread serverThread;

    void Start()
    {
        serverThread = new(ServerMain);
        serverThread.Start();
    }

    private void ServerMain(object obj)
    {
        while (true)
        {
            if (pipeClient.IsNull() || !pipeClient.IsConnected)
            {
                StartClient();
            }
            else if (ss.readComplete)
            {
                ParseData();
            }
            else
            {
                ss.ReadString();
            }
        }
    }
    public void StartClient()
    {
        try
        {
            pipeClient = new NamedPipeClientStream(".", "unity_server",
                        PipeDirection.In);
            Debug.Log("Connecting to server...\n");
            pipeClient.Connect(connectionTimeOutMs); //blocking call
            ss = new StreamString(pipeClient, 4);

            Debug.Log("connected to: " + "unity_server");
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message);
            pipeClient.Dispose();
            Debug.Log("failed to connect to server");
        }
    }
    private void ParseData()
    {
        //display data
        Debug.Log("Full msg received: " + ss.fullMsg);
        //parse the data
        Debug.Log("data parsed");
        //send data over to python
        Debug.Log("sending data to python");

        ss.readComplete= false;
    }
}

public class StreamString
{
    private Stream ioStream;
    private UnicodeEncoding streamEncoding;

    public int lengthByteSize;
    public bool readingSize = true;
    public byte[] lengthBytes;
    public byte[] msgBytes;
    public int remainingBytes;
    public int msgLength = -1;
    public bool readComplete = false;
    public string fullMsg = "";

    public StreamString(Stream ioStream, int lengthByteSize)
    {
        this.ioStream = ioStream;
        streamEncoding = new UnicodeEncoding(false, false);

        lengthBytes = new byte[lengthByteSize];
        remainingBytes = lengthByteSize;
        this.lengthByteSize = lengthByteSize;
    }

    public void ReadString()
    {
        if (readingSize)
        {
            ReadSize();
            return;
        }
        try
        {
            int bytesRead = ioStream.Read(msgBytes, msgLength - remainingBytes, remainingBytes);
            remainingBytes -= bytesRead;
            if (remainingBytes > 0)
                return;

            //setup for new msg arrival
            remainingBytes = lengthByteSize;
            lengthBytes = new byte[lengthByteSize];
            readingSize = true;

            //convert byte array to msg
            string readString = streamEncoding.GetString(msgBytes);
            Debug.Log("Received data: " + readString);

            readComplete = true;
            fullMsg = readString; ;
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message);
        }
        
    }

    private void ReadSize()
    {
        try
        {
            int bytesRead = ioStream.Read(lengthBytes, lengthByteSize - remainingBytes, remainingBytes);
            remainingBytes -= bytesRead;
            if (remainingBytes > 0)
            {
                return;
            }
        } 
        catch(Exception e)
        {
            Debug.LogWarning("Can't read from stream due to: " + e.Message);
            return;
        }

        string lengthString = streamEncoding.GetString(lengthBytes);
        bool convertToInt = int.TryParse(lengthString, out int length);
        if (!convertToInt)
        {
            Debug.LogWarning($"Failed to convert \"{lengthString}\" to int");
            Array.Clear(lengthBytes, 0, lengthBytes.Length);
            remainingBytes = lengthBytes.Length;
            return;
        }

        Console.WriteLine("Convert to int operation: " + convertToInt);

        //setup for reading msg
        readingSize = false;
        remainingBytes = length;
        msgLength = length;
        msgBytes = new byte[msgLength];
    }

    public int WriteString(string outString)
    {
        byte[] outBuffer = streamEncoding.GetBytes(outString);
        int len = outBuffer.Length;
        if (len > UInt16.MaxValue)
        {
            len = (int)UInt16.MaxValue;
        }
        ioStream.WriteByte((byte)(len / 256));
        ioStream.WriteByte((byte)(len & 255));
        ioStream.Write(outBuffer, 0, len);
        ioStream.Flush();

        return outBuffer.Length + 2;
    }
}
