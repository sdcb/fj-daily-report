import { useEffect, useState } from 'react';
import { useRouter } from 'next/router';
import { authApi } from '@/lib/api';
import { useAuth } from '@/lib/auth';
import { User } from '@/lib/types';

export default function DashboardPage() {
  const { user, logout, isAuthenticated, initialized } = useAuth();
  const [allUsers, setAllUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const router = useRouter();

  useEffect(() => {
    if (!initialized) return; // 等待Auth初始化结束

    if (!isAuthenticated) {
      router.replace('/login');
      return;
    }

    // 获取所有用户列表
    authApi.getAllUsers()
      .then(setAllUsers)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, [initialized, isAuthenticated, router]);

  const handleLogout = () => {
    logout();
    router.push('/login');
  };

  if (!initialized) {
    return null; // 初始化期间不渲染，避免闪跳
  }

  if (!isAuthenticated) {
    return null;
  }

  return (
    <div style={{ padding: '20px', maxWidth: '800px', margin: '0 auto' }}>
      <header style={{ 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'center',
        marginBottom: '30px',
        padding: '20px',
        backgroundColor: '#f8f9fa',
        borderRadius: '8px'
      }}>
        <div>
          <h1>欢迎, {user?.displayName}</h1>
          <p style={{ margin: 0, color: '#666' }}>{user?.email}</p>
        </div>
        <button 
          onClick={handleLogout}
          style={{
            padding: '8px 16px',
            backgroundColor: '#dc3545',
            color: 'white',
            border: 'none',
            borderRadius: '4px',
            cursor: 'pointer'
          }}
        >
          退出登录
        </button>
      </header>

      <main>
        <h2>用户列表</h2>
        {loading ? (
          <p>加载中...</p>
        ) : (
          <div style={{ 
            display: 'grid', 
            gap: '16px',
            gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))'
          }}>
            {allUsers.map((u) => (
              <div key={u.id} style={{
                padding: '16px',
                border: '1px solid #ddd',
                borderRadius: '8px',
                backgroundColor: user?.id === u.id ? '#e3f2fd' : 'white'
              }}>
                <h3 style={{ margin: '0 0 8px 0' }}>{u.displayName}</h3>
                <p style={{ margin: '0 0 4px 0', color: '#666' }}>{u.email}</p>
                <p style={{ margin: 0, fontSize: '12px', color: '#999' }}>
                  ID: {u.id}
                </p>
                {user?.id === u.id && (
                  <span style={{
                    display: 'inline-block',
                    marginTop: '8px',
                    padding: '4px 8px',
                    backgroundColor: '#007bff',
                    color: 'white',
                    fontSize: '12px',
                    borderRadius: '4px'
                  }}>
                    当前用户
                  </span>
                )}
              </div>
            ))}
          </div>
        )}
      </main>
    </div>
  );
}