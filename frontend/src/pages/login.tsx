import { authApi } from '@/lib/api';

export default function LoginPage() {
  const handleKeycloakLogin = () => {
    authApi.redirectToKeycloak();
  };

  return (
    <div style={{ 
      display: 'flex', 
      justifyContent: 'center', 
      alignItems: 'center', 
      height: '100vh',
      flexDirection: 'column',
      gap: '20px'
    }}>
      <h1>日报填报</h1>
      <div style={{ 
        padding: '20px', 
        border: '1px solid #ddd', 
        borderRadius: '8px',
        textAlign: 'center'
      }}>
        <h2>请选择登录方式</h2>
        <button 
          onClick={handleKeycloakLogin}
          style={{
            padding: '12px 24px',
            fontSize: '16px',
            backgroundColor: '#007bff',
            color: 'white',
            border: 'none',
            borderRadius: '4px',
            cursor: 'pointer',
            marginTop: '16px'
          }}
        >
          使用 Keycloak 登录
        </button>
      </div>
    </div>
  );
}