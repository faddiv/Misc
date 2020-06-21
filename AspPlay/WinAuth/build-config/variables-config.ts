import { Configuration, DefinePlugin } from "webpack";

/**
 * This file inject variables into the code.
 */
export default function (mode: string): Configuration {
    var options = {
        "process.env.NODE_ENV": JSON.stringify(mode)
    };

    return {
        plugins: [
            new DefinePlugin(options)
        ]
    };
}