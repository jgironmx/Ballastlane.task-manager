import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** Applied at the FormGroup level (not the confirm-password control) so it can compare two sibling
 * controls. Sets `passwordMismatch` on the confirm-password control itself so field-level error
 * display stays consistent with every other field. */
export function passwordMatchValidator(
  passwordControlName: string,
  confirmControlName: string,
): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const password = group.get(passwordControlName);
    const confirm = group.get(confirmControlName);

    if (!password || !confirm) {
      return null;
    }

    if (confirm.value && confirm.value !== password.value) {
      confirm.setErrors({ ...confirm.errors, passwordMismatch: true });
    } else if (confirm.errors) {
      const { passwordMismatch, ...rest } = confirm.errors;
      confirm.setErrors(Object.keys(rest).length > 0 ? rest : null);
    }

    return null;
  };
}
