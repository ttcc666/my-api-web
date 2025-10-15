# 前端配置说明文档

本文档旨在详细说明前端项目中的各项配置，以便于开发人员理解、维护和扩展。

## 目录

- [前端配置说明文档](#前端配置说明文档)
  - [目录](#目录)
  - [1. 环境变量 (`.env.*`)](#1-环境变量-env)
    - [变量说明](#变量说明)
    - [环境文件](#环境文件)
  - [2. 构建与开发服务器配置 (`vite.config.ts`)](#2-构建与开发服务器配置-viteconfigts)
    - [主要配置项](#主要配置项)
  - [3. 应用核心配置 (`src/config/index.ts`)](#3-应用核心配置-srcconfigindexts)
    - [`apiConfig` - API 配置](#apiconfig---api-配置)
    - [`appConfig` - 应用配置](#appconfig---应用配置)
    - [`storageConfig` - 本地存储配置](#storageconfig---本地存储配置)
    - [`cacheConfig` - 缓存配置](#cacheconfig---缓存配置)
    - [`routeConfig` - 路由配置](#routeconfig---路由配置)
    - [`paginationConfig` - 分页配置](#paginationconfig---分页配置)
    - [`uiConfig` - UI 组件配置](#uiconfig---ui-组件配置)
    - [`monitorConfig` - 监控配置](#monitorconfig---监控配置)
  - [4. 项目脚本与依赖 (`package.json`)](#4-项目脚本与依赖-packagejson)
    - [`engines` - 环境要求](#engines---环境要求)
    - [`scripts` - 项目脚本](#scripts---项目脚本)
    - [主要依赖 (`dependencies`)](#主要依赖-dependencies)
    - [主要开发依赖 (`devDependencies`)](#主要开发依赖-devdependencies)

---

## 1. 环境变量 (`.env.*`)

这部分将说明用于定义各环境（开发、预发布、生产）特定变量的 `.env` 文件。

项目使用 `.env` 文件来管理不同环境下的配置变量。Vite 会根据当前的 `mode`（例如 `development`、`staging`）自动加载相应的 `.env` 文件（如 `.env.development`）。

### 变量说明

| 变量名              | 示例值                      | 说明                                                |
| :------------------ | :-------------------------- | :-------------------------------------------------- |
| `VITE_API_BASE_URL` | `http://localhost:5000/api` | 后端 API 的基础请求地址。                           |
| `VITE_APP_TITLE`    | `My API Web`                | 显示在浏览器标签页上的应用标题。                    |
| `VITE_APP_VERSION`  | `1.0.0`                     | (可选) 应用的版本号。                               |
| `VITE_ENABLE_MOCK`  | `false`                     | (可选) 是否启用 Mock 数据，用于前端独立开发和测试。 |

### 环境文件

- **`.env.development`**: `npm run dev` 命令默认加载的开发环境配置。
- **`.env.staging`**: 预发布环境的配置。
- **`.env.example`**: 提供了一个所有必需环境变量的模板。在搭建新环境时，应复制此文件为 `.env.local` 并填入相应的值，`.env.local` 的配置会覆盖所有其他 `.env` 文件。

## 2. 构建与开发服务器配置 (`vite.config.ts`)

这部分将详细解释 Vite 的配置，包括插件、别名、代理、构建优化等。

项目使用 Vite 作为构建工具。核心配置文件是 `vite.config.ts`。

### 主要配置项

- **`plugins`**: 配置 Vite 插件。
  - `vue()`: 启用 Vue 3 支持。
  - `vueDevTools()`: 集成 Vue 开发者工具。
  - `Components({ resolvers: [NaiveUiResolver()] })`: 自动按需引入 `naive-ui` 组件，无需手动 `import`。

- **`resolve.alias`**: 配置路径别名。
  - `'@'`: 指向 `src` 目录，方便在代码中引用模块，例如 `import Home from '@/views/Home.vue'`。

- **`server`**: 开发服务器配置。
  - `port`: 开发服务器监听的端口，设置为 `3000`。
  - `host`: 设置为 `true`，允许通过 IP 地址访问开发服务器。
  - `proxy`: 配置开发环境的 API 请求代理。
    - `'/api'`: 将所有以 `/api` 开头的请求代理到 `http://localhost:5000`，以解决跨域问题。

- **`build`**: 生产环境构建配置。
  - `target`: 编译目标设置为 `es2015`，以获得更好的浏览器兼容性。
  - `minify`: 使用 `esbuild`进行代码压缩，速度更快。
  - `rollupOptions.output`: 自定义 Rollup 的输出配置，实现了精细的代码分割（`manualChunks`）和资源文件（JS, CSS, images, fonts）的分类存放。
  - `chunkSizeWarningLimit`: 将大代码块的警告阈值提高到 `2000KB`。
  - `sourcemap`: 在生产环境中禁用了 `sourcemap`，以减少最终包的体积并提高安全性。

- **`optimizeDeps`**: 依赖预构建配置。
  - `include`: 强制预构建指定的依赖项（如 `vue`, `axios`），以提升开发环境的启动速度和热更新性能。

## 3. 应用核心配置 (`src/config/index.ts`)

这部分将深入介绍应用内部的中央配置模块，涵盖 API、路由、缓存、UI 等业务逻辑相关设置。

文件 `src/config/index.ts` 是应用的中央配置枢纽，它将所有业务相关的配置项整合并导出，提供了统一的配置管理方案。

### `apiConfig` - API 配置

- `baseURL`: API 请求的基础 URL，其值优先取自环境变量 `VITE_API_BASE_URL`。
- `timeout`: 请求超时时间（10秒）。
- `tokenRefreshBuffer`: Token 刷新缓冲时间（30秒），在 Token 过期前此段时间内会自动触发刷新。
- `retryCount` / `retryDelay`: 请求失败时的重试次数（3次）和重试延迟（1秒）。

### `appConfig` - 应用配置

- `title`: 应用标题，值优先取自 `VITE_APP_TITLE`。
- `version`: 应用版本，值优先取自 `VITE_APP_VERSION`。
- `isDev` / `isProd` / `mode`: 从 Vite 的 `import.meta.env` 中获取当前的环境状态。

### `storageConfig` - 本地存储配置

- `prefix`: 存储在 `LocalStorage` 中的 Key 的统一前缀，默认为 `app_`。
- `tokenKey` / `refreshTokenKey` / `userKey`: 定义了用于存储 Token 和用户信息的 Key 名称。
- `enableEncryption`: 控制是否启用加密存储，默认为生产环境（`PROD`）时开启。

### `cacheConfig` - 缓存配置

- `permissionCacheExpiry` / `menuCacheExpiry`: 权限和菜单数据的缓存过期时间（30分钟）。
- `permissionCacheKey` / `menuCacheKey`: 定义了权限和菜单缓存的 Key 名称。

### `routeConfig` - 路由配置

- `homePath` / `loginPath` / `forbiddenPath` / `notFoundPath`: 定义了应用内核心页面的路由路径。
- `publicPages`: 定义了无需登录即可访问的公共页面数组。

### `paginationConfig` - 分页配置

- `pageSize` / `pageSizeOptions`: 定义了表格等分页场景下的默认每页条数和可选条数。

### `uiConfig` - UI 组件配置

- `messageDuration` / `notificationDuration`: 全局消息和通知的默认显示时长。
- `tableMaxHeight`: 表格组件的最大高度。
- `defaultTheme`: 默认主题（`light` 或 `dark`）。

### `monitorConfig` - 监控配置

- `sentryDsn`: 用于集成 Sentry 错误监控服务的 DSN，值来自 `VITE_SENTRY_DSN`。
- `enablePerformanceMonitor`: 控制是否启用性能监控，默认为生产环境开启。

## 4. 项目脚本与依赖 (`package.json`)

这部分将解释 `package.json` 文件中的关键信息，包括项目脚本的用途和主要依赖库的作用。

`package.json` 是 Node.js 项目的清单文件，定义了项目元数据、脚本命令、依赖关系等。

### `engines` - 环境要求

- `node`: 指定了项目运行所需的 Node.js 版本范围，确保了开发环境的一致性。

### `scripts` - 项目脚本

- `dev`: 启动 Vite 开发服务器，用于本地开发。
- `build`: 构建生产版本的应用。它会先执行 `type-check`，然后执行 `build-only`。
- `preview`: 在本地预览生产构建后的应用。
- `test:unit`: 运行 Vitest 单元测试。
- `lint`: 使用 ESLint 检查代码规范并自动修复部分问题。
- `format`: 使用 Prettier 格式化 `src` 目录下的所有代码。

### 主要依赖 (`dependencies`)

- `vue` / `vue-router` / `pinia`: Vue.js 全家桶，分别负责视图渲染、路由管理和状态管理。
- `axios`: 一个强大的 HTTP 客户端，用于与后端 API 进行通信。
- `naive-ui`: 项目使用的主要 UI 组件库。

### 主要开发依赖 (`devDependencies`)

- `vite`: 项目的构建工具和开发服务器。
- `typescript`: 为项目提供静态类型检查。
- `eslint` / `prettier`: 代码质量和风格工具，用于保证代码的一致性和可读性。
- `vitest` / `@vue/test-utils`: 单元测试框架和工具。
