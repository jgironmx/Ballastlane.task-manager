import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from '../../core/config/api-config';
import { PagedResult, TaskItem } from './task.models';
import { TaskService } from './task.service';

const BASE_URL = 'http://localhost:5276';

const task: TaskItem = {
  id: 'task-1',
  title: 'Write report',
  description: 'Quarterly summary',
  status: 'Pending',
  dueDate: '2026-08-15',
  createdAtUtc: '2026-07-01T00:00:00Z',
  updatedAtUtc: null,
};

describe('TaskService', () => {
  let service: TaskService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        { provide: API_BASE_URL, useValue: BASE_URL },
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(TaskService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('getTasks should GET /api/tasks with no params by default', () => {
    const page: PagedResult<TaskItem> = { items: [task], page: 1, pageSize: 20, totalCount: 1, totalPages: 1 };

    service.getTasks().subscribe((result) => expect(result).toEqual(page));

    const request = httpTesting.expectOne((req) => req.url === `${BASE_URL}/api/tasks`);
    expect(request.request.method).toBe('GET');
    expect(request.request.params.keys().length).toBe(0);
    request.flush(page);
  });

  it('getTasks should forward status/search/page/pageSize as query params', () => {
    service.getTasks({ status: 'InProgress', search: 'report', page: 2, pageSize: 10 }).subscribe();

    const request = httpTesting.expectOne(
      (req) => req.url === `${BASE_URL}/api/tasks` && req.method === 'GET',
    );
    expect(request.request.params.get('status')).toBe('InProgress');
    expect(request.request.params.get('search')).toBe('report');
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('10');
    request.flush({ items: [], page: 2, pageSize: 10, totalCount: 0, totalPages: 0 });
  });

  it('getTaskById should GET /api/tasks/{id}', () => {
    service.getTaskById('task-1').subscribe((result) => expect(result).toEqual(task));

    const request = httpTesting.expectOne(`${BASE_URL}/api/tasks/task-1`);
    expect(request.request.method).toBe('GET');
    request.flush(task);
  });

  it('createTask should POST /api/tasks with the request body', () => {
    const body = { title: 'New task', description: null, dueDate: null };

    service.createTask(body).subscribe((result) => expect(result).toEqual(task));

    const request = httpTesting.expectOne(`${BASE_URL}/api/tasks`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(body);
    request.flush(task);
  });

  it('updateTask should PUT /api/tasks/{id} with the request body', () => {
    const body = { title: 'Updated', description: null, dueDate: null };

    service.updateTask('task-1', body).subscribe((result) => expect(result).toEqual(task));

    const request = httpTesting.expectOne(`${BASE_URL}/api/tasks/task-1`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(body);
    request.flush(task);
  });

  it('changeStatus should PATCH /api/tasks/{id}/status with the request body', () => {
    service.changeStatus('task-1', { status: 'Completed' }).subscribe((result) => expect(result).toEqual(task));

    const request = httpTesting.expectOne(`${BASE_URL}/api/tasks/task-1/status`);
    expect(request.request.method).toBe('PATCH');
    expect(request.request.body).toEqual({ status: 'Completed' });
    request.flush(task);
  });

  it('deleteTask should DELETE /api/tasks/{id}', () => {
    service.deleteTask('task-1').subscribe();

    const request = httpTesting.expectOne(`${BASE_URL}/api/tasks/task-1`);
    expect(request.request.method).toBe('DELETE');
    request.flush(null);
  });
});
