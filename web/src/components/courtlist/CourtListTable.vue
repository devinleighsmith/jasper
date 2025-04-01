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
    <template v-slot:item.crown="{ value }">
      <v-tooltip :disabled="value?.length < 2" location="top">
        <template #activator="{ props }">
          <span v-bind="props">{{ renderCrown(value) }}</span>
        </template>
        <span v-html="renderCrownTooltip(value)"></span>
      </v-tooltip>
    </template>
    <template v-slot:item.counsel="{ item }">
      <v-tooltip
        :disabled="
          (item.counsel?.length ?? 0) + (item.justinCounsel ? 1 : 0) < 2
        "
        location="top"
      >
        <template #activator="{ props }">
          <span v-bind="props">{{
            renderCrown(item.counsel) + renderJustinCounsel(item.justinCounsel)
          }}</span>
        </template>
        <span v-html="renderCrownTooltip(item.counsel)"></span>
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

  const props = defineProps<{
    data: courtListAppearanceType[];
    search: string;
  }>();

  // props.data[0]?.crown?.push({
  //   partId: '9828.0007',
  //   lastNm: 'Traill',
  //   givenNm: 'Josh  ',
  //   assignedYn: 'N',
  // });

  //   {
  //     "counselId": 35,
  //     "lawSocietyId": 35,
  //     "lastNm": "Brown- Smith ",
  //     "givenNm": "Bobby ",
  //     "prefNm": "",
  //     "addressLine1Txt": "",
  //     "addressLine2Txt": "",
  //     "cityTxt": "Victoria",
  //     "province": "BC",
  //     "postalCode": "",
  //     "phoneNoTxt": "250-34567802",
  //     "emailAddressTxt": "",
  //     "activeYn": "Y",
  //     "counselType": "REG"
  // }
  console.log(props.data);

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
      value: (item: courtListAppearanceType) =>
        item.caseAgeDays ? item.caseAgeDays + 'd' : '',
    },
    { title: 'NOTES', key: 'actions' },
  ]);

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

  const renderJustinCounsel = (counsel: any) => {
    if (!counsel) {
      return '';
    }
    let name = counsel?.lastNm;
    if (counsel?.givenNm) {
      name += ', ' + counsel?.givenNm;
    }

    return name;
  };

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
