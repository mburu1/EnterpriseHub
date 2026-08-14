import { authApi, type UserDto } from '$lib/api/client';

const ACCESS_TOKEN_KEY = 'eh_access_token';
const REFRESH_TOKEN_KEY = 'eh_refresh_token';

function createAuthStore() {
	let user = $state<UserDto | null>(null);
	let accessToken = $state<string | null>(
		typeof localStorage !== 'undefined' ? localStorage.getItem(ACCESS_TOKEN_KEY) : null
	);

	function persist(newAccessToken: string, refreshToken: string) {
		accessToken = newAccessToken;
		localStorage.setItem(ACCESS_TOKEN_KEY, newAccessToken);
		localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
	}

	return {
		get user() {
			return user;
		},
		get accessToken() {
			return accessToken;
		},
		get isAuthenticated() {
			return accessToken !== null;
		},

		async login(email: string, password: string) {
			const result = await authApi.login({ email, password });
			persist(result.accessToken, result.refreshToken);
			user = result.user;
		},

		async register(input: {
			organizationName: string;
			email: string;
			password: string;
			firstName: string;
			lastName: string;
		}) {
			const result = await authApi.register(input);
			persist(result.accessToken, result.refreshToken);
			user = result.user;
		},

		async loadCurrentUser() {
			if (!accessToken) return;
			user = await authApi.me(accessToken);
		},

		logout() {
			user = null;
			accessToken = null;
			localStorage.removeItem(ACCESS_TOKEN_KEY);
			localStorage.removeItem(REFRESH_TOKEN_KEY);
		}
	};
}

export const authStore = createAuthStore();
