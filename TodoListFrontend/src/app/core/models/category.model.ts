export interface CategoryResponse {
  id: number;
  name: string;
  color: string;
  todoCount?: number;
  completedTodoCount?: number;
}

export interface CategoryCreateRequest {
  name: string;
  color: string;
}

export interface CategoryUpdateRequest {
  name: string;
  color: string;
}
