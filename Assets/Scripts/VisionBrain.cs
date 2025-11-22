using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace VisionGuide
{
    [Serializable]
    public class ChatResponse
    {
        public Choice[] choices;
    }

    [Serializable]
    public class Choice
    {
        public Message message;
    }

    [Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    public class VisionBrain : MonoBehaviour
    {
        [Header("OpenAI Settings")]
        [Tooltip("Set this in inspector or at runtime. Keep secure in production.")]
        public string openAIKey;
        public string model = "gpt-4o";

        [TextArea(2, 4)]
        public string systemPrompt = "You are a guide for a blind user. Briefly describe obstacles, reading text if present. Be concise.";

        [HideInInspector]
        public string lastResult;

        // Coroutine required by spec
        public IEnumerator AnalyzeScene(string base64Image)
        {
            if (string.IsNullOrEmpty(openAIKey))
            {
                lastResult = "OpenAI API key not set.";
                yield break;
            }

            if (string.IsNullOrEmpty(base64Image))
            {
                lastResult = "No image provided.";
                yield break;
            }

            string dataUrl = "data:image/jpeg;base64," + base64Image;

            string userContent = "Please briefly describe the image for a blind user.";

            // Build minimal JSON payload string manually to avoid third-party libs
            string payload = "{\"model\":\"" + EscapeJson(model) + "\",\"messages\":[" +
                             "{\"role\":\"system\",\"content\":\"" + EscapeJson(systemPrompt) + "\"}," +
                             "{\"role\":\"user\",\"content\":\"" + EscapeJson(userContent) + "\",\"image_url\":\"" + EscapeJson(dataUrl) + "\"}" +
                             "]}";

            using (UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", UnityWebRequest.kHttpVerbPOST))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", "Bearer " + openAIKey);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    lastResult = "API Error: " + request.error + "\n" + request.downloadHandler.text;
                    Debug.LogError(lastResult);
                    yield break;
                }

                string responseText = request.downloadHandler.text;
                try
                {
                    ChatResponse response = JsonUtility.FromJson<ChatResponse>(responseText);
                    if (response != null && response.choices != null && response.choices.Length > 0 && response.choices[0].message != null)
                    {
                        lastResult = response.choices[0].message.content;
                    }
                    else
                    {
                        // Fallback: try to find assistant content via naive parsing
                        lastResult = FallbackExtractContent(responseText);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("JSON parse failed: " + ex + "\nResponse:\n" + responseText);
                    lastResult = FallbackExtractContent(responseText);
                }
            }
        }

        string EscapeJson(string s)
        {
            if (s == null) return string.Empty;
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        string FallbackExtractContent(string json)
        {
            // Very simple fallback: find the first occurrence of "content": "..."
            int idx = json.IndexOf("\"content\"");
            if (idx >= 0)
            {
                int start = json.IndexOf('"', idx + 10);
                if (start >= 0)
                {
                    start++; // position after opening quote
                    int end = json.IndexOf('"', start);
                    if (end > start)
                    {
                        string content = json.Substring(start, end - start);
                        return content.Replace("\\n", "\n").Replace("\\\"", "\"");
                    }
                }
            }
            return "(no description returned by model)";
        }
    }
}
