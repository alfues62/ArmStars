using UnityEngine;

public class opponentLogic : MonoBehaviour
{
    public gameData dataScript;

    [Header("Opponent Data")]
    private int opponentIndex;
    public int totalRounds = 3;
    private int roundsWon = 0;
    private int roundsPlayed = 0;
    private bool bossDefeated = false;

    void Start()
    {
        // Si no hay referencia, buscar automáticamente
        if (dataScript == null)
        {
            dataScript = FindAnyObjectByType<gameData>();
        }

        // Comprobar si el oponente ya está derrotado
        CheckOpponentStatus();
    }

    void CheckOpponentStatus()
    {
        // Avanzar mientras el oponente actual esté derrotado
        while (opponentIndex < dataScript.opponents && dataScript.defeatedOpponents[opponentIndex])
        {
            Debug.Log($"⏩ Opponent {opponentIndex + 1} already defeated. Moving to next.");
            opponentIndex++;
        }

        // Si no quedan oponentes
        if (opponentIndex >= dataScript.opponents)
        {
            Debug.Log("🏁 All opponents defeated!");
            // Aquí puedes lanzar la lógica de final de nivel
        }
        else
        {
            Debug.Log($"🎯 Current opponent: {opponentIndex + 1}");
            roundsWon = 0;
            roundsPlayed = 0;
            bossDefeated = false;
        }
    }

    void Update()
    {
        // Simula ganar una ronda con SPACE
        if (Input.GetKeyDown(KeyCode.Space))
        {
            WinRound();
        }

        // Simula perder una ronda con L
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoseRound();
        }
    }


    public void WinRound()
    {
        if (bossDefeated) return; // already done

        roundsPlayed++;
        roundsWon++;
        Debug.Log($"🏆 Won round {roundsPlayed} / {totalRounds}");

        CheckVictoryCondition();
    }

    public void LoseRound()
    {
        if (bossDefeated) return; // already done

        roundsPlayed++;
        Debug.Log($"❌ Lost round {roundsPlayed} / {totalRounds}");

        CheckVictoryCondition();
    }

    void CheckVictoryCondition()
    {
        if (roundsPlayed >= totalRounds)
        {
            // Si ganó 2 o más, se considera victoria
            if (roundsWon >= 2)
            {
                bossDefeated = true;
                dataScript.RegisterVictory(opponentIndex);

                // PASAR AL SIGUIENTE OPONENTE AUTOMÁTICAMENTE
                opponentIndex++;
                CheckOpponentStatus();
            }
            else
            {
                Debug.Log("💀 You lost against this boss.");
                // Reinicia para volver a intentar
                roundsWon = 0;
                roundsPlayed = 0;
            }
        }
    }

}
