<template>
  <v-navigation-drawer v-model="model" location="right" temporary>
    <v-list-item color="primary" rounded="shaped">
      <template v-slot:prepend>
        <v-icon :icon="mdiAccountCircle" size="35" />
      </template>
      <v-list-item-title>{{ userName }}</v-list-item-title>
      <template v-slot:append>
        <v-btn size="25" :icon="mdiCloseCircle" @click="model = false" />
      </template>
    </v-list-item>

    <v-divider></v-divider>

    <v-list density="compact" v-model:opened="openedGroups">
      <QuickLinksMenu />
    </v-list>

    <template v-slot:append>
      <v-list-item color="primary" rounded="shaped">
        <template v-slot:prepend>
          <v-icon :icon="mdiWeatherNight"></v-icon>
        </template>
        <v-list-item-title>Dark mode</v-list-item-title>
        <template v-slot:append>
          <v-switch v-model="isDark" hide-details @click="toggleDark" />
        </template>
      </v-list-item>
    </template>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
  import QuickLinksMenu from '@/components/shared/QuickLinksMenu.vue';
  import { useCommonStore } from '@/stores';
  import { useThemeStore } from '@/stores/ThemeStore';
  import { UserInfo } from '@/types/common';
  import { mdiAccountCircle, mdiCloseCircle, mdiWeatherNight } from '@mdi/js';
  import { ref, watch } from 'vue';

  const model = defineModel<boolean>();
  const themeStore = useThemeStore();
  const theme = ref(themeStore.state);
  const isDark = ref(theme.value === 'dark');
  const openedGroups = ref(['quick-links']); // Expand Quick links by default

  const commonStore = useCommonStore();
  const userName = ref<string>(commonStore.userInfo?.userTitle || '');

  function toggleDark() {
    themeStore.changeState(theme.value === 'dark' ? 'light' : 'dark');
  }

  watch(
    () => commonStore.userInfo,
    (newUserInfo: UserInfo | null) => {
      if (!newUserInfo) {
        return;
      }
      userName.value = newUserInfo.userTitle || '';
    }
  );
</script>

<style>
  div.v-list-item__spacer {
    width: 15px !important;
  }
  .v-list-item-title {
    white-space: normal;
    word-break: break-word;
  }
</style>
