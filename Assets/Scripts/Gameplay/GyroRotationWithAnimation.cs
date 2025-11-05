using UnityEngine;
using TMPro;
using System.Collections; // Necesario para las Coroutines

public class GyroRotationWithAnimation : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text rotationText;

    [Header("Huesos a Rotar")]
    public Transform huesoBrazos; // 'bone_forearm_D' de 'brazos'
    public Transform huesoBrazos001; // 'bone_forearm_D' de 'brazos.001'

    [Header("Configuración de Animación")]
    public float duracionAnimacion = 0.25f; // Duración para la anim. de TECLADO
    public float gyroSmoothSpeed = 5.0f; // Velocidad de suavizado para el GIROSCOPIO

    [Header("Configuración Giroscopio")]
    public float rotationThreshold = 30f;
    public float cooldownTime = 1f; // Cooldown para la anim. de TECLADO

    private bool gyroEnabled;
    private float lastTriggerTime;
    private bool isRotating = false; // Flag para la anim. de TECLADO

    // --- Variables de Estado (Guardaremos las 3 posiciones) ---

    // 1. Reposo (Estable)
    private Quaternion rotOriginalBrazos;
    private Quaternion rotOriginalBrazos001;
    private Vector3 posOriginalBrazos;
    private Vector3 posOriginalBrazos001;

    // 2. Objetivo DERECHA
    private Vector3 targetPosBrazo_Right, targetPosBrazo001_Right;
    private Quaternion targetRotBrazo_Right, targetRotBrazo001_Right;

    // 3. Objetivo IZQUIERDA
    private Vector3 targetPosBrazo_Left, targetPosBrazo001_Left;
    private Quaternion targetRotBrazo_Left, targetRotBrazo001_Left;


    void Start()
    {
        // Activar giroscopio
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            gyroEnabled = true;
            if (rotationText) rotationText.text = "📱 Giroscopio 3-estados";
        }
        else
        {
            gyroEnabled = false;
            if (rotationText) rotationText.text = "🧩 Modo de prueba (Teclado)";
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

        // --- Definir Estado 2: DERECHA ---
        targetPosBrazo_Right = new Vector3(-1.862645e-08f, 0.01437014f, 9.220095e-08f);
        targetRotBrazo_Right = Quaternion.Euler(18.948f, -88.626f, 45.951f);
        targetPosBrazo001_Right = new Vector3(-7.450581e-09f, 0.01437012f, 1.629815e-08f);
        targetRotBrazo001_Right = Quaternion.Euler(-26.946f, 79.819f, 61.009f);

        // --- Definir Estado 3: IZQUIERDA ---
        targetPosBrazo_Left = new Vector3(-1.862645e-08f, 0.01437014f, 9.220095e-08f);
        targetRotBrazo_Left = Quaternion.Euler(-26.949f, 79.839f, 61.002f);
        targetPosBrazo001_Left = new Vector3(-7.450581e-09f, 0.01437012f, 1.629815e-08f);
        targetRotBrazo001_Left = Quaternion.Euler(19.042f, -87.506f, 46.403f);
    }

    void Update()
    {
        if (gyroEnabled)
        {
            // --- MODO GIROSCOPIO (3 ESTADOS: Izquierda, Derecha, Estable) ---
            UpdateGyroscopeState();
        }
        else
        {
            // --- MODO TECLADO (Animación de Ida y Vuelta) ---
            if (isRotating) return; // No detectar input si la anim. (coroutine) se está ejecutando
            HandleKeyboardInput();
        }
    }

    // --- NUEVA FUNCIÓN ---
    // Esta función se ejecuta en cada frame si el giroscopio está activo
    void UpdateGyroscopeState()
    {
        float rotationSpeedZ = Input.gyro.rotationRateUnbiased.z * Mathf.Rad2Deg;

        // 1. Determinar el estado objetivo (Izquierda, Derecha, o Estable)
        Vector3 targetPosBrazo, targetPosBrazo001;
        Quaternion targetRotBrazo, targetRotBrazo001;

        if (rotationSpeedZ > rotationThreshold) // IZQUIERDA
        {
            targetPosBrazo = targetPosBrazo_Left;
            targetRotBrazo = targetRotBrazo_Left;
            targetPosBrazo001 = targetPosBrazo001_Left;
            targetRotBrazo001 = targetRotBrazo001_Left;
            if (rotationText) rotationText.text = "📱 Estado: IZQUIERDA";
        }
        else if (rotationSpeedZ < -rotationThreshold) // DERECHA
        {
            targetPosBrazo = targetPosBrazo_Right;
            targetRotBrazo = targetRotBrazo_Right;
            targetPosBrazo001 = targetPosBrazo001_Right;
            targetRotBrazo001 = targetRotBrazo001_Right;
            if (rotationText) rotationText.text = "📱 Estado: DERECHA";
        }
        else // ESTABLE
        {
            targetPosBrazo = posOriginalBrazos;
            targetRotBrazo = rotOriginalBrazos;
            targetPosBrazo001 = posOriginalBrazos001;
            targetRotBrazo001 = rotOriginalBrazos001;
            if (rotationText) rotationText.text = "📱 Estado: ESTABLE";
        }

        // 2. Moverse suavemente (Slerp/Lerp) al estado objetivo
        // Usamos Time.deltaTime * gyroSmoothSpeed para un movimiento fluido
        float smoothFactor = Time.deltaTime * gyroSmoothSpeed;

        if (huesoBrazos)
        {
            huesoBrazos.localPosition = Vector3.Lerp(huesoBrazos.localPosition, targetPosBrazo, smoothFactor);
            huesoBrazos.localRotation = Quaternion.Slerp(huesoBrazos.localRotation, targetRotBrazo, smoothFactor);
        }
        if (huesoBrazos001)
        {
            huesoBrazos001.localPosition = Vector3.Lerp(huesoBrazos001.localPosition, targetPosBrazo001, smoothFactor);
            huesoBrazos001.localRotation = Quaternion.Slerp(huesoBrazos001.localRotation, targetRotBrazo001, smoothFactor);
        }
    }


    // =================================================================
    // --- LÓGICA DE TECLADO (Se mantiene igual que antes) ---
    // =================================================================

    void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.A)) // Izquierda
        {
            TriggerRotation("IZQUIERDA");
        }

        if (Input.GetKeyDown(KeyCode.D)) // Derecha
        {
            TriggerRotation("DERECHA");
        }
    }

    void TriggerRotation(string direction)
    {
        if (Time.time - lastTriggerTime < cooldownTime)
            return;
        lastTriggerTime = Time.time;

        if (rotationText != null)
        {
            rotationText.text = $"🎬 Rotando (teclado) hacia {direction}";
            rotationText.color = direction.Contains("IZQUIERDA") ? Color.red : Color.blue;
        }

        // Iniciamos la Coroutine que hace la animación de ida Y VUELTA
        StartCoroutine(DoRotation_Keyboard(direction));

        Debug.Log($"🎮 Rotación de teclado activada → {direction}");
    }

    // Renombramos esta Coroutine para que sea claro que es solo para el teclado
    IEnumerator DoRotation_Keyboard(string direction)
    {
        isRotating = true; // Bloqueamos el input de teclado

        // --- Definimos los valores OBJETIVO ---
        Vector3 targetPosBrazo, targetPosBrazo001;
        Quaternion targetRotBrazo, targetRotBrazo001;

        if (direction.Contains("DERECHA"))
        {
            targetPosBrazo = targetPosBrazo_Right;
            targetRotBrazo = targetRotBrazo_Right;
            targetPosBrazo001 = targetPosBrazo001_Right;
            targetRotBrazo001 = targetRotBrazo001_Right;
        }
        else // "IZQUIERDA"
        {
            targetPosBrazo = targetPosBrazo_Left;
            targetRotBrazo = targetRotBrazo_Left;
            targetPosBrazo001 = targetPosBrazo001_Left;
            targetRotBrazo001 = targetRotBrazo001_Left;
        }

        // --- Bucle de Animación DE IDA (Hacia el objetivo) ---
        float tiempoPasado = 0;
        while (tiempoPasado < duracionAnimacion)
        {
            float t = tiempoPasado / duracionAnimacion;

            if (huesoBrazos)
            {
                huesoBrazos.localPosition = Vector3.Lerp(posOriginalBrazos, targetPosBrazo, t);
                huesoBrazos.localRotation = Quaternion.Slerp(rotOriginalBrazos, targetRotBrazo, t);
            }
            if (huesoBrazos001)
            {
                huesoBrazos001.localPosition = Vector3.Lerp(posOriginalBrazos001, targetPosBrazo001, t);
                huesoBrazos001.localRotation = Quaternion.Slerp(rotOriginalBrazos001, targetRotBrazo001, t);
            }

            tiempoPasado += Time.deltaTime;
            yield return null;
        }

        // Asegurar que llega al final
        if (huesoBrazos) { huesoBrazos.localPosition = targetPosBrazo; huesoBrazos.localRotation = targetRotBrazo; }
        if (huesoBrazos001) { huesoBrazos001.localPosition = targetPosBrazo001; huesoBrazos001.localRotation = targetRotBrazo001; }

        yield return new WaitForSeconds(0.1f); // Pequeña pausa

        // --- Bucle de Animación DE VUELTA (Hacia el reposo) ---
        tiempoPasado = 0;
        while (tiempoPasado < duracionAnimacion)
        {
            float t = tiempoPasado / duracionAnimacion;

            if (huesoBrazos)
            {
                huesoBrazos.localPosition = Vector3.Lerp(targetPosBrazo, posOriginalBrazos, t);
                huesoBrazos.localRotation = Quaternion.Slerp(targetRotBrazo, rotOriginalBrazos, t);
            }
            if (huesoBrazos001)
            {
                huesoBrazos001.localPosition = Vector3.Lerp(targetPosBrazo001, posOriginalBrazos001, t);
                huesoBrazos001.localRotation = Quaternion.Slerp(targetRotBrazo001, rotOriginalBrazos001, t);
            }

            tiempoPasado += Time.deltaTime;
            yield return null;
        }

        // Asegurar que vuelve al reposo
        if (huesoBrazos) { huesoBrazos.localPosition = posOriginalBrazos; huesoBrazos.localRotation = rotOriginalBrazos; }
        if (huesoBrazos001) { huesoBrazos001.localPosition = posOriginalBrazos001; huesoBrazos001.localRotation = rotOriginalBrazos001; }

        isRotating = false; // Desbloqueamos el input de teclado
    }
}