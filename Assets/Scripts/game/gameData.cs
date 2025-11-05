using UnityEngine;

public class gameData : MonoBehaviour
{
    [Header("Defeated Opponents")]
    public int opponents = 3;
    public bool[] defeatedOpponents;

    [Header("Opponent GameObjects / Image Targets")]
    public GameObject[] opponentGameObjects;

    private int currentOpponent = 0;

    void Awake()
    {
        defeatedOpponents = new bool[opponents];

        for (int i = 0; i < opponents; i++)
        {
            defeatedOpponents[i] = PlayerPrefs.GetInt($"opponent_{i}", 0) == 1;
        }

        UpdateCurrentOpponent();

        UpdateActiveTargets();
    }

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

    void UpdateCurrentOpponent()
    {
        currentOpponent = 0;
        while (currentOpponent < opponents && defeatedOpponents[currentOpponent])
        {
            currentOpponent++;
        }
    }

    void UpdateActiveTargets()
    {
        for (int i = 0; i < opponentGameObjects.Length; i++)
        {
            if (i == currentOpponent && !defeatedOpponents[i])
                opponentGameObjects[i].SetActive(true);
            else
                opponentGameObjects[i].SetActive(false);
        }
    }

    public void TryInteract(int targetIndex)
    {
        if (targetIndex == currentOpponent)
        {
            Debug.Log($"🎯 You can play this boss: {targetIndex + 1}");
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

    public int GetCurrentOpponentIndex()
    {
        return currentOpponent;
    }
}
