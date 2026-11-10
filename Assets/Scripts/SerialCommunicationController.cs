using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System;

namespace SerialCommunication
{
    public class SerialCommunicationController : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private SeraialPortConfig _serialPortConfig;
        [SerializeField] private bool _openOnStart = false;
        public bool btn_OpenPort = false;

        [Space(10)]
        public string messageToSend;
        public bool sendMessage = false;

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

            if (btn_OpenPort == true)
            {
                btn_OpenPort = false;
                OpenPort();
            }
        }

        void Start()
        {
            if (_openOnStart)
                OpenPort(_serialPortConfig);
        }

        public void OpenPort(SeraialPortConfig config = null)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                Debug.LogError("Serial port is already open.");
                return;
            }

            if (config == null)
                config = _serialPortConfig;

            _serialPort = new SerialPort
            {
                PortName = config.PortName,
                BaudRate = config.BaudRate,
                Parity = config.Parity,
                DataBits = config.DataBits,
                StopBits = config.StopBits,
                Handshake = config.Handshake
            };

            _cts = new CancellationTokenSource();
            _readThread = new Thread(() => ReadSerialData(_cts.Token));

            try
            {
                _serialPort.Open();
            }
            catch (Exception e)
            {
                throw new Exception($"Error opening serial port: {e.Message}");
            }

            if (_serialPort.IsOpen)
            {
                Debug.Log($"Serial port {config.PortName} opened successfully.");
                _readThread.Start();
            }
        }

        public void ClosePort()
        {
            if (_serialPort == null || _serialPort.IsOpen == false)
            {
                Debug.LogError("Serial port is not open.");
                return;
            }

            ClosePortAndCancelReadThread();
        }

        private void OnDestroy()
        {
            ClosePortAndCancelReadThread();
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

        private void ClosePortAndCancelReadThread()
        {
            if (_cts != null)
                _cts.Cancel();

            if (_readThread != null && _readThread.IsAlive)
            {
                _readThread.Join(5000); // Wait for the thread to finish, with a timeout of 5 seconds
            }

            if (_cts != null)
                _cts.Dispose();

            _serialPort.Close();
        }
    }

    [System.Serializable]
    public class SeraialPortConfig
    {
        public string PortName;
        public int BaudRate = 9600;
        public Parity Parity;
        public int DataBits = 8;
        public StopBits StopBits = StopBits.One;
        public Handshake Handshake;
        public SeraialPortConfig(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake)
        {
            PortName = portName;
            BaudRate = baudRate;
            Parity = parity;
            DataBits = dataBits;
            StopBits = stopBits;
            Handshake = handshake;
        }
    }
}