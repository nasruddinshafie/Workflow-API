import api from './api';
import { LeaveBalance, ApiResponse } from '../types';

export const leaveBalanceService = {
  getUserBalances: async (userId: string, year?: number): Promise<LeaveBalance[]> => {
    const yearParam = year ? `?year=${year}` : '';
    const response = await api.get<ApiResponse<LeaveBalance[]>>(
      `/leavebalance/user/${userId}${yearParam}`
    );
    return response.data.data;
  },

  getBalance: async (
    userId: string,
    leaveTypeCode: string,
    year?: number
  ): Promise<LeaveBalance> => {
    const yearParam = year ? `?year=${year}` : '';
    const response = await api.get<ApiResponse<LeaveBalance>>(
      `/leavebalance/user/${userId}/type/${leaveTypeCode}${yearParam}`
    );
    return response.data.data;
  },

  checkSufficientBalance: async (
    userId: string,
    leaveTypeCode: string,
    year: number,
    daysRequired: number
  ): Promise<boolean> => {
    const response = await api.get<ApiResponse<boolean>>(
      `/leavebalance/user/${userId}/check-balance?leaveTypeCode=${leaveTypeCode}&year=${year}&daysRequired=${daysRequired}`
    );
    return response.data.data;
  },

  getAvailableDays: async (
    userId: string,
    year?: number
  ): Promise<Record<string, number>> => {
    const yearParam = year ? `?year=${year}` : '';
    const response = await api.get<ApiResponse<Record<string, number>>>(
      `/leavebalance/user/${userId}/available${yearParam}`
    );
    return response.data.data;
  },
};
