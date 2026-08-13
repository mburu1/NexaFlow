// Mirrors NexaFlow.Application DTOs (backend/src/NexaFlow.Application/DTOs).
// Phase 2: keep in sync manually, or generate from the OpenAPI document.

export interface UserResponse {
  id: string;
  email: string;
  fullName: string;
  role: 'Admin' | 'Manager' | 'Member';
  tenantId: string;
  tenantName: string;
}

export interface AuthResponse {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
  user: UserResponse;
}

export interface WorkflowResponse {
  id: string;
  name: string;
  description: string | null;
  status: 'Draft' | 'Active' | 'Paused' | 'Completed' | 'Archived';
  createdByUserId: string;
  createdAtUtc: string;
  taskCount: number;
}

export interface WorkflowTaskResponse {
  id: string;
  workflowId: string;
  title: string;
  description: string | null;
  status: 'Pending' | 'InProgress' | 'Blocked' | 'Completed' | 'Cancelled';
  assignedToUserId: string | null;
  dueAtUtc: string | null;
  createdAtUtc: string;
}
