using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Android;

namespace VisionGuide
{
    public class CameraVision : MonoBehaviour
    {
        public WebCamTexture webCamTexture;
        public int captureSize = 512;
        public bool autoStart = true;

        void Start()
        {
            if (autoStart) StartCoroutine(EnsurePermissionsAndStart());
        }

        IEnumerator EnsurePermissionsAndStart()
        {
            // Request the Horizon OS headset camera permission and standard camera permission
            if (!Permission.HasUserAuthorizedPermission("horizonos.permission.HEADSET_CAMERA"))
            {
                Permission.RequestUserPermission("horizonos.permission.HEADSET_CAMERA");
            }

            if (!Permission.HasUserAuthorizedPermission("android.permission.CAMERA"))
            {
                Permission.RequestUserPermission("android.permission.CAMERA");
            }

            // Wait a short time for user to respond to permission prompt
            float timeout = 5f;
            float t = 0f;
            while (t < timeout && (!Permission.HasUserAuthorizedPermission("horizonos.permission.HEADSET_CAMERA") || !Permission.HasUserAuthorizedPermission("android.permission.CAMERA")))
            {
                t += Time.deltaTime;
                yield return null;
            }

            if (!Permission.HasUserAuthorizedPermission("horizonos.permission.HEADSET_CAMERA") || !Permission.HasUserAuthorizedPermission("android.permission.CAMERA"))
            {
                Debug.LogWarning("Required camera permissions not granted. Passthrough capture may fail.");
            }

            StartCamera();
        }

        public void StartCamera()
        {
            if (webCamTexture != null && webCamTexture.isPlaying) return;

            var devices = WebCamTexture.devices;
            if (devices == null || devices.Length == 0)
            {
                Debug.LogWarning("No webcam devices found.");
                return;
            }

            string deviceName = devices[0].name;
            webCamTexture = new WebCamTexture(deviceName);
            webCamTexture.Play();
            Debug.Log($"Started WebCamTexture on: {deviceName}");
        }

        public void StopCamera()
        {
            if (webCamTexture != null && webCamTexture.isPlaying)
            {
                webCamTexture.Stop();
            }
        }

        // Helper method required by the spec: takes a frame, resizes to 512x512 and returns a Base64 JPG string
        public string CaptureAndConvert()
        {
            if (webCamTexture == null || !webCamTexture.isPlaying)
            {
                Debug.LogWarning("WebCamTexture not running. Attempting to start camera.");
                StartCamera();
                if (webCamTexture == null || !webCamTexture.isPlaying)
                {
                    Debug.LogError("Unable to capture frame - camera not available.");
                    return null;
                }
            }

            // Ensure we have a valid frame
            if (webCamTexture.width <= 16 || webCamTexture.height <= 16)
            {
                // Wait briefly for the camera to warm up
                float start = Time.time;
                while ((webCamTexture.width <= 16 || webCamTexture.height <= 16) && Time.time - start < 1.0f)
                {
                    // busy-wait small amount
                }
                if (webCamTexture.width <= 16 || webCamTexture.height <= 16)
                {
                    Debug.LogError("Camera frame not ready.");
                    return null;
                }
            }

            // Create a temporary Texture2D from the current frame
            Texture2D frameTex = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGB24, false);
            try
            {
                frameTex.SetPixels(webCamTexture.GetPixels());
                frameTex.Apply();

                // Resize to captureSize x captureSize using a RenderTexture
                RenderTexture rt = RenderTexture.GetTemporary(captureSize, captureSize, 0, RenderTextureFormat.Default);
                Graphics.Blit(frameTex, rt);

                RenderTexture prev = RenderTexture.active;
                RenderTexture.active = rt;

                Texture2D resized = new Texture2D(captureSize, captureSize, TextureFormat.RGB24, false);
                resized.ReadPixels(new Rect(0, 0, captureSize, captureSize), 0, 0);
                resized.Apply();

                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);

                byte[] jpg = resized.EncodeToJPG(75);
                string base64 = Convert.ToBase64String(jpg);

                // Cleanup
                UnityEngine.Object.Destroy(frameTex);
                UnityEngine.Object.Destroy(resized);

                return base64;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Capture error: {ex}");
                return null;
            }
        }
    }
}
