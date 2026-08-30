import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { AuthSessionService } from '../../core/auth/auth-session.service';

@Component({
  selector: 'app-student-home',
  imports: [RouterLink],
  templateUrl: './student-home.component.html',
  styleUrl: './student-home.component.scss',
})
export class StudentHomeComponent {
  readonly session = inject(AuthSessionService);
}
