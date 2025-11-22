// SceneAnalyzer: handles OpenAI calls. By default this module uses a local proxy
// (useful for development) which avoids shipping the OpenAI key to the client.
// Set PROXY_URL to a reachable endpoint (default points to Android emulator '10.0.2.2').

const OPENAI_URL = 'https://api.openai.com/v1/chat/completions';
const PROXY_URL = typeof process !== 'undefined' && process.env && process.env.PROXY_URL
  ? process.env.PROXY_URL
  : 'http://10.0.2.2:3000/analyze';

async function analyzeSceneWithKey(base64Image, openAIKey) {
  if (!openAIKey) throw new Error('OpenAI key not provided.');
  if (!base64Image) throw new Error('No image data provided.');

  const dataUrl = `data:image/jpeg;base64,${base64Image}`;

  const payload = {
    model: 'gpt-4o',
    messages: [
      { role: 'system', content: 'You are a guide for a blind user. Briefly describe obstacles, reading text if present. Be concise.' },
      { role: 'user', content: 'Please briefly describe the image for a blind user.', image_url: dataUrl }
    ]
  };

  const res = await fetch(OPENAI_URL, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${openAIKey}`
    },
    body: JSON.stringify(payload)
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(`OpenAI error: ${res.status} ${text}`);
  }

  const json = await res.json();
  if (json && json.choices && json.choices.length > 0 && json.choices[0].message) {
    return json.choices[0].message.content;
  }
  return null;
}

async function analyzeSceneViaProxy(base64Image, proxyUrl) {
  if (!base64Image) throw new Error('No image data provided.');
  const url = proxyUrl || PROXY_URL;

  const res = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ base64: base64Image })
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(`Proxy error: ${res.status} ${text}`);
  }

  const json = await res.json();
  // Proxy returns full OpenAI response. Extract assistant content if present.
  if (json && json.choices && json.choices.length > 0 && json.choices[0].message) {
    return json.choices[0].message.content;
  }
  return null;
}

// Default export: analyzeScene uses the proxy by default. For direct OpenAI usage,
// call analyzeSceneWithKey(base64, apiKey).
export default {
  analyzeScene: (base64) => analyzeSceneViaProxy(base64, PROXY_URL),
  analyzeSceneWithKey
};
