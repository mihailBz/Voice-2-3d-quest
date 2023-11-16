using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OpenAI;
using UnityEngine.Networking;

public class WhisperAPI : MonoBehaviour
{
    public TMP_Text statusTMPText;
    private readonly string fileName = "output.wav";
    private PythonRunner pythonRunner = new();
    private GLBImporter importer = new();

    private AudioClip clip;
    private bool isRecording = false;
    private OpenAIApi openai = new();

    private void StartRecording()
    {
        isRecording = true;
        clip = Microphone.Start(Microphone.devices[0], false, 10,
            44100); // max duration set to 10, will stop on key release
        statusTMPText.text = "Recording...";
    }

    private async void EndRecording()
    {
        Microphone.End(Microphone.devices[0]);
        byte[] data = SaveWav.Save(fileName, clip);
        
        StartCoroutine(SendRequestCoroutine(data));
        statusTMPText.text = "Generating...";

    }
    private IEnumerator SendRequestCoroutine(byte[] data)
    {
        string endpointUrl = "http://localhost:5000/call_gradio";

        // Create a form and add the audio file data
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", data, fileName, "audio/wav");

        // Add additional parameters
        form.AddField("seed", 0); // Replace 0 with your desired seed value
        form.AddField("guidance_scale", 15); // Replace 15 with your desired guidance_scale value
        form.AddField("inference_steps", 64); // Replace 64 with your desired inference_steps value

        // Send the form to the server
        using (UnityWebRequest request = UnityWebRequest.Post(endpointUrl, form))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
            }
            else
            {
                // Handle the ZIP file response
                byte[] responseData = request.downloadHandler.data;

                // Extract the ZIP file contents
                using (MemoryStream zipStream = new MemoryStream(responseData))
                using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))
                        {
                            string glbFilePath = Path.Combine(Application.dataPath, "Generated Objects", entry.Name);
                            entry.ExtractToFile(glbFilePath, true);
                            importer.ImportGLTF(glbFilePath);
                        }
                        else if (entry.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                        {
                            using (StreamReader reader = new StreamReader(entry.Open()))
                            {
                                string transcription = reader.ReadToEnd();
                                statusTMPText.text = transcription;
                                StartCoroutine(ClearTextAfterDelay(10));
                            }
                        }
                    }
                }
            }
        }
    }
    
    private IEnumerator ClearTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        statusTMPText.text = ""; // Clear the text
    }
    

    private void Update()
    {
        // You can use the InputDevice class to get a list of all devices. 
        // Then, filter it for the right hand controller to check for button presses.
        var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightHandDevices);

        if (rightHandDevices.Count == 1)
        {
            UnityEngine.XR.InputDevice device = rightHandDevices[0];
            bool buttonPressed;
            if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonPressed) &&
                buttonPressed)
            {
                if (!isRecording)
                {
                    Debug.Log("Start recording (a key pressed)");
                    StartRecording();
                }
            }
            else if (isRecording)
            {
                Debug.Log("Stop recording (a key released)");
                isRecording = false;
                EndRecording();
            }
        }
    }
}
