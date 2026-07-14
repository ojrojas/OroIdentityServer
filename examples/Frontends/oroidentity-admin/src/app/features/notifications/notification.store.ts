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
        http.get<Notification[]>('/api/notifications').subscribe(notifications => {
          (store as any)._notifications.set(notifications);
        });
      },
      markAsRead(id: string): void {
        http.patch(`/api/notifications/${id}/read`, {}).subscribe(() => {
          (store as any)._notifications.update((ns: Notification[]) =>
            ns.map(n => n.id === id ? { ...n, isRead: true } : n)
          );
        });
      },
      markAllAsRead(): void {
        http.patch('/api/notifications/read-all', {}).subscribe(() => {
          (store as any)._notifications.update((ns: Notification[]) =>
            ns.map(n => ({ ...n, isRead: true }))
          );
        });
      },
      addNotification(notification: Notification): void {
        (store as any)._notifications.update((ns: Notification[]) => [notification, ...ns]);
      },
    };
  })
);
