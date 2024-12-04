using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ReelsManager : MonoBehaviour
{
    public Button slideLeftButton;
    public Button slideRightButton;
    public VideoGallery videoGalleryScript;

    [SerializeField] private List<VideoPlayer> ReelsVideoPlayerList;
    [SerializeField] private List<Transform> slotsList;
    [SerializeField] private float slideDuration = 0.5f;

    private List<VideoMetadata> videoList;
    private int currentStartIndex = 0;

    private void Start()
    {
        slideLeftButton.onClick.AddListener(() => StartCoroutine(SlideLeft()));
        slideRightButton.onClick.AddListener(() => StartCoroutine(SlideRight()));

        GetVideoList();
        if (videoList != null && videoList.Count > 0)
        {
            PopulateReels();
        }
        Invoke("PauseAllVideos", 0.2f);
    }

    /// <summary>
    /// Gets the video metadata list from the VideoGallery script.
    /// </summary>
    public void GetVideoList()
    {
        videoList = videoGalleryScript.GetVideoMetadataList();
    }

    /// <summary>
    /// Populates the video players with the appropriate video metadata.
    /// </summary>
    private void PopulateReels()
    {
        for (int i = 0; i < ReelsVideoPlayerList.Count; i++)
        {
            int videoIndex = (currentStartIndex + i) % videoList.Count;
            ReelsVideoPlayerList[i].url = videoList[videoIndex].filePath;
            ReelsVideoPlayerList[i].Prepare();
        }
        PauseAllVideos();
    }

    /// <summary>
    /// Slides the video reels to the left.
    /// </summary>
    private IEnumerator SlideLeft()
    {
        PauseAllVideos();

        currentStartIndex = (currentStartIndex - 1 + videoList.Count) % videoList.Count;

        ReelsVideoPlayerList[0].transform.position = slotsList[2].position;

        Vector3 startPosition1 = ReelsVideoPlayerList[1].transform.position;
        Vector3 startPosition2 = ReelsVideoPlayerList[2].transform.position;

        Vector3 targetPosition1 = slotsList[0].position;
        Vector3 targetPosition2 = slotsList[1].position;

        float elapsedTime = 0;
        while (elapsedTime < slideDuration)
        {
            ReelsVideoPlayerList[1].transform.position = Vector3.Lerp(startPosition1, targetPosition1, elapsedTime / slideDuration);
            ReelsVideoPlayerList[2].transform.position = Vector3.Lerp(startPosition2, targetPosition2, elapsedTime / slideDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ReelsVideoPlayerList[1].transform.position = targetPosition1;
        ReelsVideoPlayerList[2].transform.position = targetPosition2;

        VideoPlayer tempPlayer = ReelsVideoPlayerList[0];
        ReelsVideoPlayerList[0] = ReelsVideoPlayerList[1];
        ReelsVideoPlayerList[1] = ReelsVideoPlayerList[2];
        ReelsVideoPlayerList[2] = tempPlayer;

        int newVideoIndex = (currentStartIndex + 2) % videoList.Count;
        ReelsVideoPlayerList[2].url = videoList[newVideoIndex].filePath;
        ReelsVideoPlayerList[2].Prepare();

        PauseAllVideos();
    }

    /// <summary>
    /// Slides the video reels to the right.
    /// </summary>
    private IEnumerator SlideRight()
    {
        PauseAllVideos();

        currentStartIndex = (currentStartIndex + 1) % videoList.Count;

        ReelsVideoPlayerList[2].transform.position = slotsList[0].position;

        Vector3 startPosition0 = ReelsVideoPlayerList[0].transform.position;
        Vector3 startPosition1 = ReelsVideoPlayerList[1].transform.position;

        Vector3 targetPosition0 = slotsList[1].position;
        Vector3 targetPosition1 = slotsList[2].position;

        float elapsedTime = 0;
        while (elapsedTime < slideDuration)
        {
            ReelsVideoPlayerList[0].transform.position = Vector3.Lerp(startPosition0, targetPosition0, elapsedTime / slideDuration);
            ReelsVideoPlayerList[1].transform.position = Vector3.Lerp(startPosition1, targetPosition1, elapsedTime / slideDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ReelsVideoPlayerList[0].transform.position = targetPosition0;
        ReelsVideoPlayerList[1].transform.position = targetPosition1;

        VideoPlayer tempPlayer = ReelsVideoPlayerList[2];
        ReelsVideoPlayerList[2] = ReelsVideoPlayerList[1];
        ReelsVideoPlayerList[1] = ReelsVideoPlayerList[0];
        ReelsVideoPlayerList[0] = tempPlayer;

        int newVideoIndex = currentStartIndex % videoList.Count;
        ReelsVideoPlayerList[0].url = videoList[newVideoIndex].filePath;
        ReelsVideoPlayerList[0].Prepare();

        PauseAllVideos();
    }

    /// <summary>
    /// Plays or pauses the middle video in the reels.
    /// </summary>
    public void PlayPauseVideo(bool shouldPlay)
    {
        if (ReelsVideoPlayerList != null && ReelsVideoPlayerList.Count > 1)
        {
            VideoPlayer middleVideoPlayer = ReelsVideoPlayerList[1];

            if (middleVideoPlayer != null)
            {
                if (shouldPlay)
                {
                    middleVideoPlayer.Play();
                }
                else
                {
                    middleVideoPlayer.Pause();
                }
            }
            else
            {
                Debug.LogError("Middle Video Player is not assigned.");
            }
        }
        else
        {
            Debug.LogError("ReelsVideoPlayerList is not set properly or doesn't have enough video players.");
        }
    }

    /// <summary>
    /// Toggles the play/pause state of the middle video in the reels.
    /// </summary>
    public void ToggleVideo()
    {
        VideoPlayer middleVideoPlayer = ReelsVideoPlayerList[1];

        if (middleVideoPlayer != null)
        {
            if (middleVideoPlayer.isPlaying)
            {
                middleVideoPlayer.Pause();
            }
            else
            {
                middleVideoPlayer.Play();
            }
        }
        else
        {
            Debug.LogError("Middle Video Player is not assigned.");
        }
    }

    /// <summary>
    /// Pauses all videos in the reels.
    /// </summary>
    private void PauseAllVideos()
    {
        foreach (VideoPlayer videoPlayer in ReelsVideoPlayerList)
        {
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
            }
        }
    }
}
