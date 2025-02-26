<template>
  <v-card color="#efedf5" class="w-100 my-2">
    <v-card-title>
      <v-row>
        <v-col>
          <h3>{{ cardInfo.courtListLocation }}</h3>
        </v-col>
      </v-row>
    </v-card-title>
    <v-card-text>
      <v-row>
        <v-col>
          <h5>Rooms: {{ cardInfo.courtListRoom }} ({{ cardInfo.amPM }})</h5>
        </v-col>
        <v-col>
          <h5>Presider: {{ cardInfo.presider }}</h5>
        </v-col>
        <v-col>
          <h5>Court clerk: {{ cardInfo.courtClerk ?? 'Not scheduled' }}</h5>
        </v-col>
        <v-col>
          <v-btn-secondary> View shared folder </v-btn-secondary>
        </v-col>
      </v-row>

      <v-row>
        <v-col>
          <h5>Activity: {{ cardInfo.activity }}</h5>
        </v-col>
        <v-col>
          <h5>Scheduled: {{ cardInfo.fileCount }} files</h5>
        </v-col>
        <v-col>
          <h5>
            <a href="#">{{ cardInfo.email }}</a>
          </h5>
        </v-col>
        <v-col>
          <h5><a href="#">See more about this location</a></h5>
        </v-col>
      </v-row>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
  import { CourtListCardInfo } from '@/types/courtlist';
  import { PropType } from 'vue';

  defineProps({
    cardInfo: {
      type: Object as PropType<CourtListCardInfo>,
      required: true,
    },
    // courtListLocation: {
    //   type: String,
    //   required: true,
    // },
    // courtListLocationID: {
    //   type: String,
    //   required: true,
    // },
    // courtListRoom: {
    //   type: String,
    //   required: true,
    // },
    // activity: {
    //   type: String,
    //   required: true,
    // },
    // amPM: {
    //   type: String,
    //   required: true,
    // },
    // fileCount: {
    //   type: Number,
    //   required: true,
    // },
    // presider: {
    //   type: String,
    //   required: true,
    // },
    // courtClerk: {
    //   type: String,
    //   default: 'Not scheduled',
    // },
    // email: {
    //   type: String,
    //   default: '',
    // },
  });
</script>
