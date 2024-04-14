require("dotenv").config();

export const env = {
  NODE_ENV: process.env.NODE_ENV || "development",
  HOST: process.env.HOST || "localhost",
  SECRET_KEY: process.env.SECRET_KEY,
  PORT: process.env.PORT || 3000,
};
