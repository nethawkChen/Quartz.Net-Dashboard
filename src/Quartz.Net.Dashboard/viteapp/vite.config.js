import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    {
      name: 'html-transform',
      transformIndexHtml(html) {
        return html.replace(/<title>(.*?)<\/title>/, '<title>Quartz.Net Dashboard</title>')  // 頁面 title 修改標題
      }
    }
  ],
  build: {
    outDir: '../wwwroot',  // 指定輸出目錄
    emptyOutDir: true,      // 在 build 之前清空輸出目錄
    sourcemap: true,   // 生成 source map 可以進行前端 browser debug
    // rollupOptions: {
    //   output: {  // build 後的輸出的檔案不帶有哈希值
    //     entryFileNames: 'assets/[name].js',
    //     chunkFileNames: 'assets/[name].js',
    //     assetFileNames: 'assets/[name].[ext]'
    //   }
    // }
  },
  resolve: {
    alias: {
      '@': resolve(__dirname, './src')  // 設置 @ 指向 src
    }
  }
  // server: {
  //   port: 3000   // 可以根據需要修改開發時使用的 port, 若沒有設置則預設為 5173
  // }
})
