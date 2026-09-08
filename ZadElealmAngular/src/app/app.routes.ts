import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { ForgotPasswordComponent } from './features/auth/forgot-password/forgot-password.component';
import { LandingPageComponent } from './features/landing/landing-page.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { ResetPasswordComponent } from './features/auth/reset-password/reset-password.component';
import { VerifyOtpComponent } from './features/auth/verify-otp/verify-otp.component';
import { AuthLayoutComponent } from './layouts/auth-layout/auth-layout.component';
import { StudentLayoutComponent } from './layouts/student-layout/student-layout.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', component: LandingPageComponent },
  {
    path: 'guide',
    data: { publicGuide: true },
    loadComponent: () =>
      import('./features/platform-guide/platform-guide.component').then(
        (component) => component.PlatformGuideComponent,
      ),
  },
  {
    path: '',
    component: AuthLayoutComponent,
    children: [
      {
        path: 'login',
        canActivate: [guestGuard],
        loadComponent: () =>
          import('./features/auth/login/login.component').then(
            (component) => component.LoginComponent,
          ),
      },
      { path: 'register', component: RegisterComponent, canActivate: [guestGuard] },
      { path: 'forgot-password', component: ForgotPasswordComponent, canActivate: [guestGuard] },
      { path: 'verify-otp', component: VerifyOtpComponent, canActivate: [guestGuard] },
      { path: 'reset-password', component: ResetPasswordComponent, canActivate: [guestGuard] },
      {
        path: 'confirm-email',
        loadComponent: () =>
          import('./features/auth/confirm-email/confirm-email.component').then(
            (component) => component.ConfirmEmailComponent,
          ),
      },
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
          import('./features/course-categories/course-categories.component').then(
            (component) => component.CourseCategoriesComponent,
          ),
      },
      {
        path: 'courses/catalog',
        loadComponent: () =>
          import('./features/course-catalog/course-catalog.component').then(
            (component) => component.CourseCatalogComponent,
          ),
      },
      {
        path: 'courses/:courseId',
        loadComponent: () =>
          import('./features/course-details/course-details.component').then(
            (component) => component.CourseDetailsComponent,
          ),
      },
      {
        path: 'my-courses',
        loadComponent: () =>
          import('./features/my-courses/my-courses.component').then(
            (component) => component.MyCoursesComponent,
          ),
      },
      {
        path: 'favorites',
        loadComponent: () =>
          import('./features/favorites/favorites.component').then(
            (component) => component.FavoritesComponent,
          ),
      },
      {
        path: 'guide',
        loadComponent: () =>
          import('./features/platform-guide/platform-guide.component').then(
            (component) => component.PlatformGuideComponent,
          ),
      },
      {
        path: 'courses/:courseId/quiz/:quizId',
        loadComponent: () =>
          import('./features/quiz/quiz.component').then((component) => component.QuizComponent),
      },
      {
        path: 'certificates',
        loadComponent: () =>
          import('./features/certificates/certificates.component').then(
            (component) => component.CertificatesComponent,
          ),
      },
      {
        path: 'assessments',
        loadComponent: () =>
          import('./features/assessment-center/assessment-center.component').then(
            (component) => component.AssessmentCenterComponent,
          ),
      },
      {
        path: 'assessments/:assessmentId',
        loadComponent: () =>
          import('./features/assessment-exam/assessment-exam.component').then(
            (component) => component.AssessmentExamComponent,
          ),
      },
      {
        path: 'leaderboard',
        loadComponent: () =>
          import('./features/leaderboard/leaderboard.component').then(
            (component) => component.LeaderboardComponent,
          ),
      },
      {
        path: 'achievements',
        loadComponent: () =>
          import('./features/achievements/achievements.component').then(
            (component) => component.AchievementsComponent,
          ),
      },
      {
        path: 'account',
        loadComponent: () =>
          import('./features/account/account.component').then(
            (component) => component.AccountComponent,
          ),
      },
      {
        path: 'support',
        loadComponent: () =>
          import('./features/support/support.component').then(
            (component) => component.SupportComponent,
          ),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
