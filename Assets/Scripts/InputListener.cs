using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace VisionGuide
{
    public class InputListener : MonoBehaviour
    {
        public CameraVision cameraVision;
        public VisionBrain visionBrain;
        public VoiceOutput voiceOutput;

        InputDevice rightController;
        bool previousPrimaryButton = false;

        void Start()
        {
            // Initial device query
            TryInitController();
        }

        void TryInitController()
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, devices);
            if (devices.Count > 0)
            {
                rightController = devices[0];
            }
        }

        void Update()
        {
            if (!rightController.isValid) TryInitController();

            if (rightController.isValid)
            {
                bool primaryButton = false;
                if (rightController.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButton))
                {
                    if (primaryButton && !previousPrimaryButton)
                    {
                        StartCoroutine(OnAPressed());
                    }
                    previousPrimaryButton = primaryButton;
                }
            }
        }

        IEnumerator OnAPressed()
        {
            if (cameraVision == null || visionBrain == null || voiceOutput == null)
            {
                Debug.LogWarning("InputListener: One or more required components not assigned.");
                yield break;
            }

            // Capture a frame and convert to base64
            string base64 = cameraVision.CaptureAndConvert();
            if (string.IsNullOrEmpty(base64))
            {
                Debug.LogWarning("Capture failed or returned no data.");
                yield break;
            }

            // Send to AI and wait for completion
            yield return StartCoroutine(visionBrain.AnalyzeScene(base64));

            string result = visionBrain.lastResult;
            if (string.IsNullOrEmpty(result)) result = "I couldn't analyze the scene.";

            // Speak result via TTS
            voiceOutput.Speak(result);
        }
    }
}
