# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a **Vue 3 + TypeScript + Pinia + Naive UI** frontend application implementing a **RBAC (Role-Based Access Control)** permission management system. The application features JWT authentication, token auto-refresh, encrypted storage, and a production-ready build configuration.

**Tech Stack:**
- Vue 3.5.22 (Composition API)
- TypeScript 5.9
- Pinia 3.0 (State Management)
- Vue Router 4.5 (Routing)
- Naive UI 2.43 (UI Components)
- Axios 1.12 (HTTP Client)
- Vite 7.1 (Build Tool)
- Vitest 3.2 (Testing)

**Node Version Required:** `^20.19.0 || >=22.12.0`

---

## Development Commands

### Essential Commands

```bash
# Install dependencies
npm install

# Start development server (http://localhost:3000)
npm run dev

# Build for production (includes type-check)
npm run build

# Build only (skip type-check)
npm run build-only

# Preview production build
npm run preview

# Type checking
npm run type-check

# Lint and auto-fix
npm run lint

# Format code
npm run format

# Run unit tests
npm run test:unit

# Run tests in watch mode
npm run test:unit -- --watch

# Run specific test file
npm run test:unit -- src/stores/__tests__/auth.spec.ts
```

### Docker Deployment

```bash
# Build Docker image
docker build -t my-api-web-frontend:latest .

# Start with docker-compose
docker-compose up -d

# View logs
docker-compose logs -f frontend

# Stop services
docker-compose down
```

---

## Architecture Overview

### Application Bootstrap Flow

The application initialization follows a strict sequence defined in `src/app.ts` and `src/main.ts`:

1. **Create Vue App** → Creates Pinia store instance
2. **Register Custom Directives** → Registers `v-permission` directive
3. **Setup API Interceptors** → Configures axios request/response interceptors with token refresh logic
4. **Initialize Auth State** → Loads tokens and user data from encrypted storage
5. **Configure Router** → Attaches router with permission guards
6. **Mount Application** → Mounts to DOM

**Critical:** API interceptors MUST be set up before any API calls. Auth state MUST be initialized before router configuration.

### Core Architecture Patterns

#### 1. Layered Architecture

```
Views (UI Components)
    ↓
Composables (Business Logic)
    ↓
Stores (State Management)
    ↓
API Layer (HTTP Services)
    ↓
Interceptors (Auth, Error Handling)
    ↓
Utils (Request Client, Storage, Config)
```

#### 2. Configuration Management

**DO NOT** access `import.meta.env` directly. Always use the centralized configuration module:

```typescript
// ❌ Wrong
const baseURL = import.meta.env.VITE_API_BASE_URL

// ✅ Correct
import { apiConfig } from '@/config'
const baseURL = apiConfig.baseURL
```

All configuration is centralized in `src/config/index.ts`:
- `apiConfig` - API endpoints, timeouts, retry logic
- `appConfig` - Application metadata
- `storageConfig` - Storage keys, encryption settings
- `routeConfig` - Route paths
- `paginationConfig` - Pagination defaults
- `uiConfig` - UI component settings
- `monitorConfig` - Monitoring and logging

#### 3. Secure Storage System

**DO NOT** use `localStorage` directly. Use the encrypted storage utilities:

```typescript
// ❌ Wrong
localStorage.setItem('token', token)

// ✅ Correct
import { tokenStorage } from '@/utils/storage'
await tokenStorage.setAccessToken(token)
```

**Key Features:**
- Automatic AES-GCM encryption in production
- Type-safe async API
- Unified key prefix management
- Specialized helpers for tokens and user data

**Note:** Encryption requires HTTPS in production (localhost exempt).

#### 4. Token Refresh Mechanism

The application implements **automatic token refresh** with a queue-based system in `src/api/interceptors.ts`:

- **Request Interceptor:** Checks token expiry before each request, refreshes if needed (30s buffer)
- **Response Interceptor:** Handles 401 errors with `token-expired` header, retries failed requests
- **Queue System:** Prevents concurrent refresh requests, queues pending requests during refresh

**Important:** When a token expires, all pending requests are queued and automatically retried after refresh completes.

#### 5. Permission System

Multi-layered permission enforcement:

1. **Router Guards** (`src/router/index.ts`):
   - Checks authentication status
   - Loads user permissions on first protected route access
   - Validates route-level permissions from `meta.permission`
   - Redirects to `/403` if permission denied

