const common = require("./webpack.common");

const CONFIG = {
    // The tags to include the generated JS and CSS will be automatically injected in the HTML template
    // See https://github.com/jantimon/html-webpack-plugin
    indexHtmlTemplate: './tests/Client/index.html',
    fsharpEntry: "./tests/Client/output/App.Test.js",
    outputDir: "./tests/Client/dist",
    assetsDir: "./src/Client/public",
    devServerPort: 8080
}

module.exports = common.getConfig(CONFIG);
