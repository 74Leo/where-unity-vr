using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Linq;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer audioMixer;

    [Header("UI Dropdowns")]
    public Dropdown resolutionDropdown;
    public Dropdown displayDropdown;

    Resolution[] resolutions;
    int currentResolutionIndex = 0;

    public void Start()
    {
        resolutions = Screen.resolutions
            .GroupBy(r => (r.width, r.height))
            .Select(g => g.OrderByDescending(r => r.refreshRateRatio.value)
            .First())
            .ToArray();

        resolutionDropdown.ClearOptions();
        List<string> resOptions = new List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            var hz = Mathf.RoundToInt((float)resolutions[i].refreshRateRatio.value);
            string option = $"{resolutions[i].width}x{resolutions[i].height} @{hz}Hz";
            resOptions.Add(option);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
                currentResolutionIndex = i;
        }

        resolutionDropdown.AddOptions(resOptions);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        ///////////////////////////////////////////////////////////

        displayDropdown.ClearOptions();
        List<string> displayOptions = new List<string>();

        var layout = new List<DisplayInfo>();
        Screen.GetDisplayLayout(layout);

        for (int i = 0; i < layout.Count; i++)
        {
            var di = layout[i];
            int hz = Mathf.RoundToInt((float)di.refreshRate.value);
            string option = $"{di.name} {di.width}x{di.height} @{hz}Hz (Écran {i + 1})";
            displayOptions.Add(option);
        }

        displayDropdown.AddOptions(displayOptions);
        displayDropdown.value = 0;
        displayDropdown.RefreshShownValue();

        ///////////////////////////////////////////////////////////

        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        Screen.fullScreen = true;
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("volume", volume);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution res = resolutions[resolutionIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRateRatio);
        currentResolutionIndex = resolutionIndex;
    }

    public void SetDisplay(int displayIndex)
    {
        var layout = new List<DisplayInfo>();
        Screen.GetDisplayLayout(layout);

        if (displayIndex < 0 || displayIndex >= layout.Count) return;

        var targetDisplay = layout[displayIndex];

        Screen.MoveMainWindowTo(targetDisplay, Vector2Int.zero);

        var res = resolutions[currentResolutionIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRateRatio);
    }
}