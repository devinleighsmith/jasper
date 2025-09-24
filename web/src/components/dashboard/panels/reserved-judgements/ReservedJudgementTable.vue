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
  import { ReservedJudgement } from '@/types/ReservedJudgement';
  import { formatDateInstanceToDDMMMYYYY } from '@/utils/dateUtils';
  import { ref } from 'vue';

  defineProps<{
    data: ReservedJudgement[];
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
      value: (item: ReservedJudgement) =>
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
