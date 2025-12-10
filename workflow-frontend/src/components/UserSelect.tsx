import React, { useState, useEffect } from 'react';
import { User } from '../types';
import { identityService } from '../services/identityService';
import { useAuth } from '../contexts/AuthContext';
import './UserSelect.css';

const UserSelect: React.FC = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const { setCurrentUser } = useAuth();

  useEffect(() => {
    identityService
      .getAllUsers()
      .then((data) => {
        setUsers(data);
        setLoading(false);
      })
      .catch((error) => {
        console.error('Failed to load users:', error);
        setLoading(false);
      });
  }, []);

  const handleUserSelect = (user: User) => {
    setCurrentUser(user);
  };

  if (loading) {
    return <div className="user-select-container"><p>Loading users...</p></div>;
  }

  return (
    <div className="user-select-container">
      <div className="user-select-card">
        <h1>Select User</h1>
        <p className="subtitle">Choose a user to simulate login</p>
        <div className="user-list">
          {users.map((user) => (
            <div
              key={user.id}
              className="user-item"
              onClick={() => handleUserSelect(user)}
            >
              <div className="user-info">
                <h3>{user.fullName}</h3>
                <p className="user-email">{user.email}</p>
                <p className="user-department">{user.department}</p>
                <div className="user-roles">
                  {user.roles.map((role) => (
                    <span key={role} className="role-badge">
                      {role}
                    </span>
                  ))}
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default UserSelect;
