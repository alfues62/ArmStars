using UnityEngine;

public class TouchRaycastVisualizer : MonoBehaviour
{
    [SerializeField] private float rayLength = 100f; // longitud del rayo
    [SerializeField] private Color rayColorHit = Color.green;
    [SerializeField] private Color rayColorNoHit = Color.red;

    void Update()
    {
        // --- MÓVIL / PANTALLA TÁCTIL ---
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                HandleTouchOrClick(touch.position);
            }
        }

        // --- EDITOR / PC ---
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouchOrClick(Input.mousePosition);
        }
#endif
    }

    void HandleTouchOrClick(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        bool hitSomething = Physics.Raycast(ray, out hit, rayLength);

        // Dibuja el rayo en la escena
        Color color = hitSomething ? rayColorHit : rayColorNoHit;
        Debug.DrawRay(ray.origin, ray.direction * rayLength, color, 1.5f); // dura 1.5s

        if (hitSomething)
        {
            Debug.Log("Has tocado: " + hit.collider.name);

            // Ejemplo: cambia el color del objeto tocado
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = Random.ColorHSV();
        }
        else
        {
            Debug.Log("No se tocó ningún objeto.");
        }
    }
}
