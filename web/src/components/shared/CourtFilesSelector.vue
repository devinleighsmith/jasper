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
        <template v-for="(tab, index) in tabs.items" :key="tab.key">
          <v-tab
            class="text-body-1 mb-0"
            :class="{ 'bg-white': isActive(index) }"
            rounded="t-lg"
            :ripple="false"
            :to="tab.key"
            hide-slider
            active-color="yellow"
            base-color="white"
            color="black"
            @click="fileNumber = tab.key"
            >{{ tab.value }}</v-tab
          >
          <v-divider class="ms-2" inset vertical thickness="2"></v-divider>
        </template>
        <!-- <template v-for="file in props.files" :key="file.key">
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
        </template> -->
      </v-tabs>
    </v-container>
  </v-card>
</template>
<script setup lang="ts">
  import { KeyValueInfo } from '@/types/common';
  import { defineProps, PropType, computed, ref, onMounted, watch } from 'vue';

  const fileNumber = defineModel<string>();
  const props = defineProps({
    files: { type: Array as PropType<KeyValueInfo[]>, default: () => [] },
  });
  const activeTab = ref(() => fileNumber.value);
  const tabs = ref({ items: [] });

  const isActive = (id) => {
    return activeTab.value === id;
  };

  const setActive = (activeTabIndex) => {
    tabs.items[activeTabIndex].isActive = true;
  };

  onMounted(() => {
    props.files.forEach((file, index) => {
      tabs.value.items.push({
        key: file.key,
        value: file.value,
        isActive: activeTab.value === file.key,
      });
    });
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
