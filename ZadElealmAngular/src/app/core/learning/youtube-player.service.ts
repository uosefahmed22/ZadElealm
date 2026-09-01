import { DOCUMENT } from '@angular/common';
import { Injectable, inject } from '@angular/core';

export const YOUTUBE_PLAYER_STATE = {
  ended: 0,
  playing: 1,
  paused: 2,
} as const;

export interface YoutubePlayerHandle {
  destroy(): void;
  getCurrentTime(): number;
  getDuration(): number;
  loadVideoById(videoId: string, startSeconds?: number): void;
  pauseVideo(): void;
}

interface YoutubePlayerEvent {
  data: number;
  target: YoutubePlayerHandle;
}

interface YoutubePlayerOptions {
  videoId: string;
  playerVars: Record<string, number>;
  events: {
    onReady: (event: { target: YoutubePlayerHandle }) => void;
    onStateChange: (event: YoutubePlayerEvent) => void;
    onError: () => void;
  };
}

interface YoutubeApi {
  Player: new (element: HTMLElement, options: YoutubePlayerOptions) => YoutubePlayerHandle;
}

declare global {
  interface Window {
    YT?: YoutubeApi;
    onYouTubeIframeAPIReady?: () => void;
  }
}

@Injectable({ providedIn: 'root' })
export class YoutubePlayerService {
  private readonly document = inject(DOCUMENT);
  private apiPromise?: Promise<YoutubeApi>;

  createPlayer(
    element: HTMLElement,
    videoId: string,
    events: YoutubePlayerOptions['events'],
  ): Promise<YoutubePlayerHandle> {
    return this.loadApi().then(
      (api) =>
        new api.Player(element, {
          videoId,
          playerVars: { rel: 0, modestbranding: 1, controls: 1 },
          events,
        }),
    );
  }

  private loadApi(): Promise<YoutubeApi> {
    if (window.YT?.Player) return Promise.resolve(window.YT);
    if (this.apiPromise) return this.apiPromise;

    this.apiPromise = new Promise<YoutubeApi>((resolve, reject) => {
      const previousReady = window.onYouTubeIframeAPIReady;
      window.onYouTubeIframeAPIReady = () => {
        previousReady?.();
        if (window.YT?.Player) resolve(window.YT);
        else reject(new Error('YouTube API is unavailable'));
      };

      const existing = this.document.getElementById('youtube-iframe-api');
      if (existing) return;

      const script = this.document.createElement('script');
      script.id = 'youtube-iframe-api';
      script.src = 'https://www.youtube.com/iframe_api';
      script.async = true;
      script.onerror = () => reject(new Error('Failed to load YouTube API'));
      this.document.head.appendChild(script);
    });

    return this.apiPromise;
  }
}

export function extractYoutubeVideoId(url: string): string | null {
  try {
    const parsed = new URL(url);
    if (parsed.hostname === 'youtu.be') return parsed.pathname.slice(1).split('/')[0] || null;
    if (parsed.hostname.includes('youtube.com')) {
      if (parsed.pathname.startsWith('/embed/')) return parsed.pathname.split('/')[2] || null;
      return parsed.searchParams.get('v');
    }
  } catch {
    return /^[\w-]{6,}$/.test(url) ? url : null;
  }
  return null;
}
