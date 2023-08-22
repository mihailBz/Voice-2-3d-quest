using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OpenAI;

public class WhisperAPI : MonoBehaviour
{
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
    }

    private async void EndRecording()
    {
        Microphone.End(Microphone.devices[0]);
        byte[] data = SaveWav.Save(fileName, clip);


        var req = new CreateAudioTranscriptionsRequest
        {
            FileData = new FileData() { Data = data, Name = "audio.wav" },
            // File = Application.persistentDataPath + "/" + fileName,
            Model = "whisper-1",
            Language = "en"
        };
        var res = await openai.CreateAudioTranscription(req);

        Debug.Log("Result:");
        Debug.Log(res.Text);
        pythonRunner.Run(res.Text);
        // pythonRunner.Run("book");
        importer.ImportGLTF("C:/Users/msh/UnityProjects/Voice-2-3d-quest/Assets/Generated Objects/object.glb");
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
