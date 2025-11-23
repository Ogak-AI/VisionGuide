# Unity Cloud Build Setup for VisionGuide

## Prerequisites
- Unity project files are present (`Assets/`, `Packages/`, `ProjectSettings/`).
- `ProjectSettings/ProjectVersion.txt` is set to a secure Unity 6000-series version (e.g., `6000.1.17f1`).
- Android build target requires a keystore (upload via Unity Dashboard).

## Steps
1. Go to [Unity Cloud Build](https://dashboard.unity3d.com/cloud-build).
2. Connect your GitHub repository.
3. Set the Unity version to match `ProjectVersion.txt` (e.g., `6000.1.17f1`).
4. Set build target to Android.
5. Add your keystore and credentials for Android signing.
6. Set the main scene(s) in `Assets/Scenes/` (if present).
7. Start a build. Download the APK from the Cloud Build dashboard when complete.

## Notes
- If you add new scenes, update `EditorBuildSettings.asset`.
- For troubleshooting, see Unity Cloud Build logs in the dashboard.
- For more info: https://docs.unity.com/cloud-build
