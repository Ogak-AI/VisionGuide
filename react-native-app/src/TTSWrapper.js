import {NativeModules} from 'react-native';

let TTSModule = null;
try {
  // Prefer react-native-tts if available
  const RNTTS = require('react-native-tts');
  TTSModule = {
    speak: (text) => RNTTS.speak(text)
  };
} catch (e) {
  // React Native TTS not present; fallback to native Meta Voice module
  const {MetaVoiceTTS} = NativeModules;
  if (MetaVoiceTTS && MetaVoiceTTS.speak) {
    TTSModule = {
      speak: (text) => MetaVoiceTTS.speak(text)
    };
  } else {
    TTSModule = {
      speak: (text) => { console.warn('No TTS available (react-native-tts or MetaVoiceTTS).'); }
    };
  }
}

export default {
  speak: async (text) => {
    if (!text) return;
    try {
      await TTSModule.speak(text);
    } catch (e) {
      console.warn('TTS speak failed', e);
    }
  }
};
