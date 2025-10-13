import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios';
import { useAuthStore } from '@/stores/auth';

const service = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api',
  timeout: 10000,
});

// 请求拦截器
service.interceptors.request.use(
  (config) => {
    const authStore = useAuthStore();
    // 注意：现在 authStore 中的 token 就是 accessToken
    if (authStore.token) {
      config.headers['Authorization'] = `Bearer ${authStore.token}`;
    }
    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  }
);

let isRefreshing = false;
let failedQueue: { resolve: (value: unknown) => void; reject: (reason?: unknown) => void; }[] = [];

const processQueue = (error: Error | null, token: string | null = null) => {
  failedQueue.forEach(prom => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

// 响应拦截器
service.interceptors.response.use(
  (response) => {
    // 后端返回的数据结构是 { code, message, data, success }
    // 我们直接返回 data 部分
    return response.data;
  },
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };
    const authStore = useAuthStore();

    // 当 Access Token 过期时 (通常是 401)
    // 检查响应头中是否有我们自定义的 'Token-Expired' 标志
    if (error.response?.status === 401 && (error.response.headers as { 'token-expired'?: string })['token-expired'] === 'true' && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise(function(resolve, reject) {
          failedQueue.push({ resolve, reject });
        }).then(token => {
          if (originalRequest.headers) {
            originalRequest.headers['Authorization'] = 'Bearer ' + token;
          }
          return service(originalRequest);
        }).catch(err => {
          return Promise.reject(err);
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const { refreshToken } = authStore;
        if (!refreshToken) {
            authStore.logout();
            return Promise.reject(error);
        }

        const res = await axios.post<{ accessToken: string; refreshToken: string }>('/api/token/refresh', { refreshToken }, {
          baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api',
        });
        
        const newTokens = res.data;

        authStore.setTokens(newTokens.accessToken, newTokens.refreshToken);
        if (originalRequest.headers) {
          originalRequest.headers['Authorization'] = `Bearer ${newTokens.accessToken}`;
        }
        
        processQueue(null, newTokens.accessToken);
        return service(originalRequest);

      } catch (refreshError) {
        processQueue(refreshError as Error, null);
        authStore.logout();
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    // 添加 403 错误处理
    if (error.response?.status === 403) {
      // 使用 router.push 跳转到 403 页面
      // 注意：这里不能直接使用 useRouter，因为它只能在 setup 函数中使用
      // 我们需要从外部传入 router 实例，或者通过 window.location 跳转
      window.location.href = '/403';
    }

    return Promise.reject(error.response?.data || error);
  }
);

export default service;