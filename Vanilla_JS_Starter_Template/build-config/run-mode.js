const devServer = process.argv.filter(v => v.indexOf("webpack-dev-server") > -1).length > 0;

let mode = "development";
for (let index = 0; index < process.argv.length; index++) {
    const element = process.argv[index];
    if (element.startsWith("--mode=")) {
        mode = element.substr("--mode=".length);
    }
}

console.log("build mode: " + mode);
console.log("dev server mode: " + devServer);

/**
 * This file determines particular settings based on how the program started.
 */
module.exports = {
    mode: mode,
    devServer: devServer,
    cssOutput: devServer ? "site.css" : "site-[chunkhash].css",
    jsOutput: devServer ? "[name].js" : "[name]-[chunkhash].js"
};