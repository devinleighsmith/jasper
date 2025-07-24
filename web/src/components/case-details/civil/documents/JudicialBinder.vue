<template>
  <v-skeleton-loader
    class="my-3"
    v-if="isBinderLoading"
    type="table"
    :loading="isBinderLoading"
  ></v-skeleton-loader>
  <div
    class="mb-5"
    data-testid="jb-container"
    v-if="!isBinderLoading && binderDocuments.length !== 0"
  >
    <v-card class="my-3" color="var(--bg-gray-500)" elevation="0">
      <v-card-text>
        <v-row align="center" no-gutters>
          <v-col class="text-h5" cols="6">Judicial binder</v-col>
        </v-row>
      </v-card-text>
    </v-card>
    <v-alert :class="['ml-3', courtClassCdStyle]" border="start">
      <template #prepend>
        <v-icon :icon="mdiNotebookOutline" />
      </template>
      <template #text>
        To remove documents from your judicial binder, click the ellipsis icon
        on the document you want to remove, then select “Remove from binder”. To
        reorder documents click and drag the<v-icon :icon="mdiDragVertical" />
        icon.
      </template>
    </v-alert>
    <div class="d-flex justify-end my-3">
      <v-btn-secondary class="mr-3" text="View Binder"></v-btn-secondary>
      <ConfirmButton
        buttonText="Delete Judicial Binder"
        infoText="Are you sure you want to delete your Judicial Binder? This will not delete any documents."
        confirmText="Yes, delete"
        :confirmAction="deleteBinder"
      />
    </div>
    <div class="jb-table overflow-y-auto">
      <v-data-table-virtual
        item-value="civilDocumentId"
        class="my-3"
        :headers
        :items="binderDocuments"
        hide-default-footer
        show-select
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
              <v-skeleton-loader
                class="bg-transparent"
                type="text"
                :loading="rolesLoading"
              >
                {{
                  roles
                    ? getLookupShortDescription(role.roleTypeCode, roles)
                    : ''
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
        <template v-slot:item.binderMenu="{ item }">
          <EllipsesMenu :menuItems="removeFromBinder(item)" />
        </template>
      </v-data-table-virtual>
    </div>
  </div>
</template>
<script setup lang="ts">
  import EllipsesMenu from '@/components/shared/EllipsesMenu.vue';
  import { civilDocumentType } from '@/types/civil/jsonTypes';
  import { Anchor, LookupCode } from '@/types/common';
  import { DataTableHeader } from '@/types/shared';
  import { getLookupShortDescription } from '@/utils/utils';
  import { mdiDragVertical, mdiNotebookOutline } from '@mdi/js';
  import { BinderDocument } from '@/types';
  import ConfirmButton from '@/components/shared/ConfirmButton.vue';

  const props = defineProps<{
    isBinderLoading: boolean;
    courtClassCdStyle: string;
    rolesLoading: boolean;
    roles: LookupCode[];
    baseHeaders: DataTableHeader[];
    binderDocuments: civilDocumentType[];
    selectedItems: civilDocumentType[];
    removeDocumentFromBinder: (documentId: string) => void;
    openIndividualDocument: (data: civilDocumentType) => void;
    deleteBinder: () => void;
  }>();

  const headers = [
    ...props.baseHeaders,
    {
      title: 'ACTIONS',
      key: 'binderMenu',
      align: 'end' as const,
      sortable: false,
    },
  ];

  const removeFromBinder = (item: civilDocumentType) => {
    return [
      {
        title: 'Remove from binder',
        action: () => props.removeDocumentFromBinder(item.civilDocumentId),
        enable: true,
      },
    ];
  };
</script>
<style>
  .jb-table {
    max-height: 400px;
  }
</style>
