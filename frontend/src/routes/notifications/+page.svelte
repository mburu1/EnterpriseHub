<script lang="ts">
	import { notificationsStore } from '$lib/stores/notifications.svelte';
</script>

<h1>Notifications</h1>
{#if notificationsStore.items.length === 0}
	<p>You're all caught up.</p>
{:else}
	<ul>
		{#each notificationsStore.items as item (item.id)}
			<li>{item.message}</li>
		{/each}
	</ul>
{/if}
