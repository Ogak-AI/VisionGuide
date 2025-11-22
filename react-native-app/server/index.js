const express = require('express');
const bodyParser = require('body-parser');
const cors = require('cors');
const axios = require('axios');

const app = express();
app.use(cors());
app.use(bodyParser.json({limit: '10mb'}));

const OPENAI_URL = 'https://api.openai.com/v1/chat/completions';
const PORT = process.env.PORT || 3000;

if (!process.env.OPENAI_API_KEY) {
  console.warn('Warning: OPENAI_API_KEY not set. Set this env var before starting the server.');
}

app.post('/analyze', async (req, res) => {
  try {
    const { base64 } = req.body;
    if (!base64) return res.status(400).json({ error: 'base64 is required' });

    const dataUrl = `data:image/jpeg;base64,${base64}`;

    const payload = {
      model: 'gpt-4o',
      messages: [
        { role: 'system', content: 'You are a guide for a blind user. Briefly describe obstacles, reading text if present. Be concise.' },
        { role: 'user', content: 'Please briefly describe the image for a blind user.', image_url: dataUrl }
      ]
    };

    const response = await axios.post(OPENAI_URL, payload, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${process.env.OPENAI_API_KEY}`
      }
    });

    return res.json(response.data);
  } catch (err) {
    console.error(err.response ? err.response.data : err.message);
    return res.status(500).json({ error: 'OpenAI request failed', detail: err.response ? err.response.data : err.message });
  }
});

app.listen(PORT, () => {
  console.log(`VisionGuide proxy listening on port ${PORT}`);
});
