using UnityEngine;

// This file integrates with the Meta Voice SDK TTSSpeaker component.
// Ensure you have the Meta/Wit.ai TTS package imported and a TTSSpeaker on the same GameObject or assigned in inspector.

namespace VisionGuide
{
    public class VoiceOutput : MonoBehaviour
    {
        // The TTSSpeaker type/namespace depends on the Meta Voice SDK version. Inspect your package and set this reference in the Inspector.
        public Component ttsSpeakerComponent; // use a generic Component so compile won't fail if SDK not present at edit-time

        public void Speak(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogWarning("Speak called with empty text.");
                return;
            }

            if (ttsSpeakerComponent == null)
            {
                // Try to get a component named TTSSpeaker dynamically to avoid hard dependency at compile time
                var comp = GetComponent("TTSSpeaker");
                if (comp != null)
                {
                    ttsSpeakerComponent = (Component)comp;
                }
            }

            if (ttsSpeakerComponent == null)
            {
                Debug.LogWarning("TTSSpeaker component not assigned or found. Cannot speak.");
                return;
            }

            // Use reflection to call Speak(string) on the TTSSpeaker component so we avoid a hard compile-time dependency
            var method = ttsSpeakerComponent.GetType().GetMethod("Speak", new System.Type[] { typeof(string) });
            if (method != null)
            {
                method.Invoke(ttsSpeakerComponent, new object[] { text });
            }
            else
            {
                Debug.LogWarning("TTSSpeaker.Speak(string) method not found on the assigned component.");
            }
        }
    }
}
