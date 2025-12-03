<template>
  <div>
    <template
      v-for="(group, courtClass) in groupedSelections"
      :key="courtClass"
    >
      <ActionBar
        :selected="group"
        :selectionPrependText="courtClass + ' file/s'"
      >
        <template #default>
          <v-btn
            size="large"
            class="mx-2"
            :prepend-icon="mdiFileDocumentOutline"
            style="letter-spacing: 0.001rem"
            data-testid="view-case-details"
            @click="() => onViewApprCaseDetails(group)"
          >
            View case details
          </v-btn>
          <v-btn
            v-if="isCourtClassLabelCriminal(courtClass)"
            size="large"
            class="mx-2"
            :prepend-icon="mdiFileDocumentMultipleOutline"
            style="letter-spacing: 0.001rem"
            data-testid="view-key-documents"
            @click="() => onViewKeyDocuments(group)"
          >
            View key documents
          </v-btn>
          <v-btn
            v-else
            size="large"
            class="mx-2"
            :prepend-icon="mdiFolderEyeOutline"
            style="letter-spacing: 0.001rem"
            data-testid="view-judicial-binders"
            :disabled="binderLoading || totalBinderCount === 0"
            @click="() => onViewJudicialBinders(group)"
          >
            View judicial binder(s)&nbsp;
              <v-progress-circular
                v-if="binderLoading"
                  indeterminate
                  size="18"
                  width="2"
                  color="primary"
                  class="mr-2 align-middle"
                />
                <span v-else-if="totalBinderCount > 0">
                  ({{ totalBinderCount }} / {{ selected.length }})
                </span>
          </v-btn>
        </template>
      </ActionBar>
    </template>
  </div>
</template>

<script setup lang="ts">
  import ActionBar from '@/components/shared/table/ActionBar.vue';
  import { CourtListAppearance } from '@/types/courtlist';
  import { getCourtClassLabel, isCourtClassLabelCriminal } from '@/utils/utils';
  import {
    mdiFileDocumentMultipleOutline,
    mdiFileDocumentOutline,
    mdiFolderEyeOutline
  } from '@mdi/js';
  import { computed, inject, ref, watch } from 'vue';
  import { BinderService } from '@/services';
  import { useCommonStore } from '@/stores';

  const props = defineProps<{
    selected: CourtListAppearance[];
  }>();

  const binderService = inject<BinderService>('binderService');
  const commonStore = useCommonStore();

  if (!binderService) {
    throw new Error('Service is undefined.');
  }

  const emit = defineEmits<{
    (e: 'view-case-details', appearances: CourtListAppearance[]): void;
    (e: 'view-key-documents', appearances: CourtListAppearance[]): void;
    (e: 'unique-civil-file-selected', appearance: CourtListAppearance): void;
    (e: 'view-judicial-binders', appearances: CourtListAppearance[]): void;
  }>();

  const previousCivilFileIds = new Set<string>();
  const binderCounts = ref<Record<string, number>>({});
  const binderRequests = ref<Promise<any>[]>([]);
  const binderLoading = computed(() => binderRequests.value.length > 0);

  const groupedSelections = computed(() => {
    const groups: Record<string, CourtListAppearance[]> = {};
    const currentCivilFileIds = new Set<string>();
    
    for (const item of props.selected) {
      const group = getCourtClassLabel(item.courtClassCd);
      if (!groups[group]) {
        groups[group] = [];
      }
      groups[group].push(item);

      // Track civil/family appearances by physicalFileId
      if (!isCourtClassLabelCriminal(group) && item.physicalFileId) {
        currentCivilFileIds.add(item.physicalFileId);
        
        // Emit event if this is a new unique physicalFileId
        if (!previousCivilFileIds.has(item.physicalFileId)) {
          emit('unique-civil-file-selected', item);
        }
      }
    }

    // Update the previous set for next comparison
    previousCivilFileIds.clear();
    currentCivilFileIds.forEach(id => previousCivilFileIds.add(id));

    return groups;
  });

  // Watch for changes in selected civil/family files and fetch binder counts
  watch(
    () => props.selected,
    async (newSelected, oldSelected) => {
      const getCivilFiles = (items: CourtListAppearance[]) =>
        items.filter((item) => 
          !isCourtClassLabelCriminal(getCourtClassLabel(item.courtClassCd)) && 
          item.physicalFileId
        );

      const civilFiles = getCivilFiles(newSelected);
      const isSelection = newSelected.length > (oldSelected?.length || 0);

      if (civilFiles.length === 0) {
        binderCounts.value = {};
        return;
      }
      if(!isSelection) {
        // No need to fetch if all files are already tracked
        const currentFileIds = new Set(civilFiles.map((f) => f.physicalFileId));
        binderCounts.value = Object.fromEntries(
          Object.entries(binderCounts.value).filter(([fileId]) => currentFileIds.has(fileId))
        );
        return;
      }
      // Fetch binder count for newly selected file
      const fileId = newSelected[newSelected.length - 1].physicalFileId;
      const appearance = civilFiles.find((f) => f.physicalFileId === fileId);
      if(!appearance) {
        return;
      }
      const labels = {
        physicalFileId: appearance.physicalFileId,
        courtClassCd: appearance.courtClassCd,
        judgeId: commonStore.userInfo?.userId,
      };

      try {
        const request = binderService.getBinders(labels).then((response) => {
            binderCounts.value[fileId] = response.succeeded && response.payload 
              ? response.payload.length 
              : 0;
          }).finally(() => {
            binderRequests.value = binderRequests.value.filter(r => r !== request);
          });
        binderRequests.value.push(request);
      } catch (error) {
        console.error('Error fetching binders:', error);
        binderCounts.value[fileId] = 0;
      }
    },
    { immediate: true }
  );

  const totalBinderCount = computed(() => {
    return Object.values(binderCounts.value).reduce((sum, count) => sum + count, 0);
  });

  const onViewApprCaseDetails = (appearances: CourtListAppearance[]) => {
    emit('view-case-details', appearances);
  };
  const onViewKeyDocuments = (appearances: CourtListAppearance[]) => {
    emit('view-key-documents', appearances);
  };
  const onViewJudicialBinders = (appearances: CourtListAppearance[]) => {
    emit('view-judicial-binders', appearances);
  };
</script>