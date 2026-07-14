module.exports = {
  "/identity": {
    target:
      process.env["services__identity-api__https__0"] ||
      process.env["services__identity-api__http__0"],
    secure: process.env["NODE_ENV"] !== "development",
    pathRewrite: {
      "^/identity": ""
    },
    changeOrigin: true,
    logLevel: "debug"
  },

  // "/api": {
  //   target:
  //   process.env["services__accountants-api__https__0"] ||
  //   process.env["services__accountants-api__http__0"],
  //   secure: process.env["NODE_ENV"] !== "development",
  //   pathRewrite: {
  //     "^/api": "/api/v1"
  //   },
  //   changeOrigin: true,
  //   logLevel: "debug"
  // },

  // "/seq": {
  //   target:
  //     process.env["ConnectionStrings__Seq"],
  //   secure: process.env["NODE_ENV"] !== "development",
  //   pathRewrite: {
  //     "^/seq": ""
  //   },
  //   changeOrigin: true,
  //   logLevel: "debug"
  // },

};
console.debug(module.exports);
