import { PUBLIC_API_BASE_URL } from '$env/static/public';

export interface AuthResponse {
	accessToken: string;
	accessTokenExpiresAt: string;
	refreshToken: string;
	user: UserDto;
}

export interface UserDto {
	id: string;
	tenantId: string;
	email: string;
	firstName: string;
	lastName: string;
	role: string;
}

export class ApiError extends Error {
	constructor(
		public status: number,
		message: string
	) {
		super(message);
	}
}

async function request<T>(path: string, init?: RequestInit, accessToken?: string): Promise<T> {
	const response = await fetch(`${PUBLIC_API_BASE_URL}${path}`, {
		...init,
		headers: {
			'Content-Type': 'application/json',
			...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
			...init?.headers
		}
	});

	if (!response.ok) {
		const problem = await response.json().catch(() => ({ title: response.statusText }));
		throw new ApiError(response.status, problem.title ?? 'Request failed');
	}

	return response.status === 204 ? (undefined as T) : ((await response.json()) as T);
}

export const authApi = {
	register: (body: {
		organizationName: string;
		email: string;
		password: string;
		firstName: string;
		lastName: string;
	}) => request<AuthResponse>('/auth/register', { method: 'POST', body: JSON.stringify(body) }),

	login: (body: { email: string; password: string }) =>
		request<AuthResponse>('/auth/login', { method: 'POST', body: JSON.stringify(body) }),

	refresh: (refreshToken: string) =>
		request<AuthResponse>('/auth/refresh', {
			method: 'POST',
			body: JSON.stringify({ refreshToken })
		}),

	me: (accessToken: string) => request<UserDto>('/auth/me', {}, accessToken)
};
