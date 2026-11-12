using UnityEngine;

namespace SerialCommunication
{
    public class SerialCommunicationTester : MonoBehaviour
    {
        public bool btn_OpenPort = false;
        [Space]
        public string messageToSend;
        public bool sendMessage = false;

        private void OnValidate()
        {
            if (btn_OpenPort == true)
            {
                btn_OpenPort = false;
                SerialCommunicationController.Instance.OpenPort();
            }

            if (sendMessage)
            {
                sendMessage = false;
                SerialCommunicationController.Instance.WriteMessage(messageToSend);
            }
        }
    }
}