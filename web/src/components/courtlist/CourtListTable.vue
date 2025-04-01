<template>
  <v-data-table
    v-model="selected"
    :items="data"
    :headers
    :search="search"
    item-value="courtFileNumber"
    class="pb-5"
  >
    <template v-slot:item.estimatedTime="{ item }">
      {{ hoursMinsFormatter(item.estimatedTimeHour, item.estimatedTimeMin) }}
    </template>
    <template v-slot:item.fileMarkers="{ item }">
      <file-markers
        :appearances="[item as unknown as criminalApprDetailType]"
        :participants="[]"
        division="Criminal"
      />
    </template>
    <template v-slot:item.appearanceReasonCd="{ value, item }">
      <v-tooltip :text="item.appearanceReasonDsc" location="top">
        <template v-slot:activator="{ props }">
          <span v-bind="props">{{ value }}</span>
        </template>
      </v-tooltip>
    </template>
    <template v-slot:item.crown="{ value }">
      <!-- ?? only grabbing first value, there could be multiple in the array ?? -->
      {{ value?.length ? value[0].lastNm + ', ' + value[0].givenNm : '' }}
    </template>
    <template v-slot:item.counsel="{ value }">
      <!-- ?? only grabbing first value, there could be multiple in the array ?? -->
      <!-- ?? what about justin counsel ?? -->
      {{ value?.length ? value[0].lastNm + ', ' + value[0].givenNm : '' }}
    </template>
    <template v-slot:item.actions="{ item }">
      <v-icon :icon="mdiNotebookEditOutline" size="large" />
      <v-icon :icon="mdiFileDocumentEditOutline" size="large" />
    </template>
    <template v-slot:bottom />
  </v-data-table>
</template>

<script setup lang="ts">
  import {
    courtListAppearanceType,
    criminalApprDetailType,
  } from '@/types/criminal/jsonTypes';
  import { mdiFileDocumentEditOutline, mdiNotebookEditOutline } from '@mdi/js';
  import { ref } from 'vue';

  // does it need to be define model?w
  const search = defineModel<string>('search');
  const selected = defineModel<string>('selectedItems');

  const props = defineProps<{
    data: courtListAppearanceType[];
  }>();

  const headers = ref([
    { key: 'data-table-group' },
    { title: 'FILE #', key: 'courtFileNumber' },
    { title: 'ACCUSED/PARTIES', key: 'accusedNm' },
    { title: 'TIME', key: 'appearanceTm' },
    { title: 'EST.', key: 'estimatedTime' },
    { title: 'ROOM', key: 'courtRoomCd' },
    { title: 'REASON', key: 'appearanceReasonCd' },
    { title: 'FILE MARKERS', key: 'fileMarkers' },
    // what if justinCounsel has a value?
    { title: 'COUNSEL', key: 'counsel' },
    { title: 'CROWN', key: 'crown' },
    {
      title: 'CASE AGE',
      key: 'caseAgeDays',
      value: (item: courtListAppearanceType) => item.caseAgeDays + 'd',
    },
    { title: 'NOTES', key: 'actions' },
  ]);

  const hoursMinsFormatter = (hours: string, minutes: string) => {
    // return to this... this will make 1hrs 3mins
    const hrs = parseInt(hours, 10);
    const mins = parseInt(minutes, 10);
    let result = '';
    if (hrs) {
      result += `${hrs} Hr(s)`;
    }
    if (mins) {
      result += `${result ? ' ' : ''}${mins} Min(s)`;
    }
    return result || '0 Mins';
  };
</script>
