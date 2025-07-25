import { describe, it, expect, vi } from 'vitest';
import { mount } from '@vue/test-utils';
import ConfirmButton from 'CMP/shared/ConfirmButton.vue';

describe('ConfirmButton.vue', () => {
  const confirmAction = vi.fn()
  const defaultProps = {
    buttonText: 'Confirm',
    infoText: 'Are you sure?',
    confirmText: 'Yes, confirm',
    confirmAction: confirmAction,
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

  it('calls confirmAction and closes dialog on confirm', async () => {
    const wrapper = mount(ConfirmButton, { props: defaultProps });
    await wrapper.find('v-btn-secondary').trigger('click');
    const confirmBtn = wrapper.find('v-btn-tertiary');
    expect(confirmBtn).toBeTruthy()
    await confirmBtn!.trigger('click')
    expect(confirmAction).toHaveBeenCalled()
    expect(wrapper.vm.show).toBe(false)
  });

  it('does not call confirmAction and closes dialog on cacnel', async () => {
    const wrapper = mount(ConfirmButton, { props: defaultProps });
    await wrapper.find('v-btn-secondary').trigger('click');
    const cancelBtn = wrapper.findAll('v-btn-secondary')[1];
    expect(cancelBtn).toBeTruthy()
    await cancelBtn!.trigger('click')
    expect(confirmAction).not.toHaveBeenCalled()
    expect(wrapper.vm.show).toBe(false)
  })
});