const ts = require("awesome-typescript-loader");
const typescriptCheckerPlugin = new ts.CheckerPlugin();
const typescriptConfigPathsPlugin = new ts.TsConfigPathsPlugin();

console.log("tsconfig path: ",typescriptConfigPathsPlugin.configFilePath);


/**
 * This file contains the typescript related webpack config.
 */
module.exports =
    {
        module: {
            rules: [
                {
                    test: /\.(ts|tsx)(\?|$)/,
                    use: [
                        {
                            loader: "awesome-typescript-loader"
                        }
                    ]
                }
            ]
        },
        resolve: {
            plugins: [
                typescriptConfigPathsPlugin
            ],
            extensions: [".ts", ".tsx", ".js"]
        }
    };