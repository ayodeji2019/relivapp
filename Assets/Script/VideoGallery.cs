using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections;

public class VideoGallery : MonoBehaviour
{
    [Header("Prefabs and Parent Object")]
    public GameObject memoryClipPrefab;
    public Transform galleryParent;
    public MemoryClipPool memoryClipPool;

    [Header("Video Player Setup")]
    public VideoPlayer videoPlayer;

    public GameObject galleryMenu;
    public GameObject reelsMenu;

    public TextMeshProUGUI debugText;

    private string recordingsPath;
    private string metadataPath;
    private List<VideoMetadata> videoMetadataList;

    private void Awake()
    {
        recordingsPath = $"{Application.persistentDataPath}/recordings";
        metadataPath = $"{Application.persistentDataPath}/videoMetadata.json";
        videoMetadataList = LoadMetadata();
        Debug.Log($"Awake: Loaded {videoMetadataList.Count} videos from metadata.");
        PopulateGallery();
    }

    /// <summary>
    /// Returns the list of video metadata.
    /// </summary>
    public List<VideoMetadata> GetVideoMetadataList()
    {
        return videoMetadataList;
    }

    /// <summary>
    /// Populates the gallery with video clips based on the available metadata.
    /// </summary>
    public void PopulateGallery()
    {
        Debug.Log("Populating the gallery...");
        // Return all current memory clips to the pool before populating the gallery
        List<GameObject> memoryClipsToReturn = new List<GameObject>();

        foreach (Transform memory in galleryParent)
        {
            memoryClipsToReturn.Add(memory.gameObject);
        }

        foreach (GameObject memoryClip in memoryClipsToReturn)
        {
            Debug.Log($"Returning memory clip '{memoryClip.name}' to pool.");
            memoryClipPool.ReturnToPool(memoryClip);
        }

        // Load existing metadata from file
        videoMetadataList = LoadMetadata();
        Debug.Log($"Loaded {videoMetadataList.Count} metadata items from file.");

        // Always check the directory for new videos
        if (Directory.Exists(recordingsPath))
        {
            string[] videoFiles = Directory.GetFiles(recordingsPath, "*.mp4");
            Debug.Log($"Found {videoFiles.Length} video files in directory '{recordingsPath}'.");

            // Track deleted videos: Remove metadata for videos that no longer exist
            videoMetadataList.RemoveAll(metadata =>
            {
                bool fileExists = File.Exists(metadata.filePath);
                if (!fileExists)
                {
                    Debug.LogWarning($"Video file '{metadata.fileName}' no longer exists. Removing from metadata.");
                }
                return !fileExists;
            });

            // Add new videos to the metadata list if they aren't already there
            foreach (string videoFilePath in videoFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(videoFilePath);

                // Check if the video is already in the metadata list
                bool alreadyAdded = videoMetadataList.Exists(metadata => metadata.fileName == fileName);
                Debug.Log(alreadyAdded ? $"Video '{fileName}' already exists in metadata." : $"Adding new video '{fileName}' to metadata.");

                if (!alreadyAdded)
                {
                    VideoMetadata metadata = new VideoMetadata
                    {
                        fileName = fileName,
                        filePath = videoFilePath
                    };
                    videoMetadataList.Add(metadata);
                }
            }

            // Save updated metadata back to the file
            SaveMetadata(videoMetadataList);
        }
        else
        {
            string errorMsg = "Directory does not exist: " + recordingsPath;
            Debug.LogError(errorMsg);
            if (debugText != null) debugText.text = errorMsg;
            return;
        }

        // Now populate the gallery with the metadata
        foreach (VideoMetadata metadata in videoMetadataList)
        {
            // Check if the memory clip already exists, to avoid duplication
            if (galleryParent.Find(metadata.fileName) == null)
            {
                Debug.Log($"Creating memory clip for video '{metadata.fileName}'.");
                GameObject memoryClip = memoryClipPool.GetPooledObject(galleryParent); // Pass galleryParent as parent

                if (memoryClip == null)
                {
                    Debug.LogError("Memory clip could not be obtained from pool!");
                    continue;
                }

                memoryClip.name = metadata.fileName; // Set name to avoid duplicate entries

                Button button = memoryClip.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => OpenReels(metadata.filePath));
                    Debug.Log($"Assigned button click event for '{metadata.fileName}'.");
                }
                else
                {
                    Debug.LogError($"Button component not found for memory clip '{metadata.fileName}'.");
                }

                VideoPlayer thumbnailVideoPlayer = memoryClip.transform.GetChild(0).GetComponent<VideoPlayer>();
                RawImage thumbnailRawImage = memoryClip.transform.GetChild(0).GetComponent<RawImage>();

                if (thumbnailVideoPlayer != null && thumbnailRawImage != null)
                {
                    RenderTexture renderTexture = new RenderTexture(1920, 1080, 16);
                    thumbnailVideoPlayer.targetTexture = renderTexture;
                    thumbnailRawImage.texture = renderTexture;
                    thumbnailVideoPlayer.url = metadata.filePath;
                    StartCoroutine(PrepareThumbnailVideo(thumbnailVideoPlayer));
                    Debug.Log($"Thumbnail video prepared for '{metadata.fileName}'.");
                }
                else
                {
                    Debug.LogError($"Thumbnail components missing for '{metadata.fileName}'.");
                }
            }
            else
            {
                Debug.Log($"Memory clip for '{metadata.fileName}' already exists, skipping creation.");
            }
        }
    }

    /// <summary>
    /// Plays the selected video in the video player.
    /// </summary>
    private void PlayVideo(string videoPath)
    {
        Debug.Log($"Playing video: {videoPath}");
        reelsMenu.SetActive(true);
        galleryMenu.SetActive(true);
        if (videoPlayer != null)
        {
            if (File.Exists(videoPath))
            {
                videoPlayer.url = videoPath;
                videoPlayer.Play();
                Debug.Log("Video started playing.");
            }
            else
            {
                string errorMsg = "Video file does not exist: " + videoPath;
                Debug.LogError(errorMsg);
                if (debugText != null) debugText.text = errorMsg;
            }
        }
        else
        {
            string errorMsg = "Video Player is not assigned.";
            Debug.LogError(errorMsg);
            if (debugText != null) debugText.text = errorMsg;
        }
    }

    /// <summary>
    /// Opens the reels menu and prepares the selected video.
    /// </summary>
    public void OpenReels(string videoPath)
    {
        Debug.Log($"Opening reels for video: {videoPath}");
        reelsMenu.SetActive(true);
        galleryMenu.SetActive(true);
        videoPlayer.url = videoPath;
        StartCoroutine(PrepareThumbnailVideo(videoPlayer));
    }

    /// <summary>
    /// Prepares the video thumbnail by loading the video to a specific frame.
    /// </summary>
    private IEnumerator PrepareThumbnailVideo(VideoPlayer thumbnailVideoPlayer)
    {
        Debug.Log($"Preparing thumbnail for video: {thumbnailVideoPlayer.url}");
        thumbnailVideoPlayer.Prepare();

        while (!thumbnailVideoPlayer.isPrepared)
        {
            yield return null;
        }

        thumbnailVideoPlayer.frame = 10;
        thumbnailVideoPlayer.Pause();
        Debug.Log("Thumbnail video prepared and paused at frame 10.");
    }

    /// <summary>
    /// Saves the metadata list to a JSON file.
    /// </summary>
    private void SaveMetadata(List<VideoMetadata> metadataList)
    {
        Debug.Log("Saving metadata...");
        string json = JsonUtility.ToJson(new VideoMetadataList { videos = metadataList });
        File.WriteAllText(metadataPath, json);
        Debug.Log("Metadata saved successfully.");
    }

    /// <summary>
    /// Loads the metadata from a JSON file.
    /// </summary>
    private List<VideoMetadata> LoadMetadata()
    {
        if (File.Exists(metadataPath))
        {
            string json = File.ReadAllText(metadataPath);
            Debug.Log("Metadata loaded from file.");
            return JsonUtility.FromJson<VideoMetadataList>(json).videos;
        }
        Debug.LogWarning("Metadata file does not exist, returning empty list.");
        return new List<VideoMetadata>();
    }
}

[System.Serializable]
public class VideoMetadata
{
    public string fileName;
    public string filePath;
    public string thumbnailPath;
}

[System.Serializable]
public class VideoMetadataList
{
    public List<VideoMetadata> videos = new List<VideoMetadata>();
}
