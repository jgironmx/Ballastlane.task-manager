import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { AuthStore } from '../../../core/auth/auth.store';
import { Register } from './register';

describe('Register', () => {
  let fixture: ComponentFixture<Register>;
  let component: Register;
  let authStore: { register: ReturnType<typeof vi.fn> };
  let router: { navigate: ReturnType<typeof vi.fn> };

  const validValue = {
    firstName: 'Jane',
    lastName: 'Doe',
    email: 'jane@example.com',
    password: 'Password1',
    confirmPassword: 'Password1',
  };

  beforeEach(async () => {
    authStore = { register: vi.fn() };
    router = { navigate: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [Register],
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

    fixture = TestBed.createComponent(Register);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should require all fields', () => {
    expect(component['form'].valid).toBe(false);

    component.submit();

    expect(authStore.register).not.toHaveBeenCalled();
  });

  it('should flag a confirm-password mismatch', () => {
    component['form'].setValue({ ...validValue, confirmPassword: 'Different1' });

    expect(component['form'].controls.confirmPassword.hasError('passwordMismatch')).toBe(true);
  });

  it('should call register and navigate to /login on success', () => {
    authStore.register.mockReturnValue(
      of({ id: '1', email: validValue.email, firstName: 'Jane', lastName: 'Doe' }),
    );
    component['form'].setValue(validValue);

    component.submit();

    expect(authStore.register).toHaveBeenCalledWith({
      email: validValue.email,
      password: validValue.password,
      firstName: 'Jane',
      lastName: 'Doe',
    });
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should display a safe message on duplicate email (409)', () => {
    authStore.register.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 409,
            error: { title: 'An account with this email already exists.' },
          }),
      ),
    );
    component['form'].setValue(validValue);

    component.submit();

    expect(component['errorMessage']()).toBe('An account with this email already exists.');
  });
});
