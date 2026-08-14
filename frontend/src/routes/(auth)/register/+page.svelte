<script lang="ts">
	import { goto } from '$app/navigation';
	import { authStore } from '$lib/stores/auth.svelte';
	import { ApiError } from '$lib/api/client';

	let organizationName = $state('');
	let email = $state('');
	let password = $state('');
	let firstName = $state('');
	let lastName = $state('');
	let error = $state<string | null>(null);
	let submitting = $state(false);

	async function handleSubmit(event: SubmitEvent) {
		event.preventDefault();
		error = null;
		submitting = true;
		try {
			await authStore.register({ organizationName, email, password, firstName, lastName });
			await goto('/dashboard');
		} catch (err) {
			error = err instanceof ApiError ? err.message : 'Registration failed.';
		} finally {
			submitting = false;
		}
	}
</script>

<h1>Create your organization</h1>

<form onsubmit={handleSubmit}>
	<label>
		Organization name
		<input bind:value={organizationName} required />
	</label>
	<label>
		First name
		<input bind:value={firstName} required />
	</label>
	<label>
		Last name
		<input bind:value={lastName} required />
	</label>
	<label>
		Email
		<input type="email" bind:value={email} required />
	</label>
	<label>
		Password
		<input type="password" bind:value={password} required minlength="8" />
	</label>
	{#if error}
		<p role="alert">{error}</p>
	{/if}
	<button type="submit" disabled={submitting}>Create account</button>
</form>
