import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { AuthFacadeService } from '../../core/auth/auth-facade.service';
import { AuthSessionService } from '../../core/auth/auth-session.service';
import { NotificationApiService } from '../../core/notifications/notification-api.service';
import { UserNotificationDto } from '../../core/notifications/notification.models';
import { StudentLayoutComponent } from './student-layout.component';

describe('StudentLayoutComponent notifications', () => {
  let notificationApi: {
    getNotifications: ReturnType<typeof vi.fn>;
    markAsRead: ReturnType<typeof vi.fn>;
    markAllAsRead: ReturnType<typeof vi.fn>;
    deleteNotification: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    notificationApi = {
      getNotifications: vi.fn(() =>
        of({
          statusCode: 200,
          data: { notifications: [notification()], unreadCount: 1, totalCount: 1 },
        }),
      ),
      markAsRead: vi.fn(() => of({ statusCode: 200 })),
      markAllAsRead: vi.fn(() => of({ statusCode: 200 })),
      deleteNotification: vi.fn(() => of({ statusCode: 200 })),
    };

    await TestBed.configureTestingModule({
      imports: [StudentLayoutComponent],
      providers: [
        provideRouter([]),
        { provide: NotificationApiService, useValue: notificationApi },
        { provide: AuthFacadeService, useValue: { logout: vi.fn(() => of(null)) } },
        {
          provide: AuthSessionService,
          useValue: {
            user: signal({ displayName: 'محمد أحمد', email: 'user@test.com' }),
          },
        },
      ],
    }).compileComponents();
  });

  it('loads the real unread count and refreshes when the panel opens', () => {
    const fixture = TestBed.createComponent(StudentLayoutComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    expect(component.unreadCount()).toBe(1);
    expect(fixture.nativeElement.querySelector('.notification-badge').textContent).toContain('1');
    expect(fixture.nativeElement.querySelector('.notification-bell')).not.toBeNull();

    component.toggleNotifications();
    fixture.detectChanges();

    expect(notificationApi.getNotifications).toHaveBeenCalledTimes(2);
    expect(fixture.nativeElement.querySelector('.notification-panel').textContent).toContain(
      'تم تسجيلك في الدورة',
    );
    expect(
      fixture.nativeElement.querySelector('.notification-icon[data-type="0"] svg'),
    ).not.toBeNull();
  });

  it('keeps four primary links and moves secondary navigation and logout into account menu', () => {
    const fixture = TestBed.createComponent(StudentLayoutComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('.student-nav a')).toHaveLength(4);
    expect(fixture.nativeElement.querySelector('.account-panel')).toBeNull();

    const trigger = fixture.nativeElement.querySelector('.account-trigger') as HTMLButtonElement;
    trigger.click();
    fixture.detectChanges();

    const panel = fixture.nativeElement.querySelector('.account-panel') as HTMLElement;
    expect(panel.textContent).toContain('شهاداتي');
    expect(panel.textContent).toContain('الترتيب');
    expect(panel.textContent).toContain('الحساب');
    expect(panel.textContent).toContain('الدعم');
    expect(panel.textContent).toContain('تسجيل الخروج');
  });

  it('places notifications beside the outer account menu and keeps only one panel open', () => {
    const fixture = TestBed.createComponent(StudentLayoutComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;
    const groups = Array.from(fixture.nativeElement.querySelector('.account').children).map(
      (element) => (element as HTMLElement).className,
    );

    expect(groups).toEqual(['notification-center', 'account-menu']);
    component.toggleNotifications();
    component.toggleAccountMenu();

    expect(component.notificationOpen()).toBe(false);
    expect(component.accountMenuOpen()).toBe(true);
  });

  it('marks one notification as read without fetching the list again', () => {
    const fixture = TestBed.createComponent(StudentLayoutComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    component.markAsRead(component.notifications()[0]);

    expect(notificationApi.markAsRead).toHaveBeenCalledWith(12);
    expect(component.unreadCount()).toBe(0);
    expect(component.notifications()[0].isRead).toBe(true);
    expect(notificationApi.getNotifications).toHaveBeenCalledTimes(1);
  });

  it('marks all as read and deletes an owned notification locally after API success', () => {
    const fixture = TestBed.createComponent(StudentLayoutComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    component.markAllAsRead();
    component.deleteNotification(component.notifications()[0]);

    expect(notificationApi.markAllAsRead).toHaveBeenCalledTimes(1);
    expect(notificationApi.deleteNotification).toHaveBeenCalledWith(12);
    expect(component.unreadCount()).toBe(0);
    expect(component.notifications()).toEqual([]);
  });
});

function notification(): UserNotificationDto {
  return {
    id: 12,
    title: 'تم تسجيلك في الدورة',
    description: 'يمكنك الآن بدء مشاهدة الدروس.',
    type: 0,
    createdAt: '2026-09-01T01:00:00Z',
    isRead: false,
  };
}
