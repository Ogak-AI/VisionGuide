package com.yourcompany.visionguide;

import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Matrix;
import android.util.Base64;

import java.io.ByteArrayOutputStream;

public class BitmapUtils {
    // Resize bitmap to targetSize x targetSize (square) and return Base64-encoded JPEG
    public static String bitmapToBase64Jpeg(Bitmap src, int targetSize, int quality) {
        if (src == null) return null;

        // Calculate scale
        int width = src.getWidth();
        int height = src.getHeight();
        float scale = (float) targetSize / Math.max(width, height);

        Matrix matrix = new Matrix();
        matrix.postScale(scale, scale);

        Bitmap scaled = Bitmap.createBitmap(src, 0, 0, width, height, matrix, true);

        // If result is not square, center-crop to targetSize
        Bitmap cropped;
        if (scaled.getWidth() != targetSize || scaled.getHeight() != targetSize) {
            int x = Math.max(0, (scaled.getWidth() - targetSize) / 2);
            int y = Math.max(0, (scaled.getHeight() - targetSize) / 2);
            int w = Math.min(targetSize, scaled.getWidth() - x);
            int h = Math.min(targetSize, scaled.getHeight() - y);
            cropped = Bitmap.createBitmap(scaled, x, y, w, h);
        } else {
            cropped = scaled;
        }

        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        cropped.compress(Bitmap.CompressFormat.JPEG, quality, baos);
        byte[] bytes = baos.toByteArray();
        String base64 = Base64.encodeToString(bytes, Base64.NO_WRAP);

        // Cleanup bitmaps
        if (!scaled.isRecycled()) scaled.recycle();
        if (!cropped.isRecycled()) cropped.recycle();

        return base64;
    }
}
