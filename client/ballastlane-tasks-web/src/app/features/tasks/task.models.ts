/** Matches Ballastlane.Tasks.Domain.Tasks.TaskItemStatus's JSON string serialization exactly
 * (confirmed against a running instance — the backend registers a global JsonStringEnumConverter). */
export type TaskItemStatus = 'Pending' | 'InProgress' | 'Completed';

export const TASK_STATUSES: TaskItemStatus[] = ['Pending', 'InProgress', 'Completed'];

/** Matches Ballastlane.Tasks.Application.Contracts.TaskDto exactly. `dueDate` is a plain
 * "yyyy-MM-dd" DateOnly string — see shared/utilities/date-only.ts. */
export interface TaskItem {
  id: string;
  title: string;
  description: string | null;
  status: TaskItemStatus;
  dueDate: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

/** Matches Ballastlane.Tasks.Application.Contracts.PagedResult<TaskDto> exactly. */
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

/** Matches Ballastlane.Tasks.Api.Contracts.Tasks.CreateTaskRequest exactly. */
export interface CreateTaskRequest {
  title: string;
  description: string | null;
  dueDate: string | null;
}

/** Matches Ballastlane.Tasks.Api.Contracts.Tasks.UpdateTaskRequest exactly. */
export interface UpdateTaskRequest {
  title: string;
  description: string | null;
  dueDate: string | null;
}

/** Matches Ballastlane.Tasks.Api.Contracts.Tasks.ChangeTaskStatusRequest exactly. */
export interface ChangeTaskStatusRequest {
  status: TaskItemStatus;
}

/** Query parameters supported by GET /api/tasks. */
export interface ListTasksParams {
  status?: TaskItemStatus;
  search?: string;
  page?: number;
  pageSize?: number;
}
