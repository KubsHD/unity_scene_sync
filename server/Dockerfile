FROM node:18-alpine as base


FROM base as builder
WORKDIR /app
COPY . .

RUN yarn --frozen-lockfile
RUN yarn build

# download runtime-only dependencies
CMD ["rm", "-rf", "node_modules"]
RUN yarn --frozen-lockfile --production

FROM base as runner
ENV NODE_ENV=production
WORKDIR /app

COPY --from=builder /app/dist ./dist
COPY --from=builder /app/node_modules ./node_modules

EXPOSE 3060
CMD ["node", "dist/main.js"]