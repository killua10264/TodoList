import { Routes } from '@angular/router';
import { AuthLayoutComponent } from './layouts/auth-layout/auth-layout';
import { RegisterComponent } from './features/auth/register/register';
import { LoginComponent } from './features/auth/login/login';

export const routes: Routes = [
  {
    path: '',
    component: AuthLayoutComponent,
    children: [
      { path: 'register', component: RegisterComponent },
      { path: '', redirectTo: 'register', pathMatch: 'full' },
      { path: 'login', component: LoginComponent },
      { path: '', redirectTo: 'login', pathMatch: 'full' }
    ]
  }
];
