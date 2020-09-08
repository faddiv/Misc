const webpack = require('webpack');

/**
 * This file inject variables into the code.
 */
module.exports = function (mode) {
    var options = {
        "process.env.NODE_ENV": JSON.stringify(mode)
    };

    return {
        plugins: [
            new webpack.DefinePlugin(options)
        ]
    };
}