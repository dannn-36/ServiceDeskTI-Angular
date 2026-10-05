# Interfaz de ServiceDesk TI: Angular compilado y servido por nginx.
# nginx también reenvía /api y /chathub a la API, así que navegador, API y cookie
# comparten origen (sin CORS y con la cookie SameSite=Strict funcionando).

FROM node:20-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund

COPY . .
RUN npx ng build --configuration production


FROM nginx:1.27-alpine AS final

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/servicedeskng.client/browser /usr/share/nginx/html

EXPOSE 80
