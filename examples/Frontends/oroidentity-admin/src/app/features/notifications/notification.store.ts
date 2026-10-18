import { inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { signalStore, withState, withMethods, withComputed } from '@ngrx/signals';
import { computed } from '@angular/core';
import { Notification } from '../../core/models';

interface NotificationState {
  notifications: Notification[];
  loading: boolean;
}

export const NotificationStore = signalStore(
  { providedIn: 'root' },
  withState<NotificationState>({ notifications: [], loading: false }),
  withComputed(store => ({
    unreadCount: computed(() => store.notifications().filter(n => !n.isRead).length),
    unreadNotifications: computed(() => store.notifications().filter(n => !n.isRead)),
  })),
  withMethods(store => {
    const http = inject(HttpClient);
    return {
      load(): void {
        // The identity server exposes no /api/notifications today, so this legitimately 404s.
        // Swallowing it keeps the header bell empty instead of throwing an unhandled observable
        // error on every render of the shell.
        http.get<Notification[]>('/api/notifications').subscribe({
          next: notifications => (store as any)._notifications.set(notifications ?? []),
          error: () => (store as any)._notifications.set([]),
        });
      },
      markAsRead(id: string): void {
        http.patch(`/api/notifications/${id}/read`, {}).subscribe({
          next: () => {
            (store as any)._notifications.update((ns: Notification[]) =>
              ns.map(n => n.id === id ? { ...n, isRead: true } : n)
            );
          },
          error: () => undefined,
        });
      },
      markAllAsRead(): void {
        http.patch('/api/notifications/read-all', {}).subscribe({
          next: () => {
            (store as any)._notifications.update((ns: Notification[]) =>
              ns.map(n => ({ ...n, isRead: true }))
            );
          },
          error: () => undefined,
        });
      },
      addNotification(notification: Notification): void {
        (store as any)._notifications.update((ns: Notification[]) => [notification, ...ns]);
      },
    };
  })
);
