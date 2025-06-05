<template>
  <v-data-table
    v-model="selected"
    must-sort
    :sort-by="sortBy"
    :items="data"
    :headers
    :search="search"
    :group-by
    item-value="appearanceId"
    items-per-page="100"
    class="pb-5"
  >
    <template v-slot:group-header="{ item, columns, isGroupOpen, toggleGroup }">
      <tr>
        <td class="pa-0" style="height: 1rem" :colspan="columns.length">
          <v-banner
            :class="
              item.value === 'Family'
                ? 'family-table-banner'
                : item.value === 'Small Claims'
                  ? 'small-claims-table-banner'
                  : 'table-banner'
            "
            :ref="
              () => {
                if (!isGroupOpen(item)) toggleGroup(item);
              }
            "
          >
            {{ item.value }}
          </v-banner>
        </td>
      </tr>
    </template>
    <template v-slot:item.icons="{ item }">
      <v-tooltip
        v-if="item.isComplete"
        text="Appearance is complete, one or more of the charges has a recorded result"
        location="top"
      >
        <template v-slot:activator="{ props }">
          <v-icon
            v-bind="props"
            :icon="mdiCheck"
            color="primary"
            size="large"
          />
        </template>
      </v-tooltip>

      <v-tooltip
        v-else-if="item.appearanceStatusCd === 'CNCL'"
        text="Cancelled"
        location="top"
      >
        <template v-slot:activator="{ props }">
          <v-icon
            v-bind="props"
            :icon="mdiTrashCanOutline"
            color="primary"
            size="large"
          />
        </template>
      </v-tooltip>

      <v-tooltip
        v-else-if="item.appearanceStatusCd === 'UNCF'"
        text="Unconfirmed"
        location="top"
      >
        <template v-slot:activator="{ props }">
          <v-icon
            v-bind="props"
            :icon="mdiWeatherNight"
            color="primary"
            size="large"
          />
        </template>
      </v-tooltip>
      <v-tooltip
        v-if="item.homeLocationNm"
        :text="'Court file home location: ' + item.homeLocationNm"
        location="top"
      >
        <template v-slot:activator="{ props }">
          <v-icon
            v-bind="props"
            :icon="mdiHomeOutline"
            color="primary"
            size="large"
          />
        </template>
      </v-tooltip>
    </template>
    <template v-slot:item.estimatedTime="{ item }">
      {{ hoursMinsFormatter(item.estimatedTimeHour, item.estimatedTimeMin) }}
    </template>
    <template v-slot:item.fileMarkers="{ item }">
      <FileMarkers
        class-override="ml-1 mt-1"
        :markers="getFileMarkers(item) ?? []"
      />
    </template>
    <template v-slot:item.appearanceReasonCd="{ value, item }">
      <v-tooltip :text="item.appearanceReasonDsc" location="top">
        <template v-slot:activator="{ props }">
          <span v-bind="props" class="has-tooltip">{{ value }}</span>
        </template>
      </v-tooltip>
    </template>
    <template v-slot:item.counsel="{ item }">
      <v-tooltip
        :disabled="
          (item.counsel?.length ?? 0) + (item.accusedCounselNm ? 1 : 0) < 2
        "
        location="top"
      >
        <template #activator="{ props }">
          <span
            v-bind="props"
            :class="{
              'has-tooltip':
                (item?.accusedCounselNm ? 1 : 0) + (item.counsel?.length ?? 0) >
                1,
            }"
            >{{ renderCounsel(item.accusedCounselNm, item.counsel) }}</span
          >
        </template>
        <span
          v-html="renderCounselTooltip(item.accusedCounselNm, item.counsel)"
        ></span>
      </v-tooltip>
    </template>
    <template v-slot:item.crown="{ value }">
      <v-tooltip :disabled="value?.length < 2" location="top">
        <template #activator="{ props }">
          <span :class="{ 'has-tooltip': value?.length > 1 }" v-bind="props">
            {{ renderName(value) }}
          </span>
        </template>
        <span v-html="renderTooltip(value)"></span>
      </v-tooltip>
    </template>
    <template v-slot:item.actions>
      <v-icon :icon="mdiNotebookEditOutline" size="large" />
      <v-icon :icon="mdiFileDocumentEditOutline" size="large" />
    </template>
    <template v-slot:bottom />
  </v-data-table>
  <ActionBar :selected="selected">
    <v-btn
      size="large"
      class="mx-2"
      :prepend-icon="mdiFileDocumentMultipleOutline"
      style="letter-spacing: 0.001rem"
    >
      View document bundle
    </v-btn>
    <v-btn
      size="large"
      class="mx-2"
      :prepend-icon="mdiFileDocumentOutline"
      style="letter-spacing: 0.001rem"
    >
      View case details
    </v-btn>
  </ActionBar>
