using UnityEngine;
using UnityEngine.UI;
using VideoKit;
using TMPro;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Video;

public class ToggleRecordAR : MonoBehaviour
{
    [Header("UI Elements")]
    public Button recordToggleButton;
    public GameObject recordStatusIcon;
    public TextMeshProUGUI buttonText;

    [Header("Recorder Component")]
    public VideoKitRecorder videoKitRecorder;

    public VideoGallery videoGallery;

    private bool isRecording = false;

    void Start()
    {
        buttonText.text = "Start Recording";

        recordToggleButton.onClick.AddListener(ToggleRecording);

        if (videoKitRecorder == null)
        {
            Debug.LogError("VideoKitRecorder is not assigned.");
            buttonText.text = "Recorder Not Assigned";
            return;
        }

        Debug.Log("Initialization complete. Ready to record.");
        buttonText.text = "Start Recording";
    }

    /// <summary>
    /// Toggles the recording state between start and stop.
    /// </summary>
    void ToggleRecording()
    {
        if (videoKitRecorder == null)
        {
            Debug.LogError("VideoKitRecorder is not assigned.");
            return;
        }

        if (isRecording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
        videoGallery.PopulateGallery();
    }

    /// <summary>
    /// Starts the recording process.
    /// </summary>
    private void StartRecording()
    {
        try
        {
            videoKitRecorder.StartRecording();
            isRecording = true;
            buttonText.text = "Stop Recording";
            recordStatusIcon.SetActive(true);
            Debug.Log("Recording Started");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error starting recording: " + e.Message);
        }
    }

    /// <summary>
    /// Stops the recording process.
    /// </summary>
    private void StopRecording()
    {
        try
        {
            videoKitRecorder.StopRecording();
            isRecording = false;
            buttonText.text = "Start Recording";
            recordStatusIcon.SetActive(false);
            Debug.Log("Recording Stopped");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error stopping recording: " + e.Message);
        }
    }
}
