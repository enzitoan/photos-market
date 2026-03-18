// Utility function to get API URL from runtime config or environment
export const getApiUrl = () => {
  // First try runtime config (injected at container startup)
  if (window.__ENV__ && window.__ENV__.VITE_API_URL) {
    return window.__ENV__.VITE_API_URL
  }
  // Fallback to build-time env var or localhost
  return import.meta.env.VITE_API_URL || 'http://localhost:5000'
}

// For convenience, export with /api suffix
export const getApiBaseUrl = () => {
  const url = getApiUrl()
  return url.endsWith('/api') ? url : `${url}/api`
}
