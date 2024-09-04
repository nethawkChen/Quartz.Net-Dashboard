const { defineConfig } = require("@vue/cli-service");
module.exports = defineConfig({
  transpileDependencies: true,
  outputDir: "../wwwwroot",
  configureWebpack: {
    devtool: "source-map"
  },
  //頁面 title
  chainWebpack: config => {
    config.plugin("html").tap(args => {
      args[0].title = "Quartz.Net Dashboard";
      return args;
    });
  }
});
