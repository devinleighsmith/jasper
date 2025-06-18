<template>
  <v-row>
    <v-col cols="6" />
    <v-col cols="3" class="ml-auto" v-if="documentTypes.length > 1">
      <v-select
        v-model="selectedType"
        label="Documents"
        placeholder="All documents"
        hide-details
        :items="documentTypes"
      >
        <template v-slot:item="{ props: itemProps, item }">
          <v-list-item
            v-bind="itemProps"
            :title="item.title + ' (' + typeCount(item.raw) + ')'"
          ></v-list-item>
        </template>
      </v-select>
    </v-col>
  </v-row>
  <div
    v-for="(documents, type) in {
      // binder: binder,
      documents: documents,
    }"
    :key="type"
  >
    <v-card
      class="my-6"
      color="var(--bg-gray-500)"
      elevation="0"
      v-if="documents?.length > 0"
    >
      <v-card-text>
        <v-row align="center" no-gutters>
          <v-col class="text-h5" cols="6">
            All Documents ({{ filteredDocuments.length }})
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>
    <v-data-table-virtual
      v-if="documents?.length"
      v-model="selectedItems"
      :headers="headers"
      :items="filteredDocuments"
      :sort-by="sortBy"
      return-object
      item-value="civilDocumentId"
      show-select
      class="my-3"
      height="400"
    >
      <template v-slot:item.documentTypeDescription="{ item }">
        <a
          v-if="item.imageId"
          href="javascript:void(0)"
          @click="openIndividualDocument(item)"
        >
          {{ item.documentTypeDescription }}
        </a>
        <span v-else>
          {{ item.documentTypeDescription }}
        </span>
      </template>
      <template v-slot:item.activity="{ item }">
        <v-chip-group>
          <div v-for="info in item.documentSupport" :key="info.actCd">
            <v-chip rounded="lg">{{ info.actCd }}</v-chip>
          </div>
        </v-chip-group>
      </template>
      <template v-slot:item.filedBy="{ item }">
        <span v-for="(role, index) in item.filedBy" :key="index">
          <span v-if="role.roleTypeCode">
            <v-skeleton-loader type="text" :loading="rolesLoading">
              {{
                roles ? getLookupShortDescription(role.roleTypeCode, roles) : ''
              }}
            </v-skeleton-loader>
          </span>
        </span>
      </template>
      <template v-slot:item.issue="{ item }">
        <LabelWithTooltip
          v-if="item.issue?.length > 0"
          :values="item.issue.map((issue) => issue.issueTypeDesc)"
          :location="Anchor.Top"
        />
      </template>
      <!-- <template v-slot:item.binderMenu>
        <EllipsesMenu :menuItems="menuItems" />
      </template> -->
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
    getCivilDocumentType,
    prepareCivilDocumentData,
  } from '@/components/documents/DocumentUtils';
  import shared from '@/components/shared';
  import LabelWithTooltip from '@/components/shared/LabelWithTooltip.vue';
  import ActionBar from '@/components/shared/table/ActionBar.vue';
  import { civilDocumentType } from '@/types/civil/jsonTypes';
  import { Anchor, LookupCode } from '@/types/common';
  import { CourtDocumentType, DocumentData } from '@/types/shared';
  import { formatDateToDDMMMYYYY } from '@/utils/dateUtils';
  import { getLookupShortDescription, getRoles } from '@/utils/utils';
  import { mdiFileDocumentMultipleOutline } from '@mdi/js';
  import { computed, onMounted, ref } from 'vue';

  const props = defineProps<{ documents: civilDocumentType[] }>();

  const selectedItems = ref<civilDocumentType[]>([]);
  const showActionbar = computed<boolean>(
    () => selectedItems.value.filter((item) => item.imageId).length > 1
  );
  const sortBy = ref([{ key: 'fileSeqNo', order: 'desc' }] as const);
  const selectedType = ref<string>();
  const menuItems = [{ title: 'Add to binder' }];
  const rolesLoading = ref(false);
  const roles = ref<LookupCode[]>();
  const headers = [
    { key: 'data-table-group' },
    {
      title: 'SEQ',
      key: 'fileSeqNo',
    },
    {
      title: 'DOCUMENT TYPE',
      key: 'documentTypeDescription',
    },
    {
      title: 'ACT',
      key: 'activity',
    },
    {
      title: 'DATE FILED',
      key: 'filedDt',
      value: (item) => formatDateToDDMMMYYYY(item.filedDt),
      sortRaw: (a: civilDocumentType, b: civilDocumentType) =>
        new Date(a.filedDt).getTime() - new Date(b.filedDt).getTime(),
    },
    {
      title: 'FILED BY',
      key: 'filedBy',
    },
    {
      title: 'ISSUES',
      key: 'issue',
    },
    // {
    //   title: 'JUDICIAL BINDER',
    //   key: 'binderMenu',
    //   width: '2%',
    // },
  ];
  const documentTypes = ref<any[]>([
    ...new Map(
      props.documents.map((doc) => [
        doc.documentTypeCd,
        { title: doc.documentTypeDescription, value: doc.documentTypeCd },
      ])
    ).values(),
  ]);
  const filterByType = (item: any) =>
    !selectedType.value ||
    item.documentTypeCd?.toLowerCase() === selectedType.value?.toLowerCase();

  const filteredDocuments = computed(() =>
    props.documents.filter(filterByType)
  );

  const typeCount = (type: any): number =>
    props.documents.filter((doc) => doc.documentTypeCd === type.value).length;

  onMounted(async () => {
    rolesLoading.value = true;
    roles.value = await getRoles();
    rolesLoading.value = false;
  });

  const openIndividualDocument = (data: civilDocumentType) =>
    shared.openDocumentsPdf(
      getCivilDocumentType(data),
      prepareCivilDocumentData(data)
    );
  const openMergedDocuments = () => {
    const documents: [CourtDocumentType, DocumentData][] = [];
    selectedItems.value
      .filter((item) => item.imageId)
      .forEach((item) => {
        const documentType = getCivilDocumentType(item);
        const documentData = prepareCivilDocumentData(item);
        documents.push([documentType, documentData]);
      });
    shared.openMergedDocumentsPdf(documents);
  };
</script>

<style scoped>
  .v-chip {
    cursor: default;
  }
</style>
