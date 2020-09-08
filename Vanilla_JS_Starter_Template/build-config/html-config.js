const HtmlWebpackPlugin = require('html-webpack-plugin');


/**
 * This file contains the html related webpack config.
 */
module.exports = function (templatePath) {
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
                                minimize: true
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
