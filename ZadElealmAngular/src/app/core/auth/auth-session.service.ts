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
      return JSON.parse(serialized) as UserDto;
    } catch {
      sessionStorage.removeItem(sessionStorageKey);
      return null;
    }
  }
}
