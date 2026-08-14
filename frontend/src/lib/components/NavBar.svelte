<script lang="ts">
	import { authStore } from '$lib/stores/auth.svelte';
</script>

<nav>
	<a href="/">EnterpriseHub</a>
	{#if authStore.isAuthenticated}
		<a href="/dashboard">Dashboard</a>
		<a href="/projects">Projects</a>
		<a href="/notifications">Notifications</a>
		<a href="/billing">Billing</a>
		<a href="/admin">Admin</a>
		<button onclick={() => authStore.logout()}>Log out</button>
	{:else}
		<a href="/login">Log in</a>
		<a href="/register">Register</a>
	{/if}
</nav>

<style>
	nav {
		display: flex;
		gap: 1rem;
		align-items: center;
		padding: 1rem;
		border-bottom: 1px solid #ddd;
	}
</style>
