import {NativeModules} from 'react-native';

// Wrapper around the native Quest passthrough capture module.
// Native implementation must provide a method `captureFrame()` that returns a Base64 JPEG string (512x512 ideally).

const {QuestPassthrough} = NativeModules;

async function captureFrame() {
  if (QuestPassthrough && QuestPassthrough.captureFrame) {
    // Native module should return a Promise<string>
    return await QuestPassthrough.captureFrame();
  }

  // Fallback: no native module available
  throw new Error('QuestPassthrough native module not implemented. Implement Android native module to capture passthrough frames.');
}

export default {captureFrame};
