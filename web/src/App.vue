<template>
  <v-app>
    <v-app-bar app>
      <v-app-bar-title>
        <router-link to="/"> JASPER </router-link>
      </v-app-bar-title>
      <v-tabs align-tabs="start">
        <v-tab to="/dashboard">Dashboard</v-tab>
        <v-tab to="/court-list">Court list</v-tab>
        <v-tab to="/court-file-search">Court file search</v-tab>
      </v-tabs>
    </v-app-bar>
    <v-main>
      <router-view />
    </v-main>
  </v-app>
</template>

<script lang="ts">
  import { defineComponent } from 'vue';

  export default defineComponent({
    name: 'App',
  });
</script>

<style>
  .v-tabs {
    flex: 10;
  }
  .v-app-bar-title a {
    text-decoration: none;
    color: inherit;
  }
</style>
