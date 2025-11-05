using UnityEngine;

public class matchSettings : MonoBehaviour
{
    public gameLogic gameLogic;

    // Array para almacenar los tiempos aleatorios
    public float[] randomTimes;
    public float minTime = 1f; // Valor mínimo para el tiempo aleatorio
    public float maxTime = 5f; // Valor máximo para el tiempo aleatorio

    void Start()
    {
        // Si no se ha asignado el gameLogic, lo buscamos
        if (gameLogic == null)
        {
            gameLogic = FindAnyObjectByType<gameLogic>();
        }

        // Llenar el array de tiempos aleatorios
        GenerateRandomTimes();
    }

    void Update()
    {
        // Aquí podrías usar los tiempos aleatorios para cualquier propósito, 
        // por ejemplo, para gestionar eventos o tiempos de espera.
    }   

    // Método para generar los tiempos aleatorios
    void GenerateRandomTimes()
    {
        randomTimes = new float[10];  // Puedes cambiar el tamaño del array según necesites

        for (int i = 0; i < randomTimes.Length; i++)
        {
            randomTimes[i] = Random.Range(minTime, maxTime);
        }
    }
}
