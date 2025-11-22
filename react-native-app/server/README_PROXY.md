# VisionGuide OpenAI Proxy

Quick local proxy to forward base64 images to OpenAI chat completions (model `gpt-4o`).

Setup

```bash
cd react-native-app/server
npm install
export OPENAI_API_KEY="sk-..."
npm start
```

API

- POST `/analyze` JSON body: `{ "base64": "<BASE64_JPG_STRING>" }`
- Response: raw OpenAI response JSON.

Security

- Do not expose this server publicly without authentication.
- Use HTTPS for production, and add rate-limiting and authentication.
