<template>
  <v-tabs v-model="tab" align-tabs="start" slider-color="primary">
    <v-tab value="documents">Scheduled Documents</v-tab>
    <v-tab disabled value="binder">Judicial Binder</v-tab>
    <v-tab value="parties">Scheduled Parties</v-tab>
  </v-tabs>

  <v-card-text>
    <v-tabs-window v-model="tab">
      <v-tabs-window-item value="documents">
        <ScheduledDocuments />
      </v-tabs-window-item>

      <v-tabs-window-item value="binder"> Binder </v-tabs-window-item>

      <v-tabs-window-item value="parties">
        <ScheduledParties />
      </v-tabs-window-item>
    </v-tabs-window>
  </v-card-text>
</template>

<script setup lang="ts">
  import ScheduledDocuments from '@/components/case-details/civil/appearances/ScheduledDocuments.vue';
  import ScheduledParties from '@/components/case-details/civil/appearances/ScheduledParties.vue';
  import { FilesService } from '@/services/FilesService';
  import { inject, onMounted, ref } from 'vue';

  const props = defineProps<{
    fileId: string;
    appearanceId: string;
  }>();
  const filesService = inject<FilesService>('filesService');
  const details = ref();
  if (!filesService) {
    throw new Error('Files service is undefined.');
  }

  //console.log(props.fileId);
  //console.log(props.appearanceId);
  onMounted(async () => {
    details.value = await filesService.civilAppearanceDetails(
      props.fileId,
      props.appearanceId
    );
    console.log(details);
  });
  const tab = ref('documents');
</script>

<style>
  .v-tabs {
    flex: 10;
  }
</style>
