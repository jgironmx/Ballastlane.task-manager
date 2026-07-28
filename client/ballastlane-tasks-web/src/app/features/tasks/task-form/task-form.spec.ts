import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { todayAsDateOnly } from '../../../shared/utilities/date-only';
import { TaskForm, TaskFormValue } from './task-form';

describe('TaskForm', () => {
  let fixture: ComponentFixture<TaskForm>;
  let component: TaskForm;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TaskForm],
    }).compileComponents();

    fixture = TestBed.createComponent(TaskForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should require a title', () => {
    component['form'].controls.title.setValue('');

    expect(component['form'].controls.title.hasError('required')).toBe(true);
  });

  it('should reject a title longer than 200 characters', () => {
    component['form'].controls.title.setValue('a'.repeat(201));

    expect(component['form'].controls.title.hasError('maxlength')).toBe(true);
  });

  it('should reject a description longer than 2000 characters', () => {
    component['form'].controls.description.setValue('a'.repeat(2001));

    expect(component['form'].controls.description.hasError('maxlength')).toBe(true);
  });

  it('should reject a due date before today when enforceFutureDueDate is true (default)', () => {
    component['form'].controls.title.setValue('Task');
    component['form'].controls.dueDate.setValue('2000-01-01');

    expect(component['form'].controls.dueDate.hasError('pastDate')).toBe(true);
  });

  it('should allow a past due date when enforceFutureDueDate is false (edit mode)', () => {
    fixture.componentRef.setInput('enforceFutureDueDate', false);
    component['form'].controls.title.setValue('Task');
    component['form'].controls.dueDate.setValue('2000-01-01');

    expect(component['form'].controls.dueDate.hasError('pastDate')).toBe(false);
  });

  it('should emit a trimmed, correctly-shaped value on save — no owner/status fields', () => {
    const emitted: TaskFormValue[] = [];
    component.save.subscribe((value) => emitted.push(value));

    component['form'].setValue({
      title: '  Write report  ',
      description: '  ',
      dueDate: '',
    });
    component.submit();

    expect(emitted).toEqual([{ title: 'Write report', description: null, dueDate: null }]);
    expect(Object.keys(emitted[0])).toEqual(['title', 'description', 'dueDate']);
  });

  it('should not emit when the form is invalid', () => {
    const onSave = vi.fn();
    component.save.subscribe(onSave);

    component['form'].controls.title.setValue('');
    component.submit();

    expect(onSave).not.toHaveBeenCalled();
  });

  it('should pre-fill the form from initialValue', () => {
    fixture.componentRef.setInput('initialValue', {
      id: 'task-1',
      title: 'Existing task',
      description: 'Existing description',
      status: 'Pending',
      dueDate: todayAsDateOnly(),
      createdAtUtc: '2026-01-01T00:00:00Z',
      updatedAtUtc: null,
    });
    fixture.detectChanges();

    expect(component['form'].controls.title.value).toBe('Existing task');
    expect(component['form'].controls.description.value).toBe('Existing description');
  });
});
