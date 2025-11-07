using UnityEngine;
using TMPro;

public class gameLogic : MonoBehaviour
{
    public gameData dataScript;
    public GyroRotationWithAnimation gamePlay;

    [Header("UI Elements")]
    public TMP_Text roundText;

    // --- NUEVO: Audio de Victoria/Derrota de la Partida ---
    [Header("Audio de Partida")]
    [Tooltip("AudioSource para los sonidos de victoria/derrota de la partida")]
    public AudioSource audioSourcePartida;
    public AudioClip sonidoVictoriaPartida; // Sonido al ganar el "mejor de 3"
    public AudioClip sonidoDerrotaPartida;  // Sonido al perder el "mejor de 3"
    // --- FIN NUEVO ---

    [Header("Opponent Data")]
    private int opponentIndex;
    public int totalRounds = 3;
    private int roundsWon = 0;
    private int roundsPlayed = 0;
    private bool bossDefeated = false;
    public bool opponentDefeated = false;

    void Start()
    {
        if (dataScript == null)
        {
            dataScript = FindAnyObjectByType<gameData>();
        }

        // --- NUEVO: Asegurarse de tener un AudioSource ---
        // Si no asignas uno en el Inspector, buscará uno en este GameObject.
        // Si no lo encuentra, añadirá uno nuevo.
        if (audioSourcePartida == null)
        {
            audioSourcePartida = GetComponent<AudioSource>();
            if (audioSourcePartida == null)
            {
                audioSourcePartida = gameObject.AddComponent<AudioSource>();
                audioSourcePartida.playOnAwake = false; // Buena práctica
            }
        }
        // --- FIN NUEVO ---

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
                // --- Has Ganado la Partida ---
                bossDefeated = true;
                opponentIndex = dataScript.GetCurrentOpponentIndex();
                dataScript.RegisterVictory(opponentIndex);
                opponentDefeated = true;

                // --- NUEVO: Reproducir sonido de VICTORIA ---
                PlayMatchSound(sonidoVictoriaPartida);
            }
            else
            {
                // --- Has Perdido la Partida ---
                
                // --- NUEVO: Reproducir sonido de DERROTA ---
                PlayMatchSound(sonidoDerrotaPartida);
                
                // Reseteas para el reintento
                roundsWon = 0;
                roundsPlayed = 0;
                UpdateRoundText();
            }
        }
    }

    // --- NUEVA FUNCIÓN ---
    // Un simple helper para reproducir el sonido
    void PlayMatchSound(AudioClip clip)
    {
        if (clip != null && audioSourcePartida != null)
        {
            audioSourcePartida.PlayOneShot(clip);
        }
    }
    // --- FIN NUEVO ---

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