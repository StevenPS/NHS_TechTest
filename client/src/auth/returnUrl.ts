/**
 * Where to go after sign-in. Only paths on this site are allowed, so a crafted link
 * cannot send the user to another site (open redirect).
 */
export function safeReturnUrl(value: string | null): string {
  if (!value || !value.startsWith('/') || value.startsWith('//') || value.startsWith('/\\')) return '/'
  return value
}
