import shared from '@/components/shared';
import { DocumentRequestType } from '@/types/shared';
import { flushPromises, shallowMount } from '@vue/test-utils';
import DocumentsView from 'CMP/case-details/criminal/CriminalDocumentsView.vue';
import { createPinia, setActivePinia } from 'pinia';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { nextTick } from 'vue';
import { useCriminalFileStore } from '@/stores';

describe('CriminalDocumentsView.vue', () => {
  let wrapper: any;
  let mockParticipantOne: any;
  let mockParticipantTwo: any;
  let mockParticipants: any[];
  let mockDocumentOne: any;

  beforeEach(() => {
    setActivePinia(createPinia());

    // Initialize criminal file store with required data
    const criminalFileStore = useCriminalFileStore();
    criminalFileStore.criminalFileInformation = {
      detailsData: {
        courtClassCd: 'A',
        courtLevelCd: 'P',
        fileNumberTxt: 'TEST-123',
        homeLocationAgencyName: 'Vancouver',
      },
      fileNumber: 'TEST-123',
      participantList: [],
      adjudicatorRestrictionsInfo: [],
      bans: [],
      courtLevel: 'Provincial',
      courtClass: 'Adult',
    } as any;

    mockDocumentOne = {
      issueDate: '2023-01-01',
      documentTypeDescription: 'Type A',
      category: 'bail',
      documentPageCount: 5,
      imageId: '123',
      docmDispositionDsc: 'Disposition',
    };
    mockParticipantOne = {
      fullName: 'John Doe',
      lastNm: 'Doe',
      givenNm: 'John',
      profSeqNo: 1,
      document: [mockDocumentOne],
      keyDocuments: [],
    };
    mockParticipantTwo = {
      fullName: 'Jane Smith',
      profSeqNo: 2,
      lastNm: 'Smith',
      givenNm: 'Jane',
      document: [
        {
          issueDate: '2023-02-01',
          documentTypeDescription: 'Type B',
          category: 'other',
          documentPageCount: 3,
          imageId: '456',
          docmDispositionDsc: 'Disposition',
        },
      ],
    };
    mockParticipants = [mockParticipantOne, mockParticipantTwo];
    wrapper = shallowMount(DocumentsView, {
      props: {
        participants: mockParticipants,
      },
    });
  });

  it('renders only all documents table when no key documents', () => {
    const sections = wrapper.findAll('v-card-text .text-h5');
    const tables = wrapper.findAll('v-data-table-virtual');

    expect(tables).toHaveLength(1);
    expect(sections).toHaveLength(2);
    expect(sections[0].text()).toContain('Key Documents');
    expect(sections[1].text()).toContain('All Documents');
  });

  it('renders both all and key document tables', () => {
    mockParticipants[0].keyDocuments.push(mockDocumentOne);
    const wrapper = shallowMount(DocumentsView, {
      props: {
        participants: mockParticipants,
      },
    });

    const sections = wrapper.findAll('v-card-text .text-h5');
    const tables = wrapper.findAll('v-data-table-virtual');

    expect(tables).toHaveLength(2);
    expect(sections).toHaveLength(2);
    expect(sections[0].text()).toContain('Key Documents');
    expect(sections[1].text()).toContain('All Documents');
  });

  it('renders correct all documents header with correct count', () => {
    mockParticipants.push(mockParticipantOne);
    const wrapper = shallowMount(DocumentsView, {
      props: {
        participants: mockParticipants,
      },
    });
    const sections = wrapper.findAll('v-card-text .text-h5');
    expect(sections).toHaveLength(2);
    expect(sections[1].text()).toBe('All Documents (3)');
  });

  it('renders correct key documents header with correct count', () => {
    mockParticipants[0].keyDocuments.push(mockDocumentOne);
    const wrapper = shallowMount(DocumentsView, {
      props: {
        participants: mockParticipants,
      },
    });
    const sections = wrapper.findAll('v-card-text .text-h5');
    expect(sections).toHaveLength(2);
    expect(sections[0].text()).toBe('Key Documents (1)');
  });

  it('does not render table when no documents', () => {
    mockParticipants.length = 0;
    mockParticipantOne.document = [];
    mockParticipants.push(mockParticipantOne);

    const wrapper = shallowMount(DocumentsView, {
      props: {
        participants: mockParticipants,
      },
    });
    const sections = wrapper.findAll('v-card-text .text-h5');
    const tables = wrapper.findAll('v-data-table-virtual');
    expect(tables).toHaveLength(0);
    expect(sections).toHaveLength(0);
  });

  it('computes unfilteredDocuments correctly', async () => {
    wrapper.vm.selectedCategory = 'ROP';

    const unfilteredDocuments = wrapper.vm.unfilteredDocuments;
    expect(unfilteredDocuments).toHaveLength(2);
  });

  it('filters documents by category', async () => {
    wrapper.vm.selectedCategory = 'bail';

    const documents = wrapper.vm.documents;
    expect(documents).toHaveLength(1);
    expect(documents[0].category).toBe('bail');
  });

  it('renders action-bar when two or more documents with imageIds are selected', async () => {
    wrapper.vm.selectedItems = [
      mockParticipantOne.document[0],
      mockParticipantTwo.document[0],
    ];

    await nextTick();

    expect(wrapper.findComponent({ name: 'ActionBar' }).exists()).toBe(true);
  });

  it('does not render action-bar when two or more documents without imageIds are selected', async () => {
    wrapper.vm.selectedItems = [{}, {}];

    await nextTick();

    expect(wrapper.findComponent({ name: 'ActionBar' }).exists()).toBe(false);
  });

  it('does not render action-bar when one document with imageId is selected', async () => {
    wrapper.vm.selectedItems = [mockParticipantOne.document[0]];

    await nextTick();

    expect(wrapper.findComponent({ name: 'ActionBar' }).exists()).toBe(false);
  });

  it('renders bail document with perfected label date', async () => {
    mockDocumentOne.docmDispositionDsc = 'Perfected';
    mockParticipants[0].keyDocuments.push(mockDocumentOne);
    wrapper = shallowMount(DocumentsView, {
      global: {
        stubs: {
          'v-data-table-virtual': {
            template: `
                        <slot name="item.documentTypeDescription" :item="items && items[0] ? items[0] : { category: 'bail' }"></slot>
                        `,
            props: ['headers', 'items', 'itemValue', 'columns'],
            methods: {
              isGroupOpen: () => true,
              toggleGroup: () => {},
            },
          },
          'v-banner': true,
          'v-card-text': true,
        },
      },
      props: {
        participants: mockParticipants,
      },
    });
    await flushPromises();

    expect(wrapper.text()).toContain('Perfected');
    expect(wrapper.text()).toContain('01-Jan-2023');
  });

  it('does not render bail document with unperfected label date', async () => {
    mockDocumentOne.docmDispositionDsc = 'Unperfected';
    mockParticipants[0].keyDocuments.push(mockDocumentOne);
    wrapper = shallowMount(DocumentsView, {
      global: {
        stubs: {
          'v-data-table-virtual': {
            template: `
                        <slot name="item.documentTypeDescription" :item="items && items[0] ? items[0] : { category: 'bail' }"></slot>
                        `,
            props: ['headers', 'items', 'itemValue', 'columns'],
            methods: {
              isGroupOpen: () => true,
              toggleGroup: () => {},
            },
          },
          'v-banner': true,
          'v-card-text': true,
        },
      },
      props: {
        participants: mockParticipants,
      },
    });
    await flushPromises();

    expect(wrapper.text()).not.toContain('Unperfected');
    expect(wrapper.text()).not.toContain('01-Jan-2023');
  });

  describe('openMergedDocuments', () => {
    beforeEach(() => {
      vi.spyOn(shared, 'openMergedDocuments');
    });

    it('should handle regular documents with File request type', () => {
      const regularDoc = {
        issueDate: '2023-01-01',
        documentTypeDescription: 'Regular Document',
        category: 'other',
        documentPageCount: 5,
        imageId: '123',
        docmDispositionDsc: 'Disposition',
        partId: 'part1',
      };

      mockParticipantOne.document = [regularDoc];
      wrapper = shallowMount(DocumentsView, {
        props: {
          participants: [mockParticipantOne],
        },
      });

      wrapper.vm.selectedItems = [
        { ...regularDoc, fullName: 'John Doe', profSeqNo: 1 },
      ];
      wrapper.vm.openMergedDocuments();

      expect(shared.openMergedDocuments).toHaveBeenCalledWith(
        expect.arrayContaining([
          expect.objectContaining({
            documentType: DocumentRequestType.File,
          }),
        ])
      );
    });

    it('should handle transcript documents with Transcript request type', () => {
      const transcriptDoc = {
        issueDate: '2023-01-01',
        documentTypeDescription: 'Transcript',
        category: 'Transcript',
        documentPageCount: 10,
        imageId: '456',
        docmDispositionDsc: 'Complete',
        partId: 'part2',
      };

      mockParticipantOne.document = [transcriptDoc];
      wrapper = shallowMount(DocumentsView, {
        props: {
          participants: [mockParticipantOne],
        },
      });

      wrapper.vm.selectedItems = [
        { ...transcriptDoc, fullName: 'John Doe', profSeqNo: 1 },
      ];
      wrapper.vm.openMergedDocuments();

      expect(shared.openMergedDocuments).toHaveBeenCalledWith(
        expect.arrayContaining([
          expect.objectContaining({
            documentType: DocumentRequestType.Transcript,
          }),
        ])
      );
    });

    it('should handle ROP documents with ROP request type', () => {
      const ropDoc = {
        issueDate: '2023-01-01',
        documentTypeDescription: 'ROP Document',
        category: 'ROP',
        documentPageCount: 15,
        imageId: '789',
        docmDispositionDsc: 'Disposition',
        partId: 'part3',
      };

      mockParticipantOne.document = [ropDoc];
      wrapper = shallowMount(DocumentsView, {
        props: {
          participants: [mockParticipantOne],
        },
      });

      wrapper.vm.selectedItems = [
        { ...ropDoc, fullName: 'John Doe', profSeqNo: 1 },
      ];
      wrapper.vm.openMergedDocuments();

      expect(shared.openMergedDocuments).toHaveBeenCalledWith(
        expect.arrayContaining([
          expect.objectContaining({
            documentType: DocumentRequestType.ROP,
          }),
        ])
      );
    });

    it('should handle mixed document types correctly', () => {
      const regularDoc = {
        issueDate: '2023-01-01',
        documentTypeDescription: 'Regular Document',
        category: 'other',
        documentPageCount: 5,
        imageId: '123',
        partId: 'part1',
      };

      const transcriptDoc = {
        issueDate: '2023-02-01',
        documentTypeDescription: 'Transcript',
        category: 'Transcript',
        documentPageCount: 10,
        imageId: '456',
        partId: 'part2',
      };

      const ropDoc = {
        issueDate: '2023-03-01',
        documentTypeDescription: 'ROP Document',
        category: 'ROP',
        documentPageCount: 15,
        imageId: '789',
        partId: 'part3',
      };

      mockParticipantOne.document = [regularDoc, transcriptDoc, ropDoc];
      wrapper = shallowMount(DocumentsView, {
        props: {
          participants: [mockParticipantOne],
        },
      });

      wrapper.vm.selectedItems = [
        { ...regularDoc, fullName: 'John Doe', profSeqNo: 1 },
        { ...transcriptDoc, fullName: 'John Doe', profSeqNo: 1 },
        { ...ropDoc, fullName: 'John Doe', profSeqNo: 1 },
      ];
      wrapper.vm.openMergedDocuments();

      expect(shared.openMergedDocuments).toHaveBeenCalledWith(
        expect.arrayContaining([
          expect.objectContaining({
            documentType: DocumentRequestType.File,
          }),
          expect.objectContaining({
            documentType: DocumentRequestType.Transcript,
          }),
          expect.objectContaining({
            documentType: DocumentRequestType.ROP,
          }),
        ])
      );
    });

    it('should filter out documents without imageId', () => {
      const docWithImage = {
        issueDate: '2023-01-01',
        documentTypeDescription: 'Document 1',
        category: 'other',
        documentPageCount: 5,
        imageId: '123',
        partId: 'part1',
      };

      const docWithoutImage = {
        issueDate: '2023-02-01',
        documentTypeDescription: 'Document 2',
        category: 'other',
        documentPageCount: 3,
        imageId: '',
        partId: 'part2',
      };

      mockParticipantOne.document = [docWithImage, docWithoutImage];
      wrapper = shallowMount(DocumentsView, {
        props: {
          participants: [mockParticipantOne],
        },
      });

      wrapper.vm.selectedItems = [
        { ...docWithImage, fullName: 'John Doe', profSeqNo: 1 },
        { ...docWithoutImage, fullName: 'John Doe', profSeqNo: 1 },
      ];
      wrapper.vm.openMergedDocuments();

      expect(shared.openMergedDocuments).toHaveBeenCalledWith(
        expect.arrayContaining([
          expect.objectContaining({
            documentType: DocumentRequestType.File,
          }),
        ])
      );
      expect(shared.openMergedDocuments).toHaveBeenCalledWith(
        expect.not.arrayContaining([
          expect.objectContaining({
            documentData: expect.objectContaining({
              documentDescription: 'Document 2',
            }),
          }),
        ])
      );
    });

    it('should include correct grouping keys and document name', () => {
      const doc = {
        issueDate: '2023-01-01',
        documentTypeDescription: 'Test Document',
        category: 'other',
        documentPageCount: 5,
        imageId: '123',
        partId: 'part1',
      };

      mockParticipantOne.document = [doc];
      wrapper = shallowMount(DocumentsView, {
        props: {
          participants: [mockParticipantOne],
        },
      });

      wrapper.vm.selectedItems = [
        { ...doc, fullName: 'John Doe', profSeqNo: 1 },
      ];
      wrapper.vm.openMergedDocuments();

      expect(shared.openMergedDocuments).toHaveBeenCalledWith(
        expect.arrayContaining([
          expect.objectContaining({
            groupKeyTwo: 'John Doe',
            documentName: 'Test Document',
          }),
        ])
      );
    });
  });
});
