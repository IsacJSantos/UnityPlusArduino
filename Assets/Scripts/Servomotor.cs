using SerialCommunication;
using UnityEngine;

public class Servomotor : MonoBehaviour
{
    [SerializeField] private Transform _motorAxis;

    [SerializeField, Range(0, 180)] private int _axisAngle;

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            _motorAxis.transform.localEulerAngles = Vector3.up * (_axisAngle * -1);
            SerialCommunicationController.Instance.WriteMessage(_axisAngle.ToString());
        }
    }
}
