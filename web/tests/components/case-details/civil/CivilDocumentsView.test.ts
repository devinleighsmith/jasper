import { shallowMount } from '@vue/test-utils';
import { nextTick } from 'vue';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useCommonStore } from '@/stores';
import CivilDocumentsView from 'CMP/case-details/civil/CivilDocumentsView.vue';

vi.mock('@/stores', () => ({
    useCivilFileStore: vi.fn(() => ({
        civilFileInformation: {
            fileNumber: '12345',
            detailsData: {
                courtClassCd: 'A',
                courtLevelCd: 'B',
                homeLocationAgencyName: 'Test Location',
            },
        },
    })),
    useCommonStore: vi.fn(() => ({
        roles: [{}],
        setRoles: vi.fn(() => ({})),
    }))
}));

describe('CivilDocumentsView.vue', () => {
    let wrapper: any;
    let commonStore: any;
    const mockDocuments = [
        {
            civilDocumentId: '1',
            documentTypeCd: 'CSR',
            documentTypeDescription: 'Civil Document 1',
            filedDt: '2023-01-01',
            filedBy: [{ roleTypeCode: 'Role1' }],
            issue: [{ issueTypeDesc: 'Issue1' }],
            documentSupport: [{ actCd: 'Act1' }],
            imageId: '123',
            lastAppearanceDt: '2023-01-02',
        },
        {
            civilDocumentId: '2',
            documentTypeCd: 'DOC',
            documentTypeDescription: 'Civil Document 2',
            filedDt: '2023-02-01',
            filedBy: [{ roleTypeCode: 'Role2' }],
            issue: [{ issueTypeDesc: 'Issue2' }],
            documentSupport: [{ actCd: 'Act2' }],
        },
    ];
    beforeEach(() => {
        wrapper = shallowMount(CivilDocumentsView, {
            props: { documents: mockDocuments },
        });
        commonStore = {
            setRoles: vi.fn(),
          };
        (useCommonStore as any).mockReturnValue(commonStore);
    });

    it('renders the component correctly', () => {
        expect(wrapper.exists()).toBe(true);
        expect(wrapper.find('v-select').exists()).toBe(true);
        expect(wrapper.findAll('v-data-table-virtual').length).toBe(1);
    });

    it('filters documents by selected type', async () => {
        wrapper.vm.selectedType = 'CSR';

        expect(wrapper.vm.filteredDocuments).toEqual([mockDocuments[0]]);
    });

    it('displays the correct number of filtered documents', async () => {
        const header = wrapper.find('.text-h5');

        expect(header.text()).toContain('All Documents (2)');

        wrapper.vm.selectedType = 'CSR';

        await nextTick();

        expect(header.text()).toContain('All Documents (1)');
    });

    it('renders action-bar when two or more documents with imageIds are selected', async () => {
        wrapper.vm.selectedItems = [mockDocuments[0], mockDocuments[0]];

        await nextTick();

        expect(wrapper.findComponent({ name: 'ActionBar' }).exists()).toBe(true);
    });

    it('does not render action-bar when two or more documents without imageIds are selected', async () => {
        wrapper.vm.selectedItems = [{}, {}];

        await nextTick();

        expect(wrapper.findComponent({ name: 'ActionBar' }).exists()).toBe(false);
    });

    it('does not render action-bar when one document with imageId is selected', async () => {
        wrapper.vm.selectedItems = [mockDocuments[0]];

        await nextTick();

        expect(wrapper.findComponent({ name: 'ActionBar' }).exists()).toBe(false);
    });
});