using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System;

public class SerialCommunicationController : MonoBehaviour
{
    public string messageToSend;
    public bool sendMessage = false;
    public bool openPort = false;

    SerialPort _serialPort;
    Thread _readThread;
    CancellationTokenSource _cts;

    private bool _dataReceived = false;
    private string _receivedData = "";

    public static SerialCommunicationController Instance;

    public static Action<string> OnDataReceived;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnValidate()
    {
        if (sendMessage && !string.IsNullOrEmpty(messageToSend))
        {
            sendMessage = false;
            WriteMessage(messageToSend);
        }

        if (openPort == true)
        {
            openPort = false;
            OpenPort();
        }
    }

    void Start()
    {
        _serialPort = new SerialPort();
        _cts = new CancellationTokenSource();
        _readThread = new Thread(() => ReadSerialData(_cts.Token));

        _serialPort.BaudRate = 9600;
        _serialPort.PortName = "COM3";
        _serialPort.Handshake = Handshake.None;

        //8N1 pattern
        _serialPort.DataBits = 8;
        _serialPort.Parity = Parity.None;
        _serialPort.StopBits = StopBits.One;
    }

    private void OpenPort()
    {
        if (_serialPort.IsOpen || _readThread == null) return;

        _serialPort.Open();
        _readThread.Start();
    }

    private void OnDestroy()
    {
        if (_cts != null)
            _cts.Cancel();

        if (_readThread != null && _readThread.IsAlive)
        {
            _readThread.Join(5000); // Wait for the thread to finish, with a timeout of 5 seconds
        }

        if (_cts != null)
            _cts.Dispose();

        if (_serialPort != null && _serialPort.IsOpen)
            _serialPort.Close();
    }

    void Update()
    {
        if (_dataReceived)
        {
            _dataReceived = false;
            Debug.Log($"Arduino: {_receivedData}");
        }
    }

    public void WriteMessage(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        if (_serialPort.IsOpen)
        {
            Debug.Log("Sending message: " + message);
            _serialPort.WriteLine(message);
        }
        else
        {
            Debug.LogWarning("Serial port is not open. Cannot send message.");
        }
    }

    private void ReadSerialData(CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.Log("Cancellation requested.");
                    break;
                }

                _receivedData = _serialPort.ReadLine();
                _dataReceived = true;
            }
            catch (TimeoutException)
            {
                throw;
            }
        }

        Debug.Log("Exiting ReadSerialData thread.");
    }
}
