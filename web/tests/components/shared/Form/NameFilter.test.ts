import { shallowMount } from '@vue/test-utils';
import { describe, it, expect } from 'vitest';
import NameFilter from 'CMP/shared/Form/NameFilter.vue';
import { personType } from '@/types/criminal/jsonTypes';

describe('NameFilter.vue', () => {
  const mockPeople: personType[] = [
    { lastNm: 'Doe', givenNm: 'John' },
    { lastNm: 'Smith', givenNm: 'Jane' },
    { lastNm: 'Doe', givenNm: 'John' }, // Duplicate to test unique names
  ];

  it('renders the v-select component when namesOnFile has more than one name', () => {
    const wrapper = shallowMount(NameFilter, {
      props: {
        people: mockPeople,
        label: 'Witness',
      },
    });

    const select = wrapper.findAll('v-select').at(0);
    expect(select?.exists()).toBe(true);
    expect(select?.attributes('items')).toEqual("Doe, John,Smith, Jane");
    expect(select?.attributes('placeholder')).toBe('All Witness');
  });

  it('does not render the v-select component when namesOnFile has less than two names', () => {
    const wrapper = shallowMount(NameFilter, {
      props: {
        people: [{ lastNm: 'Doe', givenNm: 'John' }],
        label: 'Accused',
      },
    });

    const select = wrapper.findAll('v-select').at(0);
    expect(select?.exists()).toBeFalsy();
  });

  it('uses provided vmodel as selected name', async () => {
    const wrapper = shallowMount(NameFilter, {
      props: {
        people: mockPeople,
        label: 'Accused',
        modelValue: 'Jane Smith',
      },
    });

    expect(wrapper.vm.selectedName).toBe('Jane Smith');
  });

  it('uses the default label if none is provided', () => {
    const wrapper = shallowMount(NameFilter, {
      props: {
        people: mockPeople,
        label: 'Accused',
        modelValue: '',
      },
    });

    const select = wrapper.findAll('v-select').at(0);
    expect(select?.exists()).toBe(true);
    expect(select?.attributes('placeholder')).toBe('All Accused');
  });
});