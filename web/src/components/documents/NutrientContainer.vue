<template>
  <v-progress-linear v-if="loading" indeterminate />
  <v-skeleton-loader v-if="loading" :loading="loading" type="ossein" />
  <v-row class="py-12" v-if="emptyStore">
    <v-col>
      <p class="text-center mx-auto">No documents available to display.</p>
    </v-col>
  </v-row>

  <div v-show="!loading" ref="pdf-container" class="pdf-container" />
</template>

<script setup lang="ts">
  import { getCriminalDocumentType } from '@/components/documents/DocumentUtils';
  import {
    GeneratePdfRequest,
    GeneratePdfResponse,
  } from '@/components/documents/models/GeneratePdf';
  import { FilesService } from '@/services/FilesService';
  import { usePDFViewerStore } from '@/stores';
  import { StoreDocument } from '@/stores/PDFViewerStore';
  import { inject, onMounted, onUnmounted, ref } from 'vue';
  import { CourtDocumentType } from '@/types/shared';
  //import NutrientViewer from '@/components/documents/NutrientViewer'; // Adjust path as needed

  const pdfStore = usePDFViewerStore();
  const filesService = inject<FilesService>('filesService');
  if (!filesService) {
    throw new Error('HttpService is not available!');
  }
  const loading = ref(false);
  const emptyStore = ref(false);
  const configuration = {
    initialViewState: new NutrientViewer.ViewState({
      sidebarMode: NutrientViewer.SidebarMode.DOCUMENT_OUTLINE,
    }),
    container: '.pdf-container',
  };
  let documentResponse: GeneratePdfResponse | null = null;

  const loadNutrient = async () => {
    loading.value = true;
    emptyStore.value = false;

    if (!pdfStore.documentRequests.length && !pdfStore.storeDocs.length) {
      loading.value = false;
      emptyStore.value = true;
      return;
    }
    return loadMultiple();
  };

  const loadMultiple = async () => {
    const groupedDocs = pdfStore.groupedDocuments;
    console.log(groupedDocs);
    const allDocs: StoreDocument[] = [];
    Object.values(groupedDocs).forEach((userGroup: any) => {
      Object.values(userGroup).forEach((docs) => {
        allDocs.push(...(docs as StoreDocument[]));
      });
    });
    console.log('All Docs:', allDocs);

    documentResponse = await filesService.generatePdf(allDocs.map(doc => doc.request));
    loading.value = false;

    var instance = await NutrientViewer.load({
      ...configuration,
      document: `data:application/pdf;base64,${documentResponse.base64Pdf}`,
    });
    configureOutline(instance);
  };

  const configureOutline = (
    instance: any,
  ) => {
    const indexRef = { current: 0 };
    const outline = NutrientViewer.Immutable.List(
      Object.entries(pdfStore.groupedDocuments).map(([groupKey, userGroup]) =>
        makeCaseElement(groupKey, userGroup, indexRef)
      )
    );
    instance.setDocumentOutline(outline);
  };

  const makeCaseElement = (
    groupKey: string,
    userGroup: Record<string, StoreDocument[]>,
    indexRef: { current: number }
  ) => {
    return new NutrientViewer.OutlineElement({
      title: groupKey, //groupKey,
      children: NutrientViewer.Immutable.List(
        Object.entries(userGroup).map(([name, docs]) =>
          makeMemberElement(name || 'Party', docs, indexRef)
        )
      ),
    });
  }

  const makeMemberElement = (
    memberName: string,
    docs: StoreDocument[],
    indexRef: { current: number },
  ) => {
    return new NutrientViewer.OutlineElement({
      title: memberName, //memberName,
      children: NutrientViewer.Immutable.List(
        docs.map((doc) =>
          makeDocElement(doc, indexRef.current++)
        )
      ),
    });
  }

  const makeDocElement = (
    doc: StoreDocument,
    index: number
  ) => {
    return new NutrientViewer.OutlineElement({
      title: doc.documentName, //doc.documentName
      action: new NutrientViewer.Actions.GoToAction({
        pageIndex: documentResponse?.pageRanges?.[index]?.start ?? 0,
      }),
    });
  }

  onMounted(() => {
    loadNutrient();
  });

  onUnmounted(() => {
    if (NutrientViewer) {
      NutrientViewer.unload('.pdf-container');
    }

    pdfStore.clearDocuments();
  });
</script>

<style scoped>
  .pdf-container {
    height: 90vh;
  }
  .v-skeleton-loader {
    height: 100%;
  }
</style>
