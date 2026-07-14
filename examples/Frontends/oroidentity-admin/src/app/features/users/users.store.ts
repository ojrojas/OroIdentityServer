import { inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { UserDto, CreateUserCommand, UpdateUserCommand, ApiEnvelope } from '../../core/models';

interface UsersState {
  users: UserDto[];
  loading: boolean;
}

export const UsersStore = signalStore(
  { providedIn: 'root' },
  withState<UsersState>({ users: [], loading: false }),
  withMethods((store, http = inject(HttpClient)) => ({
    load(): void {
      patchState(store, { loading: true });
      http.get<ApiEnvelope<UserDto[]>>('/api/users').subscribe({
        next: res => patchState(store, { users: res.data ?? [] }),
        complete: () => patchState(store, { loading: false }),
      });
    },
    create(cmd: CreateUserCommand): void {
      http.post('/api/users', cmd).subscribe(() => patchState(store, { loading: true }));
      this.load();
    },
    update(id: string, cmd: Omit<UpdateUserCommand, 'userId'>): void {
      http.put(`/api/users/${id}`, { ...cmd, userId: id }).subscribe(() => this.load());
    },
    delete(id: string): void {
      http.delete(`/api/users/${id}`).subscribe(() => this.load());
    },
  }))
);
