import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { AuthStore } from '../../../core/auth/auth.store';
import { Login } from './login';

describe('Login', () => {
  let fixture: ComponentFixture<Login>;
  let component: Login;
  let authStore: { login: ReturnType<typeof vi.fn> };
  let router: { navigateByUrl: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    authStore = { login: vi.fn() };
    router = { navigateByUrl: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AuthStore, useValue: authStore },
        { provide: Router, useValue: router },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { queryParamMap: { get: () => null } } },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should not call login when the form is invalid', () => {
    component.submit();

    expect(authStore.login).not.toHaveBeenCalled();
  });

  it('should call login with the form values when valid', () => {
    authStore.login.mockReturnValue(of({ id: '1', email: 'jane@example.com', firstName: 'Jane', lastName: 'Doe' }));
    component['form'].setValue({ email: 'jane@example.com', password: 'Password1!' });

    component.submit();

    expect(authStore.login).toHaveBeenCalledWith({ email: 'jane@example.com', password: 'Password1!' });
  });

  it('should navigate to /tasks on successful login by default', () => {
    authStore.login.mockReturnValue(of({ id: '1', email: 'jane@example.com', firstName: 'Jane', lastName: 'Doe' }));
    component['form'].setValue({ email: 'jane@example.com', password: 'Password1!' });

    component.submit();

    expect(router.navigateByUrl).toHaveBeenCalledWith('/tasks');
  });

  it('should display the backend error message on failed login', () => {
    authStore.login.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 401,
            error: { title: 'Invalid email or password.' },
          }),
      ),
    );
    component['form'].setValue({ email: 'jane@example.com', password: 'wrong' });

    component.submit();

    expect(component['errorMessage']()).toBe('Invalid email or password.');
  });
});
