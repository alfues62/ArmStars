using UnityEngine;
using TMPro;

public class gameLogic : MonoBehaviour
{
    public gameData dataScript;

    [Header("UI Elements")]
    public TMP_Text roundText;

    [Header("Opponent Data")]
    private int opponentIndex;
    public int totalRounds = 3;
    private int roundsWon = 0;
    private int roundsPlayed = 0;
    private bool bossDefeated = false;

    void Start()
    {
        if (dataScript == null)
        {
            dataScript = FindAnyObjectByType<gameData>();
        }

        opponentIndex = dataScript.GetCurrentOpponentIndex();  // Sincroniza el oponente actual
        int status = CheckOpponentStatus();

        // Puedes usar el valor `status` para hacer algo más adelante en tu código.
        // Aquí solo lo estamos mostrando por ejemplo.
        Debug.Log("Opponent status: " + status);
    }

    // Función que devuelve el estado del oponente actual
    public int CheckOpponentStatus()
    {
        int currentOpponentIndex = dataScript.GetCurrentOpponentIndex();

        // Si el oponente ya ha sido derrotado
        if (opponentIndex < currentOpponentIndex)
        {
            return 1; // Oponente ya derrotado
        }
        // Si el oponente aún no ha sido desbloqueado
        else if (opponentIndex > currentOpponentIndex)
        {
            return 2; // Oponente aún no desbloqueado
        }
        // Si el oponente es el que toca (actual)
        else
        {
            return 0; // Es el turno de este oponente
        }
        }
    void UpdateRoundText()
        {
            if (roundText != null)
            {
                // Usamos $ para formatear el string fácilmente
                roundText.text = $"Ronda: {roundsPlayed} / {totalRounds}";
            }
        }

    public void WinRound()
    {
        if (bossDefeated) return; // already done

        roundsPlayed++;
        roundsWon++;

        UpdateRoundText();

        CheckVictoryCondition();
    }

    public void LoseRound()
    {
        if (bossDefeated) return; // already done

        roundsPlayed++;
        Debug.Log($"❌ Lost round {roundsPlayed} / {totalRounds}");

        UpdateRoundText();

        CheckVictoryCondition();
    }

    void CheckVictoryCondition()
    {
        if (roundsPlayed >= totalRounds)
        {
            if (roundsWon >= 2)
            {
                bossDefeated = true;
                dataScript.RegisterVictory(opponentIndex);

                // Actualizamos el oponente actual
                dataScript.UpdateCurrentOpponent();  // Asegúrate de que el índice se actualice

                // Obtener el siguiente oponente
                opponentIndex = dataScript.GetCurrentOpponentIndex();
                CheckOpponentStatus();
            }
            else
            {

                roundsWon = 0;
                roundsPlayed = 0;

                UpdateRoundText();
            }
        }
    }

}
