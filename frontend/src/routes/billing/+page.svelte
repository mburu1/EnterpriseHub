<script lang="ts">
	import { authStore } from '$lib/stores/auth.svelte';
</script>

<h1>Billing</h1>
<p>Subscription plan and usage for {authStore.user?.tenantId ?? 'your organization'}.</p>
