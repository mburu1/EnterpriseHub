<script lang="ts">
	import { onMount } from 'svelte';
	import { authStore } from '$lib/stores/auth.svelte';

	onMount(() => {
		authStore.loadCurrentUser();
	});
</script>

<h1>Dashboard</h1>
{#if authStore.user}
	<p>Welcome back, {authStore.user.firstName}.</p>
{/if}
