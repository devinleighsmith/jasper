import { mount } from '@vue/test-utils';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import ScheduledDocuments from 'CMP/case-details/civil/appearances/ScheduledDocuments.vue';

const mockDocuments = [
    {
        appearanceId: '1',
        fileSeqNo: 1,
        documentTypeDescription: 'Affidavit',
        imageId: 'img1',
        documentSupport: [{ actCd: 'ACT1' }, { actCd: 'ACT2' }],
        filedDt: '2024-06-01',
        filedByName: 'John Doe',
        runtime: 'Completed',
        issue: [{ issueDsc: 'Issue 1' }],
        category: 'civil',
        DateGranted: '2024-06-01',
        civilDocumentId: 'doc1',
        partId: 'part1',
    },
    {
        appearanceId: '2',
        fileSeqNo: 2,
        documentTypeDescription: 'Order',
        imageId: null,
        documentSupport: [],
        filedDt: '2024-06-02',
        filedByName: 'Jane Smith',
        runtime: 'Pending',
        issue: [],
        category: 'csr',
        DateGranted: '2024-06-02',
        civilDocumentId: 'doc2',
        partId: 'part2',
    },
];

vi.mock('@/components/shared', () => ({
    default: {
        openDocumentsPdf: vi.fn(),
    }
}));

vi.mock('@/stores', () => ({
    useCommonStore: () => ({
        courtRoomsAndLocations: [
            { agencyIdentifierCd: 'AG1', name: 'Courtroom 1' },
            { agencyIdentifierCd: 'AG2', name: 'Courtroom 2' },
        ],
    }),
}));

vi.mock('@/components/documents/DocumentUtils', () => ({
    prepareCivilDocumentData: vi.fn(() => ({
        fileId: undefined,
        fileNumberText: undefined,
        courtLevel: undefined,
        location: undefined,
    })),
    getCivilDocumentType: vi.fn(() => 'Civil'),
}));

