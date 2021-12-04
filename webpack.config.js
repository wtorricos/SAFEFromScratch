const common = require("./webpack.common");

const CONFIG = {
    // The tags to include the generated JS and CSS will be automatically injected in the HTML template
    // See https://github.com/jantimon/html-webpack-plugin
    indexHtmlTemplate: './src/Client/index.html',
    fsharpEntry: "./src/Client/output/App.js",
    outputDir: "./deploy/public",
    assetsDir: './src/Client/public',
    devServerPort: 8080,
    // When using webpack-dev-server, you may need to redirect some calls
    // to a external API server. See https://webpack.js.org/configuration/dev-server/#devserver-proxy
    devServerProxy: {
        // redirect requests that start with /api/ to the server on port 8085
        '/api/**': {
            target: 'http://localhost:' + (process.env.SERVER_PROXY_PORT || "8085"),
            changeOrigin: true
        },
        // redirect websocket requests that start with /socket/ to the server on the port 8085
        '/socket/**': {
            target: 'http://localhost:' + (process.env.SERVER_PROXY_PORT || "8085"),
            ws: true
        }
    }
}

module.exports = common.getConfig(CONFIG);
