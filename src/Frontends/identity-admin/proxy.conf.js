module.exports = {
  "/identity": {
    target:
      process.env["services__identity-server__http__0"] ||
      process.env["services__identity-server__https__0"],
    secure: process.env["NODE_ENV"] !== "development",
    pathRewrite: {
      "^/identity": ""
    },
    changeOrigin: true,
    logLevel: "debug"
  },

  "/seq": {
    target:
      process.env["ConnectionStrings__Seq"],
    secure: process.env["NODE_ENV"] !== "development",
    pathRewrite: {
      "^/seq": ""
    },
    changeOrigin: true,
    logLevel: "debug"
  },


};
console.debug(module.exports);
