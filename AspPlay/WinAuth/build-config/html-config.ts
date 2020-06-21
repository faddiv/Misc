import HtmlWebpackPlugin from "html-webpack-plugin";
import { Configuration } from "webpack";

/**
 * This file contains the html related webpack config.
 */
export default function (templatePath: string): Configuration {
    var indexHtml = new HtmlWebpackPlugin({
        template: templatePath
    });
    return {
        module: {
            rules: [
                {
                    test: /\.html$/,
                    use: [
                        {
                            loader: "html-loader",
                            options: {
                                minimize: true,
                                removeAttributeQuotes: false,
                                keepClosingSlash: true,
                            }
                        }
                    ]
                }
            ]
        },
        plugins: [
            indexHtml
        ]
    };
}
