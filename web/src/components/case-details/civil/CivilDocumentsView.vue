<template>
  <div
    v-for="(documents, type) in {
      // binder: binder,
      documents: documents,
    }"
    :key="type"
  >
    <v-card
      class="my-6"
      color="var(--bg-gray)"
      elevation="0"
      v-if="documents?.length > 0"
    >
      <v-card-text>
        <v-row align="center" no-gutters>
          <v-col class="text-h5" cols="6">
            All Documents ({{ documents.length }})
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>
    <v-data-table-virtual
      v-if="documents?.length"
      v-model="selectedItems"
      :headers="headers"
      :items="documents"
      :sort-by="sortBy"
      item-value="civilDocumentId"
      show-select
      class="my-3"
      height="400"
    >
      <template v-slot:item.documentTypeDescription="{ item }">
        <a
          v-if="item.imageId"
          href="javascript:void(0)"
          @click="cellClick({ item })"
        >
          {{ item.documentTypeDescription }}
        </a>
        <span v-else>
          {{ item.documentTypeDescription }}
        </span>
      </template>
      <!-- Only grabbing the first in array for now -->
      <template v-slot:item.activity="{ item }">
        {{
          item.documentSupport != null ? item?.documentSupport[0]?.actCd : ''
        }}
      </template>
      <!-- Only grabbing the first in array for now -->
      <template v-slot:item.filedBy="{ item }">
        {{ item.filedBy != null ? item?.filedBy[0]?.roleTypeCode : '' }}
      </template>
      <!-- Only grabbing the first in array for now -->
      <template v-slot:item.issue="{ item }">
        {{ item.issue != null ? item?.issue[0]?.issueTypeDesc : '' }}
      </template>
      <template v-slot:item.binderMenu="{ item }">
        <v-menu>
          <template v-slot:activator="{ props }">
            <v-btn
              :icon="mdiDotsVertical"
              variant="text"
              v-bind="props"
            ></v-btn>
          </template>

          <v-list>
            <v-list-item v-for="(item, i) in menuItems" :key="i" :value="i">
              <v-list-item-title>{{ item.title }}</v-list-item-title>
            </v-list-item>
          </v-list>
        </v-menu>
      </template>
    </v-data-table-virtual>
  </div>
</template>

<script setup lang="ts">
  import shared from '@/components/shared';
  import { beautifyDate } from '@/filters';
  import { useCivilFileStore } from '@/stores';
  import { civilDocumentType } from '@/types/civil/jsonTypes';
  import { CourtDocumentType, DocumentData } from '@/types/shared';
  import { formatDateToDDMMMYYYY } from '@/utils/dateUtils';
  import { mdiDotsVertical } from '@mdi/js';
  import { onMounted, ref } from 'vue';

  const props = defineProps<{ documents: civilDocumentType[] }>();

  const civilFileStore = useCivilFileStore();
  const selectedItems = defineModel<civilDocumentType[]>();
  const sortBy = ref([{ key: 'fileSeqNo', order: 'desc' }] as const);
  const headers = [
    { key: 'data-table-group' },
    {
      title: 'SEQ',
      key: 'fileSeqNo',
    },
    {
      title: 'DOCUMENT TYPE',
      key: 'documentTypeDescription',
    },
    {
      title: 'ACT',
      key: 'activity',
    },
    {
      title: 'DATE FILED',
      key: 'filedDt',
      value: (item) => formatDateToDDMMMYYYY(item.filedDt),
      sortRaw: (a: civilDocumentType, b: civilDocumentType) =>
        new Date(a.filedDt).getTime() - new Date(b.filedDt).getTime(),
    },
    {
      title: 'FILED BY',
      key: 'filedBy',
    },
    {
      title: 'ISSUES',
      key: 'issue',
    },
    {
      title: 'JUDICIAL BINDER',
      key: 'binderMenu',
      width: '2%',
    },
  ];
  const menuItems = [{ title: 'Add to binder' }];

  // This is code ported over from 'CriminalDocumentsView.vue' to keep file viewing capability
  // This will eventually be deprecated in favor of Nutrient PDF viewing functionality
  //   const cellClick = (eventData) => {
  //         const documentType =
  //           eventData.value == 'CourtSummary'
  //             ? CourtDocumentType.CSR
  //             : CourtDocumentType.Civil;
  //         const documentData: DocumentData = {
  //           appearanceDate: eventData.item.appearanceDate,
  //           appearanceId: eventData.item.appearanceId,
  //           dateFiled: eventData.item.dateFiled,
  //           documentDescription: eventData.item.documentType,
  //           documentId: eventData.item.documentId,
  //           fileId: civilFileStore.civilFileInformation.fileNumber,
  //           fileNumberText:
  //             civilFileStore.civilFileInformation.detailsData.fileNumberTxt,
  //           courtClass:
  //             civilFileStore.civilFileInformation.detailsData.courtClassCd,
  //           courtLevel:
  //             civilFileStore.civilFileInformation.detailsData.courtLevelCd,
  //           location:
  //             civilFileStore.civilFileInformation.detailsData
  //               .homeLocationAgencyName,
  //         };
  //         shared.openDocumentsPdf(documentType, documentData);
  //       };
  const stored = ref();
  onMounted(() => {
    stored.value = civilFileStore.civilFileInformation.documentsInfo;
  });

  const cellClick = (eventData) => {
    const documentType =
      eventData.item.documentTypeCd == 'CSR'
        ? CourtDocumentType.CSR
        : CourtDocumentType.Civil;
    const documentData: DocumentData = {
      appearanceDate: beautifyDate(eventData.item.lastAppearanceDt),
      appearanceId:
        eventData.item.appearanceId ?? eventData.item.civilDocumentId,
      dateFiled: beautifyDate(eventData.item.filedDt),
      documentDescription: eventData.item.documentTypeCd,
      documentId: eventData.item.civilDocumentId,
      fileId: civilFileStore.civilFileInformation.fileNumber,
      fileNumberText: eventData.item.documentTypeDescription,
      courtClass: civilFileStore.civilFileInformation.detailsData.courtClassCd,
      courtLevel: civilFileStore.civilFileInformation.detailsData.courtLevelCd,
      location:
        civilFileStore.civilFileInformation.detailsData.homeLocationAgencyName,
    };
    shared.openDocumentsPdf(documentType, documentData);
  };
</script>
