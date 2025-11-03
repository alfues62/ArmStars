using UnityEngine;

public class gameData : MonoBehaviour
{
    [Header("Defeated Opponents")]
    public int opponents = 3;
    public bool[] defeatedOpponents;

    [Header("Opponent GameObjects / Image Targets")]
    public GameObject[] opponentGameObjects;

    // Indice del jefe actual
    private int currentOpponent = 0;

    void Awake()
    {
        // Inicializar array
        defeatedOpponents = new bool[opponents];

        // Cargar datos guardados
        for (int i = 0; i < opponents; i++)
        {
            defeatedOpponents[i] = PlayerPrefs.GetInt($"opponent_{i}", 0) == 1;
        }

        // Encontrar el primer jefe no derrotado
        UpdateCurrentOpponent();

        // Actualizar Image Targets
        UpdateActiveTargets();
    }

    // Registrar victoria y avanzar al siguiente jefe
    public void RegisterVictory(int opponentIndex)
    {
        if (opponentIndex >= 0 && opponentIndex < opponents)
        {
            defeatedOpponents[opponentIndex] = true;

            // Guardar en PlayerPrefs
            PlayerPrefs.SetInt($"opponent_{opponentIndex}", 1);
            PlayerPrefs.Save();

            Debug.Log($"✅ Opponent {opponentIndex + 1} defeated and saved!");

            // Avanzar al siguiente jefe
            UpdateCurrentOpponent();
            UpdateActiveTargets();
        }
        else
        {
            Debug.LogWarning("❌ Invalid opponent index!");
        }
    }

    // Reiniciar todo el progreso
    public void ResetProgress()
    {
        for (int i = 0; i < opponents; i++)
        {
            defeatedOpponents[i] = false;
            PlayerPrefs.SetInt($"opponent_{i}", 0);
        }
        PlayerPrefs.Save();
        Debug.Log("♻️ All progress reset!");

        currentOpponent = 0;
        UpdateActiveTargets();
    }

    // Actualiza currentOpponent al primer jefe no derrotado
    void UpdateCurrentOpponent()
    {
        currentOpponent = 0;
        while (currentOpponent < opponents && defeatedOpponents[currentOpponent])
        {
            currentOpponent++;
        }
    }

    // Activa solo el Image Target del jefe que toca
    void UpdateActiveTargets()
    {
        for (int i = 0; i < opponentGameObjects.Length; i++)
        {
            if (i == currentOpponent && !defeatedOpponents[i])
                opponentGameObjects[i].SetActive(true); // jefe activo
            else
                opponentGameObjects[i].SetActive(false); // jefe bloqueado o ya pasado
        }
    }

    // Intentar interactuar con un Image Target
    public void TryInteract(int targetIndex)
    {
        if (targetIndex == currentOpponent)
        {
            Debug.Log($"🎯 You can play this boss: {targetIndex + 1}");
            // Aquí lanzarías la lógica de gameplay de ese jefe
        }
        else if (targetIndex < currentOpponent)
        {
            Debug.Log("✅ Already defeated, go to next boss!");
        }
        else
        {
            Debug.Log("⛔ You need to defeat previous bosses first!");
        }
    }

    // Devuelve el índice del jefe actual
    public int GetCurrentOpponentIndex()
    {
        return currentOpponent;
    }
}
