FROM node:lts-alpine

WORKDIR /usr/src/app

COPY ["package.json", "package-lock.json", "./"]

RUN npm install

COPY . .

EXPOSE 3000

# RUN chown -R node /usr/src/app

# USER node

ENTRYPOINT ["node","--inspect=0.0.0.0:9229","./server/index.js"]
