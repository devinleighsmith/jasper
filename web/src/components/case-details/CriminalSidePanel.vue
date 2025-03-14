<template>
  <h5 class="my-1">Case Details</h5>
  <v-card color="#efedf5">
    <division-badge :activityClassDesc division="Criminal" class="mx-3 my-3" />
    <v-card-title>{{ names }}</v-card-title>
    <v-card-subtitle>{{ location }}</v-card-subtitle>
    <file-markers
      :restrictions="details.restrictions"
      :participants
      :appearances
      division="Criminal"
      class="mx-3 mt-2"
    />
    <v-row class="mx-1 my-1">
      <v-col cols="6">Elections</v-col>
      <v-col>PCJ-Summary</v-col>
    </v-row>
    <v-row class="mx-1 my-1">
      <v-col cols="6">Crown</v-col>
      <v-col>{{ crownName }}</v-col>
    </v-row>
    <v-row class="mx-1 my-1">
      <v-col cols="6">Case Age</v-col>
      <v-col>{{ details.caseAgeDays }} days</v-col>
    </v-row>
  </v-card>
</template>

<script setup lang="ts">
  import { ref, computed } from 'vue';
  import { criminalFileDetailsType } from '@/types/criminal/jsonTypes';
  import DivisionBadge from './DivisionBadge.vue';
  import FileMarkers from '@/components/shared/FileMarkers.vue';

  const props = defineProps<{ details: criminalFileDetailsType }>();

  const details = ref(props.details);
  const participants = ref(details.value.participant);
  const appearances = ref(details.value.appearances.apprDetail);
  const names = computed(() => {
    return (
      participants.value[0].lastNm.toUpperCase() +
      ', ' +
      participants.value[0].givenNm +
      (participants.value.length > 1
        ? ` and ${participants.value.length - 1} other`
        : '')
    );
  });
  const hearingRestriction = details.value.hearingRestriction;
  const activityClassDesc = details.value.activityClassDesc;
  const location = details.value.homeLocationAgencyName;
  const crownAssigned = details.value.crown?.filter((c) => c.assigned)[0];
  const crownName = crownAssigned
    ? crownAssigned.lastNm + ', ' + crownAssigned.givenNm
    : '';
  console.log(details.value);
</script>
