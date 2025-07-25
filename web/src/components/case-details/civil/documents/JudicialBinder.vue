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
      <v-table :headers="headers" :items="binderDocuments">
        <template v-slot:default>
          <thead>
            <tr>
              <th
                id="table-header"
                v-for="header in headers"
                :key="header.value"
              >
                {{ header.title }}
              </th>
            </tr>
          </thead>
          <draggable
            v-model="draggableItems"
            item-key="civilDocumentId"
            tag="tbody"
            handle=".handle"
            @change="dropped"
          >
            <template #item="{ element }">
              <tr>
                <!-- Handle Column -->
                <td>
                  <!-- drag handle column -->
                  <v-icon
                    style="cursor: move"
                    class="handle"
                    :icon="mdiDragVertical"
                  />
                </td>
                <!-- Sequence Number column -->
                <td>
                  {{ element.fileSeqNo }}
                </td>
                <td>
                  <!-- documentTypeDescription column -->
                  <a
                    v-if="element.imageId"
                    href="javascript:void(0)"
                    @click="props.openIndividualDocument(element)"
                  >
                    {{ element.documentTypeDescription }}
                  </a>
                  <span v-else>
                    {{ element.documentTypeDescription }}
                  </span>
                </td>
                <td>
                  <!-- activity column -->
                  <v-chip-group>
                    <div
                      v-for="info in element.documentSupport"
                      :key="info.actCd"
                    >
                      <v-chip rounded="lg">{{ info.actCd }}</v-chip>
                    </div>
                  </v-chip-group>
                </td>
                <!-- date files column -->
                <td>
                  {{ formatDateToDDMMMYYYY(element.filedDt) }}
                </td>
                <td>
                  <!-- filedBy column -->
                  <span v-for="(role, index) in element.filedBy" :key="index">
                    <span v-if="role.roleTypeCode">
                      <v-skeleton-loader
                        class="bg-transparent"
                        type="text"
                        :loading="props.rolesLoading"
                      >
                        {{
                          props.roles
                            ? getLookupShortDescription(
                                role.roleTypeCode,
                                props.roles
                              )
                            : ''
                        }}
                      </v-skeleton-loader>
                    </span>
                  </span>
                </td>
                <td>
                  <!-- issue column -->
                  <LabelWithTooltip
                    v-if="element.issue?.length > 0"
                    :values="element.issue.map((issue) => issue.issueTypeDesc)"
                    :location="Anchor.Top"
                  />
                </td>
                <td>
                  <!-- binderMenu column -->
                  <EllipsesMenu :menuItems="removeFromBinder(element)" />
                </td>
              </tr>
            </template>
          </draggable>
        </template>
      </v-table>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref } from 'vue';
  import { mdiDrag } from '@mdi/js';
  import draggable from 'vuedraggable';
  // import { VueDraggableNext } from 'vue-draggable-next';
  import EllipsesMenu from '@/components/shared/EllipsesMenu.vue';
  import { civilDocumentType } from '@/types/civil/jsonTypes';
  import { Anchor, LookupCode } from '@/types/common';
  import { DataTableHeader } from '@/types/shared';
  import { getLookupShortDescription } from '@/utils/utils';
  import { formatDateToDDMMMYYYY } from '@/utils/dateUtils';
  import { mdiDragVertical, mdiNotebookOutline } from '@mdi/js';
  import ConfirmButton from '@/components/shared/ConfirmButton.vue';
  import { watch } from 'vue';

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

  const emit = defineEmits<
    (
      e: 'update:reordered',
      value: {
        oldIndex: number;
        newIndex: number;
        document: civilDocumentType;
      }
    ) => void
  >();

  const draggableItems = ref<civilDocumentType[]>([...props.binderDocuments]);

  watch(
    () => props.binderDocuments,
    (newVal) => {
      draggableItems.value = [...newVal];
    },
    { immediate: true, deep: true }
  );

  const test = [
    { id: 1, name: 'Abby', sport: 'basket' },
    { id: 2, name: 'Brooke', sport: 'foot' },
    { id: 3, name: 'Courtenay', sport: 'volley' },
    { id: 4, name: 'David', sport: 'rugby' },
  ];

  const headers = [
    {
      title: '',
      key: 'drag',
      align: 'start' as const,
      sortable: false,
    },
    ...props.baseHeaders,
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
  const dropped = (event) =>
    emit('update:reordered', {
      oldIndex: event.moved.oldIndex,
      newIndex: event.moved.newIndex,
      document: event.moved.element,
    });
</script>
<style>
  .jb-table {
    max-height: 400px;
  }
</style>
