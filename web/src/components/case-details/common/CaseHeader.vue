<template>
  <v-container>
    <v-row>
      <v-col cols="10">
        <v-tabs
          v-model="selectedTab"
          color="#72acca"
          align-tabs="start"
          hide-slider
        >
          <v-tab
            :prepend-icon="mdiTextBoxOutline"
            :class="{ 'active-tab': selectedTab === 'documents' }"
            border="md"
            rounded="lg"
            value="documents"
          >
            Documents
          </v-tab>
          <v-tab
            :prepend-icon="mdiCalendar"
            :class="{ 'active-tab': selectedTab === 'appearances' }"
            border="md"
            rounded="lg"
            value="appearances"
            class="mx-3"
          >
            Appearances
          </v-tab>
          <v-tab
            v-if="isCriminal"
            :prepend-icon="mdiScaleBalance"
            :class="{ 'active-tab': selectedTab === 'sentence' }"
            value="sentence"
            border="md"
            rounded="lg"
            disabled
          >
            Sentence/order details
          </v-tab>
        </v-tabs>
      </v-col>
      <v-col cols="2">
        <v-btn-secondary text="View shared folder" />
      </v-col>
    </v-row>
    <v-window
      v-if="isCriminal"
      mandatory
      continuous
      v-model="selectedTab"
      class="mt-3"
    >
      <v-window-item value="documents">
        <DocumentsView :participants="participants" />
      </v-window-item>
      <v-window-item value="appearances">
        <AppearancesView :appearances="appearances" />
      </v-window-item>
      <v-window-item value="sentence">
        <!-- <SentenceOrderDetailsView /> -->
      </v-window-item>
    </v-window>
  </v-container>
</template>

<script setup lang="ts">
  import { civilFileDetailsType } from '@/types/civil/jsonTypes';
import { criminalFileDetailsType } from '@/types/criminal/jsonTypes';
import { fileDetailsType } from '@/types/shared';
import { mdiCalendar, mdiScaleBalance, mdiTextBoxOutline } from '@mdi/js';
import { computed, ref } from 'vue';
import AppearancesView from './AppearancesView.vue';
import DocumentsView from './DocumentsView.vue';

  const props = defineProps<{
    details: fileDetailsType;
    activityClassCd: string;
  }>();
  const isCriminal = computed(() => props.activityClassCd === 'R');
  const selectedTab = ref('documents');
  let appearances: any[];
  let participants: any[];

  if (isCriminal) {
    const criminalDetails = props.details as criminalFileDetailsType;
    appearances = criminalDetails.appearances?.apprDetail;
    participants = criminalDetails.participant;
  } else {
    const civilDetails = props.details as civilFileDetailsType;
    appearances = civilDetails.appearances?.apprDetail;
    participants = [];
  }
</script>

<style scoped>
  .active-tab {
    border-color: #72acca !important;
    background-color: #72acca !important;
    color: white !important;
  }
</style>
