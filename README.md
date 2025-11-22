# VisionGuide — Quick Setup & Build Guide

VisionGuide is an accessibility prototype for Meta Quest 3 (Horizon OS). It captures passthrough frames, sends a resized image (512×512 JPG) to OpenAI `gpt-4o` for scene description, and speaks the result via Meta Voice TTS.

---

## What was added
- `Assets/Scripts/CameraVision.cs` — captures `WebCamTexture`, requests `horizonos.permission.HEADSET_CAMERA` and `android.permission.CAMERA`, provides `CaptureAndConvert()` (512×512 Base64 JPG).
- `Assets/Scripts/VisionBrain.cs` — `AnalyzeScene(string base64Image)` coroutine that POSTs raw JSON to `https://api.openai.com/v1/chat/completions` (model `gpt-4o`).
- `Assets/Scripts/VoiceOutput.cs` — reflection wrapper to call Meta Voice `TTSSpeaker.Speak(string)`.
- `Assets/Scripts/InputListener.cs` — listens for right-controller A (primaryButton) to trigger Capture → Analyze → Speak.
- `Assets/Scripts/TestImageProvider.cs` — helper to test AI flow using a `Texture2D` in Editor/Play mode.
- `Assets/Plugins/Android/AndroidManifest.xml` — minimal manifest with required permissions (`horizonos.permission.HEADSET_CAMERA` and `android.permission.CAMERA`).

---

## Important: Android Manifest & Permissions
1. Unity may generate its own manifest during build. To ensure permissions are included, keep the file `Assets/Plugins/Android/AndroidManifest.xml` (present in this repo). Merge any additional XR meta-data required by your XR/OpenXR plugin into this file.
2. The runtime script `CameraVision` will request both `horizonos.permission.HEADSET_CAMERA` and `android.permission.CAMERA` via `UnityEngine.Android.Permission.RequestUserPermission(...)`.

## OpenAI Key
- For testing, set `VisionBrain.openAIKey` in the Inspector (on the GameObject with `VisionBrain`).
- For production, do NOT embed your API key in the client — use a secure server-side proxy.

## Setup in Unity (short)
1. Open the project in Unity 2022.3 LTS.
2. Switch Platform to Android (`File > Build Settings`).
3. Set up XR (OpenXR + Quest/Horizon plugins) per Meta docs.
4. Create a `VisionSystem` GameObject and add `CameraVision`, `VisionBrain`, and `VoiceOutput` components.
5. Add `InputListener` (same or separate GameObject) and assign references to the components.
6. If using Meta Voice SDK: add `TTSSpeaker` to the same object as `VoiceOutput` and assign it in the inspector (or rely on the reflection auto-find).

## Testing
- Editor: Use `Assets/Scripts/TestImageProvider.cs`. Assign a `Texture2D` in the Inspector and the `VisionBrain` reference, then press Play and call `TriggerTest()` (you can attach a small editor button or call from another script).
- Device (Quest 3): Build & Run. Accept camera permissions when prompted. Press right controller A to capture and speak the description.

## Build & Deploy (example commands)
1. Build from Unity Editor: `File > Build Settings > Build And Run`.
2. Alternatively, use `adb` to install a built .apk:

```bash
# install apk (example)
adb install -r path/to/your.apk
adb logcat -s Unity ActivityManager PackageManager
```

## Privacy & Cost
- Images are sent to OpenAI. Explicitly inform users and acquire consent.
- Consider throttling requests to save bandwidth and API costs.

## Next recommended steps
- Add a secure server-side proxy to hold your OpenAI key.
- Improve error handling and parsing for OpenAI responses.
- Add accessibility flows for continuous scene monitoring, toggles for verbosity, and user settings.

If you want, I can:
- Create a secure Node.js example proxy that forwards requests to OpenAI.
- Add an Editor inspector button for `TestImageProvider.TriggerTest()`.
- Expand the Android manifest with XR metadata specific to your OpenXR plugin.
# VisionGuide
An app for visually impaired users that reads real-world signs or menus out loud when the user looks at them
