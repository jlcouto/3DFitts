using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using UnityEngine;

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

    public static string GetConfigurationFileFormat()
    {
        return ".cfg";
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

    public static List<string> GetFilenamesOnDirectory(string directory, string fileFormat)
    {
        if (Directory.Exists(directory))
        {
            return new DirectoryInfo(directory).GetFiles()
                        .Where(f => f.Extension.Equals(fileFormat))
                        .OrderByDescending(f => f.LastAccessTime)
                        .Select(f => f.Name)
                        .ToList();
        }
        else
        {
            return new List<string>();
        }
    }

    public static void SaveRecordsToCSVFile(string directory, string filename, IEnumerable<ExperimentResultRecord> records)
    {
        Directory.CreateDirectory(directory);
        StreamWriter writer = new StreamWriter(directory + filename);
        CsvWriter csvWriter = new CsvWriter(writer);
        csvWriter.WriteRecords(records);
        writer.Flush();
        writer.Close();
    }
}
