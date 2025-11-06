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

    [Header("UI")]
    public GameObject uiPanelOponenteDerrotado;  // Panel que muestra el mensaje si el oponente ya ha sido derrotado
    public GameObject uiPanelOponenteNoDisponible;  // Panel que muestra el mensaje si el oponente no está disponible
    public GameObject uiPanelVictoria;
    public GameObject[] mesa;

    // --- CAMBIO: Reemplazamos los Transforms individuales por arrays ---
    // Asegúrate de que el tamaño de estos arrays coincida con el número de oponentes (ej: 3)
    // y que el orden sea el mismo que el del array 'mesa'.
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

    // --- CAMBIO: Guardaremos arrays de las posiciones originales ---
    private Quaternion[] rotOriginalBrazos_Array;
    private Vector3[] posOriginalBrazos_Array;
    private Quaternion[] rotOriginalBrazos001_Array;
    private Vector3[] posOriginalBrazos001_Array;

    // --- Variables de Estado (LAS POSICIONES TARGET SON LAS MISMAS PARA TODOS) ---
    private Vector3 targetPosBrazo_Success, targetPosBrazo001_Success; // Izquierda
    private Quaternion targetRotBrazo_Success, targetRotBrazo001_Success; // Izquierda
    private Vector3 targetPosBrazo_Fail, targetPosBrazo001_Fail; // Derecha
    private Quaternion targetRotBrazo_Fail, targetRotBrazo001_Fail; // Derecha

    private enum GameState { Countdown, Listening, Animating, Idle }
    private GameState currentState = GameState.Idle;

    // Referencia a la corutina del juego para poder pararla
    private Coroutine currentGameLoop = null;
    private bool isGameWon = false;

    void Start()
    {
        miGameLogic = FindAnyObjectByType<gameLogic>();

        // Activar giroscopio
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            gyroEnabled = true;
        }
        else
        {
            gyroEnabled = false;
        }

        // --- CAMBIO: Guardar Estado 1: REPOSO (Iterando sobre los arrays) ---
        if (huesosBrazos_Array != null && huesosBrazos001_Array != null &&
            huesosBrazos_Array.Length == huesosBrazos001_Array.Length)
        {
            int numArmSets = huesosBrazos_Array.Length;

            // Inicializar los arrays de estado
            posOriginalBrazos_Array = new Vector3[numArmSets];
            rotOriginalBrazos_Array = new Quaternion[numArmSets];
            posOriginalBrazos001_Array = new Vector3[numArmSets];
            rotOriginalBrazos001_Array = new Quaternion[numArmSets];

            // Rellenar los arrays con las posiciones/rotaciones originales
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


        // --- Definir Estado 2: ÉXITO (IZQUIERDA) ---
        // (Estos valores son constantes, como en tu script original)
        targetPosBrazo_Success = new Vector3(-1.862645e-08f, 0.01437014f, 9.220095e-08f);
        targetRotBrazo_Success = Quaternion.Euler(-26.949f, 79.839f, 61.002f);
        targetPosBrazo001_Success = new Vector3(-7.450581e-09f, 0.01437012f, 1.629815e-08f);
        targetRotBrazo001_Success = Quaternion.Euler(19.042f, -87.506f, 46.403f);

        // --- Definir Estado 3: FALLO (DERECHA) ---
        targetPosBrazo_Fail = new Vector3(-1.862645e-08f, 0.01437014f, 9.220095e-08f);
        targetRotBrazo_Fail = Quaternion.Euler(18.948f, -88.626f, 45.951f);
        targetPosBrazo001_Fail = new Vector3(-7.450581e-09f, 0.01437012f, 1.629815e-08f);
        targetRotBrazo001_Fail = Quaternion.Euler(-26.946f, 79.819f, 61.009f);

        // Ya NO iniciamos el bucle aquí. Esperamos la señal de Vuforia.
        if (rotationText) rotationText.text = "Apunta a la marca para empezar...";
    }

    void Update()
    {
        // El input se sigue detectando siempre,
        // pero solo importa cuando el estado es 'Listening'
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
            // Cuando Ganas UI
            mesa[miGameLogic.dataScript.GetCurrentOpponentIndex()].SetActive(false);
            uiPanelVictoria.SetActive(true);
        }
    }

    public void StartGameProcess()
    {
        // Solo iniciar si no está ya en marcha
        if (currentGameLoop == null)
        {
            if (rotationText) rotationText.text = "¡Marca detectada!";
            // Iniciar el bucle y guardar la referencia
            mesa[miGameLogic.dataScript.GetCurrentOpponentIndex()].SetActive(true);
            uiPanelVictoria.SetActive(false);
            currentGameLoop = StartCoroutine(GameLoop());
        }
    }

    public void startGame()
    {
        // Solo iniciar si no está ya en marcha
        if (currentGameLoop == null)
        {
            // Llamamos a CheckOpponentStatus para obtener el estado del oponente
            int status = miGameLogic.CheckOpponentStatus();
            Debug.Log(status);

            // Dependiendo del estado, ejecutamos la acción correspondiente
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
                    // Mostrar el panel de oponente derrotado
                    if (uiPanelOponenteDerrotado) uiPanelOponenteDerrotado.SetActive(true);
                    if (mesa[miGameLogic.dataScript.GetCurrentOpponentIndex()]) mesa[miGameLogic.dataScript.GetCurrentOpponentIndex()].SetActive(false);
                    break;

                case 2:  // Oponente aún no desbloqueado
                    if (rotationText) rotationText.text = "Este oponente aún no está disponible.";
                    // Mostrar el panel de oponente no disponible
                    if (uiPanelOponenteNoDisponible) uiPanelOponenteNoDisponible.SetActive(true);
                    if (mesa[miGameLogic.dataScript.GetCurrentOpponentIndex()]) mesa[miGameLogic.dataScript.GetCurrentOpponentIndex()].SetActive(false);
                    break;

                default:
                    break;
            }
        }
    }


    // --- NUEVA FUNCIÓN PÚBLICA (Llamada por Vuforia) ---
    public void StopGameProcess()
    {
        // Solo parar si se está ejecutando
        if (currentGameLoop != null)
        {
            StopCoroutine(currentGameLoop);
            currentGameLoop = null; // Borrar la referencia
            currentState = GameState.Idle;
            isAnimating = false;

            // --- CAMBIO: Obtener el índice actual para saber qué huesos resetear ---
            int currentIndex = miGameLogic.dataScript.GetCurrentOpponentIndex();

            // Forzar el reseteo de los brazos a su estado original
            StopAllCoroutines(); // Detener cualquier animación en curso (p.ej. DoRotation_Fail)

            // --- CAMBIO: Animar al estado original del índice actual ---
            if (currentIndex >= 0 && currentIndex < posOriginalBrazos_Array.Length)
            {
                StartCoroutine(AnimateToTarget(
                    currentIndex,
                    posOriginalBrazos_Array[currentIndex], rotOriginalBrazos_Array[currentIndex],
                    posOriginalBrazos001_Array[currentIndex], rotOriginalBrazos001_Array[currentIndex]
                ));
            }

            if (rotationText) rotationText.text = "Marca perdida. Apunta de nuevo...";
        }
    }

    // Coroutine principal que maneja el "3, 2, 1, Go!"
    IEnumerator GameLoop()
    {
        while (true) // Este bucle ahora se repite solo mientras la corutina esté activa
        {
            // --- CAMBIO: Obtener el índice del oponente actual ---
            int currentIndex = miGameLogic.dataScript.GetCurrentOpponentIndex();

            // Determinar el tiempo de reacción según el enemigo actual
            float tiempoReaccion = GetReactionTimeForEnemy(currentIndex);

            // --- 1. FASE DE ESPERA (IDLE) ---
            currentState = GameState.Idle;
            rotationText.text = "Prepárate...";
            yield return new WaitForSeconds(waitBeforeRestart);

            // --- 2. FASE DE CUENTA ATRÁS (COUNTDOWN) ---
            currentState = GameState.Countdown;
            rotationText.text = "3,2,1...";
            PlayAudioClip(countdownClip);
            yield return new WaitForSeconds(countdownClip.length);
            yield return new WaitForSeconds(Random.Range(randomWaitTimeMin, randomWaitTimeMax));

            // --- 3. FASE DE ESCUCHA (LISTENING) ---
            currentState = GameState.Listening;
            rotationText.text = "¡GO!";

            // Reproducir el sonido de "¡GO!".
            PlayAudioClip(goClip);

            bool success = false;
            float windowTimer = 0f;

            while (windowTimer < tiempoReaccion) // El tiempo de reacción cambia por enemigo
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
                // --- CAMBIO: Pasar el índice a la corutina de animación ---
                yield return StartCoroutine(DoRotation_Success(currentIndex));
                if (miGameLogic != null)
                {
                    miGameLogic.WinRound();
                }
            }
            else
            {
                rotationText.text = "¡FALLO!";
                // --- CAMBIO: Pasar el índice a la corutina de animación ---
                yield return StartCoroutine(DoRotation_Fail(currentIndex));
                if (miGameLogic != null)
                {
                    miGameLogic.LoseRound();
                }
            }
        }
    }

    // --- CAMBIO: La corutina ahora acepta un 'index' ---
    IEnumerator DoRotation_Success(int index)
    {
        isAnimating = true;
        // Animar a la posición de ÉXITO
        yield return AnimateToTarget(index, targetPosBrazo_Success, targetRotBrazo_Success, targetPosBrazo001_Success, targetRotBrazo001_Success);
        yield return new WaitForSeconds(0.1f);
        // Animar de vuelta al REPOSO original de ESE índice
        yield return AnimateToTarget(index, posOriginalBrazos_Array[index], rotOriginalBrazos_Array[index], posOriginalBrazos001_Array[index], rotOriginalBrazos001_Array[index]);
        isAnimating = false;
    }

    // --- CAMBIO: La corutina ahora acepta un 'index' ---
    IEnumerator DoRotation_Fail(int index)
    {
        isAnimating = true;
        // Animar a la posición de FALLO
        yield return AnimateToTarget(index, targetPosBrazo_Fail, targetRotBrazo_Fail, targetPosBrazo001_Fail, targetRotBrazo001_Fail);
        yield return new WaitForSeconds(0.1f);
        // Animar de vuelta al REPOSO original de ESE índice
        yield return AnimateToTarget(index, posOriginalBrazos_Array[index], rotOriginalBrazos_Array[index], posOriginalBrazos001_Array[index], rotOriginalBrazos001_Array[index]);
        isAnimating = false;
    }

    // --- CAMBIO: La corutina genérica ahora acepta un 'index' ---
    IEnumerator AnimateToTarget(int index, Vector3 p1_target, Quaternion r1_target, Vector3 p2_target, Quaternion r2_target)
    {
        // --- CAMBIO: Obtener los huesos correctos del array usando el índice ---
        if (index < 0 || index >= huesosBrazos_Array.Length)
        {
            Debug.LogError($"Índice de animación ({index}) fuera de rango.");
            yield break;
        }
        Transform huesoBrazos = huesosBrazos_Array[index];
        Transform huesoBrazos001 = huesosBrazos001_Array[index];

        // --- El resto de la lógica es idéntica a tu script original ---
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

    void PlayAudioClip(AudioClip clip)
    {
        if (!AudioSource.isPlaying)
        {
            AudioSource.PlayOneShot(clip);
        }
    }

    float GetReactionTimeForEnemy(int enemyIndex)
    {
        // --- Comprobación de seguridad ---
        // Asegurarse de que el array tenga suficientes elementos para tu lógica (al menos 6 en este caso)
        if (reactionTimes == null || reactionTimes.Length < 6)
        {
            Debug.LogError("El array 'reactionTimes' no está configurado o no tiene suficientes valores (se necesitan al menos 6).");
            return 1.0f; // Valor por defecto
        }

        float minTime;
        float maxTime;

        // Usamos un switch para asignar el rango según el índice del enemigo
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
                // Un "fallback" por si el índice es inesperado (ej: 3 o más)
                Debug.LogWarning($"Índice de enemigo no reconocido: {enemyIndex}. Usando valores por defecto.");
                minTime = 1.0f;
                maxTime = 1.0f;
                break;
        }

        // --- Devolver el valor aleatorio ---

        // Random.Range(float, float) devuelve un valor aleatorio entre los dos números.
        // Usamos Mathf.Min y Mathf.Max para asegurarnos de que funciona 
        // incluso si pones el valor más alto primero en el array.
        return Random.Range(Mathf.Min(minTime, maxTime), Mathf.Max(minTime, maxTime));
    }
}