describe('ScheduledDocuments.vue', () => {
    let wrapper: ReturnType<typeof mount>;
    const mountComponent = (documents = mockDocuments) => {
        return mount(ScheduledDocuments, {
            props: {
                documents,
                fileId: 'file123',
                fileNumberTxt: 'FN123',
                courtLevel: 'Provincial',
                agencyId: 'AG1',
            },
            global: {
                stubs: {
                    LabelWithTooltip: {
                        template: '<div class="label-tooltip"></div>',
                        props: ['values', 'location'],
                    },
                    'v-data-table-virtual': {
                        template: `<div>
                            <div v-for="item in items" :key="item.appearanceId">
                                <slot name="item.documentTypeDescription" :item="item"></slot>
                            </div>
                        </div>`,
                        props: ['headers', 'items', 'itemValue'],
                    },
                },
            },
        });
    };

    beforeEach(() => {
        vi.clearAllMocks();
        const testDocuments = [
            {
                ...mockDocuments[0],
                imageId: 'test-image-id',
            }
        ];
        wrapper = mountComponent(testDocuments);
    });

    it('renders document type as link if imageId exists', () => {
        const links = wrapper.findAll('a');
        expect(links.length).toBeGreaterThan(0);
        expect(links[0].text()).toContain('Affidavit');
    });

    it('does not render document type as link if no imageId exists', () => {
        mockDocuments[0].imageId = null;
        wrapper = mountComponent(mockDocuments);
        const links = wrapper.findAll('a');
        expect(links.length).toBe(0);
    });

    it('should populate fileId from props into documentData', async () => {
        const { default: shared } = await import('@/components/shared');
        const openDocumentsPdfSpy = vi.spyOn(shared, 'openDocumentsPdf');

        // Find and click the link - ensure it exists
        const link = wrapper.find('a[href="javascript:void(0)"]');
        expect(link.exists()).toBe(true);
        await link.trigger('click');

        expect(openDocumentsPdfSpy).toHaveBeenCalled();
        const [, documentData] = openDocumentsPdfSpy.mock.calls[0];
        expect(documentData.fileId).toBe('file123');
    });

    it('should populate fileNumberText from props when not set in documentData', async () => {
        const { default: shared } = await import('@/components/shared');
        const openDocumentsPdfSpy = vi.spyOn(shared, 'openDocumentsPdf');

        const link = wrapper.find('a[href="javascript:void(0)"]');
        expect(link.exists()).toBe(true);
        await link.trigger('click');

        const [, documentData] = openDocumentsPdfSpy.mock.calls[0];
        expect(documentData.fileNumberText).toBe('FN123');
    });

    it('should populate courtLevel from props when not set in documentData', async () => {
        const { default: shared } = await import('@/components/shared');
        const openDocumentsPdfSpy = vi.spyOn(shared, 'openDocumentsPdf');

        const link = wrapper.find('a[href="javascript:void(0)"]');
        expect(link.exists()).toBe(true);
        await link.trigger('click');

        const [, documentData] = openDocumentsPdfSpy.mock.calls[0];
        expect(documentData.courtLevel).toBe('Provincial');
    });

    it('should populate location from commonStore based on agencyId when not set', async () => {
        const { default: shared } = await import('@/components/shared');
        const openDocumentsPdfSpy = vi.spyOn(shared, 'openDocumentsPdf');

        const link = wrapper.find('a[href="javascript:void(0)"]');
        expect(link.exists()).toBe(true);
        await link.trigger('click');

        const [, documentData] = openDocumentsPdfSpy.mock.calls[0];
        expect(documentData.location).toBe('Courtroom 1');
    });

    it('should not overwrite existing values in documentData', async () => {
        // Mock DocumentUtils to return pre-populated data
        const { prepareCivilDocumentData } = await import('@/components/documents/DocumentUtils');
        vi.mocked(prepareCivilDocumentData).mockReturnValue({
            fileId: 'existing-file-id',
            fileNumberText: 'existing-file-number',
            courtLevel: 'existing-court-level',
            location: 'existing-location',
        });

        const { default: shared } = await import('@/components/shared');
        const openDocumentsPdfSpy = vi.spyOn(shared, 'openDocumentsPdf');

        const link = wrapper.find('a[href="javascript:void(0)"]');
        expect(link.exists()).toBe(true);
        await link.trigger('click');

        const [, documentData] = openDocumentsPdfSpy.mock.calls[0];
        // fileId should always be overwritten
        expect(documentData.fileId).toBe('file123');
        // Other values should preserve existing values
        expect(documentData.fileNumberText).toBe('existing-file-number');
        expect(documentData.courtLevel).toBe('existing-court-level');
        expect(documentData.location).toBe('existing-location');
    });

    it('should call prepareCivilDocumentData with the document', async () => {
        const { prepareCivilDocumentData } = await import('@/components/documents/DocumentUtils');

        const link = wrapper.find('a[href="javascript:void(0)"]');
        expect(link.exists()).toBe(true);
        await link.trigger('click');

        expect(prepareCivilDocumentData).toHaveBeenCalledWith(
            expect.objectContaining({
                appearanceId: '1',
                documentTypeDescription: 'Affidavit',
                imageId: 'test-image-id'
            })
        );
    });

    it('should call getCivilDocumentType with the document', async () => {
        const { getCivilDocumentType } = await import('@/components/documents/DocumentUtils');

        const link = wrapper.find('a[href="javascript:void(0)"]');
        expect(link.exists()).toBe(true);
        await link.trigger('click');

        expect(getCivilDocumentType).toHaveBeenCalledWith(
            expect.objectContaining({
                appearanceId: '1',
                documentTypeDescription: 'Affidavit',
                imageId: 'test-image-id'
            })
        );
    });

    it('should call shared.openDocumentsPdf with documentType and enhanced documentData', async () => {
        const { getCivilDocumentType } = await import('@/components/documents/DocumentUtils');
        const { default: shared } = await import('@/components/shared');
        
        vi.mocked(getCivilDocumentType).mockReturnValue('TestDocumentType' as any);
        const openDocumentsPdfSpy = vi.spyOn(shared, 'openDocumentsPdf');

        const link = wrapper.find('a[href="javascript:void(0)"]');
        expect(link.exists()).toBe(true);
        await link.trigger('click');

        expect(openDocumentsPdfSpy).toHaveBeenCalledWith(
            'TestDocumentType',
            expect.objectContaining({
                fileId: 'file123',
                fileNumberText: 'FN123',
                courtLevel: 'Provincial',
                location: 'Courtroom 1',
            })
        );
    });
});
