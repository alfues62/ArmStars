using UnityEngine;
using TMPro;
using System.Collections;

public class GyroRotationWithAnimation : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text rotationText; // Para mostrar el 3, 2, 1, Go!

    [Header("Huesos a Rotar")]
    public Transform huesoBrazos;
    public Transform huesoBrazos001;

    [Header("Configuración del Juego")]
    public float successWindowDuration = 0.5f; // Ventana de 0.5 seg para acertar
    public float rotationThreshold = 30f; // Sensibilidad del giro
    public float duracionAnimacion = 0.25f; // Duración de la anim (ida)
    public float waitBeforeRestart = 2.0f; // Tiempo de espera antes de reiniciar

    private bool gyroEnabled;
    private bool isAnimating = false;
    private bool playerTiltedLeft = false;

    // --- Variables de Estado (Guardaremos las 3 posiciones) ---
    private Quaternion rotOriginalBrazos, rotOriginalBrazos001;
    private Vector3 posOriginalBrazos, posOriginalBrazos001;
    private Vector3 targetPosBrazo_Success, targetPosBrazo001_Success; // Izquierda
    private Quaternion targetRotBrazo_Success, targetRotBrazo001_Success; // Izquierda
    private Vector3 targetPosBrazo_Fail, targetPosBrazo001_Fail; // Derecha
    private Quaternion targetRotBrazo_Fail, targetRotBrazo001_Fail; // Derecha

    private enum GameState { Countdown, Listening, Animating, Idle }
    private GameState currentState = GameState.Idle;

    // Referencia a la corutina del juego para poder pararla
    private Coroutine currentGameLoop = null;

    void Start()
    {
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

        // --- Guardar Estado 1: REPOSO (Estable) ---
        if (huesoBrazos)
        {
            posOriginalBrazos = huesoBrazos.localPosition;
            rotOriginalBrazos = huesoBrazos.localRotation;
        }
        if (huesoBrazos001)
        {
            posOriginalBrazos001 = huesoBrazos001.localPosition;
            rotOriginalBrazos001 = huesoBrazos001.localRotation;
        }

        // --- Definir Estado 2: ÉXITO (IZQUIERDA) ---
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
    }

    // --- NUEVA FUNCIÓN PÚBLICA (Llamada por Vuforia) ---
    public void StartGameProcess()
    {
        // Solo iniciar si no está ya en marcha
        if (currentGameLoop == null)
        {
            if (rotationText) rotationText.text = "¡Marca detectada!";
            // Iniciar el bucle y guardar la referencia
            currentGameLoop = StartCoroutine(GameLoop());
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

            // Forzar el reseteo de los brazos a su estado original
            StopAllCoroutines(); // Detener cualquier animación en curso (p.ej. DoRotation_Fail)
            StartCoroutine(AnimateToTarget(posOriginalBrazos, rotOriginalBrazos, posOriginalBrazos001, rotOriginalBrazos001));

            if (rotationText) rotationText.text = "Marca perdida. Apunta de nuevo...";
        }
    }

    // Coroutine principal que maneja el "3, 2, 1, Go!"
    IEnumerator GameLoop()
    {
        while (true) // Este bucle ahora se repite solo mientras la corutina esté activa
        {
            // --- 1. FASE DE ESPERA (IDLE) ---
            currentState = GameState.Idle;
            rotationText.text = "Prepárate...";
            yield return new WaitForSeconds(waitBeforeRestart);

            // --- 2. FASE DE CUENTA ATRÁS (COUNTDOWN) ---
            currentState = GameState.Countdown;
            rotationText.text = "3";
            yield return new WaitForSeconds(1.0f);
            rotationText.text = "2";
            yield return new WaitForSeconds(1.0f);
            rotationText.text = "1";
            yield return new WaitForSeconds(1.0f);

            // --- 3. FASE DE ESCUCHA (LISTENING) ---
            currentState = GameState.Listening;
            rotationText.text = "¡GO!";
            bool success = false;
            float windowTimer = 0f;

            while (windowTimer < successWindowDuration)
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
                yield return StartCoroutine(DoRotation_Success());
            }
            else
            {
                rotationText.text = "¡FALLO!";
                yield return StartCoroutine(DoRotation_Fail());
            }
            // (La corutina de animación ya pone isAnimating en false al acabar)
        }
    }

    // --- Coroutine para la animación de ÉXITO (IZQUIERDA) ---
    IEnumerator DoRotation_Success()
    {
        isAnimating = true;
        yield return AnimateToTarget(targetPosBrazo_Success, targetRotBrazo_Success, targetPosBrazo001_Success, targetRotBrazo001_Success);
        yield return new WaitForSeconds(0.1f);
        yield return AnimateToTarget(posOriginalBrazos, rotOriginalBrazos, posOriginalBrazos001, rotOriginalBrazos001);
        isAnimating = false;
    }

    // --- Coroutine para la animación de FALLO (DERECHA) ---
    IEnumerator DoRotation_Fail()
    {
        isAnimating = true;
        yield return AnimateToTarget(targetPosBrazo_Fail, targetRotBrazo_Fail, targetPosBrazo001_Fail, targetRotBrazo001_Fail);
        yield return new WaitForSeconds(0.1f);
        yield return AnimateToTarget(posOriginalBrazos, rotOriginalBrazos, posOriginalBrazos001, rotOriginalBrazos001);
        isAnimating = false;
    }

    // --- Coroutine genérica que hace la animación (para no repetir código) ---
    IEnumerator AnimateToTarget(Vector3 p1_target, Quaternion r1_target, Vector3 p2_target, Quaternion r2_target)
    {
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
}