FROM node:18 AS base

WORKDIR /app

COPY package.json package-lock.json ./
ARG API_URL

ENV NEXT_PUBLIC_BACKEND_URL=${API_URL}
RUN npm install

COPY . .

RUN npm run build
EXPOSE 3000

CMD ["npm", "run", "start"]