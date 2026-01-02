import api from './api';
import { LeaveRequest, LeaveStatus, LeaveApproval, LeaveType, PendingLeaveRequest, MyLeaveRequest, ApiResponse } from '../types';

export const leaveService = {
  submitLeave: async (request: LeaveRequest): Promise<any> => {
    const response = await api.post<ApiResponse<any>>('/leave/submit', request);
    return response.data.data;
  },

  getLeaveStatus: async (leaveId: string): Promise<LeaveStatus> => {
    const response = await api.get<ApiResponse<LeaveStatus>>(`/leave/${leaveId}`);
    return response.data.data;
  },

  getAvailableActions: async (leaveId: string, userId: string): Promise<string[]> => {
    const response = await api.get<ApiResponse<string[]>>(
      `/leave/${leaveId}/actions?userId=${userId}`
    );
    return response.data.data;
  },

  managerAction: async (leaveId: string, approval: LeaveApproval): Promise<boolean> => {
    const response = await api.post<ApiResponse<boolean>>(
      `/leave/${leaveId}/manager-action`,
      approval
    );
    return response.data.data;
  },

  hrAction: async (leaveId: string, approval: LeaveApproval): Promise<boolean> => {
    const response = await api.post<ApiResponse<boolean>>(
      `/leave/${leaveId}/hr-action`,
      approval
    );
    return response.data.data;
  },

  cancelLeave: async (leaveId: string, employeeId: string): Promise<boolean> => {
    const response = await api.post<ApiResponse<boolean>>(
      `/leave/${leaveId}/cancel?employeeId=${employeeId}`
    );
    return response.data.data;
  },

  getAllLeaveTypes: async (): Promise<LeaveType[]> => {
    const response = await api.get<ApiResponse<LeaveType[]>>('/leavetype');
    return response.data.data;
  },

  getLeaveTypeByCode: async (code: string): Promise<LeaveType> => {
    const response = await api.get<ApiResponse<LeaveType>>(`/leavetype/${code}`);
    return response.data.data;
  },

  getUserManager: async (userId: string): Promise<any> => {
    const response = await api.get<ApiResponse<any>>(`/leave/approvers/${userId}`);
    return response.data.data;
  },

  getPendingApprovals: async (approverId: string): Promise<PendingLeaveRequest[]> => {
    const response = await api.get<ApiResponse<PendingLeaveRequest[]>>(
      `/leave/pending-approvals/${approverId}`
    );
    return response.data.data;
  },

  getMyLeaveRequests: async (userId: string): Promise<MyLeaveRequest[]> => {
    const response = await api.get<ApiResponse<MyLeaveRequest[]>>(
      `/leave/my-requests/${userId}`
    );
    return response.data.data;
  },

  executeCommand: async (
    leaveId: string,
    command: string,
    identityId: string,
    parameters?: Record<string, any>
  ): Promise<any> => {
    const response = await api.post<ApiResponse<any>>(
      `/leave/${leaveId}/execute-command`,
      {
        processId: leaveId,
        command: command,
        identityId: identityId,
        parameters: parameters || {},
      }
    );
    return response.data.data;
  },
};
