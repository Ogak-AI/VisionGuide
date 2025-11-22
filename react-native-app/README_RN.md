# VisionGuide (React Native for Quest 3)

This directory contains a React Native scaffold targeting Meta Quest 3 (Horizon OS). It provides JS wrappers and placeholders for the native modules you will need to implement to capture the passthrough camera and use TTS.

Important: Quest passthrough access requires Horizon OS / Meta native APIs. The JS app expects two native Android modules to be implemented and registered with React Native:

- `QuestPassthrough` with a method `captureFrame()` that returns a Promise resolving to a Base64 JPEG string (ideally 512x512) captured from the headset passthrough.
- `MetaVoiceTTS` with a method `speak(text)` to invoke Meta Voice TTS if you prefer native TTS instead of `react-native-tts`.

Files added
- `App.js` — main UI with `Capture & Describe` button.
- `src/QuestNativeModule.js` — JS wrapper for `QuestPassthrough` native module.
- `src/SceneAnalyzer.js` — builds raw POST to `https://api.openai.com/v1/chat/completions` using model `gpt-4o` and includes `image_url` as `data:image/jpeg;base64,<BASE64>` in the `messages` array.
- `src/TTSWrapper.js` — tries `react-native-tts` if installed; otherwise falls back to `NativeModules.MetaVoiceTTS`.
- `android/app/src/main/AndroidManifest.xml` — manifest snippet including `horizonos.permission.HEADSET_CAMERA` and `android.permission.CAMERA`.

Example native helpers & stubs
- `android/app/src/main/java/com/yourcompany/visionguide/BitmapUtils.java` — helper to resize/crop a Bitmap to 512x512 and return a Base64 JPEG string.
- `android/app/src/main/java/com/yourcompany/visionguide/QuestPassthroughExample.java` — documented example showing how to convert a captured Bitmap to base64 and return it to JS (you must implement actual capture using Horizon APIs).
- `android/app/src/main/java/com/yourcompany/visionguide/QuestPassthroughModule.java` — stub native module (added earlier) that you should implement or replace with Kotlin version.
- `android/app/src/main/java/com/yourcompany/visionguide/QuestPassthroughModule.kt` — Kotlin stub showing the `captureFrame(promise)` signature and recommended flow.


Native module guidance (Android)
1. Implement `QuestPassthrough` as a React Native native module (Java/Kotlin) that uses the Horizon OS/Meta SDK to access passthrough frames. It should:
   - Request/verify the `horizonos.permission.HEADSET_CAMERA` and `android.permission.CAMERA` permissions.
   - Capture a frame, resize to 512x512, compress to JPEG, and return a Base64 string to JS.

2. Implement `MetaVoiceTTS` to call Meta Voice runtime TTS if you prefer native TTS.

Android manifest
- Manifest includes the required permission entries. Merge additional XR/OpenXR metadata and Gradle configuration required by your Horizon OS setup.

OpenAI usage
- `SceneAnalyzer` posts raw JSON to the `chat/completions` endpoint with model `gpt-4o`. It expects you to supply an API key securely. For production, proxy requests via your server so you don't ship keys to the client.

Build notes
- Install React Native tooling and Android SDK. Use `yarn` or `npm install` in this folder.
- Implement and register the Android native modules before building to avoid runtime errors.
- To run on Quest, you will need to build an Android APK and install it on the device. Test permission requests and native capture on-device.

If you want, I can scaffold the Android native module Java/Kotlin files that register as React Native modules and add method stubs for `captureFrame()` and `speak()` (they will be placeholders that you must implement using Horizon SDK calls). Would you like me to add those Java/Kotlin stubs now? 
