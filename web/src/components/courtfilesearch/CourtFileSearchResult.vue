<template>
  <div>
    <v-banner bg-color="#efedf5">
      <h3 class="px-2 py-2">Search Results</h3>
    </v-banner>
    <v-skeleton-loader type="table" :loading="isSearching">
      <v-data-table
        v-model="selectedFiles"
        :group-by
        :items-per-page
        :headers
        :items="searchResults"
        :item-value="idSelector"
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
    </v-skeleton-loader>
    <!-- todo: action-bar goes here -->
  </div>
</template>

<script setup lang="ts">
  import { beautifyDate } from '@/filters';
  import { LookupCode } from '@/types/common';
  import { FileDetail } from '@/types/courtFileSearch';
  import { roomsInfoType } from '@/types/courtlist';
  import { computed, defineProps, ref } from 'vue';

  const props = defineProps<{
    courtRooms: roomsInfoType[];
    searchResults: FileDetail[];
    classes: LookupCode[];
    isSearching: boolean;
    isCriminal: boolean;
    selectedFiles: FileDetail[];
    isSearchResultsOver: boolean;
  }>();

  // The reason we use 103 instead of 100 is due to the ability to have up to 3 group headers in the table
  const itemsPerPage = 103;
  const selectedFiles = ref<FileDetail[]>([]);
  const idSelector = computed(() =>
    props.isCriminal ? 'mdocJustinNo' : 'physicalFileId'
  );
  const groupBy = ref([
    {
      key: 'courtClassCd',
      order: 'asc' as const,
    },
  ]);
  const fullHeaders = ref([
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

  const headers = computed(() => {
    return props.isCriminal
      ? fullHeaders.value
      : fullHeaders.value.filter(
          (hdr) => !['charges', 'warrantyYN', 'inCustodyYN'].includes(hdr.key)
        );
  });

  function getFormattedFileNumber(detail: FileDetail) {
    return `${detail.ticketSeriesTxt ?? ''}
      ${detail.fileNumberTxt}
      ${detail.mdocSeqNo ? '-' + detail.mdocSeqNo : ''}
      ${detail.mdocRefTypeCd ? '-' + detail.mdocRefTypeCd : ''}`;
  }

  const getLocation = (fileHomeAgencyId: string) =>
    props.courtRooms.find((room) => room.value === fileHomeAgencyId)?.text ||
    '';

  const getClass = (courtClassCd: string) =>
    props.classes.find((lookup) => lookup.code === courtClassCd)?.shortDesc ||
    '';
</script>

<style scoped>
  .courtRowBanner {
    background-color: #3095b0;
    color: white;
    text-transform: uppercase;
  }
</style>
