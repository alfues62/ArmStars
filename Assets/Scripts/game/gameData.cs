using UnityEngine;
using Vuforia;  // Requiere Vuforia Engine

public class gameData : MonoBehaviour
{
    [Header("Defeated Opponents")]
    public int opponents = 3;
    public bool[] defeatedOpponents;

    [Header("Opponent GameObjects / Image Targets")]
    public GameObject[] opponentGameObjects; // ImageTargets en orden

    private int currentOpponent = -1; // -1 = ninguno detectado

    void Awake()
    {
        defeatedOpponents = new bool[opponents];
        LoadProgress();
        RegisterImageTargetEvents();
    }

    // 🔹 Carga el progreso guardado de oponentes derrotados
    private void LoadProgress()
    {
        for (int i = 0; i < opponents; i++)
            defeatedOpponents[i] = PlayerPrefs.GetInt($"opponent_{i}", 0) == 1;
    }

    // 🔹 Se suscribe a los eventos de detección de cada ImageTarget
    private void RegisterImageTargetEvents()
    {
        for (int i = 0; i < opponentGameObjects.Length; i++)
        {
            var observer = opponentGameObjects[i].GetComponent<ObserverBehaviour>();
            if (observer != null)
            {
                int index = i;
                observer.OnTargetStatusChanged += (target, status) =>
                {
                    HandleTargetStatus(index, status);
                };
            }
        }
    }

    // 🔹 Maneja el estado de cada marca detectada
    private void HandleTargetStatus(int index, TargetStatus status)
    {
        if (status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED)
        {
            currentOpponent = index;
            Debug.Log($"🎯 Marca detectada: {opponentGameObjects[index].name} (índice {index})");
        }
        else if (status.Status == Status.NO_POSE && currentOpponent == index)
        {
            currentOpponent = -1;
            Debug.Log($"👋 Marca perdida: {opponentGameObjects[index].name}");
        }
    }

    // 🔹 Devuelve el índice del ImageTarget actualmente detectado
    public int GetCurrentOpponentIndex()
    {
        return currentOpponent;
    }

    // 🔹 Marca un oponente como derrotado
    public void RegisterVictory(int opponentIndex)
    {
        if (opponentIndex < 0 || opponentIndex >= opponents) return;

        defeatedOpponents[opponentIndex] = true;
        PlayerPrefs.SetInt($"opponent_{opponentIndex}", 1);
        PlayerPrefs.Save();

        Debug.Log($"✅ Oponente {opponentIndex + 1} derrotado.");
    }

    // 🔹 Reinicia el progreso de los oponentes
    public void ResetProgress()
    {
        for (int i = 0; i < opponents; i++)
            PlayerPrefs.SetInt($"opponent_{i}", 0);

        PlayerPrefs.Save();
        defeatedOpponents = new bool[opponents];
        currentOpponent = -1;

        Debug.Log("♻️ Progreso reiniciado.");
    }
    private void FixedUpdate()
    {
        Input.GetKeyDown(KeyCode.R);
        {
            ResetProgress();
        }
    }
}
