const Webpack = require('webpack');
var packagesConfig = require("../package.json");

var vendor = Object.getOwnPropertyNames(packagesConfig.dependencies);
console.log("vendor:", vendor);

/**
 * This file contains the 3rd party libraries related webpack config.
 */
module.exports = {
    plugins: [
        new Webpack.ContextReplacementPlugin(/moment[\\\/]locale$/, /^\.\/(hu)$/)
    ],
    optimization: {
        splitChunks: {
            chunks: "all",
            name: "vendor"
        }
    }
}