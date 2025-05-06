<template>
    <v-data-table-virtual
    v-if="true"
    :headers
    :items
    :sort-by="sortBy"
    class="pb-3"
    item-value="appearanceId"
    >
    </v-data-table-virtual>
</template>

<script setup lang="ts">
  import { ref } from 'vue';

  const tab = ref('one');

  const headers = [
    { title: 'SEQ', key: 'title', align: 'start', sortable: true },
    { title: 'DOCUMENT TYPE', key: 'director' },
    { title: 'ACT', key: 'genre' },
    { title: 'DATE FILED', key: 'year', align: 'end' },
    { title: 'FILED BY', key: 'runtime', align: 'end' },
    { title: 'RESULTS', key: 'runtime', align: 'end' },
    { title: 'ISSUES', key: 'runtime', align: 'end' },
  ];
  const items = [
    {
      title: 'The Shawshank Redemption',
      director: 'Frank Darabont',
      genre: 'Drama',
      year: 1994,
      runtime: 142,
      details: {
        synopsis:
          'Two imprisoned men bond over years, finding solace and redemption through acts of decency.',
        cast: ['Tim Robbins', 'Morgan Freeman'],
        rating: 3.5,
      },
    },
  ];
</script>
