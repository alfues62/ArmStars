using UnityEngine;

public class gameData : MonoBehaviour
{
    [Header("Defeated Opponents")]
    public int opponents = 3;                    // Número de oponentes
    public bool[] defeatedOpponents;             // Estado de derrota de los oponentes

    [Header("Opponent GameObjects / Image Targets")]
    public GameObject[] opponentGameObjects;     // Oponentes en la escena (para mostrar/ocultar según derrota)

    private int currentOpponent = 0;

    void Awake()
    {
        defeatedOpponents = new bool[opponents];

        // Cargar el estado de los oponentes desde PlayerPrefs
        LoadProgress();

        // Actualizar el oponente actual
        UpdateCurrentOpponent();
    }

    // Función para cargar el progreso desde PlayerPrefs
    private void LoadProgress()
    {
        for (int i = 0; i < opponents; i++)
        {
            defeatedOpponents[i] = PlayerPrefs.GetInt($"opponent_{i}", 0) == 1;
        }
    }

    // Función para registrar una victoria sobre un oponente
    public void RegisterVictory(int opponentIndex)
    {
        if (opponentIndex >= 0 && opponentIndex < opponents)
        {
            defeatedOpponents[opponentIndex] = true;

            // Guardar el progreso inmediatamente solo si se actualiza el estado
            PlayerPrefs.SetInt($"opponent_{opponentIndex}", 1);

            // Actualizamos el oponente actual
            UpdateCurrentOpponent();

            // Verificar si se han derrotado todos los oponentes
            if (AreAllOpponentsDefeated())
            {
                OnAllOpponentsDefeated();
            }
            else
            {
                Debug.Log($"✅ Opponent {opponentIndex + 1} defeated!");
            }
        }
        else
        {
            Debug.LogWarning("❌ Invalid opponent index!");
        }
    }

    // Verifica si todos los oponentes han sido derrotados
    private bool AreAllOpponentsDefeated()
    {
        foreach (bool defeated in defeatedOpponents)
        {
            if (!defeated)
            {
                return false;
            }
        }
        return true;
    }

    // Función que se activa cuando todos los oponentes son derrotados
    private void OnAllOpponentsDefeated()
    {
        // Esta función se llama cuando todos los oponentes han sido derrotados.
        // Puedes dejarla vacía por ahora o agregar futuras acciones aquí.
        Debug.Log("🎉 All opponents defeated! Game completed.");

        // Aquí puedes agregar más lógica para finalizar el juego, mostrar créditos, etc.
        // Por ejemplo, cargar una escena final o activar una pantalla de victoria.

        // Aseguramos que el progreso se guarde después de completar el juego
        PlayerPrefs.Save();
    }

    // Función para resetear todo el progreso (por ejemplo, cuando el jugador reinicia el juego)
    public void ResetProgress()
    {
        for (int i = 0; i < opponents; i++)
        {
            defeatedOpponents[i] = false;
            PlayerPrefs.SetInt($"opponent_{i}", 0);
        }

        // Guardamos los cambios una sola vez
        PlayerPrefs.Save();

        Debug.Log("♻️ All progress reset!");
        currentOpponent = 0;
    }

    // Actualiza el índice del oponente actual
    public void UpdateCurrentOpponent()
    {
        currentOpponent = 0;
        while (currentOpponent < opponents && defeatedOpponents[currentOpponent])
        {
            currentOpponent++;
        }
    }

    // Devuelve el índice del oponente actual
    public int GetCurrentOpponentIndex()
    {
        return currentOpponent;
    }
}
