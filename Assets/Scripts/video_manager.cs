using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.VisualScripting;

[RequireComponent(typeof(UnityEngine.Video.VideoPlayer))]
[RequireComponent(typeof(AudioSource))]
public class VideoManager : MonoBehaviour
{
    // ───────── Components ─────────
    private UnityEngine.Video.VideoPlayer videoPlayer;
    private AudioSource audioSource;

    // ───────── UI References ─────────
    [Header("UI")]
    public Slider timeSlider;
    public TMP_Text timeText;
    public Slider volumeSlider;

    [Header("Play/Pause Button")]
    public Image playPauseButtonImage;
    public Sprite playSprite;
    public Sprite pauseSprite;

    // ───────── Playback Settings ─────────
    [Header("Playback Controls")]
    [Tooltip("Number of seconds to skip forward/backward")]
    public float skipSeconds = 5f;

    // ───────── States ─────────
    private bool isDragging = false;
    private bool isPrepared = false;
    private bool isPlaying = false;
    private bool isVolumeVisible = false;

    //─────────────────────────────────────────────
    // Initialization
    //─────────────────────────────────────────────
    void Start()
    {
        videoPlayer = GetComponent<UnityEngine.Video.VideoPlayer>();
        audioSource = GetComponent<AudioSource>();

        videoPlayer.audioOutputMode = UnityEngine.Video.VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();

        timeSlider.onValueChanged.AddListener(OnSliderValueChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);

        volumeSlider.gameObject.SetActive(false);

        audioSource.volume = volumeSlider.value;
        UpdatePlayPauseSprite(true);

    }

    void OnVideoPrepared(UnityEngine.Video.VideoPlayer vp)
    {
        isPrepared = true;
    }

    //─────────────────────────────────────────────
    // Update Loop
    //─────────────────────────────────────────────
    void Update()
    {
        if (!isPrepared || isDragging || videoPlayer.length <= 0) return;

        timeSlider.value = (float)(videoPlayer.time / videoPlayer.length);
        timeText.text = $"{FormatTime(videoPlayer.time)} / {FormatTime(videoPlayer.length)}";
    }

    private string FormatTime(double time)
    {
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        return $"{minutes:D2}:{seconds:D2}";
    }

    //─────────────────────────────────────────────
    // Time Slider
    //─────────────────────────────────────────────
    void OnSliderValueChanged(float value)
    {
        if (!isDragging || !isPrepared) return;

        double previewTime = value * videoPlayer.length;
        timeText.text = $"{FormatTime(previewTime)} / {FormatTime(videoPlayer.length)}";
    }

    public void OnPointerDown() => isDragging = true;

    public void OnPointerUp()
    {
        if (!isPrepared || videoPlayer.length <= 0) return;

        videoPlayer.time = timeSlider.value * videoPlayer.length;
        isDragging = false;
    }

    //─────────────────────────────────────────────
    // Play / Pause / Stop
    //─────────────────────────────────────────────
    public void TogglePlayPause()
    {
        if (!isPrepared) return;

        if (isPlaying)
        {
            videoPlayer.Pause();
        }
        else
        {
            videoPlayer.Play();
        }

        isPlaying = !isPlaying;
        UpdatePlayPauseSprite(isPlaying);
    }

    private void UpdatePlayPauseSprite(bool playing)
    {
        if (playPauseButtonImage == null) return;

        playPauseButtonImage.sprite = playing ? pauseSprite : playSprite;
    }

    //─────────────────────────────────────────────
    // Skip Forward / Backward
    //─────────────────────────────────────────────
    public void SkipForward()
    {
        if (!isPrepared || videoPlayer.length <= 0) return;
        videoPlayer.time = Mathf.Min((float)(videoPlayer.time + skipSeconds), (float)videoPlayer.length);
    }

    public void SkipBackward()
    {
        if (!isPrepared || videoPlayer.length <= 0) return;
        videoPlayer.time = Mathf.Max((float)(videoPlayer.time - skipSeconds), 0f);
    }

    //─────────────────────────────────────────────
    // Volume Control
    //─────────────────────────────────────────────
    void OnVolumeSliderChanged(float value)
    {
        audioSource.volume = volumeSlider.value;
        Debug.Log($"Volume set to: {audioSource.volume}");
        Debug.Log($"Volume Slider Value: {volumeSlider.value}");

    }

    //─────────────────────────────────────────────
    // Toggle Volume Slider Visibility
    //─────────────────────────────────────────────
    public void ToggleVolumeSlider()
    {
        if (volumeSlider == null) return;

        isVolumeVisible = !isVolumeVisible;
        volumeSlider.gameObject.SetActive(isVolumeVisible);
    }

    //─────────────────────────────────────────────
    // Restart Video
    //─────────────────────────────────────────────
    public void RestartVideo()
    {
        if (!isPrepared) return;

        videoPlayer.time = 0;
        videoPlayer.Play();
        isPlaying = true;
        UpdatePlayPauseSprite(true);
    }
}
