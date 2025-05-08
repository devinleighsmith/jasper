import { describe, it, expect } from 'vitest';
import { mount, shallowMount } from '@vue/test-utils';
import ScheduledParties from 'CMP/case-details/civil/appearances/ScheduledParties.vue';
import LabelWithTooltip from 'CMP/shared/LabelWithTooltip.vue';
import { PartyDetails } from '@/types/civil/jsonTypes/index';
import { nextTick } from 'vue';

describe('ScheduledParties.vue', () => {
  const mockParties: PartyDetails[] = [
    {
        fullName: 'John Doe',
        partyRole: [{ roleTypeDsc: 'Plaintiff', roleTypeCd: 'P' }],
        counsel: [{ counselFullName: 'Jane Smith', counselId: '1' }],
        partyId: '',
        lastNm: '',
        givenNm: '',
        courtParticipantId: '',
        representative: [],
        legalRepresentative: []
    },
    {
        fullName: 'Alice Johnson',
        partyRole: [{
            roleTypeDsc: 'Defendant',
            roleTypeCd: 'D'
        }],
        counsel: [{ counselFullName: 'Jane Smith', counselId: '1' }],
        partyId: '',
        lastNm: '',
        givenNm: '',
        courtParticipantId: '',
        representative: [],
        legalRepresentative: []
    },
  ];

  it.only('renders counsel names with LabelWithTooltip when counsel exists', async () => {
    const wrapper = shallowMount(ScheduledParties, {
      props: { parties: mockParties },
    });
    await nextTick();

    const tooltipComponent = wrapper.findComponent({
      name: 'LabelWithTooltip',
    });
    expect(wrapper.findComponent({name: 'LabelWithTooltip'}).exists()).toBe(true);
    //expect(tooltipComponent.exists()).toBe(true);
  });

  it('does not render LabelWithTooltip when counsel is empty', () => {
    const wrapper = mount(ScheduledParties, {
      props: { parties: mockParties },
      global: {
        components: { LabelWithTooltip },
      },
    });

    const tooltipComponents = wrapper.findComponent('LabelWithTooltip');
    expect(tooltipComponents).toHaveLength(1); // Only one counsel exists
  });
});