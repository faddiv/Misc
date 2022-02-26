const { createProxyMiddleware } = require("http-proxy-middleware");

const target = "http://localhost:58300";

const context = ["/api"];

module.exports = function (app) {
  try {
    const appProxy = createProxyMiddleware(context, {
      target: target,
      xfwd: true,
      preserveHeaderKeyCase: true
    });
    app.use(appProxy);
  } catch (error) {
    console.log(error);
  }
};
