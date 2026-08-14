FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

# SvelteKit resolves $env/static/public at build time, so it must be set here even though the
# runtime value (baked into the client bundle) gets overridden per-environment via CD.
ARG PUBLIC_API_BASE_URL=https://localhost:7001
ENV PUBLIC_API_BASE_URL=${PUBLIC_API_BASE_URL}
RUN npm run build

FROM node:22-alpine AS final
WORKDIR /app
ENV NODE_ENV=production PORT=3000
COPY --from=build /app/build ./build
COPY --from=build /app/package.json ./package.json
EXPOSE 3000
CMD ["node", "build"]
