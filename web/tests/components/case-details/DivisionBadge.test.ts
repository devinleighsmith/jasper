import DivisionBadge from '@/components/case-details/DivisionBadge.vue';
import { mount } from '@vue/test-utils';
import { describe, expect, it } from 'vitest';

describe('DivisionBadge.vue', () => {
  const createWrapper = (division: string, activityClassDesc: string) => {
    return mount(DivisionBadge, {
      props: {
        division,
        activityClassDesc,
      },
    });
  };

  it('renders correctly with Criminal division and Adult activity class', () => {
    const wrapper = createWrapper('Criminal', 'Adult');
    expect(wrapper.text()).toContain('CRIMINAL - ADULT');
    expect(wrapper.find('v-chip').attributes('color')).toEqual('#4092c1');
  });

  it('renders correctly with Criminal division and YouthJustice activity class', () => {
    const wrapper = createWrapper('Criminal', 'Youth Justice');
    expect(wrapper.text()).toContain('CRIMINAL - YOUTH');
    expect(wrapper.find('v-chip').attributes('color')).toEqual('#4092c1');
  });

  it('renders correctly with Family division', () => {
    const wrapper = createWrapper('Family', '');
    expect(wrapper.text()).toContain('FAMILY');
    expect(wrapper.find('v-chip').attributes('color')).toEqual('#2e8540');
  });

  it('renders correctly with default division', () => {
    const wrapper = createWrapper('Unknown', '');
    expect(wrapper.text()).toContain('UNKNOWN');
    expect(wrapper.find('v-chip').attributes('color')).toEqual('#79368f');
  });
});
