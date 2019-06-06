using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class FileManager
{
    public static string GetResultsFolder(string participantCode)
    {
        return "./Results/" + participantCode + "/";
    }

    public static string GetFrameDataFolder(string participantCode)
    {
        return GetResultsFolder(participantCode) + "FrameData/";
    }

    public static string GetUserConfigurationsFolder()
    {
        return "./Configurations/";
    }

    public static string GetInternalConfigurationFolder()
    {
        return Application.streamingAssetsPath + "/";
    }

    public static string GetDefaultConfigurationFilename()
    {
        return "default_configuration.cfg";
    }

    public static bool SaveFile(string directory, string filename, string data)
    {     
        string fullPath = directory + filename;
        Debug.Log("[FileManager] Saving file: " + fullPath);
        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(fullPath, data);
            return true;
        }
        catch
        {
            Debug.LogError("[FileManager] Error writing results file for experiment: " + filename);
            return false;
        }
    }

    public static string LoadFile(string directory, string filename)
    {
        string fullPath = directory + filename;
        Debug.Log("[FileManager] Loading file: " + fullPath);
        try
        {
            return File.ReadAllText(fullPath);
        }
        catch
        {
            Debug.LogError("[FileManager] Error loading file: " + filename);
            return null;
        }
    }
}
