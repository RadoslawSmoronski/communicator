# 1. node
FROM node:20-alpine AS build-stage
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

# 2. nqinx
FROM nginx:stable-alpine
# Copy react build to nginx container
COPY --from=build-stage /app/dist /usr/share/nginx/html

COPY docker/nginx.conf /etc/nginx/templates/default.conf.template

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]