<template>
  <v-tooltip :text="text" location="top">
    <template v-slot:activator="{ props }">
      <v-icon v-bind="props" :icon="icon" color="primary" size="large" />
    </template>
  </v-tooltip>
</template>

<script setup lang="ts">
  defineProps<{
    icon: string;
    text: string;
  }>();
</script>
