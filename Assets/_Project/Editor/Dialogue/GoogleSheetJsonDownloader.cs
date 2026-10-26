using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public static class GoogleSheetJsonDownloader
{
    private const string Url =
        "https://script.google.com/macros/s/AKfycbw5sQTzFxrVI1Nz6E4RFCyPQOwYm5OEMtExr3xFE-OuKASZNWyj6Lze34jtMpaHC8CrtA/exec";

    private const string OutputDirectory =
        "Assets/_Project/Game/Features/Dialogue/Scripts/Data/Replicas";

    private const string OutputFileName =
        "University.json";

    [MenuItem("ChainSawLeg/Dialogue/Download Google Sheets")]
    public static void Download()
    {
        Debug.Log("Downloading dialogue data from Google Sheets...");

        UnityWebRequest request = UnityWebRequest.Get(Url);

        UnityWebRequestAsyncOperation operation = request.SendWebRequest();

        operation.completed += _ =>
        {
            try
            {
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(
                        $"Failed to download dialogue data.\n" +
                        $"Error: {request.error}\n" +
                        $"URL: {Url}");

                    return;
                }

                SaveJson(request.downloadHandler.text);

                Debug.Log(
                    $"Dialogue data downloaded successfully.\n" +
                    $"Saved to: {OutputDirectory}/{OutputFileName}");
            }
            finally
            {
                request.Dispose();
            }
        };
    }

    private static void SaveJson(string json)
    {
        string absoluteDirectory =
            Path.GetFullPath(OutputDirectory);

        Directory.CreateDirectory(absoluteDirectory);

        string assetPath =
            Path.Combine(
                OutputDirectory,
                OutputFileName);

        File.WriteAllText(assetPath, json);

        AssetDatabase.Refresh();
    }
}