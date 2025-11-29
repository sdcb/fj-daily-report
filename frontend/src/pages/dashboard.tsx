import { useEffect, useState, useCallback, useRef } from 'react';
import { useRouter } from 'next/router';
import { dailyReportApi } from '@/lib/api';
import { useAuth } from '@/lib/auth';
import { getApiUrl } from '@/lib/config';
import { ProjectGroupDto, DailyReportDto } from '@/lib/types';
import * as signalR from '@microsoft/signalr';

export default function DashboardPage() {
  const { user, logout, isAuthenticated, initialized } = useAuth();
  const [groups, setGroups] = useState<ProjectGroupDto[]>([]);
  const [reports, setReports] = useState<DailyReportDto[]>([]);
  const [selectedDate, setSelectedDate] = useState(() => {
    const today = new Date();
    return today.toISOString().split('T')[0];
  });
  const [loading, setLoading] = useState(true);
  const [editingUserId, setEditingUserId] = useState<string | null>(null);
  const [editContent, setEditContent] = useState('');
  const [saving, setSaving] = useState(false);
  const router = useRouter();
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const previousDateRef = useRef<string | null>(null);

  // 获取日报数据
  const fetchDailyReports = useCallback(async (date: string) => {
    try {
      setLoading(true);
      const data = await dailyReportApi.getDailyReports(date);
      setGroups(data.groups);
      setReports(data.reports);
    } catch (error) {
      console.error('Failed to fetch daily reports:', error);
    } finally {
      setLoading(false);
    }
  }, []);

  // SignalR 连接
  useEffect(() => {
    if (!initialized || !isAuthenticated) return;

    const token = localStorage.getItem('auth_token');
    if (!token) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${getApiUrl()}/hubs/daily-report?access_token=${token}`)
      .withAutomaticReconnect()
      .build();

    connection.on('ReportUpdated', (report: DailyReportDto) => {
      setReports(prev => {
        const index = prev.findIndex(r => r.userId === report.userId);
        if (index >= 0) {
          const newReports = [...prev];
          newReports[index] = report;
          return newReports;
        }
        return [...prev, report];
      });
    });

    connection.start()
      .then(() => {
        console.log('SignalR connected');
        connection.invoke('JoinDateRoom', selectedDate);
        previousDateRef.current = selectedDate;
      })
      .catch((err: Error) => console.error('SignalR connection error:', err));

    connectionRef.current = connection;

    return () => {
      connection.stop();
    };
  }, [initialized, isAuthenticated]);

  // 切换日期时更换房间
  useEffect(() => {
    const connection = connectionRef.current;
    if (!connection || connection.state !== signalR.HubConnectionState.Connected) return;

    const previousDate = previousDateRef.current;
    if (previousDate && previousDate !== selectedDate) {
      connection.invoke('LeaveDateRoom', previousDate);
    }
    connection.invoke('JoinDateRoom', selectedDate);
    previousDateRef.current = selectedDate;
  }, [selectedDate]);

  // 获取数据
  useEffect(() => {
    if (!initialized) return;
    if (!isAuthenticated) {
      router.replace('/login');
      return;
    }
    fetchDailyReports(selectedDate);
  }, [initialized, isAuthenticated, selectedDate, router, fetchDailyReports]);

  const handleLogout = () => {
    logout();
    router.push('/login');
  };

  const handleDateChange = (offset: number) => {
    const date = new Date(selectedDate);
    date.setDate(date.getDate() + offset);
    setSelectedDate(date.toISOString().split('T')[0]);
    setEditingUserId(null);
  };

  const getUserReport = (userId: string): DailyReportDto | undefined => {
    return reports.find(r => r.userId === userId);
  };

  const handleEditClick = (userId: string) => {
    const report = getUserReport(userId);
    setEditingUserId(userId);
    setEditContent(report?.content || '');
  };

  const handleSave = async () => {
    if (!editingUserId) return;
    
    try {
      setSaving(true);
      await dailyReportApi.updateDailyReport({
        userId: editingUserId,
        date: selectedDate,
        content: editContent
      });
      setEditingUserId(null);
    } catch (error) {
      console.error('Failed to save report:', error);
      alert('保存失败');
    } finally {
      setSaving(false);
    }
  };

  const handleCancel = () => {
    setEditingUserId(null);
    setEditContent('');
  };

  if (!initialized) {
    return null;
  }

  if (!isAuthenticated) {
    return null;
  }

  return (
    <div style={{ padding: '20px', maxWidth: '1200px', margin: '0 auto' }}>
      {/* Header */}
      <header style={{ 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'center',
        marginBottom: '20px',
        padding: '16px 20px',
        backgroundColor: '#f8f9fa',
        borderRadius: '8px'
      }}>
        <div>
          <h1 style={{ margin: 0, fontSize: '20px' }}>日报系统</h1>
          <p style={{ margin: '4px 0 0 0', color: '#666', fontSize: '14px' }}>
            {user?.displayName} ({user?.email})
          </p>
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

      {/* Date Selector */}
      <div style={{ 
        display: 'flex', 
        alignItems: 'center', 
        justifyContent: 'center',
        gap: '16px',
        marginBottom: '24px',
        padding: '12px',
        backgroundColor: '#fff',
        border: '1px solid #ddd',
        borderRadius: '8px'
      }}>
        <button 
          onClick={() => handleDateChange(-1)}
          style={{
            padding: '8px 16px',
            backgroundColor: '#6c757d',
            color: 'white',
            border: 'none',
            borderRadius: '4px',
            cursor: 'pointer'
          }}
        >
          ← 前一天
        </button>
        <input
          type="date"
          value={selectedDate}
          onChange={(e) => {
            setSelectedDate(e.target.value);
            setEditingUserId(null);
          }}
          style={{
            padding: '8px 12px',
            fontSize: '16px',
            border: '1px solid #ccc',
            borderRadius: '4px'
          }}
        />
        <button 
          onClick={() => handleDateChange(1)}
          style={{
            padding: '8px 16px',
            backgroundColor: '#6c757d',
            color: 'white',
            border: 'none',
            borderRadius: '4px',
            cursor: 'pointer'
          }}
        >
          后一天 →
        </button>
        <button 
          onClick={() => {
            setSelectedDate(new Date().toISOString().split('T')[0]);
            setEditingUserId(null);
          }}
          style={{
            padding: '8px 16px',
            backgroundColor: '#007bff',
            color: 'white',
            border: 'none',
            borderRadius: '4px',
            cursor: 'pointer'
          }}
        >
          今天
        </button>
      </div>

      {/* Main Content */}
      {loading ? (
        <div style={{ textAlign: 'center', padding: '40px' }}>加载中...</div>
      ) : groups.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '40px', color: '#666' }}>
          暂无项目组数据
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '24px' }}>
          {groups.map(group => (
            <div key={group.id} style={{
              border: '1px solid #ddd',
              borderRadius: '8px',
              overflow: 'hidden'
            }}>
              {/* Group Header */}
              <div style={{
                padding: '12px 16px',
                backgroundColor: '#343a40',
                color: 'white',
                fontWeight: 'bold'
              }}>
                {group.name}
              </div>

              {/* Group Members */}
              <div style={{ padding: '16px' }}>
                {group.members.length === 0 ? (
                  <div style={{ color: '#999', fontStyle: 'italic' }}>暂无成员</div>
                ) : (
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
                    {group.members.map(member => {
                      const report = getUserReport(member.userId);
                      const isEditing = editingUserId === member.userId;
                      const isCurrentUser = user?.id === member.userId;

                      return (
                        <div key={member.userId} style={{
                          padding: '12px',
                          backgroundColor: isCurrentUser ? '#e3f2fd' : '#f8f9fa',
                          borderRadius: '6px',
                          border: isCurrentUser ? '2px solid #2196f3' : '1px solid #eee'
                        }}>
                          {/* Member Info */}
                          <div style={{ 
                            display: 'flex', 
                            justifyContent: 'space-between',
                            alignItems: 'center',
                            marginBottom: '8px'
                          }}>
                            <div>
                              <span style={{ fontWeight: 'bold' }}>{member.displayName}</span>
                              {isCurrentUser && (
                                <span style={{
                                  marginLeft: '8px',
                                  padding: '2px 6px',
                                  backgroundColor: '#2196f3',
                                  color: 'white',
                                  fontSize: '12px',
                                  borderRadius: '4px'
                                }}>我</span>
                              )}
                              <span style={{ 
                                marginLeft: '8px', 
                                color: '#999', 
                                fontSize: '12px' 
                              }}>
                                {member.email}
                              </span>
                            </div>
                            {!isEditing && (
                              <button
                                onClick={() => handleEditClick(member.userId)}
                                style={{
                                  padding: '4px 12px',
                                  backgroundColor: '#28a745',
                                  color: 'white',
                                  border: 'none',
                                  borderRadius: '4px',
                                  cursor: 'pointer',
                                  fontSize: '12px'
                                }}
                              >
                                编辑
                              </button>
                            )}
                          </div>

                          {/* Report Content */}
                          {isEditing ? (
                            <div>
                              <textarea
                                value={editContent}
                                onChange={(e) => setEditContent(e.target.value)}
                                placeholder="请输入日报内容..."
                                style={{
                                  width: '100%',
                                  minHeight: '100px',
                                  padding: '8px',
                                  border: '1px solid #ccc',
                                  borderRadius: '4px',
                                  resize: 'vertical',
                                  fontFamily: 'inherit',
                                  fontSize: '14px',
                                  boxSizing: 'border-box'
                                }}
                              />
                              <div style={{ 
                                display: 'flex', 
                                gap: '8px', 
                                marginTop: '8px',
                                justifyContent: 'flex-end'
                              }}>
                                <button
                                  onClick={handleCancel}
                                  disabled={saving}
                                  style={{
                                    padding: '6px 16px',
                                    backgroundColor: '#6c757d',
                                    color: 'white',
                                    border: 'none',
                                    borderRadius: '4px',
                                    cursor: 'pointer'
                                  }}
                                >
                                  取消
                                </button>
                                <button
                                  onClick={handleSave}
                                  disabled={saving}
                                  style={{
                                    padding: '6px 16px',
                                    backgroundColor: '#007bff',
                                    color: 'white',
                                    border: 'none',
                                    borderRadius: '4px',
                                    cursor: saving ? 'not-allowed' : 'pointer',
                                    opacity: saving ? 0.7 : 1
                                  }}
                                >
                                  {saving ? '保存中...' : '保存'}
                                </button>
                              </div>
                            </div>
                          ) : (
                            <div style={{
                              padding: '8px',
                              backgroundColor: 'white',
                              borderRadius: '4px',
                              minHeight: '40px',
                              whiteSpace: 'pre-wrap',
                              color: report?.content ? '#333' : '#999',
                              fontStyle: report?.content ? 'normal' : 'italic'
                            }}>
                              {report?.content || '暂无日报'}
                            </div>
                          )}

                          {/* Update Time */}
                          {report && !isEditing && (
                            <div style={{ 
                              marginTop: '4px', 
                              fontSize: '12px', 
                              color: '#999',
                              textAlign: 'right'
                            }}>
                              更新于: {new Date(report.updatedAt).toLocaleString('zh-CN')}
                            </div>
                          )}
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}