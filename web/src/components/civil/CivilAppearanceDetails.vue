<template>
  <v-tabs v-model="tab" align-tabs="start" slider-color="primary">
    <v-tab value="documents">Scheduled Documents</v-tab>
    <v-tab
      data-testid="binder-tab"
      v-if="showBinder"
      :disabled="documentsLoading || !documentDetails || documentDetails.binderDocuments?.length === 0"
      value="binder"
      >Judicial Binder</v-tab
    >
    <v-tab value="parties">Scheduled Parties</v-tab>
    <v-tab value="methods" v-if="documentDetails.appearanceMethod?.length"
      >Appearance Methods</v-tab
    >
  </v-tabs>

  <v-card-text>
    <v-tabs-window v-model="tab">
        <v-tabs-window-item value="documents">
          <v-skeleton-loader
            class="my-0"
            type="table"
            :height="200"
            color="var(--bg-gray-200)"
            :loading="documentsLoading"
          >
          <ScheduledDocuments
            :documents="documentDetails.document"
            :fileId
            :fileNumberTxt="documentDetails.fileNumberTxt"
            :courtLevel="documentDetails.courtLevelCd"
            :agencyId="documentDetails.agencyId"
          />
          </v-skeleton-loader>
        </v-tabs-window-item>
        <v-tabs-window-item v-if="showBinder" value="binder">
          <JudicialBinder :documents="documentDetails.binderDocuments"
            :fileId
            :fileNumberTxt="documentDetails.fileNumberTxt"
            :courtLevel="documentDetails.courtLevelCd"
            :agencyId="documentDetails.agencyId" />
        </v-tabs-window-item>

        <v-tabs-window-item value="parties">
          
            <ScheduledParties :file-id="fileId" :appearance-id="appearanceId" />
          
        </v-tabs-window-item>
        <v-tabs-window-item
          v-if="methods.appearanceMethod?.length"
          value="methods"
        >
          <CivilAppearanceMethods
            :appearanceMethod="methods.appearanceMethod"
          />
        </v-tabs-window-item>
      
    </v-tabs-window>
  </v-card-text>
</template>

<script setup lang="ts">
  import CivilAppearanceMethods from '@/components/case-details/civil/appearances/CivilAppearanceMethods.vue';
  import ScheduledDocuments from '@/components/case-details/civil/appearances/ScheduledDocuments.vue';
  import ScheduledParties from '@/components/case-details/civil/appearances/ScheduledParties.vue';
  import { FilesService } from '@/services';
  import { CivilAppearanceDetails } from '@/types/civil/jsonTypes';
  import { inject, onMounted, ref } from 'vue';
  import JudicialBinder from '../case-details/civil/appearances/JudicialBinder.vue';
  const props = withDefaults(
    defineProps<{
      fileId: string;
      appearanceId: string;
      showBinder?: boolean;
    }>(),
    {
      showBinder: false,
    }
  );

  const filesService = inject<FilesService>('filesService');
  const tab = ref('documents');
  const documentDetails = ref<CivilAppearanceDetails>({} as CivilAppearanceDetails);
  const methods = ref<CivilAppearanceDetails>({} as CivilAppearanceDetails);
  const documentsLoading = ref(false);

  if (!filesService) {
    throw new Error('Files service is undefined.');
  }

  onMounted(async () => {
    documentsLoading.value = true;
    let methodResponse = {};
    try {
      const documentsResponse = await filesService.civilAppearanceDocuments(
        props.fileId,
        props.appearanceId,
        true
      );
      documentDetails.value = documentsResponse;
      documentsLoading.value = false;
      // Dont await as we shouldnt hold up the UI for this
      methodResponse = filesService.civilAppearanceMethods(
        props.fileId,
        props.appearanceId
      ).then((methodResponse) => {
        methods.value = methodResponse;
      }).catch((e) => {
      // Optionally handle error
      console.error('Error occurred while retrieving appearance methods:', e);
      });
    } catch (error) {
      // Start fetching appearance methods in the background, don't await
      console.error(
        'Error occurred while retrieving appearance details:',
        error
      );
    } finally {
      documentsLoading.value = false;
    }
  });
</script>

<style scoped>
  .v-tabs {
    flex: 10;
  }
  .v-skeleton-loader {
    display: block;
  }
</style>
