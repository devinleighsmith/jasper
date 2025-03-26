<template>
  <v-card variant="text" class="my-3 pb-5">
    <v-row>
      <v-col cols="12">
        <v-chip
          rounded="lg"
          color="#d9e5f4"
          variant="flat"
          class="w-100 justify-center align-center"
        >
          {{ accused.lastNm.toUpperCase() }},
          {{ accused.givenNm.toUpperCase() }}
        </v-chip>
      </v-col>
    </v-row>

    <v-row class="mx-1 mt-2">
      <v-col cols="6" class="data-label">Ban</v-col>
      <!-- Currently grabbing the first one only? -->
      <v-col>
        <div v-for="(ban, index) in accused.ban" :key="index">
          <span style="color: #e30e0e">
            {{ ban.banTypeDescription }}
            ({{
              accused.ban.filter(
                (bnt) => bnt.banTypeDescription === ban.banTypeDescription
              ).length
            }})
          </span>
          <v-icon
            v-if="index === 0"
            class="ml-1"
            color="primary"
            :icon="mdiInformationSlabCircleOutline"
            @click="showBanModal = true"
          />
        </div>
      </v-col>
    </v-row>
    <v-row class="mx-1 mt-2">
      <v-col cols="6" class="data-label">DOB</v-col>
      <v-col>{{ dateOfBirth }}</v-col>
    </v-row>
    <v-row class="mx-1 mt-2">
      <v-col cols="6" class="data-label">Counsel</v-col>
      <v-col>{{ accused.counselLastNm }}</v-col>
    </v-row>
    <v-row class="mx-1 mt-2">
      <v-col cols="6" class="data-label">Counsel Desig. Filed</v-col>
      <v-col>{{ accused.designatedCounselYN }}</v-col>
    </v-row>
    <v-row class="mx-1 mt-2">
      <v-col cols="6" class="data-label">Bail status</v-col>
      <v-col>Test-Data</v-col>
    </v-row>
    <v-row class="mx-1 mt-2">
      <v-col cols="6" class="data-label">Plea</v-col>
      <v-col>Test-Data</v-col>
    </v-row>
    <v-row class="mx-1 mt-2">
      <v-col cols="6" class="data-label">Appearances</v-col>
      <v-col>{{ appearances.length }}</v-col>
    </v-row>
  </v-card>
  <Ban v-if="showBanModal" :bans="accused.ban" v-model="showBanModal" />
</template>

<script setup lang="ts">
  import { criminalParticipantType } from '@/types/criminal/jsonTypes';
  import { mdiInformationSlabCircleOutline } from '@mdi/js';
  import { defineProps, ref } from 'vue';
  import Ban from './Ban.vue';

  const props = defineProps<{
    accused: criminalParticipantType;
    appearances: any;
  }>();

  const showBanModal = ref(false);
  const dateOfBirth = props.accused?.birthDt
    ? new Date(props.accused.birthDt)
        .toLocaleDateString('en-GB', {
          day: '2-digit',
          month: 'short',
          year: 'numeric',
        })
        .replace(/ /g, '-')
    : '';


  console.log(props.accused);
  console.log(props.appearances);
</script>
