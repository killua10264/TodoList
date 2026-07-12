export interface ProjectResponse {
  id: number;
  name: string;
  color: string;
}

export interface ProjectCreateRequest {
  name: string;
  color: string;
}

export interface ProjectUpdateRequest {
  name: string;
  color: string;
}
