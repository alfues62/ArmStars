using UnityEngine;
using TMPro;
using System.Collections;

public class gameLogic : MonoBehaviour
{
    public gameData dataScript;
    public GyroRotationWithAnimation gamePlay;

    [Header("UI Elements")]
    public TMP_Text roundText;

    [Header("Opponent Data")]
    private int opponentIndex;
    public int totalRounds = 3;
    public int roundsWon = 0;
    private int roundsPlayed = 0;
    private bool bossDefeated = false;
    public bool bossNotDefeated = false;

    public bool opponentDefeated = false;

    void Start()
    {
        if (dataScript == null)
        {
            dataScript = FindAnyObjectByType<gameData>();
        }

        int status = CheckOpponentStatus();

        // Puedes usar el valor status para hacer algo más adelante en tu código.
        Debug.Log("Opponent status: " + status);
    }

    // Función que devuelve el estado del oponente evaluado
    public int CheckOpponentStatus()
    {
        int currentOpponentIndex = dataScript.GetCurrentOpponentIndex();
        int lastDefeatedIndex = dataScript.GetLastDefeatedOpponent();
        Debug.Log("Current: " + currentOpponentIndex + " Last: " + lastDefeatedIndex);

        if (currentOpponentIndex < lastDefeatedIndex)
        {
            return 1; // Ya derrotado
        }
        else if (currentOpponentIndex > lastDefeatedIndex)
        {
            return 2; // Aún no desbloqueado
        }
        else if (currentOpponentIndex == lastDefeatedIndex)
        {
            return 0; // Es el turno de este oponente
        }

        return -1; // Estado desconocido
    }

    void UpdateRoundText()
    {
        int indice = dataScript.GetCurrentOpponentIndex();
        var jump = gamePlay.jumbotron[indice].GetComponent<jumbotronTexts>();
        jump.SetRoundNumJumbotron(roundsPlayed+1);
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
                StartCoroutine(GanarConEspera());
                opponentIndex = dataScript.GetCurrentOpponentIndex();
                dataScript.RegisterVictory(opponentIndex);
                opponentDefeated = true;
            }
            else
            {
                bossNotDefeated = true;
                StartCoroutine(EjecutarConEspera());

                roundsWon = 0;
                roundsPlayed = 0;

                UpdateRoundText();
            }
        }
    }
    private IEnumerator EjecutarConEspera()
    {
        // Ejecutas tu código
        int indice = dataScript.GetCurrentOpponentIndex();
        var jump1 = gamePlay.jumbotron[indice].GetComponent<jumbotronTexts>();
        jump1.ShowTexts(3);

        // Espera 2 segundos antes de continuar
        yield return new WaitForSeconds(2f);
    }
    private IEnumerator GanarConEspera()
    {
        // Ejecutas tu código
        int indice = dataScript.GetCurrentOpponentIndex();
        var jump1 = gamePlay.jumbotron[indice].GetComponent<jumbotronTexts>();
        jump1.Victory();

        // Espera 2 segundos antes de continuar
        yield return new WaitForSeconds(2f);
    }

    public void ResetRounds()
    {
        // Resetea los contadores para el nuevo combate
        roundsPlayed = 0;
        roundsWon = 0;
        bossDefeated = false;

        // Actualiza el texto de la UI para que ponga "Ronda: 0 / 3"
        UpdateRoundText();

        Debug.Log("--- ¡Contadores de rondas reseteados para un nuevo combate! ---");
    }

}