2. **Custom Directive** (`src/directives/permission.ts`):
   - `v-permission="'permission:name'"` - Hides elements without permission
   - UI-only, does NOT prevent API calls

3. **Store Methods** (`src/stores/permission.ts`):
   - `hasPermission(permission)` - Check single permission
   - `hasRole(role)` - Check role
   - `hasAnyPermission(permissions[])` - Check any match
   - `hasAllPermissions(permissions[])` - Check all match

4. **Composables** (`src/composables/usePermissions.ts`):
   - Reusable permission logic for components

**Critical:** Frontend permission checks are for UX only. Always enforce permissions on the backend.

---

## Key File Locations

### Configuration & Setup
- `src/config/index.ts` - **Centralized configuration** (USE THIS, not env vars)
- `src/app.ts` - Application bootstrap logic
- `src/main.ts` - Entry point
- `env.d.ts` - Environment variable TypeScript definitions
- `vite.config.ts` - Build configuration with code splitting

### State Management (Pinia)
- `src/stores/auth.ts` - Authentication state, login/logout, token management
- `src/stores/user.ts` - User profile data
- `src/stores/permission.ts` - User permissions and roles
- `src/stores/tabs.ts` - Tab navigation state
- `src/stores/counter.ts` - Example store (can be removed)

### API & HTTP
- `src/api/interceptors.ts` - **Core auth logic**, token refresh, error handling
- `src/api/index.ts` - API service exports
- `src/api/{resource}.ts` - Resource-specific API services (users, roles, permissions)
- `src/utils/request.ts` - Axios instance configuration
- `src/utils/errorHandler.ts` - Centralized error handling with user-friendly messages
- `src/utils/storage.ts` - Encrypted storage utilities

### Routing & Guards
- `src/router/index.ts` - Route definitions and **permission guards**

### UI Components
- `src/App.vue` - Root component with Naive UI providers
- `src/layouts/MainLayout.vue` - Authenticated layout
- `src/views/` - Page components
- `src/components/` - Reusable components

### Type Definitions
- `src/types/api.ts` - **All API DTOs and response types**

---

## Important Patterns & Conventions

### API Service Classes

API services use **static methods** organized by resource:

```typescript
export class UsersApi {
  static async getProfile(): Promise<UserDto> {
    return apiClient.get('/users/profile')
  }

  static async getAllUsers(): Promise<UserDto[]> {
    return apiClient.get('/users')
  }
}
```

**Note:** Response interceptor automatically unwraps `ApiResponse<T>` to return `T`.

### Store Pattern

Stores use Composition API with `readonly()` for encapsulation:

```typescript
export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(null)

  // Export readonly refs
  return {
    token: readonly(token),
    // ... methods
  }
})
```

### Composables Pattern

Composables provide reusable stateful logic:

```typescript
export function useUserManagement() {
  const loading = ref(false)
  const users = ref<User[]>([])

  async function fetchUsers() {
    // Logic here
  }

  return { loading, users, fetchUsers }
}
```

### Error Handling

Errors are handled at multiple levels:

1. **API Interceptor** - HTTP errors, business logic errors
2. **Store Actions** - Catch and set error state
3. **Composables** - Show user-friendly messages via Naive UI
4. **Components** - Display loading/error states

```typescript
try {
  await UsersApi.getAllUsers()
} catch (error) {
  // Error already handled by interceptor and shown to user
  console.error('Failed to load users:', error)
}
```

---

## Environment Configuration

Three environment files:

1. `.env.development` - Development (auto-loaded by Vite)
2. `.env.staging` - Staging environment
3. `.env.production` - Production deployment

**Required Variables:**
```env
VITE_API_BASE_URL=http://localhost:5000/api
VITE_APP_TITLE=My API Web
```

**Optional Variables:**
```env
VITE_APP_VERSION=1.0.0
VITE_ENABLE_MOCK=false
VITE_SENTRY_DSN=https://...
```

**Type-Safe Access:**
All environment variables have TypeScript definitions in `env.d.ts`. Use autocomplete!

---

## Testing Strategy

- **Unit Tests:** Vitest with Vue Test Utils
- **Test Location:** `src/**/__tests__/*.spec.ts` or co-located `*.spec.ts`
- **Coverage Target:** 60%+ for core modules

**Priority for Testing:**
1. Stores (auth, permission)
2. API interceptors
3. Utility functions (storage, errorHandler)
4. Composables
5. Complex components

---

## Build & Optimization

