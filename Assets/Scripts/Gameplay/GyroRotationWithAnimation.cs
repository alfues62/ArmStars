using UnityEngine;
using TMPro;
using System.Collections;

public class GyroRotationWithAnimation : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text rotationText; // Para mostrar el 3, 2, 1, Go!

    [Header("Lógica de Juego")]
    public gameLogic miGameLogic;

    [Header("Tiempo antes del go")]
    public float randomWaitTimeMin = 1f; // Tiempo mínimo para el random
    public float randomWaitTimeMax = 3f; // Tiempo máximo para el random

    [Header("Configuración de Tiempos de Reacción")]
    public float[] reactionTimes;

    [Header("Configuración de Audio")]
    public AudioSource AudioSource;
    public AudioClip countdownClip;
    public AudioClip goClip;

    [Tooltip("Asigna los clips de audio para CADA enemigo. El orden debe coincidir (Enemigo 0, Enemigo 1, etc.)")]
    public EnemyAudioPool[] enemyAudioPools;

    [Header("UI")]
    public GameObject uiPanelOponenteDerrotado;  // Panel que muestra el mensaje si el oponente ya ha sido derrotado
    public GameObject uiPanelOponenteNoDisponible;  // Panel que muestra el mensaje si el oponente no está disponible
    public GameObject uiPanelVictoria;
    public GameObject[] mesa;
    public GameObject uiPanelMarcaNoDetectada;

    public GameObject[] jumbotron;
    public jumbotronTexts miJumbotronText;

    [Header("Huesos a Rotar (Arrays)")]
    public Transform[] huesosBrazos_Array;
    public Transform[] huesosBrazos001_Array;

    [Header("Configuración del Juego")]
    public float successWindowDuration = 0.5f; // Ventana de 0.5 seg para acertar
    public float rotationThreshold = 30f; // Sensibilidad del giro
    public float duracionAnimacion = 0.25f; // Duración de la anim (ida)
    public float waitBeforeRestart = 2.0f; // Tiempo de espera antes de reiniciar

    private bool gyroEnabled;
    private bool isAnimating = false;
    private bool playerTiltedLeft = false;

    private Quaternion[] rotOriginalBrazos_Array;
    private Vector3[] posOriginalBrazos_Array;
    private Quaternion[] rotOriginalBrazos001_Array;
    private Vector3[] posOriginalBrazos001_Array;

    private Vector3 targetPosBrazo_Success, targetPosBrazo001_Success; // Izquierda
    private Quaternion targetRotBrazo_Success, targetRotBrazo001_Success; // Izquierda
    private Vector3 targetPosBrazo_Fail, targetPosBrazo001_Fail; // Derecha
    private Quaternion targetRotBrazo_Fail, targetRotBrazo001_Fail; // Derecha

    private enum GameState { Countdown, Listening, Animating, Idle }
    private GameState currentState = GameState.Idle;

    private Coroutine currentGameLoop = null;
    private bool isGameWon = false;

    void Start()
    {
        miGameLogic = FindAnyObjectByType<gameLogic>();

        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            gyroEnabled = true;
        }
        else
        {
            gyroEnabled = false;
        }

        if (huesosBrazos_Array != null && huesosBrazos001_Array != null &&
            huesosBrazos_Array.Length == huesosBrazos001_Array.Length)
        {
            int numArmSets = huesosBrazos_Array.Length;
            posOriginalBrazos_Array = new Vector3[numArmSets];
            rotOriginalBrazos_Array = new Quaternion[numArmSets];
            posOriginalBrazos001_Array = new Vector3[numArmSets];
            rotOriginalBrazos001_Array = new Quaternion[numArmSets];

            for (int i = 0; i < numArmSets; i++)
            {
                if (huesosBrazos_Array[i])
                {
                    posOriginalBrazos_Array[i] = huesosBrazos_Array[i].localPosition;
                    rotOriginalBrazos_Array[i] = huesosBrazos_Array[i].localRotation;
                }
                if (huesosBrazos001_Array[i])
                {
                    posOriginalBrazos001_Array[i] = huesosBrazos001_Array[i].localPosition;
                    rotOriginalBrazos001_Array[i] = huesosBrazos001_Array[i].localRotation;
                }
            }
        }
        else
        {
            Debug.LogError("GyroRotation: Los arrays 'huesosBrazos_Array' y 'huesosBrazos001_Array' no están asignados o tienen tamaños diferentes.");
        }

        targetPosBrazo_Success = new Vector3(-1.862645e-08f, 0.01437014f, 9.220095e-08f);
        targetRotBrazo_Success = Quaternion.Euler(-26.949f, 79.839f, 61.002f);
        targetPosBrazo001_Success = new Vector3(-7.450581e-09f, 0.01437012f, 1.629815e-08f);
        targetRotBrazo001_Success = Quaternion.Euler(19.042f, -87.506f, 46.403f);

        targetPosBrazo_Fail = new Vector3(-1.862645e-08f, 0.01437014f, 9.220095e-08f);
        targetRotBrazo_Fail = Quaternion.Euler(18.948f, -88.626f, 45.951f);
        targetPosBrazo001_Fail = new Vector3(-7.450581e-09f, 0.01437012f, 1.629815e-08f);
        targetRotBrazo001_Fail = Quaternion.Euler(-26.946f, 79.819f, 61.009f);

        if (uiPanelOponenteDerrotado) uiPanelOponenteDerrotado.SetActive(false);
        if (uiPanelOponenteNoDisponible) uiPanelOponenteNoDisponible.SetActive(false);
        if (uiPanelVictoria) uiPanelVictoria.SetActive(false);
        
        if (uiPanelMarcaNoDetectada) uiPanelMarcaNoDetectada.SetActive(true);

        if (rotationText) rotationText.text = "Apunta a la marca para empezar...";
    }

    void Update()
    {
        playerTiltedLeft = false;

        if (gyroEnabled)
        {
            if (Input.gyro.rotationRateUnbiased.z * Mathf.Rad2Deg > rotationThreshold)
            {
                playerTiltedLeft = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            playerTiltedLeft = true;
        }

        if (miGameLogic.opponentDefeated == true && !isGameWon)
        {
            isGameWon = true;

            StopGameProcess();
            mesa[miGameLogic.dataScript.GetCurrentOpponentIndex()].SetActive(false);
            uiPanelVictoria.SetActive(true);
        }
    }

    public void StartGameProcess()
    {
        if (currentGameLoop == null)
        {
            if (rotationText) rotationText.text = "¡Marca detectada!";
            mesa[miGameLogic.dataScript.GetCurrentOpponentIndex()].SetActive(true);
            uiPanelVictoria.SetActive(false);
            currentGameLoop = StartCoroutine(GameLoop());
        }
    }

    public void startGame()
    {
        if (uiPanelMarcaNoDetectada) uiPanelMarcaNoDetectada.SetActive(false);

        if (currentGameLoop == null)
        {
            int status = miGameLogic.CheckOpponentStatus();
            Debug.Log(status);

            switch (status)
            {
                case 0:  // Oponente actual
                    miGameLogic.opponentDefeated = false;
                    miGameLogic.ResetRounds();
                    isGameWon = false;
                    StartGameProcess();
                    break;

                case 1:  // Oponente ya derrotado
                    if (rotationText) rotationText.text = "Este oponente ya ha sido derrotado. ¡Prueba otro!";
                    if (uiPanelOponenteDerrotado) uiPanelOponenteDerrotado.SetActive(true);
                    if (mesa[miGameLogic.dataScript.GetCurrentOpponentIndex()]) mesa[miGameLogic.dataScript.GetCurrentOpponentIndex()].SetActive(false);
                    break;

                case 2:  // Oponente aún no desbloqueado
                    if (rotationText) rotationText.text = "Este oponente aún no está disponible.";
                    if (uiPanelOponenteNoDisponible) uiPanelOponenteNoDisponible.SetActive(true);
                    if (mesa[miGameLogic.dataScript.GetCurrentOpponentIndex()]) mesa[miGameLogic.dataScript.GetCurrentOpponentIndex()].SetActive(false);
                    break;

                default:
                    break;
            }
        }
    }

    public void StopGameProcess()
    {
        if (uiPanelOponenteDerrotado) uiPanelOponenteDerrotado.SetActive(false);
        if (uiPanelOponenteNoDisponible) uiPanelOponenteNoDisponible.SetActive(false);
        
        if (currentGameLoop != null)
        {
            StopCoroutine(currentGameLoop);
            currentGameLoop = null; 
            currentState = GameState.Idle;
            isAnimating = false;

            int currentIndex = miGameLogic.dataScript.GetCurrentOpponentIndex();

            StopAllCoroutines(); 

            if (currentIndex >= 0 && currentIndex < posOriginalBrazos_Array.Length)
            {
                StartCoroutine(AnimateToTarget(
                    currentIndex,
                    posOriginalBrazos_Array[currentIndex], rotOriginalBrazos_Array[currentIndex],
                    posOriginalBrazos001_Array[currentIndex], rotOriginalBrazos001_Array[currentIndex]
                ));
            }
        }

        if (!isGameWon)
        {
            if (uiPanelMarcaNoDetectada) uiPanelMarcaNoDetectada.SetActive(true);
            if (rotationText) rotationText.text = "Marca perdida. Apunta de nuevo...";
        }
    }

    // Coroutine principal que maneja el "3, 2, 1, Go!"
    IEnumerator GameLoop()
    {
        while (true) 
        {
            int currentIndex = miGameLogic.dataScript.GetCurrentOpponentIndex();
            float tiempoReaccion = GetReactionTimeForEnemy(currentIndex);

            // --- 1. FASE DE ESPERA (IDLE) ---
            currentState = GameState.Idle;
            rotationText.text = "Prepárate...";
            
            // Reproducir un clip aleatorio del enemigo actual (ej: una burla)
            PlayRandomEnemyClip(currentIndex);

            yield return new WaitForSeconds(waitBeforeRestart);

            // --- 2. FASE DE CUENTA ATRÁS (COUNTDOWN) ---
            currentState = GameState.Countdown;
            rotationText.text = "3,2,1...";

            // --- CAMBIO 1: Interrumpir cualquier sonido (gruñido) antes de reproducir el "3,2,1" ---
            if (AudioSource.isPlaying) AudioSource.Stop();
            
            PlayAudioClip(countdownClip);
            yield return new WaitForSeconds(countdownClip.length);
            
            bool falseStart = false;
            float randomWaitTime = Random.Range(randomWaitTimeMin, randomWaitTimeMax);
            float falseStartTimer = 0f;

            while (falseStartTimer < randomWaitTime)
            {
                if (playerTiltedLeft)
                {
                    falseStart = true;
                    break;
                }
                falseStartTimer += Time.deltaTime;
                yield return null;
            }

            if (falseStart)
            {
                currentState = GameState.Animating; 
                rotationText.text = "❌ ¡SALIDA EN FALSO!";

                yield return StartCoroutine(DoRotation_Fail(currentIndex));
                if (miGameLogic != null)
                {
                    miGameLogic.LoseRound();
                }
                continue; 
            }
            // --- 3. FASE DE ESCUCHA (LISTENING) ---
            currentState = GameState.Listening;
            rotationText.text = "¡GO!";

            // --- CAMBIO 2: Interrumpir el "3,2,1" (si aún suena) para dar paso al "GO!" ---
            if (AudioSource.isPlaying) AudioSource.Stop();

            // Reproducir el sonido de "¡GO!".
            PlayAudioClip(goClip);

            bool success = false;
            float windowTimer = 0f;

            while (windowTimer < tiempoReaccion) 
            {
                if (playerTiltedLeft)
                {
                    success = true;
                    break;
                }
                windowTimer += Time.deltaTime;
                yield return null;
            }

            // --- 4. FASE DE ANIMACIÓN (ANIMATING) ---
            currentState = GameState.Animating;

            if (success)
            {
                rotationText.text = "¡BIEN HECHO!";
                yield return StartCoroutine(DoRotation_Success(currentIndex));
                if (miGameLogic != null)
                {
                    miGameLogic.WinRound();
                }
            }
            else
            {
                rotationText.text = "¡FALLO!";
                yield return StartCoroutine(DoRotation_Fail(currentIndex));
                if (miGameLogic != null)
                {
                    miGameLogic.LoseRound();
                }
            }
        }
    }

    IEnumerator DoRotation_Success(int index)
    {
        isAnimating = true;
        yield return AnimateToTarget(index, targetPosBrazo_Success, targetRotBrazo_Success, targetPosBrazo001_Success, targetRotBrazo001_Success);
        yield return new WaitForSeconds(0.1f);
        yield return AnimateToTarget(index, posOriginalBrazos_Array[index], rotOriginalBrazos_Array[index], posOriginalBrazos001_Array[index], rotOriginalBrazos001_Array[index]);
        isAnimating = false;
    }

    IEnumerator DoRotation_Fail(int index)
    {
        isAnimating = true;
        yield return AnimateToTarget(index, targetPosBrazo_Fail, targetRotBrazo_Fail, targetPosBrazo001_Fail, targetRotBrazo001_Fail);
        yield return new WaitForSeconds(0.1f);
        yield return AnimateToTarget(index, posOriginalBrazos_Array[index], rotOriginalBrazos_Array[index], posOriginalBrazos001_Array[index], rotOriginalBrazos001_Array[index]);
        isAnimating = false;
    }

    IEnumerator AnimateToTarget(int index, Vector3 p1_target, Quaternion r1_target, Vector3 p2_target, Quaternion r2_target)
    {
        if (index < 0 || index >= huesosBrazos_Array.Length)
        {
            Debug.LogError($"Índice de animación ({index}) fuera de rango.");
            yield break;
        }
        Transform huesoBrazos = huesosBrazos_Array[index];
        Transform huesoBrazos001 = huesosBrazos001_Array[index];

        float tiempoPasado = 0;

        Vector3 p1_start = huesoBrazos.localPosition;
        Quaternion r1_start = huesoBrazos.localRotation;
        Vector3 p2_start = huesoBrazos001.localPosition;
        Quaternion r2_start = huesoBrazos001.localRotation;

        while (tiempoPasado < duracionAnimacion)
        {
            float t = tiempoPasado / duracionAnimacion;

            if (huesoBrazos)
            {
                huesoBrazos.localPosition = Vector3.Lerp(p1_start, p1_target, t);
                huesoBrazos.localRotation = Quaternion.Slerp(r1_start, r1_target, t);
            }
            if (huesoBrazos001)
            {
                huesoBrazos001.localPosition = Vector3.Lerp(p2_start, p2_target, t);
                huesoBrazos001.localRotation = Quaternion.Slerp(r2_start, r2_target, t);
            }

            tiempoPasado += Time.deltaTime;
            yield return null;
        }

        if (huesoBrazos) { huesoBrazos.localPosition = p1_target; huesoBrazos.localRotation = r1_target; }
        if (huesoBrazos001) { huesoBrazos001.localPosition = p2_target; huesoBrazos001.localRotation = r2_target; }
    }

    void PlayRandomEnemyClip(int enemyIndex)
    {
        if (AudioSource == null) return;
        if (enemyAudioPools == null || enemyAudioPools.Length == 0) return;

        if (enemyIndex < 0 || enemyIndex >= enemyAudioPools.Length)
        {
            Debug.LogWarning($"Índice de enemigo {enemyIndex} fuera de rango para 'enemyAudioPools'.");
            return;
        }

        EnemyAudioPool pool = enemyAudioPools[enemyIndex];
        if (pool.clips == null || pool.clips.Length == 0)
        {
            Debug.LogWarning($"No hay clips de audio asignados para el enemigo {enemyIndex}.");
            return;
        }

        int randomIndex = Random.Range(0, pool.clips.Length);
        AudioClip clipToPlay = pool.clips[randomIndex];

        if (clipToPlay != null)
        {
            AudioSource.PlayOneShot(clipToPlay);
        }
    }


    void PlayAudioClip(AudioClip clip)
    {
        if (clip == null) return;
        
        // --- CAMBIO 3: Eliminar la comprobación 'if (!AudioSource.isPlaying)' ---
        // Ahora que usamos Stop() para dar prioridad, ya no necesitamos esta
        // comprobación, que era la que causaba el problema.
        AudioSource.PlayOneShot(clip);
    }

    float GetReactionTimeForEnemy(int enemyIndex)
    {
        if (reactionTimes == null || reactionTimes.Length < 6)
        {
            Debug.LogError("El array 'reactionTimes' no está configurado o no tiene suficientes valores (se necesitan al menos 6).");
            return 1.0f; // Valor por defecto
        }

        float minTime;
        float maxTime;

        switch (enemyIndex)
        {
            case 0:  // Enemigo Fácil (usa los índices 0 y 1)
                minTime = reactionTimes[0];
                maxTime = reactionTimes[1];
                break;

            case 1:  // Enemigo Intermedio (usa los índices 2 y 3)
                minTime = reactionTimes[2];
                maxTime = reactionTimes[3];
                break;

            case 2:  // Enemigo Difícil (usa los índices 4 y 5)
                minTime = reactionTimes[4];
                maxTime = reactionTimes[5];
                break;

            default:
                Debug.LogWarning($"Índice de enemigo no reconocido: {enemyIndex}. Usando valores por defecto.");
                minTime = 1.0f;
                maxTime = 1.0f;
                break;
        }
        
        return Random.Range(Mathf.Min(minTime, maxTime), Mathf.Max(minTime, maxTime));
    }
}


[System.Serializable]
public class EnemyAudioPool
{
    [Tooltip("Los 3 clips de audio para este enemigo")]
    public AudioClip[] clips;
}