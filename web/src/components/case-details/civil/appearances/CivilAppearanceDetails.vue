<template>
  <v-tabs v-model="tab" align-tabs="start" slider-color="primary">
    <v-tab value="documents">Scheduled Documents</v-tab>
    <v-tab disabled value="binder">Judicial Binder</v-tab>
    <v-tab value="parties">Scheduled Parties</v-tab>
  </v-tabs>

  <v-card-text>
    <v-tabs-window v-model="tab">
      <v-tabs-window-item value="documents">
        <v-skeleton-loader
          class="my-0"
          type="table"
          :height="200"
          color="var(--bg-light-gray)"
          :loading="loading"
        >
          <ScheduledDocuments :documents="details.document" />
        </v-skeleton-loader>
      </v-tabs-window-item>

      <v-tabs-window-item value="binder"> Binder </v-tabs-window-item>

      <v-tabs-window-item value="parties">
        <ScheduledParties :parties="details.party" />
      </v-tabs-window-item>
    </v-tabs-window>
  </v-card-text>
</template>

<script setup lang="ts">
  import ScheduledDocuments from '@/components/case-details/civil/appearances/ScheduledDocuments.vue';
  import ScheduledParties from '@/components/case-details/civil/appearances/ScheduledParties.vue';
  import { FilesService } from '@/services/FilesService';
  import { CivilAppearanceDetails } from '@/types/civil/jsonTypes';
  import { inject, onMounted, ref } from 'vue';

  const props = defineProps<{
    fileId: string;
    appearanceId: string;
  }>();
  const filesService = inject<FilesService>('filesService');
  const tab = ref('documents');
  const details = ref<CivilAppearanceDetails>({} as CivilAppearanceDetails);
  const pcssDetails = ref();
  const loading = ref(false);
  if (!filesService) {
    throw new Error('Files service is undefined.');
  }

  onMounted(async () => {
    loading.value = true;
    details.value = await filesService.civilAppearanceDetails(
      props.fileId,
      props.appearanceId
    );
    // pcssDetais.value = await filesService.PCSScivilAppearanceDetails(
    //   props.fileId,
    //   props.appearanceId
    // );
    loading.value = false;
  });
</script>

<style scoped>
  .v-tabs {
    flex: 10;
  }
</style>
