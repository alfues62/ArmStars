using UnityEngine;
using Vuforia;

public class gameData : MonoBehaviour
{
    [Header("Defeated Opponents")]
    public int opponents = 3;
    public bool[] defeatedOpponents;

    [Header("Opponent ImageTargets")]
    public GameObject[] opponentGameObjects; // Asignar los ImageTargets en orden

    private int currentOpponent = -1; // ninguno detectado
    private int lastDefeatedOpponent = 0;


    void Awake()
    {
        defeatedOpponents = new bool[opponents];
        LoadProgress();
        RegisterImageTargetEvents();
    }

    private void LoadProgress()
    {
        for (int i = 0; i < opponents; i++)
            defeatedOpponents[i] = PlayerPrefs.GetInt($"opponent_{i}", 0) == 1;

        lastDefeatedOpponent = PlayerPrefs.GetInt("lastDefeatedOpponent", -1);
    }

    private void RegisterImageTargetEvents()
    {
        for (int i = 0; i < opponentGameObjects.Length; i++)
        {
            var observer = opponentGameObjects[i].GetComponent<ObserverBehaviour>();
            if (observer != null)
            {
                int index = i; // Captura índice para la lambda
                observer.OnTargetStatusChanged += (target, status) =>
                {
                    HandleTargetStatus(index, status);
                };
            }
        }
    }

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

    public int GetCurrentOpponentIndex()
    {
        return currentOpponent;
    }

    public void RegisterVictory(int opponentIndex)
    {
        if (opponentIndex < 0 || opponentIndex >= opponents) return;

        defeatedOpponents[opponentIndex] = true;
        PlayerPrefs.SetInt($"opponent_{opponentIndex}", 1);
        int nextOpponentUnlocked = opponentIndex + 1;

        // Comprobamos si este es un nuevo récord de progreso
        if (nextOpponentUnlocked > lastDefeatedOpponent)
        {
            PlayerPrefs.SetInt("lastDefeatedOpponent", nextOpponentUnlocked);
            PlayerPrefs.Save();
            lastDefeatedOpponent = nextOpponentUnlocked;
            Debug.Log($"✅ Oponente {opponentIndex} derrotado. Desbloqueado hasta el {nextOpponentUnlocked}.");
        }
        else
        {
            Debug.Log($"✅ Oponente {opponentIndex} derrotado (de nuevo).");
        }
        CheckForAllDefeated();
    }

    private void CheckForAllDefeated()
    {

        bool allDefeated = true;
        for (int i = 0; i < opponents; i++)
        {
            if (!defeatedOpponents[i])
            {
                allDefeated = false;
                break;
            }
        }

        if (allDefeated)
        {
            Debug.Log("🎉🎊 ¡FELICIDADES! ¡Has derrotado a TODOS los oponentes! 🎊🎉");
           
        }
    }

    public void ResetProgress()
    {
        for (int i = 0; i < opponents; i++)
            PlayerPrefs.SetInt($"opponent_{i}", 0);

        PlayerPrefs.Save();
        defeatedOpponents = new bool[opponents];
        currentOpponent = 0;
        lastDefeatedOpponent = 0;

        Debug.Log("♻️ Progreso reiniciado.");
    }

    public int GetLastDefeatedOpponent()
    {
        return lastDefeatedOpponent;
    }

    void Update()
    {
        // Detecta si presionan R para reiniciar el progreso
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetProgress();
        }
    }
}