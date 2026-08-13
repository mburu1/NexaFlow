const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5080';

/**
 * Minimal fetch wrapper. Phase 2: replace with a client generated from the
 * backend's OpenAPI document (see /scalar/v1) once the dashboard needs it.
 */
export async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...init?.headers,
    },
  });

  if (!response.ok) {
    throw new Error(`API request to ${path} failed with status ${response.status}`);
  }

  return (await response.json()) as T;
}
