import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { appEnvironment } from '../config/app-environment';
import { SupportApiService } from './support-api.service';

describe('SupportApiService', () => {
  let service: SupportApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(SupportApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('posts only the student-owned report fields to the protected report route', () => {
    const payload = {
      titleOfTheIssue: 'الفيديو لا يعمل',
      description: 'الفيديو لا يبدأ عند الضغط على زر التشغيل',
      reportType: 'Technical' as const,
    };

    service.createReport(payload).subscribe();

    const request = httpMock.expectOne(`${appEnvironment.apiBaseUrl}/Report`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);
    request.flush({ statusCode: 201, message: 'تم إرسال البلاغ بنجاح' });
  });
});
