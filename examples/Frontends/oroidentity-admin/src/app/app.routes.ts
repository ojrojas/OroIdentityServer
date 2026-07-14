import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./core/auth/login/login.component').then(m => m.LoginComponent) },
  { path: 'auth/callback', loadComponent: () => import('./core/auth/callback/callback.component').then(m => m.CallbackComponent) },
  {
    path: '',
    loadComponent: () => import('./core/layout/shell/shell.component').then(m => m.ShellComponent),
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      { path: 'notifications', loadComponent: () => import('./features/notifications/notification-full-page.component').then(m => m.NotificationFullPageComponent) },
      { path: 'settings', loadComponent: () => import('./features/settings/settings-page.component').then(m => m.SettingsPageComponent) },
      { path: 'admin/settings', loadComponent: () => import('./features/settings/tenant-settings-page.component').then(m => m.TenantSettingsPageComponent), canActivate: [authGuard] },

      // Users
      { path: 'users', loadComponent: () => import('./features/users/users-list.component').then(m => m.UsersListComponent) },
      { path: 'users/new', loadComponent: () => import('./features/users/users-form.component').then(m => m.UsersFormComponent) },
      { path: 'users/:id/edit', loadComponent: () => import('./features/users/users-form.component').then(m => m.UsersFormComponent) },

      // Roles
      { path: 'roles', loadComponent: () => import('./features/roles/roles-list.component').then(m => m.RolesListComponent) },
      { path: 'roles/new', loadComponent: () => import('./features/roles/roles-form.component').then(m => m.RolesFormComponent) },
      { path: 'roles/:id/edit', loadComponent: () => import('./features/roles/roles-form.component').then(m => m.RolesFormComponent) },

      // Permissions
      { path: 'permissions', loadComponent: () => import('./features/permissions/permissions-list.component').then(m => m.PermissionsListComponent) },
      { path: 'permissions/new', loadComponent: () => import('./features/permissions/permissions-form.component').then(m => m.PermissionsFormComponent) },
      { path: 'permissions/:id/edit', loadComponent: () => import('./features/permissions/permissions-form.component').then(m => m.PermissionsFormComponent) },

      // Applications (OAuth)
      { path: 'applications', loadComponent: () => import('./features/applications/applications-list.component').then(m => m.ApplicationsListComponent) },
      { path: 'applications/new', loadComponent: () => import('./features/applications/applications-form.component').then(m => m.ApplicationsFormComponent) },
      { path: 'applications/:clientId/edit', loadComponent: () => import('./features/applications/applications-form.component').then(m => m.ApplicationsFormComponent) },

      // Scopes
      { path: 'scopes', loadComponent: () => import('./features/scopes/scopes-list.component').then(m => m.ScopesListComponent) },
      { path: 'scopes/new', loadComponent: () => import('./features/scopes/scopes-form.component').then(m => m.ScopesFormComponent) },
      { path: 'scopes/:name/edit', loadComponent: () => import('./features/scopes/scopes-form.component').then(m => m.ScopesFormComponent) },
    ]
  },
  { path: '**', redirectTo: '' }
];
