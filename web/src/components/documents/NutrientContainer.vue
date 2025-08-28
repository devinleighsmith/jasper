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
  import {
    GeneratePdfRequest,
    GeneratePdfResponse,
  } from '@/components/documents/models/GeneratePdf';
  import { FilesService } from '@/services/FilesService';
  import { usePDFViewerStore } from '@/stores';
  import { inject, onMounted, onUnmounted, ref } from 'vue';
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
    // const documentResponse = await filesService.generatePdf(pdfStore.documentRequests);
    // Flatten all document objects into a single array
    const groupedDocs = pdfStore.groupedDocuments;
    const allDocs: GeneratePdfRequest[] = [];
    Object.values(groupedDocs).forEach((userGroup: any) => {
      Object.values(userGroup).forEach((docs) => {
        allDocs.push(...(docs as any[]));
      });
    });
    console.log('All Docs:', allDocs);

    // Singular call to generatePdf with allDocs
    const documentResponse = await filesService.generatePdf(allDocs);
    loading.value = false;

    var instance = await NutrientViewer.load({
      ...configuration,
      document: `data:application/pdf;base64,${documentResponse.base64Pdf}`,
    });
    configureOutline(instance, documentResponse);
  };

  //   {
  //     "279899-1": {
  //         "John Johnson": [
  //             {
  //                 "type": 0,
  //                 "data": {
  //                 }
  //             },
  //             {
  //                 "type": 0,
  //                 "data": {
  //                 }
  //             }
  //         ]
  //     }
  // }
  const configureOutline = (
    instance: any,
    documentResponse: GeneratePdfResponse
  ) => {
    // var indexCount = 0;
    // const outline = NutrientViewer.Immutable.List(
    //   Object.entries(pdfStore.groupedDocuments).map(
    //     ([groupKey, userGroup]: [string, Record<string, GeneratePdfRequest[]>]) =>
    //       new NutrientViewer.OutlineElement({
    //         isExpanded: true,
    //         title: groupKey,
    //         children: NutrientViewer.Immutable.List(
    //           Object.entries(userGroup).map(
    //             ([name, docs]: [string, GeneratePdfRequest[]]) =>
    //               new NutrientViewer.OutlineElement({
    //                 isExpanded: true,
    //                 title: name,
    //                 children: NutrientViewer.Immutable.List(
    //                   (docs as GeneratePdfRequest[]).map(
    //                     (doc) =>
    //                       new NutrientViewer.OutlineElement({
    //                         isExpanded: true,
    //                         title: `Document ${doc.data.documentId}`,
    //                         action: new NutrientViewer.Actions.GoToAction({
    //                           pageIndex:
    //                             documentResponse.pageRanges?.[indexCount++]
    //                               ?.start ?? 0,
    //                         }),
    //                       })
    //                   )
    //                 ),
    //               })
    //           )
    //         ),
    //       })
    //   )
    //);
    
    const indexRef = { current: 0 };
    const outline = NutrientViewer.Immutable.List(
      Object.entries(pdfStore.groupedDocuments).map(([groupKey, userGroup]) =>
        makeGroupElement(groupKey, userGroup, indexRef, documentResponse)
      )
    );
    instance.setDocumentOutline(outline);
  };

  function makeDocElement(doc: GeneratePdfRequest, index: number, documentResponse: any) {
    return new NutrientViewer.OutlineElement({
      isExpanded: true,
      title: `Document ${doc.data.documentId}`,
      action: new NutrientViewer.Actions.GoToAction({
        pageIndex: documentResponse.pageRanges?.[index]?.start ?? 0,
      }),
    });
  }

  function makeUserElement(
    name: string,
    docs: GeneratePdfRequest[],
    indexRef: { current: number },
    documentResponse: any
  ) {
    return new NutrientViewer.OutlineElement({
      isExpanded: true,
      title: name,
      children: NutrientViewer.Immutable.List(
        docs.map(doc => makeDocElement(doc, indexRef.current++, documentResponse))
      ),
    });
  }

  function makeGroupElement(
    groupKey: string,
    userGroup: Record<string, GeneratePdfRequest[]>,
    indexRef: { current: number },
    documentResponse: any
  ) {
    return new NutrientViewer.OutlineElement({
      isExpanded: true,
      title: groupKey,
      children: NutrientViewer.Immutable.List(
        Object.entries(userGroup).map(([name, docs]) =>
          makeUserElement(name, docs, indexRef, documentResponse)
        )
      ),
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