### Code Splitting Strategy

Configured in `vite.config.ts`:

- `vue-vendor` - Vue core (vue, vue-router, pinia)
- `naive-ui` - UI library
- `utils` - Axios and utilities
- `icons` - Icon library

**Result:** ~37% reduction in initial bundle size, better caching.

### Production Optimizations

- Terser minification with `drop_console` and `drop_debugger`
- CSS code splitting
- Asset hashing for long-term caching
- Gzip compression via Nginx
- Tree-shaking of unused code

---

## Common Tasks

### Adding a New Route

1. Define route in `src/router/index.ts`:
```typescript
{
  path: '/new-page',
  name: 'new-page',
  component: () => import('@/views/NewPage.vue'),
  meta: {
    permission: 'page:view', // Optional permission
    title: '新页面',
  }
}
```

2. Add to sidebar/navigation if needed

### Adding a New API Service

1. Define DTO types in `src/types/api.ts`
2. Create service class in `src/api/resource.ts`
3. Export from `src/api/index.ts`

```typescript
export class ResourceApi {
  static async getAll(): Promise<ResourceDto[]> {
    return apiClient.get('/resources')
  }
}
```

### Adding a New Store

1. Create in `src/stores/resource.ts`
2. Use Composition API pattern
3. Export readonly state, expose methods

```typescript
export const useResourceStore = defineStore('resource', () => {
  const items = ref<Resource[]>([])

  async function fetchItems() {
    items.value = await ResourceApi.getAll()
  }

  return {
    items: readonly(items),
    fetchItems
  }
})
```

### Working with Permissions

```typescript
// In component
import { usePermissionStore } from '@/stores/permission'

const permissionStore = usePermissionStore()

// Check permission
if (permissionStore.hasPermission('user:delete')) {
  // Show delete button
}

// Or use directive
<n-button v-permission="'user:delete'">删除</n-button>
```

---

## Debugging Tips

### Token Issues

Check `src/stores/auth.ts` and `src/api/interceptors.ts`. Enable logging:

```typescript
console.log('Token expiry:', authStore.tokenExpiresAt)
console.log('Should refresh:', authStore.shouldRefreshToken())
```

### Permission Issues

Check:
1. `src/stores/permission.ts` - Is `permissionsLoaded` true?
2. `src/router/index.ts` - Router guard logic
3. Browser DevTools → Application → Local Storage → Check `app_` prefixed keys

### Storage Decryption Errors

If encrypted storage fails, check:
1. Are you on HTTPS? (Required in production)
2. Check browser console for crypto errors
3. Clear localStorage and re-login

### API Request Issues

Check Network tab:
- Request headers → `Authorization: Bearer ...`
- Response headers → `token-expired: true` (indicates refresh needed)
- Check `src/api/interceptors.ts` for error handling logic

---

## Build Artifacts

After `npm run build`:

- `dist/` - Production build output
  - `dist/assets/js/` - JavaScript chunks
  - `dist/assets/css/` - Stylesheets
  - `dist/assets/images/` - Images
  - `dist/index.html` - Entry HTML

The build is optimized for:
- Modern browsers (ES2015+)
- Gzip/Brotli compression
- Long-term caching (hashed filenames)
- Tree-shaking and dead code elimination

---

## Production Deployment

### Nginx Deployment

Use provided `nginx.conf`:
- Handles SPA routing
- Gzip compression
- Security headers
- Static asset caching (7 days)
- API proxying to backend

### Docker Deployment

Multi-stage build:
1. **Build stage:** Compiles production bundle
2. **Runtime stage:** Nginx serves static files

**Deployment:**
```bash
docker-compose up -d
```

**Environment Variables:**
Set in `.env.production` or override in `docker-compose.yml`.

---

## Known Limitations

1. **Web Crypto API:** Requires HTTPS in production (encryption will be disabled in HTTP)
2. **Browser Support:** No IE support due to Composition API and Web Crypto
3. **Token Storage:** Currently localStorage-based; consider httpOnly cookies for enhanced security
4. **CSRF Protection:** Not implemented; should be added for production

---

## Additional Resources

- **Optimization Report:** See `OPTIMIZATION_REPORT.md` for detailed performance improvements
- **README:** See `README.md` for IDE setup and basic commands
- **Vue 3 Docs:** https://vuejs.org/
- **Naive UI Docs:** https://www.naiveui.com/
- **Vite Docs:** https://vite.dev/
