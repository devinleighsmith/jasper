<template>
  <v-data-table
    v-model="selected"
    :items="data"
    :headers
    :search="search"
    item-value="appearanceId"
    items-per-page="100"
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
    <template v-slot:item.counsel="{ item }">
      <v-tooltip
        :disabled="
          (item.counsel?.length ?? 0) + (item.justinCounsel ? 1 : 0) < 2
        "
        location="top"
      >
        <!-- <template #activator="{ props }">
          <span v-bind="props">{{
            renderCounsel(item.counsel, item.justinCounsel)
          }}</span>
        </template>
        <span
          v-html="renderCounselTooltip(item.counsel, item.justinCounsel)"
        ></span> -->
      </v-tooltip>
    </template>
    <template v-slot:item.crown="{ value }">
      <v-tooltip :disabled="value?.length < 2" location="top">
        <template #activator="{ props }">
          <span v-bind="props">{{ renderCrown(value) }}</span>
        </template>
        <span v-html="renderCrownTooltip(value)"></span>
      </v-tooltip>
    </template>
    <template v-slot:item.actions>
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

  const selected = defineModel<string[]>('selectedItems');

  defineProps<{
    data: courtListAppearanceType[];
    search: string;
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
    { title: 'COUNSEL', key: 'counsel' },
    { title: 'CROWN', key: 'crown' },
    {
      title: 'CASE AGE',
      key: 'caseAgeDays',
      value: (item: courtListAppearanceType) =>
        item.caseAgeDays ? item.caseAgeDays + 'd' : '',
    },
    { title: 'NOTES', key: 'actions' },
  ]);

  //   const renderCounselTooltip = (counsel: any, justinCounsel: any) => {
  //     if (!counsel) {
  //       return '';
  //     }
  //     let tooltip = '';
  //     counsel.forEach((counsel: any) => {
  //       tooltip += counsel?.lastNm + ', ' + counsel?.givenNm + '<br/>';
  //     });
  //     if (justinCounsel) {
  //       tooltip += justinCounsel?.lastNm + ', ' + justinCounsel?.givenNm;
  //     }
  //     return tooltip;
  //   };

  const renderCrownTooltip = (crown: any) => {
    if (!crown) {
      return '';
    }
    let tooltip = '';
    crown.forEach((crown: any) => {
      tooltip += crown?.lastNm + ', ' + crown?.givenNm + '<br/>';
    });
    return tooltip;
  };

  //   const renderCounsel = (counsel: any, justinCounsel: any) => {
  //     if (
  //       !counsel ||
  //       (counsel.length === 0 && (!justinCounsel || justinCounsel.length === 0))
  //     ) {
  //       return '';
  //     }
  //     let name = '';
  //     if (counsel[0].lastNm != null && counsel[0].givenNm != null) {
  //       name = counsel[0]?.lastNm + ', ' + counsel[0]?.givenNm;
  //     } else {
  //       name = justinCounsel.lastNm + ', ' + justinCounsel.givenNm;
  //     }
  //     const count = counsel.length + (justinCounsel != null ? 1 : 0);
  //     if (count > 1) {
  //       name += `+${count - 1}`;
  //     }
  //     return name;
  //   };

  const renderCrown = (crown: any) => {
    if (!crown || crown.length === 0) {
      return '';
    }
    let name = crown[0]?.lastNm + ', ' + crown[0]?.givenNm;
    if (crown.length > 1) {
      name += `+${crown.length - 1}`;
    }
    return name;
  };

//   const renderJustinCounsel = (counsel: any) => {
//     if (!counsel) {
//       return '';
//     }
//     let name = counsel?.lastNm;
//     if (counsel?.givenNm) {
//       name += ', ' + counsel?.givenNm;
//     }

//     return name;
//   };

  const hoursMinsFormatter = (hours: string, minutes: string) => {
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
