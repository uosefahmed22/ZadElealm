import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { AchievementStateService } from '../../core/achievements/achievement-state.service';
import { AchievementsComponent } from './achievements.component';

describe('AchievementsComponent', () => {
  let state: {
    dashboard: ReturnType<typeof signal>;
    errorMessage: ReturnType<typeof signal>;
    load: ReturnType<typeof vi.fn>;
    reload: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    state = {
      dashboard: signal(dashboard()),
      errorMessage: signal(''),
      load: vi.fn(() => of(dashboard())),
      reload: vi.fn(() => of(dashboard())),
    };
    await TestBed.configureTestingModule({
      imports: [AchievementsComponent],
      providers: [{ provide: AchievementStateService, useValue: state }],
    }).compileComponents();
  });

  it('renders the real achievement catalog and streak summary', () => {
    const fixture = TestBed.createComponent(AchievementsComponent);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(state.load).toHaveBeenCalledOnce();
    expect(text).toContain('سلسلتك الحالية');
    expect(text).toContain('٣ أيام');
    expect(text).toContain('البداية');
    expect(text).toContain('أسبوع من الهمة');
    expect(fixture.nativeElement.querySelectorAll('app-achievement-badge')).toHaveLength(2);
    expect(fixture.nativeElement.querySelector('.badge-card--locked')).not.toBeNull();
  });

  it('shows a retry action when loading fails', () => {
    state.dashboard.set(null);
    state.errorMessage.set('achievements unavailable');
    state.load.mockReturnValue(throwError(() => new Error('achievements unavailable')));
    const fixture = TestBed.createComponent(AchievementsComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('تعذر تحميل الإنجازات');
    expect(fixture.nativeElement.textContent).toContain('إعادة المحاولة');
  });

  it('switches the streak flames to the hot style after ten days', () => {
    state.dashboard.set({ ...dashboard(), currentStreak: 10 });
    state.load.mockReturnValue(of({ ...dashboard(), currentStreak: 10 }));
    const fixture = TestBed.createComponent(AchievementsComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.streak-icon--hot')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('.flame-emoji--hot')).not.toBeNull();
  });
});

function dashboard() {
  return {
    currentStreak: 3,
    longestStreak: 5,
    unlockedCount: 1,
    totalCount: 2,
    newlyUnlocked: ['FirstLesson'],
    achievements: [
      {
        code: 'FirstLesson',
        title: 'البداية',
        description: 'أكمل أول درس.',
        iconKey: 'first-lesson',
        isUnlocked: true,
        unlockedAtUtc: '2026-09-08T10:00:00Z',
        currentValue: 1,
        target: 1,
      },
      {
        code: 'SevenDayStreak',
        title: 'أسبوع من الهمة',
        description: 'زر المنصة سبعة أيام متتالية.',
        iconKey: 'seven-day-streak',
        isUnlocked: false,
        unlockedAtUtc: null,
        currentValue: 3,
        target: 7,
      },
    ],
  };
}
