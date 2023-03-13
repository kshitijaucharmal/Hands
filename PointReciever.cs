using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class PointReciever : MonoBehaviour {

    private TcpClient socketConn;
    private Thread clientRecvThread;

    [SerializeField] private string host = "localhost";
    [SerializeField] private int port = 65432;

    void Start() {
        ConnectToServer();
    }

    void ConnectToServer(){
        try{
            clientRecvThread = new Thread(new ThreadStart(Listen));
            clientRecvThread.IsBackground = true;
            clientRecvThread.Start();
        }
        catch (Exception e){
            Debug.Log(e);
        }
    }

    void Listen(){
        try{
            socketConn = new TcpClient(host, port);
            Byte[] bytes = new Byte[1024];
            while(true){
                using (NetworkStream stream = socketConn.GetStream()){
                    int length;
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0){
                        var incomingData = new byte[length];
                        Array.Copy(bytes, 0, incomingData, 0, length);
                        string serverMsg = Encoding.ASCII.GetString(incomingData);
                        Debug.Log(serverMsg);
                    }
                }
            }
        }
        catch (SocketException e){
            Debug.Log("Socket Exception " + e);
        }
    }

    // Update is called once per frame
    void Update() {
        
    }
}
