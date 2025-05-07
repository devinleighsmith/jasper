<template>
  <v-data-table-virtual
    v-if="true"
    class="partyTable"
    :headers
    :items="parties"
    item-value="appearanceId"
  >
    <template v-slot:item.role="{ item }">
      <span v-for="(role, index) in item.partyRole" :key="index">
        <span v-if="role.roleTypeDsc">
          {{ role.roleTypeDsc }}
        </span>
      </span>
    </template>
  </v-data-table-virtual>
</template>

<script setup lang="ts">
  import { PartyDetails } from '@/types/civil/jsonTypes/index';

  const props = defineProps<{
    parties: PartyDetails[];
  }>();
  console.log(props.parties);
  const headers = [
    { title: 'NAME', key: 'fullName' },
    { title: 'ROLE', key: 'role' },
    { title: 'CURRENT COUNSEL', key: 'counsel' },
  ];
  //   [
  //     {
  //         "roleTypeCd": "APP",
  //         "roleTypeDsc": "Applicant"
  //     }
  // ]
</script>

<style scoped>
  .partyTable {
    background-color: var(--bg-light-gray) !important;
    padding-bottom: 2rem !important;
  }
</style>
