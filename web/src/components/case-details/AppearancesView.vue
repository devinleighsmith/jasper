<template>
  <!-- Past -->
  <div v-if="pastAppearances?.length">
    <v-card class="my-5" color="#efedf5" elevation="0" min-height="3rem">
      <v-card-text>
        <v-row align="center" no-gutters>
          <v-col class="text-h5" cols="6"> Past Appearances </v-col>
        </v-row>
      </v-card-text>
    </v-card>
    <v-data-table-virtual
      :headers="pastHeaders"
      :items="pastAppearances"
      :sort-by="sortBy"
      class="my-3"
      max-height="400"
      item-value="appearanceId"
      fixed-header
    >
      <template v-slot:item.appearanceReasonCd="{ value, item }">
        <v-tooltip :text="item.appearanceReasonDsc" location="top">
          <template v-slot:activator="{ props }">
            <span v-bind="props" class="has-tooltip">{{ value }}</span>
          </template>
        </v-tooltip>
      </template>
      <template v-slot:item.courtLocation="{ value, item }">
        {{ value }} <br />
        <span style="color: gray">Room {{ item.courtRoomCd }}</span>
      </template>
      <template v-slot:item.appearanceStatusCd="{ value }">
        <AppearanceStatusChip :status="value" />
      </template>
    </v-data-table-virtual>
  </div>
  <!-- Future -->
  <v-card
    v-if="futureAppearances?.length"
    class="mt-12"
    color="#efedf5"
    elevation="0"
    min-height="4rem"
  >
    <v-card-text>
      <v-row align="center" no-gutters>
        <v-col class="text-h5" cols="6"> Future Appearances </v-col>
      </v-row>
    </v-card-text>
  </v-card>
  <v-data-table-virtual
    :headers="futureHeaders"
    :items="futureAppearances"
    :sort-by="sortBy"
    class="mt-3"
    max-height="400"
    item-value="appearanceId"
    fixed-header
  >
    <template v-slot:item.appearanceReasonCd="{ value, item }">
      <v-tooltip :text="item.appearanceReasonDsc" location="top">
        <template v-slot:activator="{ props }">
          <span v-bind="props" class="has-tooltip">{{ value }}</span>
        </template>
      </v-tooltip>
    </template>
    <template v-slot:item.courtLocation="{ value, item }">
      {{ value }} <br />
      <span style="color: gray">Room {{ item.courtRoomCd }}</span>
    </template>
    <template v-slot:item.appearanceStatusCd="{ value }">
      <AppearanceStatusChip :status="value" />
    </template>
  </v-data-table-virtual>
</template>

<script setup lang="ts">
  import AppearanceStatusChip from '@/components/shared/AppearanceStatusChip.vue';
  import { criminalApprDetailType } from '@/types/criminal/jsonTypes';
  import { formatDateToDDMMMYYYY } from '@/utils/utils';
  import { computed, ref } from 'vue';

  const props = defineProps<{ appearances: criminalApprDetailType[] }>();
  const now = new Date('2020-10-01T00:00:00Z'); // Replace with actual current date
  const pastHeaders = [
    {
      title: 'DATE',
      key: 'appearanceDt',
      value: (item: criminalApprDetailType) =>
        formatDateToDDMMMYYYY(item.appearanceDt),
    },
    { title: '', key: 'DARS' },
    { title: 'REASON', key: 'appearanceReasonCd' },
    {
      title: 'TIME DURATION',
      key: 'appearanceTm',
      value: (item: criminalApprDetailType) => {
        if (item.appearanceTm) {
          const time = item.appearanceTm.split(' ')[1];
          const hours = parseInt(time.slice(0, 2), 10);
          const minutes = time.slice(3, 5);
          const period = hours >= 12 ? 'PM' : 'AM';
          const formattedHours = hours % 12 || 12; // Convert to 12-hour format
          return `${formattedHours}:${minutes} ${period}`;
        }
        return '';
      },
    },
    { title: 'LOCATION ROOM', key: 'courtLocation' },
    { title: 'PRESIDER', key: 'judgeFullNm' },
    {
      title: 'ACCUSED',
      key: 'name',
      value: (item: criminalApprDetailType) =>
        item.lastNm && item.givenNm ? item.lastNm + ', ' + item.givenNm : '',
    },
    { title: 'STATUS', key: 'appearanceStatusCd' },
  ];
  const futureHeaders = [
    {
      title: 'DATE',
      key: 'appearanceDt',
      value: (item: criminalApprDetailType) =>
        formatDateToDDMMMYYYY(item.appearanceDt),
    },
    { title: 'REASON', key: 'appearanceReasonCd' },
    {
      title: 'TIME DURATION',
      key: 'appearanceTm',
      value: (item: criminalApprDetailType) => {
        if (item.appearanceTm) {
          const time = item.appearanceTm.split(' ')[1];
          const hours = parseInt(time.slice(0, 2), 10);
          const minutes = time.slice(3, 5);
          const period = hours >= 12 ? 'PM' : 'AM';
          const formattedHours = hours % 12 || 12; // Convert to 12-hour format
          return `${formattedHours}:${minutes} ${period}`;
        }
        return '';
      },
    },
    { title: 'LOCATION ROOM', key: 'courtLocation' },
    { title: 'ACTIVITY', key: 'activity' },
    {
      title: 'ACCUSED',
      key: 'name',
      value: (item: criminalApprDetailType) =>
        item.lastNm && item.givenNm ? item.lastNm + ', ' + item.givenNm : '',
    },
    { title: 'STATUS', key: 'appearanceStatusCd' },
  ];

  const sortBy = ref([{ key: 'appearanceDt', order: 'asc' }] as const);

  const futureAppearances = computed(() =>
    props.appearances?.filter(
      (app: criminalApprDetailType) => new Date(app?.appearanceDt) > now
    )
  );
  const pastAppearances = computed(() =>
    props.appearances?.filter(
      (app: criminalApprDetailType) => new Date(app?.appearanceDt) <= now
    )
  );
  console.log(props.appearances);
</script>
