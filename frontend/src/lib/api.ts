import axios, { AxiosResponse, AxiosError, InternalAxiosRequestConfig } from 'axios';
import { getApiUrl } from './config';

export interface LoginRequest {
  provider?: string;
  code?: string;
}

export interface LoginResponse {
  token: string;
  user: {
    id: string;
    email: string;
    displayName: string;
  };
}

export interface UserProfile {
  id: string;
  email: string;
  displayName: string;
}

const api = axios.create({
  baseURL: getApiUrl(),
  timeout: 10000,
});

// 请求拦截器：添加认证token
api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = localStorage.getItem('auth_token');
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// 响应拦截器：处理认证错误
api.interceptors.response.use(
  (response: AxiosResponse) => response,
  (error: AxiosError) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('auth_token');
      localStorage.removeItem('user_info');
      if (typeof window !== 'undefined') {
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

export const authApi = {
  login: async (request: LoginRequest): Promise<LoginResponse> => {
    const response = await api.post('/api/auth/login', request);
    return response.data;
  },

  getUserProfile: async (): Promise<UserProfile> => {
    const response = await api.get('/api/user/profile');
    return response.data;
  },

  getAllUsers: async (): Promise<UserProfile[]> => {
    const response = await api.get('/api/user/all');
    return response.data;
  },

  redirectToKeycloak: () => {
    if (typeof window !== 'undefined') {
      window.location.href = `${getApiUrl()}/api/auth/keycloak/signin`;
    }
  }
};

export default api;