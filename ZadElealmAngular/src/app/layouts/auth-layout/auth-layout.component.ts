import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-auth-layout',
  imports: [CommonModule, RouterLink, RouterOutlet],
  templateUrl: './auth-layout.component.html',
  styleUrl: './auth-layout.component.scss'
})
export class AuthLayoutComponent {
  readonly trustPoints = [
    'محتوى تعليمي منظّم',
    'متابعة واضحة لتقدّمك',
    'تجربة عربية على كل الأجهزة'
  ];
}
