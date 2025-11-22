Unity and CI notes for VisionGuide

Unity build (Cloud Build or GitHub Actions)
- This repository contains a Unity project skeleton in `Assets/` and `ProjectSettings/`.
- `ProjectSettings/ProjectVersion.txt` is set to `6000.1.17f1`. Configure your CI (Cloud Build or GitHub Actions) to use the same Unity editor version.
- For Unity Cloud Build: choose an Android build target and the matching Unity version; ensure the Android SDK/NDK and Java toolchain selection match the Cloud Build options.
- For GitHub Actions: consider using `game-ci/unity-builder` or `game-ci/unity-builder@v2` to run builds in the GitHub Actions runner. You will need to provide a Unity license or use a Cloud Build alternative.

React Native / Android
- The React Native JS app is in `react-native-app/`. The Android native scaffolding is partial (Java sources and `AndroidManifest.xml` exist), but there is no complete `android/` Gradle wrapper present in the repo. Local Android builds require a full React Native Android project folder with `gradlew` and `android/app/build.gradle`.
- If you want CI to build RN Android, either:
  - Add the missing `android/` directory (create via `npx react-native init` and copy sources), or
  - Build on-device using Android Studio / a dev machine with the Android SDK installed.

Node proxy
- A lightweight proxy to forward base64 images to OpenAI is in `react-native-app/server`.
- The repo includes a GitHub Actions workflow `.github/workflows/node-proxy-ci.yml` that installs dependencies and runs a smoke test for `/analyze`.

Next steps
- If you want, I can:
  - Add a Unity GitHub Actions workflow using `game-ci/unity-builder` (requires Unity license secret),
  - Generate a `react-native-app/android` Gradle project scaffold so RN Android can be built in CI,
  - Implement Quest/Horizon passthrough native capture code (requires Horizon SDK and device-specific APIs), or
  - Commit a GitHub Actions workflow that packages the repo and triggers Unity Cloud Build via API.
