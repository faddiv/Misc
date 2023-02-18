FROM node:lts-alpine

VOLUME [ "/usr/src/app" ]
WORKDIR /usr/src/app

# COPY ["package.json", "package-lock.json", "./"]
RUN ls -l

# RUN npm install

# COPY . .

EXPOSE 3000

# RUN chown -R node /usr/src/app

# USER node

# ENTRYPOINT [ "/bin/sh" ]
ENTRYPOINT ["npm", "run", "docker"]
