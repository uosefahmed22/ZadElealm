import { extractYoutubeVideoId } from './youtube-player.service';

describe('extractYoutubeVideoId', () => {
  it('supports watch, short, embed, and direct YouTube identifiers', () => {
    expect(extractYoutubeVideoId('https://www.youtube.com/watch?v=abc123xyz')).toBe('abc123xyz');
    expect(extractYoutubeVideoId('https://youtu.be/abc123xyz')).toBe('abc123xyz');
    expect(extractYoutubeVideoId('https://www.youtube.com/embed/abc123xyz')).toBe('abc123xyz');
    expect(extractYoutubeVideoId('abc123xyz')).toBe('abc123xyz');
  });
});
