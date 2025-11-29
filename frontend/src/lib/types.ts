export interface User {
  id: string;
  email: string;
  displayName: string;
}

export interface AuthContextType {
  user: User | null;
  token: string | null;
  login: (token: string, user: User) => void;
  logout: () => void;
  isAuthenticated: boolean;
  // 是否已从本地存储加载完成（避免刷新时误判未登录）
  initialized: boolean;
}

export interface DailyReportDto {
  id: number;
  userId: string;
  userDisplayName: string;
  date: string;
  content: string;
  updatedAt: string;
}

export interface ProjectGroupMemberDto {
  userId: string;
  displayName: string;
  email: string;
}

export interface ProjectGroupDto {
  id: number;
  name: string;
  members: ProjectGroupMemberDto[];
}

export interface DailyReportsResponse {
  date: string;
  reports: DailyReportDto[];
  groups: ProjectGroupDto[];
}