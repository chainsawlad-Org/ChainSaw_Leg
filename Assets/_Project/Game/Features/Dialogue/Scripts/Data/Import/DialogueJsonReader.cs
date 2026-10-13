using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace ChainSawLeg.Features.Dialogue
{
    public sealed class DialogueJsonReader
    {
        public DialogueJsonData Read(string jsonPath)
        {
            if (string.IsNullOrWhiteSpace(jsonPath))
            {
                Debug.LogError(
                    "Dialogue JSON read failed: path is empty.");

                return null;
            }

            if (!File.Exists(jsonPath))
            {
                Debug.LogError(
                    $"Dialogue JSON read failed: file does not exist.\n" +
                    $"Path: {jsonPath}");

                return null;
            }

            string json;

            try
            {
                json = File.ReadAllText(jsonPath);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Dialogue JSON read failed: could not read file.\n" +
                    $"Path: {jsonPath}\n" +
                    $"Exception: {exception}");

                return null;
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogError(
                    $"Dialogue JSON read failed: file is empty.\n" +
                    $"Path: {jsonPath}");

                return null;
            }

            try
            {
                DialogueJsonData data =
                    JsonConvert.DeserializeObject<DialogueJsonData>(
                        json);

                if (data == null)
                {
                    Debug.LogError(
                        "Dialogue JSON read failed: " +
                        "deserialized data is null.");

                    return null;
                }

                return data;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Dialogue JSON read failed: invalid JSON.\n" +
                    $"Path: {jsonPath}\n" +
                    $"Exception: {exception}");

                return null;
            }
        }
    }
}
