using UnityEngine;
using UnityEngine.UI; // Asegúrate de tener esta línea para el Slider

public class volumeManager : MonoBehaviour
{
    [Header("UI")]
    public Slider volumeSlider;

    // Esta es la clave donde guardaremos el volumen
    private const string VOLUME_KEY = "MasterVolume";

    void Start()
    {
        // 1. Cargar el volumen guardado. Si no hay nada, usa 1.0 (máximo)
        float savedVolume = PlayerPrefs.GetFloat(VOLUME_KEY, 1.0f);

        // 2. Aplicar ese volumen a todo el juego
        SetMasterVolume(savedVolume);

        // 3. Poner el slider en la posición correcta
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;

            // 4. Añadir el "listener" para que la función se llame cuando muevas el slider
            volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
        }
    }

    // Esta función se llama CADA VEZ que el usuario mueve el slider
    public void OnVolumeSliderChanged(float value)
    {
        SetMasterVolume(value);
    }

    // Función central para aplicar y guardar el volumen
    private void SetMasterVolume(float value)
    {
        // 1. Aplicar el valor al "oído" principal del juego
        // Esto afecta a TODOS los AudioSources
        AudioListener.volume = value;

        // 2. Guardar el valor en el dispositivo
        PlayerPrefs.SetFloat(VOLUME_KEY, value);
        PlayerPrefs.Save(); // Forzar el guardado

        Debug.Log($"Volumen maestro guardado y aplicado: {value}");
    }
}