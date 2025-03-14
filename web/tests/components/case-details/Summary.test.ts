import { describe, it, expect } from 'vitest';
import { mount } from '@vue/test-utils';
import Summary from 'CPM/case-details/Summary.vue';
import { criminalFileDetailsType } from '@/types/criminal/jsonTypes';

  
const details: criminalFileDetailsType = {
    responseCd: '200',
    responseMessageTxt: 'Success',
    justinNo: '123456',
    fileNumberTxt: 'FN123456',
    homeLocationAgenId: 'HLA123',
    homeLocationAgencyName: 'Location A',
    homeLocationAgencyCode: 'LAC123',
    homeLocationRegionName: 'Region A',
    courtLevelCd: 'CL123',
    courtLevelDescription: 'Court Level A',
    courtClassCd: 'CC123',
    courtClassDescription: 'Court Class A',
    activityClassCd: 'AC123',
    activityClassDesc: 'Activity Class A',
    currentEstimateLenQty: '10',
    currentEstimateLenUnit: 'days',
    initialEstimateLenQty: '5',
    initialEstimateLenUnit: 'days',
    trialStartDt: '2023-01-01',
    mdocSubCategoryDsc: 'SubCategory A',
    mdocCcn: 'CCN123',
    assignedPartNm: 'Assigned Part A',
    approvedByAgencyCd: 'ABAC123',
    approvedByPartNm: 'Approved Part A',
    approvalCrownAgencyTypeCd: 'ACAT123',
    crownEstimateLenQty: 15,
    crownEstimateLenDsc: '15 days',
    crownEstimateLenUnit: 'days',
    caseAgeDays: '100',
    indictableYN: 'Y',
    complexityTypeCd: 'CT123',
    assignmentTypeDsc: 'Assignment Type A',
    trialRemark: [
    ],
    participant: [
    ],
    witness: [
    ],
    crown: [
    ],
    hearingRestriction: [
    ],
    appearances: {
        apprDetail: [],
        responseCd: '',
        responseMessageTxt: '',
        futureRecCount: '',
        historyRecCount: '',
        additionalProperties: {
            additionalProp1: {},
            additionalProp2: {},
            additionalProp3: {},
        },
        additionalProp1: {},
        additionalProp2: {},
        additionalProp3: {},
    }
};

  describe('Summary.vue', () => {
    it('renders case details correctly', () => {
      const wrapper = mount(Summary, {
        props: { details }
      });

      expect(wrapper.find('h5').text()).toBe('Case Details');
      expect(wrapper.findComponent({ name: 'DivisionBadge' }).exists()).toBe(true);
      expect(wrapper.findComponent({ name: 'FileMarkers' }).exists()).toBe(true);
      expect(wrapper.find('v-card-title').text()).toBe('DOE, John and 1 other');
      expect(wrapper.find('v-card-subtitle').text()).toBe('Location A');
      expect(wrapper.findAll('v-row').at(1).find('v-col').text()).toBe('Crown');
      expect(wrapper.findAll('v-row').at(1).findAll('v-col').at(1).text()).toBe('Brown, Charlie');
      expect(wrapper.findAll('v-row').at(2).findAll('v-col').at(1).text()).toBe('100 days');
    });
  });