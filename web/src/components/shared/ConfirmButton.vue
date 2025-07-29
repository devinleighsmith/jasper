<template>
  <v-btn-secondary @click="show = true"> {{ buttonText }} </v-btn-secondary>
  <v-dialog v-model="show" width="auto">
    <v-card max-width="400" :text="infoText" :title="buttonText" class="p-3">
      <v-card-actions class="justify-start">
        <v-btn-tertiary :text="confirmText" @click="confirmAction()" />
        <v-btn-secondary text="No, cancel" @click="show = false" />
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
  import { ref } from 'vue';

  const props = defineProps<{
    buttonText: string;
    infoText: string;
    confirmText: string;
    confirmAction: () => void;
  }>();

  const show = ref(false);

  const confirmAction = () => {
    show.value = false;
    props.confirmAction();
  };
</script>
