import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { TaskItem } from '../task.models';
import { TaskService } from '../task.service';
import { TaskList } from './task-list';

const task: TaskItem = {
  id: 'task-1',
  title: 'Write report',
  description: 'Quarterly summary',
  status: 'Pending',
  dueDate: '2026-08-15',
  createdAtUtc: '2026-07-01T00:00:00Z',
  updatedAtUtc: null,
};

describe('TaskList', () => {
  let fixture: ComponentFixture<TaskList>;
  let component: TaskList;
  let taskService: {
    getTasks: ReturnType<typeof vi.fn>;
    changeStatus: ReturnType<typeof vi.fn>;
    deleteTask: ReturnType<typeof vi.fn>;
  };

  function setup(getTasksResult: unknown) {
    taskService = {
      getTasks: vi.fn().mockReturnValue(getTasksResult),
      changeStatus: vi.fn(),
      deleteTask: vi.fn(),
    };

    TestBed.configureTestingModule({
      imports: [TaskList],
      providers: [provideRouter([]), { provide: TaskService, useValue: taskService }],
    });

    fixture = TestBed.createComponent(TaskList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  it('should not be loading once the request resolves', () => {
    setup(of({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 }));

    expect(component['loading']()).toBe(false);
  });

  it('should show the empty state when there are no tasks', () => {
    setup(of({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 }));

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('app-empty-state')).not.toBeNull();
  });

  it('should render tasks returned by the API', () => {
    setup(of({ items: [task], page: 1, pageSize: 20, totalCount: 1, totalPages: 1 }));

    expect(component['tasks']()).toEqual([task]);
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Write report');
  });

  it('should show an error state when loading fails', () => {
    setup(throwError(() => new HttpErrorResponse({ status: 500 })));

    expect(component['errorMessage']()).toBeTruthy();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.error-state')).not.toBeNull();
  });

  it('should update the task in place after a successful status change', () => {
    setup(of({ items: [task], page: 1, pageSize: 20, totalCount: 1, totalPages: 1 }));
    const updated = { ...task, status: 'InProgress' as const };
    taskService.changeStatus.mockReturnValue(of(updated));

    component['changeStatus'](task, 'InProgress');

    expect(taskService.changeStatus).toHaveBeenCalledWith('task-1', { status: 'InProgress' });
    expect(component['tasks']()[0].status).toBe('InProgress');
  });

  it('should remove the task from the list after confirmed deletion', () => {
    setup(of({ items: [task], page: 1, pageSize: 20, totalCount: 1, totalPages: 1 }));
    taskService.deleteTask.mockReturnValue(of(void 0));

    component['requestDelete'](task);
    component['confirmDelete']();

    expect(taskService.deleteTask).toHaveBeenCalledWith('task-1');
    expect(component['tasks']()).toEqual([]);
  });

  it('should remove a task and show an info message on 404 delete (already gone)', () => {
    setup(of({ items: [task], page: 1, pageSize: 20, totalCount: 1, totalPages: 1 }));
    taskService.deleteTask.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 404 })));

    component['requestDelete'](task);
    component['confirmDelete']();

    expect(component['tasks']()).toEqual([]);
  });
});
