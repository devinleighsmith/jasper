import { GeneratePdfRequest } from '@/components/documents/models/GeneratePdf';
import { defineStore } from 'pinia';

export const usePDFViewerStore = defineStore('PDFViewerStore', {
  persist: true,
  state: () => ({
    storedDocuments: [] as StoreDocument[],
    groupedDocuments: {} as Record<
      string,
      Record<string, StoreDocument[]>
    >,
  }),
  getters: {
    documents: (state) => state.storedDocuments,
  },
  actions: {
    addDocument(document: StoreDocument): void {
      this.storedDocuments.push(document);

      if (document.groupKeyOne) {
        const grouped = this.documents.reduce(
          (acc, doc) => {
            if (!acc[document.groupKeyOne]) acc[document.groupKeyOne] = {};
            if (!acc[document.groupKeyOne][document.groupKeyTwo])
              acc[document.groupKeyOne][document.groupKeyTwo] = [];
            acc[document.groupKeyOne][document.groupKeyTwo].push(doc);
            return acc;
          },
          {} as Record<string, Record<string, StoreDocument[]>>
        );
        this.groupedDocuments = grouped;
      }
    },
    clearDocuments(): void {
      this.storedDocuments.length = 0;
      this.groupedDocuments = {};
    },
  },
});

export type StoreDocument = {
  request: GeneratePdfRequest;
  groupKeyOne: string;
  groupKeyTwo: string;
  documentName: string;
};
