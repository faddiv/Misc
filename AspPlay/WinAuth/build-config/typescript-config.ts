import { Configuration } from "webpack";

/**
 * This file contains the typescript related webpack config.
 */
export default {
    module: {
        rules: [
            {
                test: /\.(ts|tsx)(\?|$)/,
                use: [
                    {
                        loader: "ts-loader"
                    }
                ]
            }
        ]
    },
    resolve: {
        extensions: [".ts",".js",".css",".scss"]
    }
} as Configuration;
