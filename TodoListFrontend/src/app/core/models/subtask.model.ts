export interface SubTaskResponse {
  id: number;
  title: string;
  isCompleted: boolean;
  sortOrder: number;
  leafShape: number; // 0-4: 5 kiểu dáng lá khác nhau
  createdAt: string;
  todoId: number;
}

export interface SubTaskCreateDto {
  title: string;
  todoId: number;
}

export interface SubTaskUpdateDto {
  title: string;
  isCompleted: boolean;
  sortOrder: number;
}
