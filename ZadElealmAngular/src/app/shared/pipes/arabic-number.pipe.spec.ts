import { ArabicNumberPipe } from './arabic-number.pipe';

describe('ArabicNumberPipe', () => {
  const pipe = new ArabicNumberPipe();

  it('formats numbers with Arabic-Indic digits', () => {
    expect(pipe.transform(0)).toBe('٠');
    expect(pipe.transform(123)).toBe('١٢٣');
  });
});
