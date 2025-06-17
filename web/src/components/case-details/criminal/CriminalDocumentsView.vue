<template>
  <v-row>
    <v-col cols="6" />
    <v-col cols="3" class="ml-auto" v-if="documentCategories.length > 1">
      <v-select
        v-model="selectedCategory"
        label="Documents"
        placeholder="All documents"
        hide-details
        :items="documentCategories"
      >
        <template v-slot:item="{ props: itemProps, item }">
          <v-list-item
            v-bind="itemProps"
            :title="item.raw + ' (' + categoryCount(item.raw) + ')'"
          ></v-list-item>
        </template>
      </v-select>
    </v-col>
    <v-col cols="3" class="ml-auto" v-if="participants.length > 1">
      <name-filter v-model="selectedAccused" :people="participants" />
    </v-col>
  </v-row>
  <div
    v-for="(documents, type) in {
      // keyDocuments: keyDocuments,
      documents: documents,
    }"
    :key="type"
  >
    <v-card
      class="my-6"
      color="var(--bg-gray-500)"
      elevation="0"
      v-if="unfilteredDocuments?.length > 0"
    >
      <v-card-text>
        <v-row align="center" no-gutters>
          <v-col class="text-h5" cols="6"
            >All Documents ({{ documents.length }})</v-col
          >
        </v-row>
      </v-card-text>
    </v-card>
    <v-data-table-virtual
      v-if="documents?.length"
      v-model="selectedItems"
      :headers="headers"
      :items="documents"
      :sort-by="sortBy"
      :group-by
      show-select
      return-object
      class="my-3"
      height="400"
    >
      <template
        v-slot:group-header="{ item, columns, isGroupOpen, toggleGroup }"
      >
        <tr>
          <td class="pa-0" style="height: 1rem" :colspan="columns.length">
            <v-banner
              class="table-banner"
              :ref="
                () => {
                  if (!isGroupOpen(item)) toggleGroup(item);
                }
              "
            >
              {{ formatFromFullname(item.value) }}
            </v-banner>
          </td>
        </tr>
      </template>
      <template v-slot:item.category="{ item }">
        {{ formatCategory(item) }}
      </template>
      <template v-slot:item.documentTypeDescription="{ item }">
        <a
          v-if="item.imageId"
          href="javascript:void(0)"
          @click="openIndividualDocument(item)"
        >
          {{ formatType(item) }}
        </a>
        <span v-else>
          {{ formatType(item) }}
        </span>
      </template>
    </v-data-table-virtual>
  </div>
  <ActionBar v-if="showActionbar" :selected="selectedItems">
    <v-btn
      size="large"
      class="mx-2"
      :prepend-icon="mdiFileDocumentMultipleOutline"
      style="letter-spacing: 0.001rem"
      @click="openMergedDocuments()"
    >
      View together
    </v-btn>
  </ActionBar>
</template>
<script setup lang="ts">
  import {
    getCriminalDocumentType,
    prepareCriminalDocumentData,
  } from '@/components/documents/DocumentUtils';
  import shared from '@/components/shared';
  import NameFilter from '@/components/shared/Form/NameFilter.vue';
  import ActionBar from '@/components/shared/table/ActionBar.vue';
  import {
    criminalParticipantType,
    documentType,
  } from '@/types/criminal/jsonTypes';
  import { CourtDocumentType, DocumentData } from '@/types/shared';
  import { formatDateToDDMMMYYYY } from '@/utils/dateUtils';
  import { formatFromFullname } from '@/utils/utils';
  import { mdiFileDocumentMultipleOutline } from '@mdi/js';
  import { computed, ref } from 'vue';

  const props = defineProps<{ participants: criminalParticipantType[] }>();
  const selectedItems = ref<documentType[]>([]);
  const showActionbar = computed<boolean>(
    () => selectedItems.value.filter((item) => item.imageId).length > 1
  );
  const sortBy = ref([{ key: 'issueDate', order: 'desc' }] as const);
  const selectedCategory = ref<string>();
  const selectedAccused = ref<string>();

  const formatCategory = (item: documentType) =>
    item.category === 'rop' ? 'ROP' : item.category;
  const formatType = (item: documentType) =>
    item.category === 'rop'
      ? 'Record of Proceedings'
      : item.documentTypeDescription;

  const filterByCategory = (item: any) =>
    !selectedCategory.value ||
    item.category?.toLowerCase() === selectedCategory.value?.toLowerCase();
  const filterByAccused = (item: any) =>
    !selectedAccused.value ||
    (item.fullName &&
      formatFromFullname(item.fullName) === selectedAccused.value);

  const unfilteredDocuments = computed(
    () =>
      props.participants?.flatMap((participant) =>
        participant.document?.map((doc) => ({
          ...doc,
          fullName: participant.fullName || '',
          profSeqNo: participant.profSeqNo,
          id: crypto.randomUUID(),
        }))
      ) || []
  );

  const documents = computed(() =>
    unfilteredDocuments.value.filter(filterByCategory).filter(filterByAccused)
  );

  const categoryCount = (category: string): number => {
    return unfilteredDocuments.value.filter(
      (doc) => formatCategory(doc) === category
    ).length;
  };

  const documentCategories = ref<string[]>([
    ...new Set(
      unfilteredDocuments.value
        ?.filter((doc) => doc.category)
        .map((doc) => formatCategory(doc)) || []
    ),
  ]);

  const groupBy = ref([
    {
      key: 'fullName',
      order: 'asc' as const,
    },
  ]);
  const headers = [
    { key: 'data-table-group' },
    {
      title: 'DATE SWORN/FILED',
      key: 'issueDate',
      value: (item) => formatDateToDDMMMYYYY(item.issueDate),
      sortRaw: (a: documentType, b: documentType) =>
        new Date(a.issueDate).getTime() - new Date(b.issueDate).getTime(),
    },
    {
      title: 'DOCUMENT TYPE',
      key: 'documentTypeDescription',
    },
    {
      title: 'CATEGORY',
      key: 'category',
    },
    {
      title: 'PAGES',
      key: 'documentPageCount',
    },
  ];

  const openIndividualDocument = (data: documentType) =>
    shared.openDocumentsPdf(
      getCriminalDocumentType(data),
      prepareCriminalDocumentData(data)
    );

  const openMergedDocuments = () => {
    const documents: [CourtDocumentType, DocumentData][] = [];
    selectedItems.value
      .filter((item) => item.imageId)
      .forEach((item) => {
        const documentType = getCriminalDocumentType(item);
        const documentData = prepareCriminalDocumentData(item);
        documents.push([documentType, documentData]);
      });

    shared.openMergedDocumentsPdf(documents);
  };
</script>
