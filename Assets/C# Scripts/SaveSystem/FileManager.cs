using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System;

public static class FileManager
{


    /// <summary>
    /// Method to get all file names of a specific type in a directory
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <param name="fileExtension"></param>
    /// <returns></returns>
    public static (bool, string[]) GetAllFileNamesFromDirectory(string directoryPath, string fileExtension = ".json")
    {
        // Construct the full path
        string path = $"{Application.persistentDataPath}/{directoryPath}";

        // Ensure the path ends with "/"
        if (!path.EndsWith("/"))
        {
            path += "/";
        }

        if (Directory.Exists(path))
        {
            // Get all file paths of the specified type in the directory
            string[] filePaths = Directory.GetFiles(path, $"*{fileExtension}");

            // Extract file names from the file paths
            string[] fileNames = new string[filePaths.Length];
            for (int i = 0; i < filePaths.Length; i++)
            {
                fileNames[i] = Path.GetFileName(filePaths[i]); // Get the file name from the path
            }

            return (true, fileNames);
        }
        else
        {
            Debug.LogWarning("Directory does not exist: " + directoryPath);
            return (false, new string[0]); // Returns false and an empty array if the directory doesn't exist
        }
    }



    /// <summary>
    /// Method to get all files and deserialize into a NativeArray of structs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="directoryPath"></param>
    /// <param name="fileExtension"></param>
    /// <returns></returns>
    public static async Task<(bool, T[])> GetAllFilesFromDirectory<T>(string directoryPath, string fileExtension = ".json")
    {
        // Get all file names with the specified extension
        (bool anyFileInDirectory, string[] fileNames) = GetAllFileNamesFromDirectory(directoryPath, fileExtension);

        //if atleast one file was found in the directory that has the correct fileExtensions
        if (anyFileInDirectory)
        {
            // Create an array for deserialized objects
            T[] fileStructArray = new T[fileNames.Length];

            int successCount = 0;

            for (int i = 0; i < fileNames.Length; i++)
            {
                string fullPath = $"{directoryPath}/{fileNames[i]}";

                (bool success, T loadedStruct) = await LoadInfo<T>(fullPath);

                if (success)
                {
                    fileStructArray[successCount++] = loadedStruct; // Add object to the NativeArray
                }
                else
                {
                    Debug.LogWarning($"Failed to load or deserialize file: {fileNames[i]}");
                }
            }

            // Resize the NativeArray if needed (there is no direct Resize; copy the valid items)
            if (successCount != fileStructArray.Length)
            {
                Array.Resize(ref fileStructArray, successCount);
            }

            return (successCount > 0, fileStructArray);
        }
        //no files found in directory with correct fileExtension
        else
        {
            Debug.LogWarning($"No files with extension '{fileExtension}' found in directory: {directoryPath}");
            return (false, new T[0]);
        }
    }



    /// <summary>
    /// Save method using JSON serialization
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="saveData"></param>
    /// <param name="pathPlusFileName"></param>
    /// <param name="encryptFile"></param>
    /// <returns></returns>
    public async static Task SaveInfo<T>(T saveData, string pathPlusFileName, bool encryptFile = true)
    {
        try
        {
            pathPlusFileName = EnsurePersistentDataPath(pathPlusFileName);

            // Separate the directory path and the file name from the provided directoryPlusFileName string
            string directoryPath = Path.GetDirectoryName(pathPlusFileName);

            string fileName = Path.GetFileName(pathPlusFileName);


            fileName = EnsureFileExtension(fileName);

            // if directory path doesnt exist, create it
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }


            string path = $"{directoryPath}/{fileName}";


            // Serialize the data to JSON format
            string outputData = JsonUtility.ToJson(saveData);


            if (encryptFile)
            {
                //encrypt if marked for encryption
                outputData = await EncryptionUtility.EncryptAsync(outputData);
            }

            //write the data to the file
            await File.WriteAllTextAsync(path, outputData);

            Debug.Log($"Game data saved to: {path}");

        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to save game data: " + ex.Message);
        }
    }



    /// <summary>
    /// Load method using JSON deserialization
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="decryptFile"></param>
    /// <returns></returns>
    public async static Task<(bool, T)> LoadInfo<T>(string path, bool decryptFile = true)
    {
        path = EnsurePersistentDataPath(path);
        path = EnsureFileExtension(path);

        if (File.Exists(path))
        {
            try
            {
                // Read the encrypted data from the file
                string outputData = await File.ReadAllTextAsync(path);


                if (decryptFile)
                {
                    // decrypt the data if marked for decryption
                    outputData = await EncryptionUtility.DecryptAsync(outputData);
                }

                T loadedData = JsonUtility.FromJson<T>(outputData);

                Debug.Log($"Game data loaded from: {path}");
                return (true, loadedData);

            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to load game data: " + ex.Message);
                return (false, default);
            }
        }
        else
        {
            Debug.LogWarning("No save file found at: " + path);
            return (false, default);
        }
    }


    /// <summary>
    /// Delete a File
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool DeleteFile(string path)
    {
        path = EnsurePersistentDataPath(path);
        path = EnsureFileExtension(path);

        try
        {
            if (File.Exists(path))
            {
                File.Delete(path); // Deletes the file

                Debug.Log($"File deleted: {path}");
                return true;
            }
            else
            {
                Debug.LogWarning($"File not found: {path}");
                return false;
            }
        }
        catch (IOException ex)
        {
            Debug.LogError($"Failed to delete file {path}: {ex.Message}");
            return false;
        }
    }


    /// <summary>
    /// Delete a Directory (Folder)
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <returns></returns>
    public static bool DeleteDirectory(string directoryPath)
    {
        directoryPath = EnsurePersistentDataPath(directoryPath);

        try
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath); // Deletes the directory

                Debug.Log($"Directory deleted: {directoryPath}");
                return true;
            }
            else
            {
                Debug.LogWarning($"Directory not found: {directoryPath}");
                return false;
            }
        }
        catch (IOException ex)
        {
            Debug.LogError($"Failed to delete directory {directoryPath}: {ex.Message}");
            return false;
        }
    }



    private static string EnsurePersistentDataPath(string path)
    {
        //if path doesnt start with "Application.persistentDataPath", add it, because all files are preferably located in a fixed path
        if (path.StartsWith(Application.persistentDataPath) == false)
        {
            return $"{Application.persistentDataPath}/{path}";
        }
        else
        {
            return path;
        }
    }

    private static string EnsureFileExtension(string path)
    {
        // if the "directoryPlusFileName" string doesnt have an extension (.json, .txt, etc) add .json automatically
        if (string.IsNullOrEmpty(Path.GetExtension(path)))
        {
            return path + ".json";
        }
        else
        {
            return path;
        }
    }
}


public struct ValueWrapper<T>
{
    public T value;

    public ValueWrapper(T _value)
    {
        value = _value;
    }
}