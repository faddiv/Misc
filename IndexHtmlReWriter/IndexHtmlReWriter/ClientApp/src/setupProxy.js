const { IncomingMessage, ServerResponse, ClientRequest } = require("http");
const createProxyMiddleware = require("http-proxy-middleware");
const { env } = require("process");
var Agent = require('agentkeepalive');

var agent =  new Agent({
  maxSockets: 100,
  keepAlive: true,
  maxFreeSockets: 10,
  keepAliveMsecs:1000,
  timeout: 60000,
  keepAliveTimeout: 30000 // free socket keepalive for 30 seconds
});
const target = env.ASPNETCORE_HTTPS_PORT
  ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`
  : env.ASPNETCORE_URLS
  ? env.ASPNETCORE_URLS.split(";")[0]
  : "http://localhost:31578";

const context = ["/weatherforecast", "/Fallback"];

module.exports = function (app) {
  const appProxy = createProxyMiddleware(context, {
    target: target,
    xfwd: true,
    preserveHeaderKeyCase: true,
    onProxyReq: onProxyReq,
    onProxyRes: onProxyRes,
    agent: agent
  });

  app.use(appProxy);
  app.use(loggingMiddleware);
};

/**
 *
 * @param {ClientRequest} proxyReq
 * @param {IncomingMessage} req
 * @param {ServerResponse} res
 */
function onProxyReq(proxyReq, req, res) {
  console.log("Proxy request started", req.method, req.url, "->", proxyReq.path);
}
/**
 *
 * @param {IncomingMessage} proxyRes
 * @param {IncomingMessage} req
 * @param {ServerResponse} res
 */
function onProxyRes(proxyRes, req, res) {
  var key = 'www-authenticate';
  proxyRes.headers[key] = proxyRes.headers[key] && proxyRes.headers[key].split(',');
  console.log("Proxy request finished", res.statusCode, res.statusMessage);
}
/**
 *
 * @param {IncomingMessage} req
 * @param {ServerResponse} res
 * @param {() => void} next
 */
function loggingMiddleware(req, res, next) {
  console.log("Request started", req.method, req.url);
  next();
  console.log("Request finished", res.statusCode, res.statusMessage);
}
