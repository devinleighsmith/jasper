<template>
  <div>
    <template
      v-for="(group, courtClass) in groupedSelections"
      :key="courtClass"
    >
      <ActionBar
        :selected="group"
        :selectionPrependText="courtClass + ' file/s'"
      >
        <template #default>
          <v-btn
            size="large"
            class="mx-2"
            :prepend-icon="mdiFileDocumentMultipleOutline"
            style="letter-spacing: 0.001rem"
          >
            View document bundle
          </v-btn>
          <v-btn
            size="large"
            class="mx-2"
            :prepend-icon="mdiFileDocumentOutline"
            style="letter-spacing: 0.001rem"
          >
            View case details
          </v-btn>
        </template>
      </ActionBar>
    </template>
  </div>
</template>

<script setup lang="ts">
  import ActionBar from '@/components/shared/table/ActionBar.vue';
  import { CourtClassEnum } from '@/types/common';
  import { CourtListAppearance } from '@/types/courtlist';
  import { getEnumName } from '@/utils/utils';
  import {
    mdiFileDocumentMultipleOutline,
    mdiFileDocumentOutline,
  } from '@mdi/js';
  import { computed } from 'vue';

  const props = defineProps<{
    selected: CourtListAppearance[];
  }>();

  const groupedSelections = computed(() => {
    const groups: Record<string, CourtListAppearance[]> = {};
    for (const item of props.selected) {
      const group = getCourtClass(item.courtClassCd);
      if (!groups[group]) {
        groups[group] = [];
      }
      groups[group].push(item);
    }
    return groups;
  });

  /**
   * Retrieves the CSS class name to represent a CourtClass
   * @param courtClassCd The court class code
   * @returns class name
   */
  const getCourtClass = (courtClassCd: string): string => {
    switch (courtClassCd) {
      case getEnumName(CourtClassEnum, CourtClassEnum.A):
        return 'Criminal - Adult';
      case getEnumName(CourtClassEnum, CourtClassEnum.Y):
        return 'Youth';
      case getEnumName(CourtClassEnum, CourtClassEnum.T):
        return 'Tickets';
      case getEnumName(CourtClassEnum, CourtClassEnum.C):
      case getEnumName(CourtClassEnum, CourtClassEnum.L):
      case getEnumName(CourtClassEnum, CourtClassEnum.M):
        return 'Small Claims';
      case getEnumName(CourtClassEnum, CourtClassEnum.F):
        return 'Family';
      default:
        return 'Unknown';
    }
  };
</script>

<style scoped>
  :deep(.action-bar .header) {
    bottom: 50px !important;
  }
</style>
