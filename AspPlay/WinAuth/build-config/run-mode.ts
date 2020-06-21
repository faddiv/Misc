const devServer = process.argv.filter(v => v.indexOf("webpack-dev-server") > -1).length > 0;

type WebpackMode = "development" | "production" | "none";
let mode: WebpackMode = "development";
for (let index = 0; index < process.argv.length; index++) {
    const element = process.argv[index];
    if (element.startsWith("--mode=")) {
        mode = element.substr("--mode=".length) as WebpackMode;
    }
}

console.log("build mode: " + mode);
console.log("dev server mode: " + devServer);

const cssOutput = devServer ? "[name].css" : "[name]-[chunkhash].css";
const jsOutput = devServer ? "[name].js" : "[name]-[chunkhash].js";
/**
 * This file determines particular settings based on how the program started.
 */
export {
    mode,
    devServer,
    cssOutput,
    jsOutput
};
