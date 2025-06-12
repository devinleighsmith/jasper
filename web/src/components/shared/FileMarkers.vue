<template>
  <div>
    <v-chip
      v-for="{ key, description, notes } in data"
      :key
      rounded="lg"
      variant="outlined"
      size="small"
      :class="classOverride"
    >
      {{ key }}
      <v-tooltip
        v-if="(notes && notes.length > 0) || description"
        activator="parent"
        location="bottom"
      >
        <div v-if="notes" class="d-flex flex-column">
          <div v-for="(item, index) in notes" :key="index">
            {{ item }}
          </div>
        </div>
        <div v-else-if="description">
          {{ description }}
        </div>
      </v-tooltip>
    </v-chip>
  </div>
</template>

<script setup lang="ts">
  import { FileMarkerEnum } from '@/types/common';

  const props = defineProps<{
    classOverride: string;
    markers: { marker: FileMarkerEnum; value: string; notes?: string[] }[];
  }>();

  const allMarkers = [
    { marker: FileMarkerEnum.ADJ, description: '' },
    { marker: FileMarkerEnum.CNT, description: 'Continuation' },
    { marker: FileMarkerEnum.CPA, description: 'Child Protection Act' },
    { marker: FileMarkerEnum.CSO, description: 'Conditional Sentence Order' },
    { marker: FileMarkerEnum.DO, description: 'Detention Order' },
    { marker: FileMarkerEnum.IC, description: 'In Custody' },
    { marker: FileMarkerEnum.INT, description: 'Interpreter Required' },
    { marker: FileMarkerEnum.LOCT, description: 'Lack of Court Time' },
    { marker: FileMarkerEnum.OTH, description: '' },
    { marker: FileMarkerEnum.W, description: 'Warrant Issued' },
  ];
  const data = props.markers
    .filter((m) => m.value === 'Y')
    .map((m) => ({
      key: Object.keys(FileMarkerEnum).find(
        (k) => FileMarkerEnum[k as keyof typeof FileMarkerEnum] === m.marker
      ),
      notes: m.notes,
      description: allMarkers[m.marker]?.description,
    }));
</script>

<style scoped>
  .v-chip {
    cursor: default;
  }
</style>
