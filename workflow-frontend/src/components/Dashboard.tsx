import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { LeaveBalance } from '../types';
import { leaveBalanceService } from '../services/leaveBalanceService';
import LeaveRequestForm from './LeaveRequestForm';
import PendingApprovals from './PendingApprovals';
import './Dashboard.css';
import MyLeaveRequests from './MyLeaveRequests';

const Dashboard: React.FC = () => {
  const { currentUser, setCurrentUser } = useAuth();
  const [balances, setBalances] = useState<LeaveBalance[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedView, setSelectedView] = useState<'request' | 'history' | 'approvals'>('request');

  useEffect(() => {
    if (currentUser) {
      leaveBalanceService
        .getUserBalances(currentUser.id)
        .then((data) => {
          setBalances(data);
          setLoading(false);
        })
        .catch((error) => {
          console.error('Failed to load balances:', error);
          setLoading(false);
        });
    }
  }, [currentUser]);

  const handleLogout = () => {
    setCurrentUser(null);
  };

  if (!currentUser) {
    return null;
  }

  return (
    <div className="dashboard">
      <header className="dashboard-header">
        <div className="header-content">
          <h1>Leave Management System</h1>
          <div className="user-info-header">
            <span className="user-name">{currentUser.fullName}</span>
            <span className="user-department">{currentUser.department}</span>
            <button onClick={handleLogout} className="logout-btn">
              Logout
            </button>
          </div>
        </div>
      </header>

      <div className="dashboard-content">
        <aside className="sidebar">
          <nav className="sidebar-nav">
            <button
              className={`nav-item ${selectedView === 'request' ? 'active' : ''}`}
              onClick={() => setSelectedView('request')}
            >
              <span className="nav-icon">📝</span>
              Request Leave
            </button>
            <button
              className={`nav-item ${selectedView === 'history' ? 'active' : ''}`}
              onClick={() => setSelectedView('history')}
            >
              <span className="nav-icon">📋</span>
              My Requests
            </button>
             <button
                className={`nav-item ${selectedView === 'approvals' ? 'active' : ''}`}
                onClick={() => setSelectedView('approvals')}
              >
                <span className="nav-icon">✓</span>
                Approvals
              </button>
          </nav>

          <div className="leave-balances">
            <h3>Leave Balances</h3>
            {loading ? (
              <p>Loading...</p>
            ) : (
              <div className="balance-list">
                {balances.map((balance) => (
                  <div key={balance.leaveTypeCode} className="balance-item">
                    <div className="balance-header">
                      <span
                        className="balance-color"
                        style={{ backgroundColor: balance.color || '#667eea' }}
                      ></span>
                      <span className="balance-type">{balance.leaveTypeName}</span>
                    </div>
                    <div className="balance-stats">
                      <div className="stat">
                        <span className="stat-value">{balance.availableDays}</span>
                        <span className="stat-label">Available</span>
                      </div>
                      <div className="stat">
                        <span className="stat-value">{balance.usedDays}</span>
                        <span className="stat-label">Used</span>
                      </div>
                      <div className="stat">
                        <span className="stat-value">{balance.pendingDays}</span>
                        <span className="stat-label">Pending</span>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </aside>

        <main className="main-content">
          {selectedView === 'request' && <LeaveRequestForm />}
          {selectedView === 'history' && <MyLeaveRequests/> }
          {selectedView === 'approvals' && <PendingApprovals />}
        </main>
      </div>
    </div>
  );
};

export default Dashboard;
