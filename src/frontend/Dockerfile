# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Build the application
RUN npm run build

# Production stage
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Remove default nginx static assets
RUN rm -rf ./*

# Copy built assets from build stage
COPY --from=build /app/dist .

# Copy custom nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy the environment injection script
COPY inject-env.sh /docker-entrypoint.d/40-inject-env.sh
RUN chmod +x /docker-entrypoint.d/40-inject-env.sh

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
