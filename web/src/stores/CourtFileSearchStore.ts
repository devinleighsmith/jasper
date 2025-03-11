import { KeyValueInfo } from '@/types/common';
import { CourtFileSearchCriteria, FileDetail } from '@/types/courtFileSearch';
import { defineStore } from 'pinia';

export const useCourtFileSearchStore = defineStore('CourtFileSearchStore', {
  persist: true,
  state: () => ({
    filesForViewing: [] as KeyValueInfo[],
    searchCriteria: {} as CourtFileSearchCriteria,
    searchResults: [] as FileDetail[],
  }),
  getters: {
    selectedFiles: (state) => state.filesForViewing,
    currentSearchCriteria: (state) => state.searchCriteria,
    currentSearchResults: (state) => state.searchResults,
  },
  actions: {
    addFilesForViewing({ searchCriteria, searchResults, files }): void {
      this.searchCriteria = searchCriteria;
      this.searchResults = [...searchResults];
      this.filesForViewing = [...files];
    },
    clearSelectedFiles(): void {
      this.filesForViewing.length = 0;
      this.searchCriteria = {} as CourtFileSearchCriteria;
      this.searchResults = [];
    },
    removeCurrentViewedFileId(fileId: string): void {
      this.filesForViewing = this.filesForViewing.filter(
        (c) => c.key !== fileId
      );
    },
  },
});
