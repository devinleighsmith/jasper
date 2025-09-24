<template>
  <v-data-table-virtual
    :headers="headers"
    :items="data"
    height="400"
    fixed-header
    :sort-by
  ></v-data-table-virtual>
</template>

<script setup lang="ts">
  import { ReservedJudgementModel } from '@/services/ReservedJudgementService';
  import { formatDateInstanceToDDMMMYYYY } from '@/utils/dateUtils';
  import { ref } from 'vue';

  defineProps<{
    data: ReservedJudgementModel[];
  }>();
  const sortBy = ref([{ key: 'cc', order: 'desc' }] as const);

  const headers = ref([
    {
      title: 'FILE #',
      key: 'fileNumber',
    },
    {
      title: 'ACCUSED / PARTIES',
      key: '',
    },
    {
      title: 'ACTIVITY',
      key: '',
    },
    {
      title: 'DECISION DATE',
      key: '',
    },
    {
      title: 'REASON',
      key: '',
    },
    {
      title: 'LAST APPEARANCE',
      key: 'appearanceDate',
      value: (item: ReservedJudgementModel) =>
        formatDateInstanceToDDMMMYYYY(new Date(item.appearanceDate)),
    },
    {
      title: 'DUE DATE',
      key: '',
    },
    {
      title: 'CASE AGE (days)',
      key: 'ageInDays',
    },
  ]);
</script>
