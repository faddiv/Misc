const path = require("path");
const { merge } = require('webpack-merge');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const contentBase = "src";
const outputPath = "wwwroot";

var runMode = require("./build-config/run-mode");
const scssConfig = require("./build-config/scss-config")(runMode.cssOutput);
const htmlConfig = require("./build-config/html-config")("./" + contentBase + "/index.html");
const vendorConfig = require("./build-config/vendor-config");
const variablesConfig = require("./build-config/variables-config")(runMode.mode);
const typescriptConfig = require("./build-config/typescript-config");

module.exports = function () {
    return [merge(scssConfig, htmlConfig, vendorConfig, typescriptConfig, variablesConfig,
        {
            entry: {
                "bundle": "./" + contentBase + "/index.ts"
            },
            output: {
                path: path.resolve(outputPath),
                publicPath: "/",
                filename: runMode.jsOutput
            },
            plugins: [
                new CleanWebpackPlugin({
                    dry: true,
                    cleanOnceBeforeBuildPatterns: [outputPath + "/*"]
                })
            ],
            devtool: runMode.mode === "production" ? undefined : 'source-map',
            devServer: {
                contentBase: contentBase,
                //openPage:"?react_perf",
                /**
                 * default false, if true .scss and .html changes trigger reload and also triggers FULL reload.
                 */
                watchContentBase: true,
                port: 4200,
                historyApiFallback: true,
                proxy: {
                    "/api/*": {
                        target: "http://localhost:56491",
                        secure: false
                    }
                }
            }
        })

    ];
};
