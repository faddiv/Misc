import * as path from "path";
import webpackMerge from "webpack-merge";
import { CleanWebpackPlugin } from "clean-webpack-plugin";

import * as runMode from "./build-config/run-mode";
import scssConfigFactory from "./build-config/scss-config";
/*import htmlConfigFactory from "./build-config/html-config";*/
import vendorConfig from "./build-config/vendor-config";
import typescriptConfig from "./build-config/typescript-config";
import variavlesConfigFactory from "./build-config/variables-config";
import entryPointConfigFactory from "./build-config/entry-point-config";

const contentBase = "Modules";
const outputPath = "wwwroot";
const scssConfig = scssConfigFactory(runMode.cssOutput);
//const htmlConfig = htmlConfigFactory("./" + contentBase + "/index.html");
const variablesConfig = variavlesConfigFactory(runMode.mode);
const entryPointConfig = entryPointConfigFactory(contentBase, /index\.ts/);

export default function () {
    return [webpackMerge(scssConfig, /*htmlConfig, */vendorConfig,
        typescriptConfig, variablesConfig, entryPointConfig, 
        {
            mode: runMode.mode,
            output: {
                path: path.resolve(outputPath),
                publicPath: "/",
                filename: runMode.jsOutput
            },
            plugins: [
                new CleanWebpackPlugin({
                    verbose: true
                })
            ],
            devtool: runMode.mode === "production" ? undefined : 'inline-source-map',
            /*devServer: {
                contentBase: contentBase,
                /**
                 * default false, if true .scss and .html changes trigger reload and also triggers FULL reload.
                 *//*
                watchContentBase: true,
                port: 4200,
                historyApiFallback: true,
                proxy: {
                    "/api/*": {
                        target: "http://localhost:56491",
                        secure: false
                    }
                }
            }*/
        })
    ];
};
