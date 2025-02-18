<template>
  <v-card class="pa-md-4 mx-lg-auto mb-3" color="#efedf5">
    <v-row>
      <v-col>
        <h3>{{ courtListLocation }}</h3>
      </v-col>
    </v-row>
    <v-row>
      <v-col>
        <h5>Rooms: {{ courtListRoom }} ({{ amPM }})</h5>
      </v-col>
      <v-col>
        <h5>Presider: {{ presider }}</h5>
      </v-col>
      <v-col>
        <h5>Court clerk: {{ courtClerk }}</h5>
      </v-col>
      <v-col>
        <v-btn-secondary> View shared folder </v-btn-secondary>
      </v-col>
    </v-row>
    <v-row>
      <v-col>
        <h5>Activity: {{ activity }}</h5>
      </v-col>
      <v-col>
        <h5>Scheduled: {{ fileCount }} files</h5>
      </v-col>
      <v-col>
        <h5>
          <a href="#">{{ email }}</a>
        </h5>
      </v-col>
      <v-col>
        <h5><a href="#">See more about this location</a></h5>
      </v-col>
    </v-row>
  </v-card>
</template>

<script setup lang="ts">
  const props = defineProps({
    courtListLocation: {
      type: String,
      required: true,
    },
    courtListLocationID: {
      type: String,
      required: true,
    },
    courtListRoom: {
      type: String,
      required: true,
    },
    activity: {
      type: String,
      required: true,
    },
    amPM: {
      type: String,
      required: true,
    },
    fileCount: {
      type: Number,
      required: true,
    },
    presider: {
      type: String,
      required: true,
    },
    courtClerk: {
      type: String,
      default: 'Not scheduled',
    },
    email: {
      type: String,
      required: true,
    },
  });
</script>
