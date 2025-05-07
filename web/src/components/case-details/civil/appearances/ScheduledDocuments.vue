<template>
  <v-data-table-virtual
    class="documentsTable"
    v-if="true"
    :headers
    :items="documents"
    item-value="appearanceId"
  >
    <template v-slot:item.activity="{ item }">
      <div v-for="info in item.documentSupport" :key="info.actCd">
        {{ info.actCd }}
      </div>
    </template>
  </v-data-table-virtual>
</template>

<script setup lang="ts">
  import { civilDocumentType } from '@/types/civil/jsonTypes/index';
  import { formatDateToDDMMMYYYY } from '@/utils/dateUtils';

  const props = defineProps<{
    documents: civilDocumentType[];
  }>();

  const headers = [
    { title: 'SEQ', key: 'fileSeqNo' },
    { title: 'DOCUMENT TYPE', key: 'documentTypeDescription' },
    { title: 'ACT', key: 'activity' },
    {
      title: 'DATE FILED',
      key: 'filedDt',
      value: (item) => formatDateToDDMMMYYYY(item.filedDt),
      sortRaw: (a: civilDocumentType, b: civilDocumentType) =>
        new Date(a.filedDt).getTime() - new Date(b.filedDt).getTime(),
    },
    { title: 'FILED BY', key: 'filedByName' },
    { title: 'RESULTS', key: 'runtime' },
    { title: 'ISSUES', key: 'issues' },
  ];
</script>

<style scoped>
  .documentsTable {
    background-color: var(--bg-light-gray) !important;
    padding-bottom: 2rem !important;
  }
</style>
