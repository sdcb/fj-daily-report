import axios, { AxiosResponse, AxiosError, InternalAxiosRequestConfig } from 'axios';
import { getApiUrl } from './config';
import { DailyReportsResponse, DailyReportDto } from './types';

export interface LoginRequest {
  provider?: string;
  code?: string;
  origin?: string;
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

export interface UpdateDailyReportRequest {
  userId: string;
  date: string;
  content: string;
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
      const origin = window.location.origin;
      const url = `${getApiUrl()}/api/auth/keycloak/signin?origin=${encodeURIComponent(origin)}`;
      window.location.href = url;
    }
  }
};

export const dailyReportApi = {
  getDailyReports: async (date?: string): Promise<DailyReportsResponse> => {
    const params = date ? { date } : {};
    const response = await api.get('/api/daily-report', { params });
    return response.data;
  },

  updateDailyReport: async (request: UpdateDailyReportRequest): Promise<DailyReportDto> => {
    const response = await api.post('/api/daily-report', request);
    return response.data;
  }
};

export default api;