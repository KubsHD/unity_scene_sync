require("dotenv").config();

export const env = {
  NODE_ENV: process.env.NODE_ENV || "development",
  HOST: process.env.HOST || "localhost",
  SECRET_KEY:
    process.env.NODE_ENV === "production" ? process.env.SECRET_KEY : "asd",
  PORT: process.env.PORT || 3000,
};
