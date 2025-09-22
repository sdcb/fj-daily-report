import { useEffect, useState } from 'react';
import { useRouter } from 'next/router';
import { authApi, LoginRequest } from '@/lib/api';
import { useAuth } from '@/lib/auth';

export default function AuthorizingPage() {
  const [isClient, setIsClient] = useState(false);
  const router = useRouter();
  const { login } = useAuth();
  const [everStarted, setEverStarted] = useState(false);

  useEffect(() => {
    setIsClient(true);
  }, []);

  useEffect(() => {
    const { code, provider } = router.query as {
      code: string;
      provider: string;
    };

    if (!router.isReady || everStarted) {
      return;
    }

    setEverStarted(true);

    if (!code) {
      router.push('/login');
      return;
    }

    const loginRequest: LoginRequest = {
      code,
      provider,
      origin: typeof window !== 'undefined' ? window.location.origin : undefined,
    };

    authApi.login(loginRequest)
      .then((response) => {
        login(response.token, response.user);
        router.push('/dashboard');
      })
      .catch((error) => {
        console.error('Login failed:', error);
        router.push('/login');
      });
  }, [router.isReady, router, login, everStarted]);

  if (!isClient) {
    return null;
  }

  return (
    <div style={{ 
      display: 'flex', 
      justifyContent: 'center', 
      alignItems: 'center', 
      height: '100vh',
      flexDirection: 'column',
      gap: '20px'
    }}>
      <div>正在登录...</div>
      <div style={{ fontSize: '14px', color: '#666' }}>
        请稍候，正在处理您的认证信息
      </div>
    </div>
  );
}