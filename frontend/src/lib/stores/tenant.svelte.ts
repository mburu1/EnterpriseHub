function createTenantStore() {
	let name = $state<string | null>(null);

	return {
		get name() {
			return name;
		},
		setName(value: string) {
			name = value;
		}
	};
}

export const tenantStore = createTenantStore();
