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
      <v-icon :icon="mdiChevronLeft" @click="AddDay(-1)" />
      <b>{{ shortHandDate }}</b>
      <v-icon :icon="mdiChevronRight" @click="AddDay(1)" />
    </template>
    <template #right>
      <v-btn @click="showDropdown = !showDropdown">
        Search by Courtroom & Date
      </v-btn>
    </template>
  </banner>

  <v-expand-transition>
    <v-card v-show="showDropdown" height="100" elevation="16">
      <court-list-search
        v-model:date="selectedDate"
        v-model:isSearching="searchingRequest"
        v-model:showDropdown="showDropdown"
        v-model:appliedDate="appliedDate"
        @courtListSearched="PopulateCards"
      />
    </v-card>
  </v-expand-transition>
  <v-container>
    <v-skeleton-loader class="my-1" type="table" :loading="searchingRequest">
      <court-list-card
        v-for="card in cards"
        :key="card.courtListLocationID"
        :cardInfo="card"
      />
    </v-skeleton-loader>
  </v-container>
</template>

<script setup lang="ts">
  import CourtListCard from '@/components/courtlist/CourtListCard.vue';
  import { HttpService } from '@/services/HttpService';
  import { CourtListCardInfo } from '@/types/courtlist';
  import { mdiChevronLeft, mdiChevronRight } from '@mdi/js';
  import { computed, inject, ref } from 'vue';

  const errorCode = ref(0);
  const searchingRequest = ref(false);
  const isLoading = ref(false);
  const selectedDate = ref(new Date());
  const appliedDate = ref<Date | null>(null);
  const showDropdown = ref(false);
  const cards = ref<CourtListCardInfo[]>([]);
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

  const httpService = inject<HttpService>('httpService');
  if (!httpService) {
    throw new Error('Service is undefined.');
  }

  const PopulateCards = (data: any) => {
    cards.value = [];
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
      card.fileCount = courtList.casesTarget;
      card.activity = courtList.activityDsc;
      card.presider = adjudicatorDetails?.adjudicatorNm;
      card.courtListRoom = courtRoomDetails.courtRoomCd;
      card.courtListLocationID = courtList.locationId;
      card.courtListLocation = courtList.locationNm;
      card.amPM = adjudicatorDetails?.amPm;
      cards.value.push(card);
    });
  };

  const AddDay = (days: number) => {
    if (appliedDate.value) {
      appliedDate.value = new Date(
        appliedDate.value.setDate(appliedDate.value.getDate() + days)
      );
      selectedDate.value = appliedDate.value;
    }
  };
</script>
