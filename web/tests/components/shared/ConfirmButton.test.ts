import { describe, it, expect, vi } from 'vitest';
import { mount } from '@vue/test-utils';
import ConfirmButton from 'CMP/shared/ConfirmButton.vue';

describe('ConfirmButton.vue', () => {
  const defaultProps = {
    buttonText: 'Confirm',
    infoText: 'Are you sure?',
    confirmText: 'Yes, confirm',
    confirmAction: vi.fn(),
  };

  it('renders button with correct text', () => {
    const wrapper = mount(ConfirmButton, { props: defaultProps });
    expect(wrapper.text()).toContain(defaultProps.buttonText);
  });

  it('shows dialog when button is clicked', async () => {
    const wrapper = mount(ConfirmButton, { props: defaultProps });
    await wrapper.find('v-btn-secondary').trigger('click');
    expect(wrapper.find('v-dialog').exists()).toBe(true);
    expect(wrapper.vm.show).toBe(true);
  });
});