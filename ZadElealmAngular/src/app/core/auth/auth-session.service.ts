import { Injectable, computed, signal } from '@angular/core';

import { UserDto } from './auth.models';

const sessionStorageKey = 'zad-elealm.auth.session';

@Injectable({ providedIn: 'root' })
export class AuthSessionService {
  private readonly userState = signal<UserDto | null>(this.readStoredUser());

  readonly user = computed(() => this.userState());
  readonly isAuthenticated = computed(() => !!this.userState()?.token);
  readonly accessToken = computed(() => this.userState()?.token ?? null);
  readonly refreshToken = computed(() => this.userState()?.refreshToken ?? null);

  setSession(user: UserDto): void {
    this.userState.set(user);
    sessionStorage.setItem(sessionStorageKey, JSON.stringify(user));
  }

  getTokenRequest(): { token: string; refreshToken: string } | null {
    const user = this.userState();
    if (!user?.token || !user.refreshToken) {
      return null;
    }

    return { token: user.token, refreshToken: user.refreshToken };
  }

  clearSession(): void {
    this.userState.set(null);
    sessionStorage.removeItem(sessionStorageKey);
  }

  private readStoredUser(): UserDto | null {
    const serialized = sessionStorage.getItem(sessionStorageKey);
    if (!serialized) {
      return null;
    }

    try {
      const candidate = JSON.parse(serialized) as Partial<UserDto>;
      if (
        typeof candidate.displayName !== 'string' ||
        typeof candidate.email !== 'string' ||
        typeof candidate.token !== 'string' ||
        typeof candidate.refreshToken !== 'string' ||
        !candidate.token ||
        !candidate.refreshToken
      ) {
        throw new Error('Invalid stored auth session.');
      }

      return candidate as UserDto;
    } catch {
      sessionStorage.removeItem(sessionStorageKey);
      return null;
    }
  }
}
