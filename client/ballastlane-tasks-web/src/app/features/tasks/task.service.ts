import { HttpClient, HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../core/config/api-config';
import {
  ChangeTaskStatusRequest,
  CreateTaskRequest,
  ListTasksParams,
  PagedResult,
  TaskItem,
  UpdateTaskRequest,
} from './task.models';

/** Focused wrapper over /api/tasks/* — one method per real backend operation, nothing generic. */
@Injectable({ providedIn: 'root' })
export class TaskService {
  constructor(
    private readonly http: HttpClient,
    @Inject(API_BASE_URL) private readonly baseUrl: string,
  ) {}

  getTasks(params: ListTasksParams = {}): Observable<PagedResult<TaskItem>> {
    let httpParams = new HttpParams();
    if (params.status) {
      httpParams = httpParams.set('status', params.status);
    }
    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }
    if (params.page) {
      httpParams = httpParams.set('page', params.page);
    }
    if (params.pageSize) {
      httpParams = httpParams.set('pageSize', params.pageSize);
    }

    return this.http.get<PagedResult<TaskItem>>(`${this.baseUrl}/api/tasks`, { params: httpParams });
  }

  getTaskById(id: string): Observable<TaskItem> {
    return this.http.get<TaskItem>(`${this.baseUrl}/api/tasks/${id}`);
  }

  createTask(request: CreateTaskRequest): Observable<TaskItem> {
    return this.http.post<TaskItem>(`${this.baseUrl}/api/tasks`, request);
  }

  updateTask(id: string, request: UpdateTaskRequest): Observable<TaskItem> {
    return this.http.put<TaskItem>(`${this.baseUrl}/api/tasks/${id}`, request);
  }

  changeStatus(id: string, request: ChangeTaskStatusRequest): Observable<TaskItem> {
    return this.http.patch<TaskItem>(`${this.baseUrl}/api/tasks/${id}/status`, request);
  }

  deleteTask(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/tasks/${id}`);
  }
}
