import api from './api';
import { User, Role, ApiResponse } from '../types';

export const identityService = {
  getAllUsers: async (): Promise<User[]> => {
    const response = await api.get<ApiResponse<User[]>>('/identity/users');
    return response.data.data;
  },

  getUserById: async (userId: string): Promise<User> => {
    const response = await api.get<ApiResponse<User>>(`/identity/users/${userId}`);
    return response.data.data;
  },

  getUsersByRole: async (role: Role): Promise<User[]> => {
    const response = await api.get<ApiResponse<User[]>>(`/identity/users/role/${role}`);
    return response.data.data;
  },

  getUsersByDepartment: async (department: string): Promise<User[]> => {
    const response = await api.get<ApiResponse<User[]>>(`/identity/users/department/${department}`);
    return response.data.data;
  },

  getManager: async (userId: string): Promise<User | null> => {
    try {
      const response = await api.get<ApiResponse<User>>(`/identity/users/${userId}/manager`);
      return response.data.data;
    } catch (error) {
      return null;
    }
  },

  getDirectReports: async (managerId: string): Promise<User[]> => {
    const response = await api.get<ApiResponse<User[]>>(`/identity/users/${managerId}/direct-reports`);
    return response.data.data;
  },
};
