export interface User {
  id: string;
  username: string;
  email: string;
  fullName: string;
  department: string;
  managerId?: string;
  isActive: boolean;
  roles: Role[];
}

export enum Role {
  Employee = 'Employee',
  Manager = 'Manager',
  HRManager = 'HRManager',
  Admin = 'Admin',
  SuperAdmin = 'SuperAdmin',
  Director = 'Director'
}

export interface LeaveType {
  id: number;
  code: string;
  name: string;
  description?: string;
  defaultDaysPerYear: number;
  requiresApproval: boolean;
  isActive: boolean;
  color?: string;
  sortOrder: number;
}

export interface LeaveBalance {
  leaveTypeCode: string;
  leaveTypeName: string;
  year: number;
  totalDays: number;
  usedDays: number;
  pendingDays: number;
  availableDays: number;
  carryForwardDays: number;
  color?: string;
}

export interface LeaveRequest {
  employeeId: string;
  employeeName: string;
  startDate: string;
  endDate: string;
  leaveType: string;
  reason: string;
  selectedApproverId?: string;
}

export interface LeaveStatus {
  leaveId: string;
  employeeId: string;
  employeeName: string;
  startDate: string;
  endDate: string;
  totalDays: number;
  leaveType: string;
  reason: string;
  currentState: string;
  createdDate: string;
  updatedDate: string;
}

export interface LeaveApproval {
  approverId: string;
  approverName: string;
  comments: string;
  approved: boolean;
}

export interface CommandParameter {
  isRequired: boolean;
  parameterName: string;
  localizedName: string;
  typeName: string;
  defaultValue?: string;
}

export interface WorkflowCommand {
  commandName: string;
  localizedName: string;
  validForActivityName?: string;
  validForStateName?: string;
  classifier?: string;
  identities?: string[];
  processId?: string;
  isForSubprocess: boolean;
  parameters?: CommandParameter[];
}

export interface PendingLeaveRequest {
  leaveRequestId: string;
  employeeId: string;
  employeeName: string;
  employeeEmail: string;
  employeeDepartment: string;
  leaveType: string;
  leaveTypeCode: string;
  leaveTypeColor?: string;
  startDate: string;
  endDate: string;
  totalDays: number;
  reason: string;
  status: string;
  submittedDate: string;
  workflowProcessId: string;
  availableCommands?: WorkflowCommand[];
  balance?: {
    totalDays: number;
    usedDays: number;
    pendingDays: number;
    availableDays: number;
  };
}

export interface MyLeaveRequest {
  leaveRequestId: string;
  employeeId: string;
  employeeName: string;
  leaveType: string;
  leaveTypeCode: string;
  leaveTypeColor?: string;
  startDate: string;
  endDate: string;
  totalDays: number;
  reason: string;
  status: string;
  submittedDate: string;
  workflowProcessId: string;
  availableCommands?: WorkflowCommand[];
  balance?: {
    totalDays: number;
    usedDays: number;
    pendingDays: number;
    availableDays: number;
  };
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
}
