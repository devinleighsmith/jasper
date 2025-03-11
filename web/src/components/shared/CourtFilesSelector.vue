<template>
  <v-card color="#72acca" variant="flat">
    <v-container class="pb-0">
      <v-row>
        <v-col cols="10" />
        <v-col>
          <v-btn base-color="white">View all documents</v-btn>
        </v-col>
      </v-row>
      <v-tabs v-model="activeTab">
        <template v-for="file in props.files" :key="file.key">
          <v-tab
            class="text-body-1 mb-0"
            :class="{ 'bg-white': activeTab === file.key }"
            rounded="t-lg"
            :ripple="false"
            :to="file.key"
            hide-slider
            active-color="yellow"
            base-color="white"
            color="black"
            @click="fileNumber = file.key"
            >{{ file.value }}</v-tab
          >
          <v-divider class="ms-2" inset vertical thickness="2"></v-divider>
        </template>
      </v-tabs>
    </v-container>
  </v-card>
</template>
<script setup lang="ts">
  import { KeyValueInfo } from '@/types/common';
  import { defineProps, PropType, ref, watch } from 'vue';

  const fileNumber = defineModel<string>();
  const props = defineProps({
    files: { type: Array as PropType<KeyValueInfo[]>, default: () => [] },
  });
  const activeTab = ref(fileNumber.value);

  watch(fileNumber, (newVal) => {
    console.log(newVal);
    activeTab.value = newVal;
  });
</script>

<style>
  /* .active-tab {
    background-color: white !important;
  }
  .active {
    color: #222222 !important;
    background-color: #ffff !important;
  } */
</style>
