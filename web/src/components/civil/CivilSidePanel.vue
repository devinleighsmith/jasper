<template>
  <v-card class="ma-2" elevation="0">
    <v-list density="compact" bg-color="white">
      <v-list-subheader class="text-caption font-weight-bold">
        ON THIS FILE
      </v-list-subheader>
      <v-list-item
        v-for="(panelItem, index) in panelItems"
        :key="index"
        @click="SelectPanelItem(panelItem)"
        :active="isActive(panelItem)"
        color="primary"
      >
        <v-list-item-title>{{ panelItem }}</v-list-item-title>
      </v-list-item>
    </v-list>
  </v-card>
</template>

<script lang="ts">
  import { useCivilFileStore } from '@/stores';
  import { defineComponent } from 'vue';

  export default defineComponent({
    setup() {
      const civilFileStore = useCivilFileStore();

      const panelItems = [
        'Case Details',
        'Future Appearances',
        'Past Appearances',
        'All Documents',
        'Documents',
        'Provided Documents',
      ];

      const SelectPanelItem = (panelItem) => {
        const sections = civilFileStore.showSections;

        for (const item of panelItems) {
          if (item == panelItem) sections[item] = true;
          else sections[item] = false;
        }
        civilFileStore.updateShowSections(sections);
      };

      const isActive = (panelItem: string) => {
        return civilFileStore.showSections[panelItem] === true;
      };

      return {
        panelItems,
        SelectPanelItem,
        isActive,
      };
    },
  });
</script>
