
FROM node:22 as build

WORKDIR /app

COPY package.json package.json
COPY package-lock.json package-lock.json

RUN npm install

COPY . .

RUN npm run build

FROM macbre/nginx-http3 as production

COPY --from=build /app/default.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist/identity-admin/browser /usr/share/nginx/html

# Expose the default nginx port
EXPOSE 80 443/tcp 443/udp

CMD ["nginx", "-g", "daemon off;"]
