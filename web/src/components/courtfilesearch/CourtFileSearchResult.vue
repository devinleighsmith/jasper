<template>
  <div>
    <!-- Do we need this? -->
    <!-- <div class="my-3 bg-warning p-2" v-if="isSearchResultsOver">
        <span
          >More than 100 records match the search criteria, only the first 100
          are returned.</span
        >
      </div> -->
    <v-banner bg-color="#efedf5">
      <h3 class="px-2 py-2">Search Results</h3>
    </v-banner>
    <v-skeleton-loader v-if="isSearching" type="table"></v-skeleton-loader>
    <v-data-table
      v-model="selectedFiles"
      :group-by="groupBy"
      :items="searchResults"
      :headers="filteredHeaders"
      item-value="fileNumberTxt"
      items-per-page="100"
      hover
      show-select
      return-object
    >
      <template
        v-slot:group-header="{ item, columns, isGroupOpen, toggleGroup }"
      >
        <tr>
          <td class="pa-0" :colspan="columns.length">
            <v-banner
              class="courtRowBanner"
              :ref="
                () => {
                  if (!isGroupOpen(item)) toggleGroup(item);
                }
              "
            >
              {{
                isCriminal
                  ? 'Criminal - ' + getClass(item.value)
                  : getClass(item.value)
              }}
            </v-banner>
          </td>
        </tr>
      </template>
      <!-- return this.getLocation(item.fileHomeAgencyId); -->

      <!-- <template v-slot:item.participant="{ data }">
          {{[...new Set(item.participant.map((p) => p.fullNm))].join('; ')}}
      </template> -->
      <!-- <template v-slot:cell(participant)="{ data }">
          <span>{{
            [...new Set(data.map((p) => p.fullNm))].join('; ')
          }}</span>
        </template> -->
      <!-- formatter: (value, key, item) => {
              return [...new Set(item.participant.map((p) => p.fullNm))].join(
                '; '
              );
            }, -->
      <!-- <template #head(nextApprDt)="data">
          <span class="text-danger no-wrap">{{ data.label }}</span>
        </template>
        <template v-slot:cell(sealStatusCd)="data">
          <span v-if="data.item.sealStatusCd === 'SD'" class="text-danger"
            >(Sealed)</span
          >
        </template>
        <template v-slot:cell(courtClassCd)="data">
          <span :class="getClassColor(data.item.courtClassCd)">
            {{ getClass(data.item.courtClassCd) }}
          </span>
        </template>
        <template v-slot:cell(warrantYN)="data">
          <b-badge
            v-if="data.item.warrantYN === 'Y'"
            variant="primary text-light"
            :style="data.field.cellStyle"
            v-b-tooltip.hover
            title="Outstanding warrant"
          > -->
      <!-- <span>W</span>
          </b-badge>
        </template> -->
      <!-- <template v-slot:cell(inCustodyYN)="data">
          <b-badge
            v-if="data.item.inCustodyYN === 'Y'"
            variant="primary text-light"
            :style="data.field.cellStyle"
            v-b-tooltip.hover
            title="In custody"
          >
            IC
          </b-badge>
        </template>
        <template v-slot:cell(nextApprDt)="data">
          <span>
            {{ beautifyDate(data.item.nextApprDt) }}
          </span>
        </template>
        <template v-slot:cell(action)="data">
          <div class="d-flex justify-content-end no-wrap">
            <b-button
              variant="outline-primary"
              class="mr-3"
              @click="() => handleCaseClick(data.item[idSelector])"
              >Add File</b-button
            >
            <b-button
              variant="primary"
              @click="() => handleAddFileAndViewClick(data.item[idSelector])"
              >Add File and View</b-button
            >
          </div>
        </template> -->
    </v-data-table>
    <action-bar :selected="selectedFiles" />

    <!-- <v-toolbar dense floating style="position: fixed">
        <v-text-field
          prepend-icon="mdi-magnify"
          hide-details
          single-line
        ></v-text-field>

        <v-btn icon>
          <v-icon>mdi-crosshairs-gps</v-icon>
        </v-btn>

        <v-btn icon>
          <v-icon>mdi-dots-vertical</v-icon>
        </v-btn>
      </v-toolbar> -->

    <!-- <h3 class="mt-3">Files to View</h3>
      <b-table
        :fields="filteredFields"
        :items="selectedFiles"
        borderless
        small
        responsive="md"
        sort-icon-left
        striped
      >
        <template #head(nextApprDt)="data">
          <span class="text-danger no-wrap">{{ data.label }}</span>
        </template>
        <template v-slot:cell(sealStatusCd)="data">
          <span v-if="data.item.sealStatusCd === 'SD'" class="text-danger"
            >(Sealed)</span
          >
        </template>
        <template v-slot:cell(courtClassCd)="data">
          <span :class="getClassColor(data.item.courtClassCd)">
            {{ getClass(data.item.courtClassCd) }}
          </span>
        </template>
        <template v-slot:cell(warrantYN)="data">
          <b-badge
            v-if="data.item.warrantYN === 'Y'"
            variant="primary text-light"
            :style="data.field.cellStyle"
            v-b-tooltip.hover
            title="Outstanding warrant"
          >
            <span>W</span>
          </b-badge>
        </template>
        <template v-slot:cell(inCustodyYN)="data">
          <b-badge
            v-if="data.item.inCustodyYN === 'Y'"
            variant="primary text-light"
            :style="data.field.cellStyle"
            v-b-tooltip.hover
            title="In custody"
          >
            IC
          </b-badge>
        </template>
        <template v-slot:cell(nextApprDt)="data">
          <span>
            {{ beautifyDate(data.item.nextApprDt) }}
          </span>
        </template>
        <template v-slot:cell(action)="data">
          <div class="d-flex justify-content-end no-wrap">
            <b-button
              variant="link"
              class="remove"
              @click="() => handleDeleteClick(data.item[idSelector])"
            >
              <b-icon icon="trash"></b-icon> Remove
            </b-button>
          </div>
        </template>
      </b-table>
      <div class="my-3 bg-light p-3">
        <div class="d-flex">
          <b-button variant="primary" class="mr-3" @click="handleViewFilesClick"
            >View File(s)</b-button
          >
          <b-button variant="outline-primary" @click="handleDeleteAllClick"
            >Remove All Files and Start Over</b-button
          >
        </div> 
      </div>-->
  </div>
