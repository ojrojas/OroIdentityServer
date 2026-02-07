import { Routes } from '@angular/router';
import { autoLoginPartialRoutesGuard } from 'angular-auth-oidc-client';
import { HomeComponent } from './home.component';

export const routes: Routes = [
	{ path: '', component: HomeComponent, canActivate: [autoLoginPartialRoutesGuard] },
	{ path: 'signin-oidc', component: HomeComponent }
];
