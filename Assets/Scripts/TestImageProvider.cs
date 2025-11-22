using System.Collections;
using UnityEngine;

namespace VisionGuide
{
    /// <summary>
    /// Runtime helper to test VisionBrain in the Editor or Play mode.
    /// Assign a Texture2D (e.g., drag an image into `testImage`) and a VisionBrain instance.
    /// Call `TriggerTest()` (e.g., via Inspector button or another script) to send the test image.
    /// </summary>
    public class TestImageProvider : MonoBehaviour
    {
        [Tooltip("A Texture2D used for testing in Editor/Play mode. Convert to JPG base64 and send to VisionBrain.")]
        public Texture2D testImage;
        public VisionBrain visionBrain;
        public VoiceOutput voiceOutput;

        public void TriggerTest()
        {
            if (visionBrain == null)
            {
                Debug.LogWarning("TestImageProvider: VisionBrain not assigned.");
                return;
            }

            if (testImage == null)
            {
                Debug.LogWarning("TestImageProvider: testImage not assigned.");
                return;
            }

            StartCoroutine(TriggerTestCoroutine());
        }

        IEnumerator TriggerTestCoroutine()
        {
            string base64 = TextureToBase64(testImage);
            if (string.IsNullOrEmpty(base64))
            {
                Debug.LogWarning("Failed to convert test image to base64.");
                yield break;
            }

            yield return StartCoroutine(visionBrain.AnalyzeScene(base64));

            string result = visionBrain.lastResult;
            if (string.IsNullOrEmpty(result)) result = "No description returned.";

            Debug.Log("TestImageProvider: AI result -> " + result);
            if (voiceOutput != null)
            {
                voiceOutput.Speak(result);
            }
        }

        string TextureToBase64(Texture2D tex)
        {
            try
            {
                byte[] jpg = tex.EncodeToJPG(75);
                return System.Convert.ToBase64String(jpg);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("TextureToBase64 error: " + ex);
                return null;
            }
        }
    }
}
