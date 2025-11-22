package com.yourcompany.visionguide;

import android.speech.tts.TextToSpeech;
import android.util.Log;

import com.facebook.react.bridge.ReactApplicationContext;
import com.facebook.react.bridge.ReactContextBaseJavaModule;
import com.facebook.react.bridge.ReactMethod;

import java.util.Locale;

/**
 * TTS native module. By default this uses Android's TextToSpeech as a fallback.
 * Replace or extend this to call Meta Voice SDK for higher-quality voices.
 */
public class MetaVoiceTTSModule extends ReactContextBaseJavaModule implements TextToSpeech.OnInitListener {
    private static final String TAG = "MetaVoiceTTSModule";
    private TextToSpeech tts;
    private boolean ready = false;

    public MetaVoiceTTSModule(ReactApplicationContext reactContext) {
        super(reactContext);
        tts = new TextToSpeech(reactContext, this);
    }

    @Override
    public String getName() {
        return "MetaVoiceTTS";
    }

    @Override
    public void onInit(int status) {
        if (status == TextToSpeech.SUCCESS) {
            int res = tts.setLanguage(Locale.getDefault());
            ready = res != TextToSpeech.LANG_MISSING_DATA && res != TextToSpeech.LANG_NOT_SUPPORTED;
        } else {
            Log.w(TAG, "TTS initialization failed: " + status);
            ready = false;
        }
    }

    @ReactMethod
    public void speak(String text) {
        if (text == null || text.length() == 0) return;
        if (!ready) {
            Log.w(TAG, "TTS not ready, cannot speak.");
            return;
        }
        tts.speak(text, TextToSpeech.QUEUE_FLUSH, null, "VisionGuideUtterance");
    }
}
