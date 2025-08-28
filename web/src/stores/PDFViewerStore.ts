import { GeneratePdfRequest } from '@/components/documents/models/GeneratePdf';
import { defineStore } from 'pinia';

export const usePDFViewerStore = defineStore('PDFViewerStore', {
  persist: true,
  state: () => ({
    documents: [] as GeneratePdfRequest[],
    storeDocuments: [] as StoreDocument[],
    groupedDocuments: {} as Record<
      string,
      Record<string, GeneratePdfRequest[]>
    >,
  }),
  getters: {
    documentRequests: (state) => state.documents,
    storeDocs: (state) => state.storeDocuments,
  },
  actions: {
    addDocuments(
      documents: GeneratePdfRequest[],
      caseNumber: string,
      userId: string
    ): void {
      if (caseNumber) {
      }
      this.documents = [...documents];
    },
    addStoreDocument(document: StoreDocument): void {
      // if (!this.storeDocs.length) {
      //   this.storeDocs.push(document);
      //   return;
      // }
      this.storeDocs.push(document);

      if (document.caseNumber) {
        // Group documents by caseNumber, then by userId
        const grouped = this.storeDocs.reduce(
          (acc, doc) => {
            if (!acc[doc.caseNumber]) acc[doc.caseNumber] = {};
            if (!acc[doc.caseNumber][doc.userId])
              acc[doc.caseNumber][doc.userId] = [];
            acc[doc.caseNumber][doc.userId].push(doc.request);
            return acc;
          },
          {} as Record<string, Record<string, GeneratePdfRequest[]>>
        );
        this.groupedDocuments = grouped;
      }
    },
    clearDocuments(): void {
      this.documents.length = 0;
      this.storeDocs.length = 0;
    },
  },
});

export type StoreDocument = {
  request: GeneratePdfRequest;
  caseNumber: string;
  userId: string;
};
