FROM node:lts-alpine

WORKDIR /usr/src/app

COPY ["package.json", "package-lock.json", "./"]

RUN npm install

COPY . .

EXPOSE 3000

# RUN chown -R node /usr/src/app

# USER node

ENTRYPOINT ["npm", "run", "docker"]
