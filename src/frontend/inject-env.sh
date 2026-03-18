#!/bin/sh

# Create the config.js file with actual environment variables
cat > /usr/share/nginx/html/config.js << EOF
window.__ENV__ = {
  VITE_API_URL: '${VITE_API_URL:-http://localhost:5000}'
}
EOF

echo "✅ Runtime configuration injected:"
echo "   VITE_API_URL = ${VITE_API_URL:-http://localhost:5000}"
