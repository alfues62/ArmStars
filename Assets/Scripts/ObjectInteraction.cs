using UnityEngine;

public class ObjectInteraction : MonoBehaviour
{
    // --- Variables para Rotación ---
    public float rotationSpeed = 1.0f; // Velocidad de rotación
    private Touch touch; // Para almacenar el toque

    // --- Variables para Escalado (Pinch-to-Zoom) ---
    public float scaleSpeed = 0.5f;   // Velocidad de escalado
    public float minScale = 0.5f;     // Escala mínima
    public float maxScale = 3.0f;     // Escala máxima

    // Variables para calcular el escalado
    private Touch touchZero;
    private Touch touchOne;
    private float prevTouchDeltaMag;

    void Update()
    {
        // --- LÓGICA DE ROTACIÓN (UN DEDO) ---
        // Comprueba si hay exactamente un toque en la pantalla
        if (Input.touchCount == 1)
        {
            touch = Input.GetTouch(0); // Obtiene el primer toque

            // Comprueba si el dedo se está moviendo
            if (touch.phase == TouchPhase.Moved)
            {
                // Calcula la rotación
                // Usamos deltaPosition.x para rotar en el eje Y (horizontalmente)
                // Puedes añadir .y para rotar en el eje X si lo necesitas
                float rotationX = touch.deltaPosition.x * rotationSpeed * Time.deltaTime;
                float rotationY = touch.deltaPosition.y * rotationSpeed * Time.deltaTime;

                // Aplica la rotación alrededor del eje Y (up) y X (right)
                // Gira el objeto sobre su propio eje vertical
                transform.Rotate(Vector3.up, -rotationX, Space.World);
                // Gira el objeto sobre su propio eje horizontal
                transform.Rotate(Vector3.right, rotationY, Space.World);
            }
        }

        // --- LÓGICA DE ESCALADO (DOS DEDOS) ---
        // Comprueba si hay exactamente dos toques en la pantalla
        if (Input.touchCount == 2)
        {
            // Obtiene ambos toques
            touchZero = Input.GetTouch(0);
            touchOne = Input.GetTouch(1);

            // Calcula la posición de los toques en el frame anterior
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Calcula la magnitud (distancia) entre los dedos en este frame y en el anterior
            float prevTouchMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentTouchMag = (touchZero.position - touchOne.position).magnitude;

            // Calcula la diferencia
            float deltaMagnitudeDiff = currentTouchMag - prevTouchMag;

            // Calcula el nuevo tamaño de la escala
            float newScale = transform.localScale.x + (deltaMagnitudeDiff * scaleSpeed * Time.deltaTime);

            // Limita la escala usando Mathf.Clamp para que no sea muy pequeña o muy grande
            newScale = Mathf.Clamp(newScale, minScale, maxScale);

            // Aplica la nueva escala de forma uniforme a X, Y, Z
            transform.localScale = new Vector3(newScale, newScale, newScale);
        }
    }
}