export interface TodoResponse {
  id: number;
  title: string;
  description: string;
  isCompleted: boolean;
  priority: number;
  dueDate: string;
  projectId: number;
  projectName: string;
  projectColor: string;
}

export interface TodoCreateRequest {
  title: string;
  description: string;
  priority: number;
  dueDate: string;
  projectId: number;
}

export interface TodoUpdateRequest {
  title: string;
  description: string;
  priority: number;
  dueDate: string;
  projectId: number;
  isCompleted: boolean;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}
