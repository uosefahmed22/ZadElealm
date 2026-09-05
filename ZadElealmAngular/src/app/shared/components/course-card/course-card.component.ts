import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { CourseDto } from '../../../core/catalog/catalog.models';

@Component({
  selector: 'app-course-card',
  imports: [DecimalPipe, RouterLink],
  templateUrl: './course-card.component.html',
  styleUrl: './course-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CourseCardComponent {
  readonly course = input.required<CourseDto>();
  readonly tone = input<'paper' | 'ink'>('paper');
  readonly imagePriority = input(false);
  readonly imageFailed = signal(false);

  readonly fallbackImage = 'assets/brand/course-placeholder.svg';

  imageSrcSet(imageUrl: string | null | undefined): string | null {
    if (!imageUrl) return null;

    const match = imageUrl.match(/i\.ytimg\.com\/vi\/([^/]+)\//i);
    const videoId = match?.[1];
    if (!videoId) return null;

    return `https://i.ytimg.com/vi/${videoId}/mqdefault.jpg 320w, https://i.ytimg.com/vi/${videoId}/hqdefault.jpg 480w`;
  }
}
