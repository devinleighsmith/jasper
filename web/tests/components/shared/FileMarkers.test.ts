import { nextTick } from 'vue';
import { mount } from '@vue/test-utils';
import { describe, it, expect } from 'vitest';
import FileMarkers from 'CMP/shared/FileMarkers.vue';

describe('FileMarkers.vue', () => {
  const props = {
    restrictions: [],
    division: 'Criminal',
    participants: [{ inCustodyYN: 'Y', detainedYN: 'N', interpreterYN: 'N' }],
    appearances: [{ appearanceReasonCd: 'CNT' }],
  };

  it('renders markers based on division and props', async () => {
    const wrapper = mount(FileMarkers, {props});

    await nextTick();

    const chips = wrapper.findAll('v-chip');
    const selectedChips = wrapper.findAll('v-chip.selected');

    expect(chips.length).toBe(4); // IC, DO, CNT, INT
    expect(selectedChips.length).toBe(2); // IC, CNT
  });

  it('renders correct marker descriptions in tooltips', async () => {
    const wrapper = mount(FileMarkers, {props});

    await nextTick();

    const tooltips = wrapper.findAll('v-tooltip');
    const descriptions = tooltips.map((tooltip) => tooltip.text());

    expect(tooltips.length).toBe(4);
    expect(descriptions).toContain('In Custody');
    expect(descriptions).toContain('Detained Order');
    expect(descriptions).toContain('Continuation');
    expect(descriptions).toContain('Interpreter Required');
  });
});