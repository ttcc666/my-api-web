import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'
import Components from 'unplugin-vue-components/vite'
import { AntDesignVueResolver } from 'unplugin-vue-components/resolvers'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
    Components({
      resolvers: [AntDesignVueResolver({ importStyle: false })],
    }),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: 3000,
    host: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false,
      },
    },
  },
  build: {
    // 生产环境构建优化
    target: 'es2015',
    minify: 'esbuild',
    // 代码分割策略
    rollupOptions: {
      output: {
        // 手动分块策略
        manualChunks: {
          // Vue 核心库
          'vue-vendor': ['vue', 'vue-router', 'pinia'],
          // UI 库
          'ant-design-vue': ['ant-design-vue'],
          // 工具库
          utils: ['axios'],
          // 图标库
          icons: ['@ant-design/icons-vue'],
        },
        // 用于从入口点创建的块的打包输出格式[name]表示文件名,[hash]表示该文件内容hash值
        entryFileNames: 'assets/js/[name]-[hash].js',
        // 用于命名代码拆分时创建的共享块的输出命名
        chunkFileNames: 'assets/js/[name]-[hash].js',
        // 用于输出静态资源的命名，[ext]表示文件扩展名
        assetFileNames: (assetInfo) => {
          const info = assetInfo.name?.split('.') || []
          let extType = info[info.length - 1]
          // 字体文件
          if (/\.(woff2?|eot|ttf|otf)(\?.*)?$/i.test(assetInfo.name || '')) {
            extType = 'fonts'
          }
          // 图片文件
          if (/\.(png|jpe?g|gif|svg|ico|webp)(\?.*)?$/i.test(assetInfo.name || '')) {
            extType = 'images'
          }
          // CSS 文件
          if (/\.css$/i.test(assetInfo.name || '')) {
            return `assets/css/[name]-[hash].${extType}`
          }
          return `assets/${extType}/[name]-[hash].[ext]`
        },
      },
    },
    // chunk 大小警告的限制（KB）
    chunkSizeWarningLimit: 2000,
    // 启用/禁用 CSS 代码拆分
    cssCodeSplit: true,
    // 构建后是否生成 source map 文件
    sourcemap: false,
    // 设置最终构建的浏览器兼容目标
    assetsInlineLimit: 4096,
  },
  // 优化依赖预构建
  optimizeDeps: {
    include: ['vue', 'vue-router', 'pinia', 'axios', 'ant-design-vue', '@ant-design/icons-vue'],
    exclude: ['vite-plugin-vue-devtools'],
  },
})
