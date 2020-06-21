import { Configuration, ContextReplacementPlugin } from "webpack";
var packagesConfig = require("../package.json");

var vendor = Object.getOwnPropertyNames(packagesConfig.dependencies);
console.log("vendor:", vendor);

/**
 * This file contains the 3rd party libraries related webpack config.
 */
export default {
    plugins: [
        new ContextReplacementPlugin(/moment[\\\/]locale$/, /^\.\/(hu)$/)
    ],
    optimization: {
        splitChunks: {
            chunks: "all",
            name: "vendor"
        }
    }
} as Configuration