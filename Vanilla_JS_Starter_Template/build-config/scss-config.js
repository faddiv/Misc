var MiniCssExtractPlugin = require("mini-css-extract-plugin");

/**
 * This file contains the style related webpack config. (css, and assets)
 */
module.exports = function(cssOutput) {
    var extractTextPlugin = new MiniCssExtractPlugin({
        filename: cssOutput
    });
    return {
        module: {
            rules: [
                {
                    test: /\.scss$/,
                    use: [MiniCssExtractPlugin.loader, "css-loader", "sass-loader"]
                },
                {
                    test: /\.(png|jpg|gif|woff|woff2|eot|ttf|svg)(\?|$)/,
                    use: [
                        {
                            loader: "file-loader",// TODO url-loader with explicit fallback come after 1.0.1 version. Recomended for a more general file handling.
                            options: {
                                name: "[name]-[hash].[ext]"
                            }
                        }
                    ]
                }
            ]
        },
        plugins: [
            extractTextPlugin
        ]
    };
};
    