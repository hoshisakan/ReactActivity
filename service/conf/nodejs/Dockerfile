FROM node:20.1-alpine3.16

ARG NODEJS_ENV

ENV NODE_ENV=${NODEJS_ENV}

RUN npm install -g create-react-app

WORKDIR /app