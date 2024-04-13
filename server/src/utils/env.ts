require("dotenv").config();

export const env = {
  NODE_ENV: process.env.NODE_ENV || "development",
  HOST: process.env.HOST || "localhost",
  PORT: process.env.PORT || 3000,
  SECRET_KEY: process.env.SECRET_KEY,
};
