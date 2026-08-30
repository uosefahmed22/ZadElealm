import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { AuthFacadeService } from '../../core/auth/auth-facade.service';
import { AuthSessionService } from '../../core/auth/auth-session.service';

@Component({
  selector: 'app-student-layout',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './student-layout.component.html',
  styleUrl: './student-layout.component.scss',
})
export class StudentLayoutComponent {
  private readonly authFacade = inject(AuthFacadeService);
  private readonly router = inject(Router);

  readonly session = inject(AuthSessionService);
  readonly isLoggingOut = signal(false);
  readonly isMenuOpen = signal(false);

  toggleMenu(): void {
    this.isMenuOpen.update((isOpen) => !isOpen);
  }

  closeMenu(): void {
    this.isMenuOpen.set(false);
  }

  logout(): void {
    if (this.isLoggingOut()) {
      return;
    }

    this.isLoggingOut.set(true);
    this.authFacade.logout().subscribe({
      complete: () => void this.router.navigate(['/login']),
      error: () => {
        this.isLoggingOut.set(false);
        void this.router.navigate(['/login']);
      },
    });
  }
}
