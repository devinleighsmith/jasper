<template>
  <Summary :details />
</template>

<script setup lang="ts">
  import { criminalFileDetailsType } from '@/types/criminal/jsonTypes';
  import { ref } from 'vue';
  import Summary from './Summary.vue';

  const props = defineProps<{ details: criminalFileDetailsType }>();

  const details = ref(props.details);
</script>
