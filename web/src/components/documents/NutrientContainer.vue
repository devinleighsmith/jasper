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

<script setup>
  import { usePDFViewerStore } from '@/stores';
  import { onMounted, onUnmounted, ref, useTemplateRef } from 'vue';

  const pdfStore = usePDFViewerStore();
  const containerRef = useTemplateRef('pdf-container');
  const loading = ref(false);
  const emptyStore = ref(false);
  const configuration = {
    initialViewState: new NutrientViewer.ViewState({
      sidebarMode: NutrientViewer.SidebarMode.THUMBNAILS,
    }),
  };

  const loadNutrient = () => {
    loading.value = true;
    emptyStore.value = false;

    if (!pdfStore.documentUrls.length) {
      loading.value = false;
      emptyStore.value = true;
      return;
    }
    return pdfStore.documentUrls.length === 1 ? loadSingle() : loadMultiple();
  };

  const loadMultiple = async () => {
    const instance = await NutrientViewer.load({
      ...configuration,
      container: containerRef.value,
      document: pdfStore.documentUrls[0],
      headless: true,
    });

    // Fetch all remaining documents as blobs
    const documentBlobs = await Promise.all(
      pdfStore.documentUrls
        .slice(1)
        .map((url) => fetch(url).then((result) => result.blob()))
    );

    // Track the starting page index for each document
    let pageIndices = [0]; // First document always starts at 0
    let afterPageIndex = instance.totalPageCount;
    // Prepare merge operations
    const mergeDocumentOperations = [];
    for (const element of documentBlobs) {
      const blob = element;
      const documentInstance = await NutrientViewer.load({
        ...configuration,
        document: await blob.arrayBuffer(),
        headless: true,
      });
      pageIndices.push(afterPageIndex);
      mergeDocumentOperations.push({
        type: 'importDocument',
        afterPageIndex: afterPageIndex - 1, // Insert after the last page (zero-based)
        treatImportedDocumentAsOnePage: false,
        document: blob,
      });
      afterPageIndex += documentInstance.totalPageCount;
      NutrientViewer.unload(documentInstance);
    }

    const mergedDocument = await instance.exportPDFWithOperations(
      mergeDocumentOperations
    );

    loading.value = false;
    // Load merged document and set outline
    const mergedInstance = await NutrientViewer.load({
      ...configuration,
      container: containerRef.value,
      document: mergedDocument,
    });
    //pageIndices.push(afterPageIndex + 1);
    // += mergedInstance.totalPageCount;

    // Build outline for each document's starting page
    const outline = NutrientViewer.Immutable.List(
      pdfStore.documentUrls.map(
        (url, idx) =>
          new NutrientViewer.OutlineElement({
            action: new NutrientViewer.Actions.GoToAction({
              pageIndex: pageIndices[idx],
            }),
            children: NutrientViewer.Immutable.List([]),
            title: `Document ${idx + 1}`,
          })
      )
    );

    mergedInstance.setDocumentOutline(outline);
  };

  const loadSingle = async () => {
    const documentBlob = await fetch(pdfStore.documentUrls[0]).then((result) =>
      result.blob()
    );
    const documentBuffer = await documentBlob.arrayBuffer();
    // We set our own loader to false, nutrient's internal loader can take it from here
    loading.value = false;

    await NutrientViewer.load({
      ...configuration,
      container: containerRef.value,
      document: documentBuffer,
    });
  };

  onMounted(() => {
    loadNutrient();
  });

  onUnmounted(() => {
    const container = containerRef.value;

    if (container && NutrientViewer) {
      NutrientViewer.unload(container);
    }
    pdfStore.clearUrls();
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