</template>

<script setup lang="ts">
  import FileMarkers from '@/components/shared/FileMarkers.vue';
  import ActionBar from '@/components/shared/table/ActionBar.vue';
  import { CourtClassEnum, DivisionEnum, FileMarkerEnum } from '@/types/common';
  import { CourtListAppearance, PcssCounsel } from '@/types/courtlist';
  import { hoursMinsFormatter } from '@/utils/dateUtils';
  import { getEnumName } from '@/utils/utils';
  import {
    mdiCheck,
    mdiFileDocumentEditOutline,
    mdiFileDocumentMultipleOutline,
    mdiFileDocumentOutline,
    mdiHomeOutline,
    mdiNotebookEditOutline,
    mdiTrashCanOutline,
    mdiWeatherNight,
  } from '@mdi/js';
  import { computed, ref } from 'vue';

  const selected = defineModel<string[]>('selectedItems');
  const sortBy = ref([
    { key: 'appearanceSequenceNumber', order: 'asc' },
  ] as const);

  const groupBy = ref([
    {
      key: 'courtClassStyle',
      order: 'asc' as const,
    },
  ]);

  const data = computed(() =>
    props.data.map((item) => ({
      ...item,
      courtClassStyle: getCourtClass(item.courtClassCd),
    }))
  );

  const props = defineProps<{
    data: CourtListAppearance[];
    search: string;
  }>();

  const headers = ref([
    { key: 'data-table-group' },
    {
      title: '#',
      key: 'appearanceSequenceNumber',
    },
    { title: 'FILE #', key: 'courtFileNumber' },
    {
      title: '',
      key: 'icons',
      width: '3%',
      sortable: false,
    },
    {
      title: 'ACCUSED/PARTIES',
      key: 'accusedNm',
      value: (item: CourtListAppearance) => item.accusedNm || item.styleOfCause,
    },
    { title: 'TIME', key: 'appearanceTm' },
    { title: 'EST.', key: 'estimatedTime' },
    { title: 'ROOM', key: 'courtRoomCd' },
    { title: 'REASON', key: 'appearanceReasonCd' },
    { title: 'FILE MARKERS', key: 'fileMarkers', sortable: false },
    { title: 'COUNSEL', key: 'counsel' },
    { title: 'CROWN', key: 'crown' },
    {
      title: 'CASE AGE (days)',
      key: 'caseAgeDays',
      value: (item: CourtListAppearance) => item.caseAgeDays ?? '',
    },
    { title: 'NOTES', key: 'actions', width: '5%', sortable: false },
  ]);

  const renderTooltip = (items: any[], additionalItem?: string) => {
    let tooltip =
      items?.map((item) => `${item?.lastNm}, ${item?.givenNm}`).join('<br/>') ||
      '';
    if (additionalItem) {
      tooltip += `${tooltip ? '<br/>' : ''}${splitNames(additionalItem)}`;
    }
    return tooltip;
  };

  const renderName = (items: any[], additionalItem?: string) => {
    if (!items?.length && !additionalItem) {
      return '';
    }
    let name = items?.[0] ? `${items[0]?.lastNm}, ${items[0]?.givenNm}` : '';
    const count = (items?.length || 0) + (additionalItem ? 1 : 0);
    if (additionalItem && !name) {
      name = splitNames(additionalItem);
    }
    return count > 1 ? `${name} +${count - 1}` : name;
  };

  const renderCounselTooltip = (
    accusedCounselNm: string,
    counsel: PcssCounsel[] | undefined
  ) => renderTooltip(counsel ?? [], accusedCounselNm);

  const renderCounsel = (
    accusedCounselNm: string,
    counsel: PcssCounsel[] | undefined
  ) => renderName(counsel ?? [], accusedCounselNm);

  const splitNames = (name: string) => {
    const [firstName, lastName] = name.split(' ');
    return `${lastName}, ${firstName}`;
  };

  const getFileMarkers = (item: CourtListAppearance) => {
    const {
      continuationYn,
      lackCourtTimeYn,
      otherFactorsYn,
      otherFactorsComment,
      appearanceAdjudicatorRestriction,
    } = item;

    let fileMarkers: {
      marker: FileMarkerEnum;
      value: string;
      notes?: string[];
    }[] = [
      { marker: FileMarkerEnum.CNT, value: continuationYn },
      { marker: FileMarkerEnum.LOCT, value: lackCourtTimeYn },
      {
        marker: FileMarkerEnum.OTH,
        value: otherFactorsYn,
        notes:
          otherFactorsYn === 'Y' && otherFactorsComment
            ? [otherFactorsComment]
            : [],
      },
    ];

    const restrictions = appearanceAdjudicatorRestriction
      ?.sort((a, b) =>
        a.hearingRestrictionTxt.localeCompare(b.hearingRestrictionTxt)
      )
      .map((ar) => ar.hearingRestrictionTxt);

    // Include AR if any
    if (restrictions && restrictions.length > 0) {
      fileMarkers.push({
        marker: FileMarkerEnum.ADJ,
        value: 'Y',
        notes: restrictions,
      });
    }

    // Criminal
    if (item.courtDivisionCd === DivisionEnum.R) {
      const [continuation, ...rest] = fileMarkers;
      const criminalFileMarkers = getCriminalFileMarkers(item);
      fileMarkers = [continuation, ...criminalFileMarkers, ...rest];
    }
    // Civil
    else if (item.courtDivisionCd === DivisionEnum.I) {
      const civilFileMarkers = getCivilFileMarkers(item);
      fileMarkers = [...civilFileMarkers, ...fileMarkers];
    }
    return fileMarkers;
  };

  const getCriminalFileMarkers = (item: CourtListAppearance) => {
    const { condSentenceOrderYn, detainedYn, inCustodyYn } = item;

    return [
      { marker: FileMarkerEnum.CSO, value: condSentenceOrderYn },
      { marker: FileMarkerEnum.DO, value: detainedYn },
      { marker: FileMarkerEnum.IC, value: inCustodyYn },
    ];
  };

  const getCivilFileMarkers = (item: CourtListAppearance) => {
    // Family
    if (item.courtClassCd === getEnumName(CourtClassEnum, CourtClassEnum.F)) {
      const { cfcsaYn } = item;

      return [{ marker: FileMarkerEnum.CPA, value: cfcsaYn }];
    }

    return [];
  };

  /**
   * Retrieves the CSS class name to represent a CourtClass
   * @param courtClassCd The court class code
   * @returns class name
   */
  const getCourtClass = (courtClassCd: string): string => {
    switch (courtClassCd) {
      case getEnumName(CourtClassEnum, CourtClassEnum.A):
        return 'Criminal - Adult';
      case getEnumName(CourtClassEnum, CourtClassEnum.Y):
        return 'Youth';
      case getEnumName(CourtClassEnum, CourtClassEnum.T):
        return 'Tickets';
      case getEnumName(CourtClassEnum, CourtClassEnum.C):
      case getEnumName(CourtClassEnum, CourtClassEnum.L):
      case getEnumName(CourtClassEnum, CourtClassEnum.M):
        return 'Small Claims';
      case getEnumName(CourtClassEnum, CourtClassEnum.F):
        return 'Family';
      default:
        return 'Unknown';
    }
  };
</script>
