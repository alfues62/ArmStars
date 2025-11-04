using UnityEngine;
using TMPro;

public class DeviceRotationDetector : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text rotationText; // Asigna aquí tu TextMeshPro en el Canvas

    [Header("Sensibilidad")]
    public float rotationThreshold = 1f; // Sensibilidad mínima para detectar giro

    private bool gyroEnabled;
    private Quaternion lastGyro;

    void Start()
    {
        // Activar el giroscopio si está disponible
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            gyroEnabled = true;
            lastGyro = Input.gyro.attitude;

            if (rotationText != null)
            {
                rotationText.text = "📱 Giroscopio activado";
                rotationText.color = Color.white;
            }
        }
        else
        {
            if (rotationText != null)
            {
                rotationText.text = "⚠️ Dispositivo sin giroscopio";
                rotationText.color = Color.red;
            }
        }
    }

    void Update()
    {
        if (!gyroEnabled) return;

        // Obtener la rotación actual del giroscopio
        Quaternion currentGyro = Input.gyro.attitude;

        // Calcular la diferencia de rotación desde el último frame
        Quaternion delta = currentGyro * Quaternion.Inverse(lastGyro);
        Vector3 deltaEuler = delta.eulerAngles;

        // Normalizar ángulos (-180 a 180)
        if (deltaEuler.z > 180) deltaEuler.z -= 360;

        // Detectar dirección del giro
        string message = "";
        Color color = Color.white;

        if (deltaEuler.z > rotationThreshold)
        {
            message = "↺ Girando hacia la IZQUIERDA";
            color = Color.red;
        }
        else if (deltaEuler.z < -rotationThreshold)
        {
            message = "↻ Girando hacia la DERECHA";
            color = Color.blue;
        }
        else
        {
            message = "📱 Estable";
            color = Color.green;
        }

        // Mostrar en pantalla
        if (rotationText != null)
        {
            rotationText.text = message;
            rotationText.color = color;
        }

        lastGyro = currentGyro;
    }
}
