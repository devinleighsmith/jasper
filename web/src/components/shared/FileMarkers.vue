<template>
  <div>
    <v-chip
      v-for="marker in markers"
      :key="marker.name"
      selected-class="selected"
      rounded="lg"
      variant="outlined"
      size="small"
      class="ml-1 mt-1"
      :class="{ selected: marker.selected }"
    >
      {{ marker.name }}
      <v-tooltip activator="parent" location="bottom">
        {{ marker.description }}
      </v-tooltip>
    </v-chip>
  </div>
</template>

<script setup lang="ts">
  import { criminalApprDetailType } from '@/types/criminal/jsonTypes';
  import { defineProps, onMounted, ref } from 'vue';

  const props = defineProps<{
    division: string;
    participants: any[];
    appearances: criminalApprDetailType[];
  }>();

  const markers = ref<any>([]);
  const selection = ref<string[]>([]);
  const markerData = ref([{ name: 'CNT', description: 'Continuation' }]);
  const criminalMarkers = [
    { name: 'IC', description: 'In Custody' },
    { name: 'DO', description: 'Detained Order' },
    { name: 'INT', description: 'Interpreter Required' },
  ];
  const familyMarkers = [
    {
      name: 'CFCSA',
      description: 'Child Matters',
    },
  ];

  if (props.division === 'Criminal') {
    markerData.value = [...markerData.value, ...criminalMarkers];
  } else if (props.division === 'Family') {
    markerData.value = [...markerData.value, ...familyMarkers];
  }

  onMounted(() => {
    const participantFlags = {
      W: 'warrantYN',
      DO: 'detainedYN',
      IC: 'inCustodyYN',
      INT: 'interpreterYN',
    };
    const appearanceFlags = {
      CNT: 'continuationYn',
    };
    // Match participant flags
    selection.value = Object.keys(participantFlags)
      .filter((key) =>
        props.participants?.some((p) => p[participantFlags[key]] === 'Y')
      )
      // Match appearance flags
      .concat(
        Object.keys(appearanceFlags).filter((key) =>
          props.appearances?.some((app) => app[appearanceFlags[key]] === 'Y')
        )
      );

    markers.value = markerData.value.map((marker) => ({
      ...marker,
      selected: selection.value.includes(marker.name),
    }));
});
</script>

<style scoped>
  .v-chip {
    cursor: default;
  }
  .selected {
    background-color: #183a4a !important;
    color: white !important;
  }
</style>
