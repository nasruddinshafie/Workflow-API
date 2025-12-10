import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { User, Role } from '../types';
import { identityService } from '../services/identityService';

interface AuthContextType {
  currentUser: User | null;
  setCurrentUser: (user: User | null) => void;
  isManager: boolean;
  isHR: boolean;
  isAdmin: boolean;
  loading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [currentUser, setCurrentUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Load user from localStorage on mount
    const storedUserId = localStorage.getItem('currentUserId');
    if (storedUserId) {
      identityService
        .getUserById(storedUserId)
        .then((user) => {
          setCurrentUser(user);
        })
        .catch((error) => {
          console.error('Failed to load user:', error);
          localStorage.removeItem('currentUserId');
        })
        .finally(() => {
          setLoading(false);
        });
    } else {
      setLoading(false);
    }
  }, []);

  const handleSetCurrentUser = (user: User | null) => {
    setCurrentUser(user);
    if (user) {
      localStorage.setItem('currentUserId', user.id);
    } else {
      localStorage.removeItem('currentUserId');
    }
  };

  const isManager = currentUser?.roles.includes(Role.Manager) || false;
  const isHR =
    currentUser?.roles.includes(Role.HRManager) ||
    currentUser?.roles.includes(Role.Admin) ||
    false;
  const isAdmin =
    currentUser?.roles.includes(Role.Admin) ||
    currentUser?.roles.includes(Role.SuperAdmin) ||
    false;

  const value: AuthContextType = {
    currentUser,
    setCurrentUser: handleSetCurrentUser,
    isManager,
    isHR,
    isAdmin,
    loading,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
