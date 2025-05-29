import { defineStore } from 'pinia';

export const usePDFStore = defineStore('PDFStore', {
  persist: true,
  state: () => ({
    urls: [] as string[],
  }),
  getters: {
    currentUrls: (state) => state.urls,
  },
  actions: {
    addUrlsForViewing({ urls }): void {
      this.urls = [...urls];
    },
    clearSelectedFiles(): void {
      this.urls.length = 0;
    },
  },
});
