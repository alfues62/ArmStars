using UnityEngine;
using Vuforia;

// Implementamos IObserverEventHandler para recibir los eventos de Vuforia
public class MiDetectorDeMarca : MonoBehaviour
{
    [Tooltip("El canvas que quieres mostrar cuando no se detecta nada")]
    public GameObject canvasAviso;

    // Un contador interno para saber cuántas marcas están activas
    private int marcasActivas = 0;

    void Start()
    {
        // Al empezar, no hay marcas activas.
        marcasActivas = 0;
        
        // Forzamos que el canvas se muestre al inicio
        if (canvasAviso != null)
        {
            canvasAviso.SetActive(true);
        }
    }

    // Esta función la llamará CADA MARCA cuando sea ENCONTRADA
    public void RegistrarMarcaEncontrada()
    {
        marcasActivas++; // Sumamos una marca al contador

        // Si es la primera marca detectada (el contador sube de 0 a 1),
        // ocultamos el canvas.
        if (canvasAviso != null && marcasActivas > 0)
        {
            canvasAviso.SetActive(false);
        }
    }

    // Esta función la llamará CADA MARCA cuando sea PERDIDA
    public void RegistrarMarcaPerdida()
    {
        marcasActivas--; // Restamos una marca al contador

        // Por seguridad, evitamos números negativos
        if (marcasActivas < 0)
        {
            marcasActivas = 0;
        }

        // Si el contador llega a CERO (no quedan más marcas activas),
        // mostramos el canvas.
        if (canvasAviso != null && marcasActivas == 0)
        {
            canvasAviso.SetActive(true);
        }
    }
}