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
    [SerializeField] private int scale = 2;
    [SerializeField] private float smoothing = 0.8f;

    private Vector3[] coords;
    [SerializeField] private GameObject ball;
    private Transform[] joints;

    void Start() {
        coords = new Vector3[21];
        joints = new Transform[21];
        for(int i = 0; i < 21; i++){
            coords[i] = Vector3.zero;
        }
        for(int i = 0; i < 21; i++){
            joints[i] = Instantiate(ball, Vector3.zero, Quaternion.identity).transform;
            joints[i].GetComponent<Renderer>().material.color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }
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
                        var msg = serverMsg.Split(',');
                        for(int i = 0, j = 0; i < msg.Length-2; i+=3, j++){
                            coords[j].x = float.Parse(msg[i]) * -scale;
                            coords[j].y = float.Parse(msg[i+1]) * -scale;
                            coords[j].z = float.Parse(msg[i+2]) * scale;
                        }
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
        for(int i = 0; i < 21; i++){
            joints[i].position = Vector3.Lerp(joints[i].position, coords[i], smoothing);
        }
    }
}
