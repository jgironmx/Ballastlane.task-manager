/**
 * The backend's `DueDate` is a .NET `DateOnly?`, serialized as a plain `"yyyy-MM-dd"` string with
 * no time or timezone component (see docs/decisions/ADR-006-taskitem-domain-model.md). Treating it
 * as an instant — e.g. `new Date('2026-07-26')` then formatting with the browser's local timezone
 * — can silently shift the displayed calendar day backward or forward depending on the viewer's
 * timezone. Every helper here works on the ISO string directly and never constructs a `Date` from
 * it, so there is no timezone conversion to get wrong.
 */

const DATE_ONLY_PATTERN = /^(\d{4})-(\d{2})-(\d{2})$/;
const MONTH_NAMES = [
  'Jan',
  'Feb',
  'Mar',
  'Apr',
  'May',
  'Jun',
  'Jul',
  'Aug',
  'Sep',
  'Oct',
  'Nov',
  'Dec',
];

/** Today's date in the viewer's local calendar, as `"yyyy-MM-dd"` — matches what `<input
 * type="date">` and the backend both expect, and what "today" means to the user. */
export function todayAsDateOnly(): string {
  const now = new Date();
  const year = now.getFullYear();
  const month = String(now.getMonth() + 1).padStart(2, '0');
  const day = String(now.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

/** ISO `yyyy-MM-dd` strings compare lexicographically in the same order as chronologically, so
 * this is a plain string comparison — no date parsing involved. */
export function isDateOnlyBefore(value: string, reference: string): boolean {
  return value < reference;
}

/** Human-readable display (e.g. "Aug 15, 2026") without ever constructing a `Date` object. */
export function formatDateOnly(value: string | null | undefined): string {
  if (!value) {
    return '—';
  }

  const match = DATE_ONLY_PATTERN.exec(value);
  if (!match) {
    return value;
  }

  const [, year, month, day] = match;
  const monthName = MONTH_NAMES[Number(month) - 1] ?? month;
  return `${monthName} ${Number(day)}, ${year}`;
}
