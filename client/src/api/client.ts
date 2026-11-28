/** RFC 9457 problem details, as returned by the API for errors. */
export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  errors?: Record<string, string[]>
}

export class ApiError extends Error {
  readonly status: number
  readonly problem?: ProblemDetails

  constructor(status: number, problem?: ProblemDetails) {
    super(problem?.detail ?? problem?.title ?? `Request failed with status ${status}`)
    this.name = 'ApiError'
    this.status = status
    this.problem = problem
  }
}

async function readProblem(response: Response): Promise<ProblemDetails | undefined> {
  const contentType = response.headers.get('content-type') ?? ''
  if (!contentType.includes('json')) return undefined
  try {
    return (await response.json()) as ProblemDetails
  } catch {
    return undefined
  }
}

/** Cookie the API sets with a fresh antiforgery token (`GET /api/auth/antiforgery`). */
const ANTIFORGERY_COOKIE = 'XSRF-TOKEN'
/** Header the API checks on every POST/PUT/PATCH/DELETE. */
const ANTIFORGERY_HEADER = 'X-XSRF-TOKEN'

function readCookie(name: string): string | undefined {
  const prefix = `${name}=`
  const match = document.cookie.split('; ').find((part) => part.startsWith(prefix))
  return match ? decodeURIComponent(match.slice(prefix.length)) : undefined
}

/**
 * Asks the API for a new antiforgery token. The token is bound to the signed-in user, so call this
 * on start-up and again after signing in or out.
 */
export async function refreshAntiforgeryToken(): Promise<void> {
  const response = await fetch('/api/auth/antiforgery', { credentials: 'same-origin' })
  if (!response.ok) {
    throw new ApiError(response.status, await readProblem(response))
  }
}

/**
 * Sends a state-changing request with the antiforgery header. Returns the parsed JSON body,
 * or undefined for 204 No Content. Throws {@link ApiError} for non-2xx responses.
 */
export async function sendJson<T = void>(
  method: 'POST' | 'PUT' | 'PATCH' | 'DELETE',
  path: string,
  body?: unknown,
): Promise<T> {
  if (readCookie(ANTIFORGERY_COOKIE) === undefined) {
    await refreshAntiforgeryToken()
  }

  const headers: Record<string, string> = {
    Accept: 'application/json',
    [ANTIFORGERY_HEADER]: readCookie(ANTIFORGERY_COOKIE) ?? '',
  }
  if (body !== undefined) headers['Content-Type'] = 'application/json'

  const response = await fetch(path, {
    method,
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
    credentials: 'same-origin',
  })

  if (!response.ok) {
    throw new ApiError(response.status, await readProblem(response))
  }

  return (response.status === 204 ? undefined : await response.json()) as T
}

/** GET a JSON resource from the API. Throws {@link ApiError} for non-2xx responses. */
export async function getJson<T>(path: string, signal?: AbortSignal): Promise<T> {
  const response = await fetch(path, {
    headers: { Accept: 'application/json' },
    credentials: 'same-origin',
    signal,
  })

  if (!response.ok) {
    throw new ApiError(response.status, await readProblem(response))
  }

  return (await response.json()) as T
}
