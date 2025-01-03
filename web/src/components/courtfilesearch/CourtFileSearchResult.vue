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
      item-value="physicalFileId"
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
    </v-data-table>
    <!-- <action-bar :selected="selectedFiles" /> -->
  </div>
</template>

<script setup lang="ts">
  //import ActionBar from '@/components/shared/table/ActionBar.vue';
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
