# 1) Build-stage: собираем SPA
FROM node:18-alpine AS build

WORKDIR /app

# Копируем только нужное для npm ci
COPY package.json package-lock.json* ./

RUN npm ci

# Копируем всё остальное и билдим
COPY . .
RUN npm run build

# 2) Prod-stage: берём nginx и копируем в папку серверных статики
FROM nginx:alpine

# (опционально) ваш кастомный конфиг nginx:
COPY nginx.conf /etc/nginx/nginx.conf

# Статика из предыдущего этапа
COPY --from=build /app/build /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
