using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using UnityEngine;

public static class FileManager
{
    public static string GetResultsFolder(string participantCode = null)
    {
        return "./Results/";
    }

    public static string GetResultsFolderForParticipant(string participantCode)
    {
        return GetResultsFolder() + participantCode + "/";
    }

    public static string GetFrameDataFolder(string participantCode)
    {
        return GetResultsFolder(participantCode) + "FrameData/";
    }

    public static string GetUserConfigurationsFolder()
    {
        return "./Configurations/";
    }

    public static string GetResultsFilenameForTest(TestMeasurements test)
    {
        return GetResultsFilenamePrefix(test.configuration) + "-" + test.timestamp.Replace(":", "_");
    }

    public static string GetResultsFilenamePrefix(ExperimentConfiguration configuration)
    {
        return string.Format("{0}-{1}-{2}-{3}",
                configuration.participantCode, configuration.conditionCode,
                configuration.sessionCode, configuration.groupCode);
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
        directory = DirectoryFromPath(directory);
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
        directory = DirectoryFromPath(directory);
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
        directory = DirectoryFromPath(directory);
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

    public static void WriteToCsvFile(string directory, string filename, IEnumerable records)
    {
        directory = DirectoryFromPath(directory);
        Directory.CreateDirectory(directory);

        try
        {
            StreamWriter writer = new StreamWriter(directory + filename);
            CsvWriter csvWriter = new CsvWriter(writer);
            csvWriter.WriteRecords(records);
            writer.Flush();
            writer.Close();
        }
        catch
        {
            Debug.LogError("[FileManager] Couldn't write to the .csv file: " + directory + filename);
        }
    }

    public static List<ExperimentResultRecord> ReadRecordsFromCsvFile(string directory, string filename)
    {
        directory = DirectoryFromPath(directory);
        try
        {
            StreamReader reader = new StreamReader(directory + filename);
            CsvReader csvReader = new CsvReader(reader);
            List<ExperimentResultRecord> records = new List<ExperimentResultRecord>(csvReader.GetRecords<ExperimentResultRecord>());
            reader.Close();
            return records;
        }
        catch
        {
            Debug.LogError("[FileManager] Couldn't read the .csv file: " + directory + filename);
        }

        return null;
    }

    public static void MergeCsvFilesInFolder(string directory, string outputFilename)
    {
        directory = DirectoryFromPath(directory);

        var filesToMerge = GetFilenamesOnDirectory(directory, ".csv");

        StreamWriter writer = new StreamWriter(directory + outputFilename);
        CsvWriter csvWriter = new CsvWriter(writer);

        foreach (var file in filesToMerge)
        {
            var records = ReadRecordsFromCsvFile(directory, file);
            if (records != null)
            {
                csvWriter.WriteRecords(records);
            }
        }

        writer.Flush();
        writer.Close();
    }

    static string DirectoryFromPath(string path)
    {
        if (File.Exists(path))
        {
            return path.Remove(path.LastIndexOf("/", StringComparison.InvariantCulture) + 1);
        }
        else
        {
            return (path.EndsWith("/", StringComparison.InvariantCulture)) ? path : path + "/";
        }
    }

    public static bool CheckExistenceOfFilesWithPrefix(string prefix, string directory, string fileFormat)
    {
        var files = GetFilenamesOnDirectory(directory, fileFormat);
        foreach (string f in files)
        {
            if (f.StartsWith(prefix, StringComparison.InvariantCulture)) {
                return true;
            }
        }
        return false;
    }
}
