<template>
  <v-menu>
    <template v-slot:activator="{ props }">
      <v-btn
        color="info"
        :icon="mdiDotsVertical"
        variant="text"
        v-bind="props"
      />
    </template>

    <v-list>
      <v-list-item v-for="(item, i) in menuItems" :key="i" :value="i">
        <v-list-item-title>{{ item.title }}</v-list-item-title>
      </v-list-item>
    </v-list>
  </v-menu>
</template>

<script setup lang="ts">
  import { mdiDotsVertical } from '@mdi/js';
  defineProps<{
    menuItems: any[];
  }>();
</script>