</template>

<script setup lang="ts">
  import ActionBar from '@/components/shared/table/ActionBar.vue';
  import { beautifyDate } from '@/filters';
  import { KeyValueInfo, LookupCode } from '@/types/common';
  import { FileDetail } from '@/types/courtFileSearch';
  import { roomsInfoType } from '@/types/courtlist';
  import { computed, defineEmits, defineProps, onMounted, ref } from 'vue';

  const props = defineProps<{
    courtRooms: roomsInfoType[];
    searchResults: FileDetail[];
    classes: LookupCode[];
    isSearching: boolean;
    isCriminal: boolean;
    selectedFiles: FileDetail[];
    isSearchResultsOver: boolean;
  }>();

  const emit = defineEmits<{
    (e: 'add-selected', file: FileDetail): void;
    (e: 'remove-selected', idSelector: string, id: string): void;
    (e: 'clear-selected'): void;
    (e: 'files-viewed', files: KeyValueInfo[]): void;
  }>();

  const groupBy = ref([
    {
      key: 'courtClassCd',
      order: 'asc' as const,
    },
  ]);

  const idSelector = ref('');
  const selectedFiles = ref<FileDetail[]>([]);
  const headers = ref([
    { key: 'data-table-group' },
    {
      title: 'File #',
      key: 'fileNumberTxt',
      value: (item: FileDetail) => `${getFormattedFileNumber(item)}`,
    },
    {
      title: 'Accused / Parties',
      key: 'participant',
      value: (item: FileDetail) =>
        `${[...new Set(item.participant.map((p) => p.fullNm))].join('; ')}`,
    },
    {
      title: 'Class',
      key: 'courtClassCd',
      value: (item: FileDetail) => `${getClass(item.courtClassCd)}`,
    },
    {
      title: 'Location',
      key: 'location',
      width: '15%',
      value: (item: FileDetail) => `${getLocation(item.fileHomeAgencyId)}`,
    },
    {
      title: 'Charges',
      key: 'charges',
      value: (item: FileDetail) => {
        const uniqueCharges = [
          ...new Set(
            item.participant.flatMap((p) => p.charge).map((c) => c.sectionTxt)
          ),
        ];
        const firstCharge = uniqueCharges.length > 0 ? uniqueCharges[0] : '';
        return uniqueCharges.length > 1
          ? `${firstCharge} + [${uniqueCharges.length - 1}]`
          : firstCharge;
      },
    },
    {
      title: 'Next appearance',
      key: 'nextApprDt',
      value: (item: FileDetail) => `${beautifyDate(item.nextApprDt)}`,
    },
    { title: 'OW', key: 'warrantyYN' },
    { title: 'IC', key: 'inCustodyYN' },
  ]);

  const filteredHeaders = computed(() => {
    return props.isCriminal
      ? headers.value
      : headers.value.filter(
          (f) => !['charges', 'warrantyYN', 'inCustodyYN'].includes(f.key)
        );
  });

  onMounted(() => {
    idSelector.value = props.isCriminal ? 'mdocJustinNo' : 'physicalFileId';
  });

  function getFormattedFileNumber(detail: FileDetail) {
    return `${detail.ticketSeriesTxt ?? ''}${detail.fileNumberTxt}${
      detail.mdocSeqNo ? '-' + detail.mdocSeqNo : ''
    }${detail.mdocRefTypeCd ? '-' + detail.mdocRefTypeCd : ''}`;
  }

  function getLocation(fileHomeAgencyId: string) {
    return (
      props.courtRooms.find((r) => r.value === fileHomeAgencyId)?.text || ''
    );
  }

  function getClass(courtClassCd: string) {
    return props.classes.find((l) => l.code === courtClassCd)?.shortDesc || '';
  }

  function handleCaseClick(id: string) {
    const isExist = props.selectedFiles.find((c) => c[idSelector.value] === id);
    if (isExist) {
      return;
    }

    const file = props.searchResults.find((c) => c[idSelector.value] === id);
    if (file) {
      emit('add-selected', file);
    }
  }

  function handleAddFileAndViewClick(id: string) {
    const isExist = props.selectedFiles.find((c) => c[idSelector.value] === id);
    if (!isExist) {
      const file = props.searchResults.find((c) => c[idSelector.value] === id);
      if (file) {
        emit('add-selected', file);
      }
    }

    handleViewFilesClick();
  }

  function handleDeleteClick(id: string) {
    emit('remove-selected', idSelector.value, id);
  }

  function handleDeleteAllClick() {
    emit('clear-selected');
  }

  function handleViewFilesClick() {
    const files = props.selectedFiles.map(
      (c) =>
        ({
          key: c[idSelector.value],
          value: getFormattedFileNumber(c),
        }) as KeyValueInfo
    );

    emit('files-viewed', files);
  }
</script>

<style scoped>
  .courtRowBanner {
    background-color: #3095b0;
    color: white;
    text-transform: uppercase;
  }
</style>
