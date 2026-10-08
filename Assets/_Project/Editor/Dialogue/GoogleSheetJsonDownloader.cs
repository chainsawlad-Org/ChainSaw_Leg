using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public static class GoogleSheetJsonDownloader
{
    private const string Url = "https://script.googleusercontent.com/macros/echo?user_content_key=AUkAhnQ_22gs1s70a91xfqJs4emlWukz9MyYkYrvj_U0hwU1wKc0YLU1xb4Ngj0o66i4oM5fjZrkXMMVfi6PjWhIHPjhzdmvYm7PVd00kofHXTFKTp0Q5qCmFNyGNdta55rRl4puxmLSfMP5tF6_f8uY3suq1Im5USLwW7cvpgYHDQtI0ne5msif0P4p8fXgY7oJdvLXJub6GTjdAtlqf6SXSrTM4NuJM9Dnr7NEJT8EYhV0RTGhhX7Mbf8SxbcV0ceQ1aFJ0gfeBf5PRgmZxc9PXj_mST5lzg&lib=Mem9ge48k0HzQ_-eOX09QhUmldho4eBO-";

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