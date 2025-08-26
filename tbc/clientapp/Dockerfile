# 1) Сборка React
FROM node:18-alpine AS build
WORKDIR /app

# Жёстко фиксируем registry и включаем ретраи (помогает против 403)
ENV NPM_CONFIG_REGISTRY=https://registry.npmjs.org \
    npm_config_fetch_retries=5 \
    npm_config_fetch_retry_factor=2 \
    npm_config_fetch_retry_maxtimeout=60000 \
    npm_config_audit=false \
    npm_config_fund=false

COPY package*.json ./

# На случай если в .npmrc/секретах завёлся токен для npmjs (он ломает public-доступ)
RUN npm config delete '//registry.npmjs.org/:_authToken' || true

# Используем buildx cache (ускоряет и уменьшает «дергание» CDN)
# Требует BuildKit (у тебя он есть через docker/setup-buildx-action)
RUN --mount=type=cache,id=npm-cache,target=/root/.npm npm ci

# Копируем исходники и билдим
COPY . .
RUN npm run build

# 2) Рантайм: Nginx со статикой
FROM nginx:alpine AS runtime
COPY --from=build /app/build /usr/share/nginx/html
EXPOSE 80
ENTRYPOINT ["nginx","-g","daemon off;"]
