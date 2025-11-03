using UnityEngine;

public class gameData : MonoBehaviour
{
    [Header("Defeated Opponents")]
    public int opponents = 3; // total bosses
    public bool[] defeatedOpponents;
    public GameObject[] opponentGameObjects;

    void Awake()
    {
        defeatedOpponents = new bool[opponents];
    }

    public void RegisterVictory(int opponentIndex)
    {
        if (opponentIndex >= 0 && opponentIndex < opponents)
        {
            defeatedOpponents[opponentIndex] = true;
            Debug.Log($"✅ Opponent {opponentIndex + 1} defeated! Proceed to next round!");
        }
        else
        {
            Debug.LogWarning("❌ Invalid opponent index!");
        }
    }
}
