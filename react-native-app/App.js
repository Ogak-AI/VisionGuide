import React, {useState} from 'react';
import {SafeAreaView, StyleSheet, Text, TouchableOpacity, View, ActivityIndicator, Alert} from 'react-native';
import SceneAnalyzer from './src/SceneAnalyzer';
import QuestNative from './src/QuestNativeModule';
import TTS from './src/TTSWrapper';

export default function App() {
  const [loading, setLoading] = useState(false);

  async function onCapturePress() {
    try {
      setLoading(true);
      // Capture frame from native Quest passthrough (native module)
      const base64 = await QuestNative.captureFrame();
      if (!base64) {
        Alert.alert('Capture failed', 'No image captured from device.');
        setLoading(false);
        return;
      }

      const description = await SceneAnalyzer.analyzeScene(base64);
      if (!description) {
        Alert.alert('Analysis failed', 'No description returned from AI.');
        setLoading(false);
        return;
      }

      // Speak via TTS
      await TTS.speak(description);
      setLoading(false);
    } catch (e) {
      console.warn(e);
      Alert.alert('Error', e.message || String(e));
      setLoading(false);
    }
  }

  return (
    <SafeAreaView style={styles.container}>
      <Text style={styles.title}>VisionGuide (Quest 3)</Text>
      <View style={styles.buttonRow}>
        <TouchableOpacity style={styles.button} onPress={onCapturePress} disabled={loading}>
          <Text style={styles.buttonText}>Capture & Describe</Text>
        </TouchableOpacity>
      </View>
      {loading && <ActivityIndicator size="large" style={{marginTop:20}} />}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {flex: 1, alignItems: 'center', justifyContent: 'center', padding: 24},
  title: {fontSize: 20, fontWeight: '600', marginBottom: 24},
  buttonRow: {flexDirection: 'row'},
  button: {backgroundColor: '#0066CC', padding: 16, borderRadius: 8},
  buttonText: {color: '#fff', fontWeight: '600'}
});
