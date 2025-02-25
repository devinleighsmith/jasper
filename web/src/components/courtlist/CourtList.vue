<template>
  <!-- todo: Extract this out to more generic location -->
  <v-overlay :opacity="0.333" v-model="isLoading" />

  <v-card style="min-height: 40px" v-if="errorCode > 0 && errorCode == 403">
    <span> You are not authorized to access this page. </span>
  </v-card>
  <!------------------------------------------------------->
  <banner title="Court list" color="#183a4a" bgColor="#b4e6ff">
    <template #left v-if="bannerDate"
      >:
      <v-icon :icon="mdiChevronLeft" @click="AddDay(-1)" />
      {{ shortHandDate }}
      <v-icon :icon="mdiChevronRight" @click="AddDay(1)" />
    </template>
    <template #right>
      <v-btn @click="showDropdown = !showDropdown">
        Search by Courtroom & Date
      </v-btn>
    </template>
  </banner>
  <!-- <v-expand-transition>
    <v-card
      v-show="showDropdown"
      class="mx-auto bg-secondary"
      height="100"
      width="100"
    ></v-card>
  </v-expand-transition> -->

  <v-expand-transition>
    <v-card v-show="showDropdown" height="100" elevation="16">
      <court-list-search
        v-model:date="selectedDate"
        v-model:isSearching="searchingRequest"
        v-model:showDropdown="showDropdown"
        v-model:bannerDate="bannerDate"
        @courtListSearched="searchForCourtList"
      />
    </v-card>
  </v-expand-transition>
  <v-skeleton-loader type="table" :loading="searchingRequest">
    <div></div>
  </v-skeleton-loader>
</template>

<script setup lang="ts">
  import { HttpService } from '@/services/HttpService';
import { useCommonStore, useCourtListStore } from '@/stores';
import { mdiChevronLeft, mdiChevronRight } from '@mdi/js';
import { computed, inject, onMounted, ref } from 'vue';

  const commonStore = useCommonStore();
  const courtListStore = useCourtListStore();
  // State variables
  const errorCode = ref(0);
  const searchingRequest = ref(false);
  const isLoading = ref(true);
  const selectedDate = ref(new Date());
  const bannerDate = ref<Date | null>(null);

  const shortHandDate = computed(() =>
    bannerDate.value
      ? bannerDate.value.toLocaleDateString('en-US', {
          weekday: 'long',
          day: '2-digit',
          month: 'long',
          year: 'numeric',
        })
      : ''
  );

  const httpService = inject<HttpService>('httpService');
  const showDropdown = ref(true);

  if (!httpService) {
    throw new Error('Service is undefined.');
  }

  // Fetch data on mount
  onMounted(() => {
    //getListOfAvailableCourts();
    isLoading.value = false;
  });

  const searchForCourtList = (data) => {
    console.log(data);
    // map cards
    //searchingRequest.value = false;
    // if (!selectedCourtLocation.value.Location) {
    //   selectedCourtLocationState.value = false;
    //   searchAllowed.value = true;
    // } else {
    //   Object.assign(courtListLocation, selectedCourtLocation.value.Location);
    //   courtListLocationID.value = selectedCourtLocation.value.LocationID;
    //   if (
    //     selectedCourtRoom.value == 'null' ||
    //     selectedCourtRoom.value == undefined
    //   ) {
    //     selectedCourtRoomState.value = false;
    //     searchAllowed.value = true;
    //   } else {
    //     courtListRoom.value = selectedCourtRoom.value;
    //     if (
    //       route.params.location != courtListLocationID.value ||
    //       route.params.room != courtListRoom.value ||
    //       route.params.date != shortHandDate.value
    //     ) {
    //       router.push({
    //         name: 'CourtListResult',
    //         params: {
    //           location: courtListLocationID.value,
    //           room: courtListRoom.value,
    //           date: shortHandDate.value,
    //         },
    //       });
    //     }
    //     searchAllowed.value = false;
    //     setTimeout(() => {
    //       getCourtListDetails();
    //     }, 50);
    //   }
    // }
  };

  const AddDay = (days: number) => {
    if (bannerDate.value) {
      bannerDate.value = new Date(
        bannerDate.value.setDate(bannerDate.value.getDate() + days)
      );
      selectedDate.value = bannerDate.value;
    }
  };
</script>

<style scoped>
  .card {
    border: white;
  }
</style>
