Integration & Debugging Guide — VisionGuide (React Native / Quest)

This guide helps you build, deploy, and debug the React Native Quest app and the native modules.

Prerequisites
- Android SDK, JDK, Node, and React Native CLI installed.
- Device: Meta Quest 3 with developer mode enabled and USB debugging allowed.
- For development with the proxy: Node.js installed for `react-native-app/server`.

Start the OpenAI proxy (recommended)
1. Open a terminal and run:

    cd react-native-app/server
    npm install
    export OPENAI_API_KEY="sk-..."
    npm start

2. By default the RN app is configured to use 'http://10.0.2.2:3000/analyze' (Android emulator host). For a physical device, host the proxy on a reachable host and set PROXY_URL env var when bundling, or change 'src/SceneAnalyzer.js'.

Build & install APK (example using RN CLI)

    # from react-native-app/
    npm install
    npx react-native run-android --variant=release

If you already built an APK, install via adb:

    adb install -r android/app/build/outputs/apk/release/app-release.apk

View logs

Use 'adb logcat' filtered to useful tags. Example:

    adb logcat *:S ReactNative:V AndroidRuntime:V VisionGuide:V

Testing the flow on-device
1. On first run, accept camera permissions when prompted.
2. Open the app; press 'Capture & Describe'.
3. Expected behavior:
   - App calls native 'QuestPassthrough.captureFrame()'.
   - Native module captures a single JPEG frame and returns Base64.
   - App sends Base64 to proxy -> proxy forwards to OpenAI -> returns JSON.
   - App extracts assistant text and calls 'MetaVoiceTTS.speak()' to speak it.

Troubleshooting
- Permission denied: ensure 'android.permission.CAMERA' and 'horizonos.permission.HEADSET_CAMERA' are present in 'AndroidManifest.xml'. Confirm runtime permission grant.
- Capture errors: camera2 may fail on specific devices. Logs from 'adb logcat' will show exceptions from the native module (look for tags like CAMERA_ERROR, CAPTURE_ERROR).
- Proxy errors: ensure 'OPENAI_API_KEY' is set and proxy reachable from device.
- TTS silence: if 'MetaVoiceTTS' uses fallback Android TTS, confirm device TTS is present and language is supported.

Debugging native module from Java

- Add Log.d("VisionGuide", "message") statements in Java/Kotlin modules and inspect via 'adb logcat'.
- To catch Promise rejections on the JS side, wrap calls in try/catch and log errors.

Notes about Horizon passthrough
- The Camera2 example in 'QuestPassthroughModule' is a generic Camera2 single-capture flow and may not access the Quest passthrough feed on Horizon OS. Consult Meta Horizon documentation for the correct passthrough APIs and required manifest entries.
- Replace the Camera2 capture logic with Horizon SDK calls to obtain the passthrough framebuffer if available.

If you want, I can patch the project further to add more logging or generate a sample Gradle configuration optimized for Quest builds.
