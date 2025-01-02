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
      :headers="headers"
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
    <div v-if="selectedFiles.length > 0">
      <!-- Here we put out actionbar -->

      <v-app-bar
        :elevation="2"
        location="bottom"
        color="#183a4a"
        rounded
        style="
          max-width: 50%;
          margin: 0 auto;
          left: 50%;
          transform: translateX(-50%);
          margin-bottom: 2.5rem;
        "
      >
        <v-app-bar-title>Application Bar</v-app-bar-title>
      </v-app-bar>

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
  </div>
</template>
<script lang="ts">
  import { beautifyDate } from '@/filters';
  import { KeyValueInfo, LookupCode } from '@/types/common';
  import { CourtClassEnum, FileDetail } from '@/types/courtFileSearch';
  import { roomsInfoType } from '@/types/courtlist';
  import { defineComponent, PropType } from 'vue';

  export default defineComponent({
    data() {
      return {
        beautifyDate,
        groupBy: [
          {
            key: 'courtClassCd',
            order: 'asc' as const,
          },
        ],
        idSelector: '',
        selected: [],
        selectedFiles: [],
        allFields: [
          {
            key: 'location',
            label: 'Location',
            tdClass: 'border-top',
            thClass: 'text-primary',
            sortable: true,
            sortByFormatted: true,
            formatter: (value, key, item) => {
              return this.getLocation(item.fileHomeAgencyId);
            },
          },
          {
            key: 'courtClassCd',
            label: 'Class',
            tdClass: 'border-top',
            thClass: 'text-primary',
            sortable: true,
          },
          {
            key: 'fileNumberTxt',
            label: 'File number',
            tdClass: 'border-top',
            thClass: 'text-primary',
            sortable: true,
            sortByFormatted: true,
            formatter: (value, key, item) => {
              return this.getFormattedFileNumber(item);
            },
          },
          {
            key: 'participant',
            label: 'Participants',
            tdClass: 'border-top max-width-300',
            thClass: 'text-primary',
            sortable: true,
            sortByFormatted: true,
            formatter: (value, key, item) => {
              return [...new Set(item.participant.map((p) => p.fullNm))].join(
                '; '
              );
            },
          },
          {
            key: 'charges',
            label: 'Charges',
            tdClass: 'border-top max-width-300',
            sortable: true,
            sortByFormatted: true,
            formatter: (value, key, item) => {
              const uniqueCharges = [
                ...new Set(
                  item.participant
                    .flatMap((p) => p.charge)
                    .map((c) => c.sectionTxt)
                ),
              ];
              const firstCharge =
                uniqueCharges.length > 0 ? uniqueCharges[0] : '';
              return uniqueCharges.length > 1
                ? `${firstCharge} + [${uniqueCharges.length - 1}]`
                : firstCharge;
            },
          },
          {
            key: 'warrantyYN',
            label: 'OW',
            tdClass: 'border-top',
            thClass: 'text-primary',
            sortable: true,
          },
          {
            key: 'inCustodyYN',
            label: 'IC',
            tdClass: 'border-top',
            thClass: 'text-primary',
            sortable: true,
          },
          {
            key: 'nextApprDt',
            label: 'Next appearance',
            tdClass: 'border-top',
            sortable: true,
            formatter: (value, key, item) => {
              return beautifyDate(item.nextApprDt);
            },
          },
          {
            key: 'sealStatusCd',
            label: '',
            tdClass: 'border-top',
          },
          {
            key: 'action',
            label: '',
            tdClass: 'border-top',
          },
        ],
        headers: [
          { key: 'data-table-group' },
          {
            title: 'File #',
            key: 'fileNumberTxt',
            width: '15%',
            value: (item: FileDetail) => `${this.getFormattedFileNumber(item)}`,
          },
          {
            title: 'Accused / Parties',
            key: 'participant',
            width: '15%',
            value: (item: FileDetail) =>
              `${[...new Set(item.participant.map((p) => p.fullNm))].join('; ')}`,
          },
          {
            title: 'Class',
            key: 'courtClassCd',
            width: '15%',
            value: (item: FileDetail) => `${this.getClass(item.courtClassCd)}`,
          },
          {
            title: 'Location',
            key: 'location',
            width: '15%',
            value: (item: FileDetail) =>
              `${this.getLocation(item.fileHomeAgencyId)}`,
          },
          {
            title: 'Charges',
            key: 'charges',
            width: '15%',
            value: (item: FileDetail) => {
              const uniqueCharges = [
                ...new Set(
                  item.participant
                    .flatMap((p) => p.charge)
                    .map((c) => c.sectionTxt)
                ),
              ];
              const firstCharge =
                uniqueCharges.length > 0 ? uniqueCharges[0] : '';
              return uniqueCharges.length > 1
                ? `${firstCharge} + [${uniqueCharges.length - 1}]`
                : firstCharge;
            },
          },
          {
            title: 'Next appearance',
            key: 'nextApprDt',
            width: '15%',
            value: (item: FileDetail) => `${beautifyDate(item.nextApprDt)}`,
          },
          { title: 'OW', key: 'warrantyYN', width: '5%' },
          { title: 'IC', key: 'inCustodyYN', width: '5%' },
        ],
      };
    },
    computed: {
      filteredFields() {
        return this.isCriminal
          ? this.allFields
          : this.allFields.filter(
              (f) => !['charges', 'warrantyYN', 'inCustodyYN'].includes(f.key)
            );
      },
    },
    mounted() {
      this.idSelector = this.isCriminal ? 'mdocJustinNo' : 'physicalFileId';
    },
    methods: {
      getFormattedFileNumber(detail: FileDetail) {
        return `${detail.ticketSeriesTxt ?? ''}${detail.fileNumberTxt}${detail.mdocSeqNo ? '-' + detail.mdocSeqNo : ''}${detail.mdocRefTypeCd ? '-' + detail.mdocRefTypeCd : ''}`;
      },
      getLocation(fileHomeAgencyId: string) {
        return (
          this.courtRooms.find((r) => r.value === fileHomeAgencyId)?.text || ''
        );
      },
      getClass(courtClassCd: string) {
        return (
          this.classes.find((l) => l.code === courtClassCd)?.shortDesc || ''
        );
      },
      getClassColor(courtClassCd: string) {
        const classValue = CourtClassEnum[courtClassCd];

        switch (classValue) {
          case CourtClassEnum.A:
          case CourtClassEnum.Y:
            return 'text-blue';

          case CourtClassEnum.F:
            return 'text-green';

          case CourtClassEnum.C:
            return 'text-purple';
          default:
            return '';
        }
      },
      handleCaseClick(id: string) {
        const isExist = this.selectedFiles.find(
          (c) => c[this.idSelector] === id
        );
        if (isExist) {
          return;
        }

        const file = this.searchResults.find((c) => c[this.idSelector] === id);
        if (file) {
          this.$emit('add-selected', file);
        }
      },
      handleAddFileAndViewClick(id: string) {
        const isExist = this.selectedFiles.find(
          (c) => c[this.idSelector] === id
        );
        if (!isExist) {
          const file = this.searchResults.find(
            (c) => c[this.idSelector] === id
          );
          if (file) {
            this.$emit('add-selected', file);
          }
        }

        this.handleViewFilesClick();
      },
      handleDeleteClick(id: string) {
        this.$emit('remove-selected', this.idSelector, id);
      },
      handleDeleteAllClick() {
        this.$emit('clear-selected');
      },
      handleViewFilesClick() {
        const files = this.selectedFiles.map(
          (c) =>
            ({
              key: c[this.idSelector],
              value: this.getFormattedFileNumber(c),
            }) as KeyValueInfo
        );

        this.$emit('files-viewed', files);
      },
    },
    props: {
      courtRooms: {
        type: Array as PropType<roomsInfoType[]>,
        default: () => [],
      },
      searchResults: {
        type: Array as PropType<FileDetail[]>,
        default: () => [],
      },
      classes: { type: Array as PropType<LookupCode[]>, default: () => [] },
      isSearching: { type: Boolean, default: () => false },
      isCriminal: { type: Boolean, default: () => true },
      selectedFiles: {
        type: Array as PropType<FileDetail[]>,
        default: () => [],
      },
      isSearchResultsOver: { type: Boolean, default: () => false },
    },
  });
</script>

<style scoped lang="scss">
  .card {
    border: white;
  }

  .text-blue {
    color: #007bff;
  }

  .text-green {
    color: #28a745;
  }

  .text-purple {
    color: #6f42c1;
  }

  table thead tr th {
    color: #28a745;
  }

  .no-wrap {
    white-space: nowrap;
  }

  .remove,
  .remove:hover,
  .remove:focus {
    text-decoration: none !important;
    color: #dc3545 !important;
    box-shadow: none;
  }
  .v-toolbar--rounded {
    border-radius: 70px;
  }
  .courtRowBanner {
    background-color: #3095b0;
    color: white;
    text-transform: uppercase;
  }
</style>
