import { describe, it, expect, beforeEach } from 'vitest';
import { mount } from '@vue/test-utils';
import CivilAppearanceDetails from 'CMP/case-details/civil/appearances/CivilAppearanceDetails.vue';

describe('CivilAppearanceDetails.vue', () => {
  let wrapper: ReturnType<typeof mount>;

  beforeEach(() => {
    wrapper = mount(CivilAppearanceDetails);
  });

  it('renders the tabs correctly', () => {
    const tabs = wrapper.findAll('v-tab');
    expect(tabs.length).toBe(3);
    expect(tabs[0].text()).toBe('Scheduled Documents');
    expect(tabs[1].text()).toBe('Judicial Binder');
    expect(tabs[2].text()).toBe('Scheduled Parties');
  });
});