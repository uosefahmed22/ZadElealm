import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { ConfirmEmailComponent } from './features/auth/confirm-email/confirm-email.component';
import { ForgotPasswordComponent } from './features/auth/forgot-password/forgot-password.component';
import { LandingPageComponent } from './features/landing/landing-page.component';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { ResetPasswordComponent } from './features/auth/reset-password/reset-password.component';
import { VerifyOtpComponent } from './features/auth/verify-otp/verify-otp.component';
import { AuthLayoutComponent } from './layouts/auth-layout/auth-layout.component';
import { StudentLayoutComponent } from './layouts/student-layout/student-layout.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', component: LandingPageComponent },
  {
    path: '',
    component: AuthLayoutComponent,
    children: [
      { path: 'login', component: LoginComponent, canActivate: [guestGuard] },
      { path: 'register', component: RegisterComponent, canActivate: [guestGuard] },
      { path: 'forgot-password', component: ForgotPasswordComponent, canActivate: [guestGuard] },
      { path: 'verify-otp', component: VerifyOtpComponent, canActivate: [guestGuard] },
      { path: 'reset-password', component: ResetPasswordComponent, canActivate: [guestGuard] },
      { path: 'confirm-email', component: ConfirmEmailComponent },
    ],
  },
  {
    path: 'app',
    component: StudentLayoutComponent,
    canActivate: [authGuard],
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () =>
          import('./features/student-home/student-home.component').then(
            (component) => component.StudentHomeComponent,
          ),
      },
      {
        path: 'courses',
        loadComponent: () =>
          import('./features/course-catalog/course-catalog.component').then(
            (component) => component.CourseCatalogComponent,
          ),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
