
import { ref } from 'vue';
import { mount } from '@vue/test-utils';
import { describe, it, expect } from 'vitest';
import CourtFilesSelector from '@/components/case-details/CourtFilesSelector.vue';

describe('CourtFilesSelector.vue', async () => {
    const files = ref([
        { key: 'file1', value: 'File 1' },
        { key: 'file2', value: 'File 2' },
    ]);
    const fileNumber = 'file1';
    
    it('renders the correct number of tabs', () => {
        const wrapper = mount(CourtFilesSelector, {
            props: { files: files.value, fileNumber },
        });
        const tabs = wrapper.findAll('v-tab');
        expect(tabs.length).toBe(files.value.length);
    });

    it('renders the correct tab labels', () => {
        const wrapper = mount(CourtFilesSelector, {
            props: { files: files.value, fileNumber },
        });
        files.value.forEach((file, index) => {
            expect(wrapper.findAll('v-tab')[index].text()).toBe(file.value);
        });
    });

    it('sets the correct active tab on click', async () => {
        const wrapper = mount(CourtFilesSelector, {
            props: { files: files.value, fileNumber },
        });
        const tab = wrapper.findAll('v-tab')[1];
        await tab.trigger('click');
        expect(wrapper.vm.fileNumber).toBe(files.value[1].key);
    });

    it('renders the "View all documents" button', () => {
        const wrapper = mount(CourtFilesSelector, {
            props: { files: files.value, fileNumber },
        });
        const button = wrapper.find('v-btn');
        expect(button.text()).toBe('View all documents');
    });
});