package com.yourcompany.visionguide;

import android.Manifest;
import android.content.Context;
import android.content.pm.PackageManager;
import android.graphics.ImageFormat;
import android.hardware.camera2.CameraAccessException;
import android.hardware.camera2.CameraCaptureSession;
import android.hardware.camera2.CameraCharacteristics;
import android.hardware.camera2.CameraDevice;
import android.hardware.camera2.CameraManager;
import android.hardware.camera2.CaptureRequest;
import android.media.Image;
import android.media.ImageReader;
import android.os.Handler;
import android.os.HandlerThread;
import android.util.Base64;
import android.util.Size;

import com.facebook.react.bridge.Promise;
import com.facebook.react.bridge.ReactApplicationContext;
import com.facebook.react.bridge.ReactContextBaseJavaModule;
import com.facebook.react.bridge.ReactMethod;

import androidx.core.content.ContextCompat;

import java.nio.ByteBuffer;
import java.util.Arrays;
import java.util.concurrent.atomic.AtomicBoolean;

/**
 * Example native module using Camera2 for a single-shot JPEG capture.
 * This is a best-effort example; true passthrough access on Quest/Horizon
 * may require vendor-specific APIs. Replace with Horizon SDK capture
 * where available.
 */
public class QuestPassthroughModule extends ReactContextBaseJavaModule {
    private final ReactApplicationContext reactContext;

    public QuestPassthroughModule(ReactApplicationContext reactContext) {
        super(reactContext);
        this.reactContext = reactContext;
    }

    @Override
    public String getName() {
        return "QuestPassthrough";
    }

    @ReactMethod
    public void captureFrame(final Promise promise) {
        // Permission check
        if (ContextCompat.checkSelfPermission(reactContext, Manifest.permission.CAMERA) != PackageManager.PERMISSION_GRANTED) {
            promise.reject("PERMISSION_DENIED", "android.permission.CAMERA not granted");
            return;
        }

        final AtomicBoolean resolved = new AtomicBoolean(false);

        final CameraManager cameraManager = (CameraManager) reactContext.getSystemService(Context.CAMERA_SERVICE);

        try {
            String[] cameraIdList = cameraManager.getCameraIdList();
            String selectedCameraId = null;

            // Prefer back-facing camera
            for (String id : cameraIdList) {
                CameraCharacteristics chars = cameraManager.getCameraCharacteristics(id);
                Integer lensFacing = chars.get(CameraCharacteristics.LENS_FACING);
                if (lensFacing != null && lensFacing == CameraCharacteristics.LENS_FACING_BACK) {
                    selectedCameraId = id;
                    break;
                }
            }

            if (selectedCameraId == null && cameraIdList.length > 0) selectedCameraId = cameraIdList[0];
            if (selectedCameraId == null) {
                promise.reject("NO_CAMERA", "No camera available");
                return;
            }

            final int width = 1280;
            final int height = 720;

            final ImageReader reader = ImageReader.newInstance(width, height, ImageFormat.JPEG, 1);

            HandlerThread thread = new HandlerThread("CameraCaptureThread");
            thread.start();
            final Handler handler = new Handler(thread.getLooper());

            reader.setOnImageAvailableListener(new ImageReader.OnImageAvailableListener() {
                @Override
                public void onImageAvailable(ImageReader ir) {
                    Image image = null;
                    try {
                        image = ir.acquireLatestImage();
                        if (image == null) return;

                        Image.Plane[] planes = image.getPlanes();
                        ByteBuffer buffer = planes[0].getBuffer();
                        byte[] bytes = new byte[buffer.remaining()];
                        buffer.get(bytes);

                        // Encode to Base64 (JPEG bytes already)
                        String base64 = Base64.encodeToString(bytes, Base64.NO_WRAP);

                        if (resolved.compareAndSet(false, true)) {
                            promise.resolve(base64);
                        }
                    } catch (Exception ex) {
                        if (resolved.compareAndSet(false, true)) {
                            promise.reject("CAPTURE_ERROR", ex.getMessage());
                        }
                    } finally {
                        if (image != null) image.close();
                        try { ir.close(); } catch (Exception e) { /* ignore */ }
                        try { thread.quitSafely(); } catch (Exception e) { thread.quit(); }
                    }
                }
            }, handler);

            // Open camera and perform single capture
            cameraManager.openCamera(selectedCameraId, new CameraDevice.StateCallback() {
                CameraDevice cameraDevice;
                CameraCaptureSession captureSession;

                @Override
                public void onOpened(CameraDevice camera) {
                    cameraDevice = camera;
                    try {
                        camera.createCaptureSession(Arrays.asList(reader.getSurface()), new CameraCaptureSession.StateCallback() {
                            @Override
                            public void onConfigured(CameraCaptureSession session) {
                                captureSession = session;
                                try {
                                    final CaptureRequest.Builder captureBuilder = cameraDevice.createCaptureRequest(CameraDevice.TEMPLATE_STILL_CAPTURE);
                                    captureBuilder.addTarget(reader.getSurface());
                                    captureBuilder.set(CaptureRequest.CONTROL_AF_MODE, CaptureRequest.CONTROL_AF_MODE_CONTINUOUS_PICTURE);

                                    session.capture(captureBuilder.build(), new CameraCaptureSession.CaptureCallback() {
                                        @Override
                                        public void onCaptureCompleted(CameraCaptureSession session, CaptureRequest request, android.hardware.camera2.TotalCaptureResult result) {
                                            // Capture complete; cleanup is handled in ImageAvailable callback
                                            try {
                                                session.close();
                                            } catch (Exception e) {}
                                            try { cameraDevice.close(); } catch (Exception e) {}
                                        }
                                    }, handler);
                                } catch (CameraAccessException e) {
                                    if (resolved.compareAndSet(false, true)) promise.reject("CAMERA_ERROR", e.getMessage());
                                }
                            }

                            @Override
                            public void onConfigureFailed(CameraCaptureSession session) {
                                if (resolved.compareAndSet(false, true)) promise.reject("CONFIGURE_FAILED", "Failed to configure camera capture session");
                            }
                        }, handler);
                    } catch (CameraAccessException e) {
                        if (resolved.compareAndSet(false, true)) promise.reject("CAMERA_ERROR", e.getMessage());
                    }
                }

                @Override
                public void onDisconnected(CameraDevice camera) {
                    try { camera.close(); } catch (Exception e) {}
                    if (resolved.compareAndSet(false, true)) promise.reject("DISCONNECTED", "Camera disconnected");
                }

                @Override
                public void onError(CameraDevice camera, int error) {
                    try { camera.close(); } catch (Exception e) {}
                    if (resolved.compareAndSet(false, true)) promise.reject("CAMERA_ERROR", "Camera error: " + error);
                }
            }, handler);

        } catch (CameraAccessException e) {
            promise.reject("CAMERA_ACCESS_ERROR", e.getMessage());
        } catch (SecurityException e) {
            promise.reject("SECURITY_EXCEPTION", e.getMessage());
        }
    }
}
