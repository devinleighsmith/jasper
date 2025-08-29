import { GeneratePdfRequest } from '@/components/documents/models/GeneratePdf';
import { defineStore } from 'pinia';

export const usePDFViewerStore = defineStore('PDFViewerStore', {
  persist: true,
  state: () => ({
    documents: [] as GeneratePdfRequest[],
    storeDocuments: [] as StoreDocument[],
    groupedDocuments: {} as Record<
      string,
      Record<string, StoreDocument[]>
    >,
  }),
  getters: {
    documentRequests: (state) => state.documents,
    storeDocs: (state) => state.storeDocuments,
  },
  actions: {
    addDocuments(
      documents: StoreDocument[]
    ): void {
      this.storeDocuments = [...documents];
    },
    addStoreDocument(document: StoreDocument): void {
      this.storeDocs.push(document);

      if (document.caseNumber) {
        // Group documents by caseNumber, then by memberName
        const grouped = this.storeDocs.reduce(
          (acc, doc) => {
            if (!acc[doc.caseNumber]) acc[doc.caseNumber] = {};
            if (!acc[doc.caseNumber][doc.memberName])
              acc[doc.caseNumber][doc.memberName] = [];
            acc[doc.caseNumber][doc.memberName].push(doc);
            return acc;
          },
          {} as Record<string, Record<string, StoreDocument[]>>
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
  memberName: string;
  documentName: string;
};
