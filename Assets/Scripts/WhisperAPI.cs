using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OpenAI
{
    public class WhisperAPI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI message;

        private readonly string fileName = "output.wav";
        
        private AudioClip clip;
        private bool isRecording = false;
        private OpenAIApi openai = new OpenAIApi();

        private void StartRecording()
        {
            isRecording = true;
            clip = Microphone.Start(Microphone.devices[0], false, 10, 44100);  // max duration set to 10, will stop on key release
            message.text = "Listening...";
        }

        private async void EndRecording()
        {
            Microphone.End(Microphone.devices[0]);
            byte[] data = SaveWav.Save(fileName, clip);
            
            message.text = "Transcripting...";
            
            var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData() {Data = data, Name = "audio.wav"},
                // File = Application.persistentDataPath + "/" + fileName,
                Model = "whisper-1",
                Language = "en"
            };
            var res = await openai.CreateAudioTranscription(req);
            
            message.text = res.Text;
            Debug.Log(res.Text);
        }

        private void Update()
        {
            if (isRecording && Input.GetKeyUp(KeyCode.A))
            {
                Debug.Log("Stop recording (a key released)");
                isRecording = false;
                EndRecording();
            }
            else if (!isRecording && Input.GetKeyDown(KeyCode.A))
            {
                Debug.Log("Start recording (a key pressed)");
                StartRecording();
            }
        }
    }
}