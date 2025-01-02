import { describe, it, expect, beforeEach } from 'vitest';
import { useThemeStore } from 'SRC/stores/ThemeStore';

describe('ThemeStore.js', () => {
  let themeStore;

  beforeEach(() => {
    themeStore = useThemeStore();
  });

  afterEach(() => {
    localStorage.clear()
  })

  it('should initialize with light theme by default', () => {
    expect(themeStore.state.value).toBe('light');
  });

  it('should change theme and update localStorage', () => {
    themeStore.changeState('dark');
    expect(themeStore.state.value).toBe('dark');
    expect(localStorage.getItem('theme')).toBe('dark');
  });

  it('should persist theme from localStorage', () => {
    localStorage.setItem('theme', 'dark');
    themeStore = useThemeStore();
    expect(themeStore.state.value).toBe('dark');
  });
});