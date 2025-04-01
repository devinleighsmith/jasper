<template>
  <!-- todo: Extract this out to more generic location -->
  <v-overlay :opacity="0.333" v-model="isLoading" />

  <v-card style="min-height: 40px" v-if="errorCode > 0 && errorCode == 403">
    <span> You are not authorized to access this page. </span>
  </v-card>
  <!------------------------------------------------------->
  <banner title="Court list" color="#183a4a" bgColor="#b4e6ff">
    <template #left v-if="appliedDate"
      >:
      <v-icon :icon="mdiChevronLeft" @click="addDay(-1)" />
      <b>{{ shortHandDate }}</b>
      <v-icon :icon="mdiChevronRight" @click="addDay(1)" />
    </template>
    <template #right>
      <v-btn @click="showDropdown = !showDropdown">
        Search by Courtroom & Date
      </v-btn>
    </template>
  </banner>

  <v-expand-transition>
    <v-card v-show="showDropdown" height="100" elevation="7">
      <court-list-search
        v-model:date="selectedDate"
        v-model:isSearching="searchingRequest"
        v-model:showDropdown="showDropdown"
        v-model:appliedDate="appliedDate"
        @courtListSearched="populateCardTablePairings"
      />
    </v-card>
  </v-expand-transition>
  <v-container>
    <v-skeleton-loader class="my-1" type="table" :loading="searchingRequest">
      <court-list-table-search
        v-if="cardTablePairings.length"
        v-model:filesFilter="selectedFilesFilter"
        v-model:AMPMFilter="selectedAMPMFilter"
        v-model:search="search"
      />
      <template
        v-for="pairing in filteredTablePairings"
        :key="pairing.card.courtListLocationID"
        class="w-100"
      >
        <court-list-card :cardInfo="pairing.card" />
        <court-list-table
          v-model:selectedItems="selectedItems"
          v-model:search="search"
          :data="pairing.table"
        />
      </template>
    </v-skeleton-loader>
  </v-container>
</template>

<script setup lang="ts">
  import { HttpService } from '@/services/HttpService';
  import { CourtListCardInfo } from '@/types/courtlist';
  import { courtListAppearanceType } from '@/types/criminal/jsonTypes';
  import { mdiChevronLeft, mdiChevronRight } from '@mdi/js';
  import { computed, inject, ref } from 'vue';
  import CourtListTable from './CourtListTable.vue';
  import CourtListTableSearch from './CourtListTableSearch.vue';

  const errorCode = ref(0);
  const searchingRequest = ref(false);
  const isLoading = ref(false);
  const selectedDate = ref(new Date());
  const appliedDate = ref<Date | null>(null);
  const showDropdown = ref(false);
  const search = ref();
  const selectedFilesFilter = ref();
  const selectedAMPMFilter = ref();
  const selectedItems = ref();
  const httpService = inject<HttpService>('httpService');
  const cardTablePairings = ref<
    {
      card: CourtListCardInfo;
      table: courtListAppearanceType[];
    }[]
  >([]);
  const filesFilterMap: {
    [key: string]: (appearance: courtListAppearanceType) => boolean;
  } = {
    Complete: (appearance: courtListAppearanceType) => !!appearance.isComplete,
    Cancelled: (appearance: courtListAppearanceType) =>
      appearance.appearanceStatusCd === 'CNCL',
    'To be called': (appearance: courtListAppearanceType) =>
      appearance.appearanceStatusCd === 'SCHD',
  };

  const filterByAMPM = (pairing: any) =>
    !selectedAMPMFilter.value || pairing.card.amPM === selectedAMPMFilter.value;

  const filterByFiles = (table: any) => {
    return selectedFilesFilter.value
      ? table.filter(filesFilterMap[selectedFilesFilter.value])
      : table;
  };

  const filteredTablePairings = computed<
    {
      card: CourtListCardInfo;
      table: courtListAppearanceType[];
    }[]
  >(() => {
    return cardTablePairings.value
      .filter(filterByAMPM)
      .map((pairing) => ({ ...pairing, table: filterByFiles(pairing.table) }));
  });

  const shortHandDate = computed(() =>
    appliedDate.value
      ? appliedDate.value.toLocaleDateString('en-US', {
          weekday: 'long',
          day: '2-digit',
          month: 'long',
          year: 'numeric',
        })
      : ''
  );

  if (!httpService) {
    throw new Error('Service is undefined.');
  }

  const populateCardTablePairings = (data: any) => {
    cardTablePairings.value = [];
    if (!data?.items?.length) {
      return;
    }
    // As of right now the cards will only ever have 1 location/room pairing.
    // If there are multiple `items` then that means there is more than 1 judge
    // in this location/room pairing on this given day.
    data.items.forEach((courtList: any) => {
      const courtRoomDetails = courtList.courtRoomDetails[0];
      const adjudicatorDetails = courtRoomDetails.adjudicatorDetails[0];
      const card = {} as CourtListCardInfo;
      const appearances = courtList.appearances as courtListAppearanceType[];
      card.fileCount = courtList.casesTarget;
      card.activity = courtList.activityDsc;
      card.presider = adjudicatorDetails?.adjudicatorNm;
      card.courtListRoom = courtRoomDetails.courtRoomCd;
      card.courtListLocationID = courtList.locationId;
      card.courtListLocation = courtList.locationNm;
      card.amPM = adjudicatorDetails?.amPm;

      cardTablePairings.value.push({ card, table: appearances });
    });
    console.log(data);
  };

  const addDay = (days: number) => {
    if (appliedDate.value) {
      appliedDate.value = new Date(
        appliedDate.value.setDate(appliedDate.value.getDate() + days)
      );
      selectedDate.value = appliedDate.value;
    }
  };
</script>
