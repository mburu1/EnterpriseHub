interface NotificationItem {
	id: string;
	message: string;
	isRead: boolean;
}

function createNotificationsStore() {
	let items = $state<NotificationItem[]>([]);

	return {
		get items() {
			return items;
		},
		get unreadCount() {
			return items.filter((n) => !n.isRead).length;
		},
		set(value: NotificationItem[]) {
			items = value;
		},
		markRead(id: string) {
			items = items.map((n) => (n.id === id ? { ...n, isRead: true } : n));
		}
	};
}

export const notificationsStore = createNotificationsStore();
