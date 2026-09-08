import { CommonModule } from '@angular/common';
import { Component, DestroyRef, HostListener, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { AuthFacadeService } from '../../core/auth/auth-facade.service';
import { AuthSessionService } from '../../core/auth/auth-session.service';
import { AchievementStateService } from '../../core/achievements/achievement-state.service';
import { normalizeApiError } from '../../core/api/api-error.utils';
import { NotificationApiService } from '../../core/notifications/notification-api.service';
import { UserNotificationDto } from '../../core/notifications/notification.models';
import { ArabicNumberPipe } from '../../shared/pipes/arabic-number.pipe';

@Component({
  selector: 'app-student-layout',
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet, ArabicNumberPipe],
  templateUrl: './student-layout.component.html',
  styleUrl: './student-layout.component.scss',
})
export class StudentLayoutComponent implements OnInit {
  private readonly authFacade = inject(AuthFacadeService);
  private readonly router = inject(Router);
  private readonly notificationApi = inject(NotificationApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly session = inject(AuthSessionService);
  readonly achievementState = inject(AchievementStateService);
  readonly isLoggingOut = signal(false);
  readonly notifications = signal<UserNotificationDto[]>([]);
  readonly unreadCount = signal(0);
  readonly notificationOpen = signal(false);
  readonly accountMenuOpen = signal(false);
  readonly isLoadingNotifications = signal(false);
  readonly notificationError = signal('');
  readonly pendingNotificationId = signal<number | null>(null);
  readonly deletingNotificationId = signal<number | null>(null);
  readonly isMarkingAll = signal(false);

  ngOnInit(): void {
    this.loadNotifications();
    this.achievementState
      .load()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ error: () => undefined });
  }

  @HostListener('document:keydown.escape')
  closeFloatingPanels(): void {
    this.notificationOpen.set(false);
    this.accountMenuOpen.set(false);
  }

  @HostListener('document:click', ['$event'])
  closePanelsOnOutsideClick(event: MouseEvent): void {
    if (!(event.target instanceof Element)) return;
    if (!event.target.closest('.notification-center')) this.notificationOpen.set(false);
    if (!event.target.closest('.account-menu')) this.accountMenuOpen.set(false);
  }

  toggleNotifications(): void {
    const nextOpen = !this.notificationOpen();
    this.accountMenuOpen.set(false);
    this.notificationOpen.set(nextOpen);
    if (nextOpen) this.loadNotifications();
  }

  toggleAccountMenu(): void {
    const nextOpen = !this.accountMenuOpen();
    this.notificationOpen.set(false);
    this.accountMenuOpen.set(nextOpen);
  }

  closeAccountMenu(): void {
    this.accountMenuOpen.set(false);
  }

  closeNotifications(): void {
    this.notificationOpen.set(false);
  }

  markAsRead(notification: UserNotificationDto): void {
    if (notification.isRead || this.pendingNotificationId() !== null) return;
    this.pendingNotificationId.set(notification.id);
    this.notificationError.set('');
    this.notificationApi.markAsRead(notification.id).subscribe({
      next: () => {
        this.pendingNotificationId.set(null);
        this.notifications.update((items) =>
          items.map((item) => (item.id === notification.id ? { ...item, isRead: true } : item)),
        );
        this.unreadCount.update((count) => Math.max(0, count - 1));
      },
      error: (error: unknown) => {
        this.pendingNotificationId.set(null);
        this.notificationError.set(normalizeApiError(error).message);
      },
    });
  }

  markAllAsRead(): void {
    if (this.unreadCount() === 0 || this.isMarkingAll()) return;
    this.isMarkingAll.set(true);
    this.notificationError.set('');
    this.notificationApi.markAllAsRead().subscribe({
      next: () => {
        this.isMarkingAll.set(false);
        this.notifications.update((items) => items.map((item) => ({ ...item, isRead: true })));
        this.unreadCount.set(0);
      },
      error: (error: unknown) => {
        this.isMarkingAll.set(false);
        this.notificationError.set(normalizeApiError(error).message);
      },
    });
  }

  deleteNotification(notification: UserNotificationDto): void {
    if (this.deletingNotificationId() !== null) return;
    this.deletingNotificationId.set(notification.id);
    this.notificationError.set('');
    this.notificationApi.deleteNotification(notification.id).subscribe({
      next: () => {
        this.deletingNotificationId.set(null);
        this.notifications.update((items) => items.filter((item) => item.id !== notification.id));
        if (!notification.isRead) {
          this.unreadCount.update((count) => Math.max(0, count - 1));
        }
      },
      error: (error: unknown) => {
        this.deletingNotificationId.set(null);
        this.notificationError.set(normalizeApiError(error).message);
      },
    });
  }

  private loadNotifications(): void {
    this.isLoadingNotifications.set(true);
    this.notificationError.set('');
    this.notificationApi.getNotifications().subscribe({
      next: (response) => {
        this.notifications.set(response.data.notifications);
        this.unreadCount.set(response.data.unreadCount);
        this.isLoadingNotifications.set(false);
      },
      error: (error: unknown) => {
        this.isLoadingNotifications.set(false);
        this.notificationError.set(normalizeApiError(error).message);
      },
    });
  }

  logout(): void {
    if (this.isLoggingOut()) {
      return;
    }

    this.isLoggingOut.set(true);
    this.accountMenuOpen.set(false);
    this.authFacade.logout().subscribe({
      complete: () => void this.router.navigate(['/login']),
      error: () => {
        this.isLoggingOut.set(false);
        void this.router.navigate(['/login']);
      },
    });
  }
}
