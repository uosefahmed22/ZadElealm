export type ReportType = 'Technical' | 'CustomerService' | 'ProductIssue' | 'Other';

export interface CreateSupportReportRequest {
  titleOfTheIssue: string;
  description: string;
  reportType: ReportType;
}

export const reportTypeOptions: ReadonlyArray<{
  value: ReportType;
  label: string;
  description: string;
}> = [
  { value: 'Technical', label: 'مشكلة تقنية', description: 'تشغيل، تحميل، أو خطأ في الموقع' },
  {
    value: 'CustomerService',
    label: 'خدمة العملاء',
    description: 'مساعدة بخصوص الحساب أو الاستخدام',
  },
  { value: 'ProductIssue', label: 'محتوى الدورة', description: 'درس، اختبار، أو مادة تعليمية' },
  { value: 'Other', label: 'أخرى', description: 'أي ملاحظة لا تنتمي للأنواع السابقة' },
];
