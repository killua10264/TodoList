export interface TodoResponse {
  id: number;
  title: string;
  description: string;
  isCompleted: boolean;
  priority: number;
  dueDate: string;
  categoryId: number;
  categoryName: string;
  categoryColor: string;
  isHidden?: boolean;
}

export interface TodoCreateRequest {
  title: string;
  description: string;
  priority: number;
  dueDate: string;
  categoryId: number;
}

export interface TodoUpdateRequest {
  title?: string;
  description?: string;
  priority?: number;
  dueDate?: string;
  categoryId?: number;
  isCompleted?: boolean;
  isHidden?: boolean;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages?: number;
}
