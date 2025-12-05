import { shallowMount, flushPromises } from '@vue/test-utils';
import DocumentsView from 'CMP/case-details/criminal/CriminalDocumentsView.vue';
import { createPinia, setActivePinia } from 'pinia';
import { beforeEach, describe, expect, it } from 'vitest';
import { nextTick } from 'vue';

describe('CriminalDocumentsView.vue', () => {
  let wrapper: any;
  let mockParticipantOne: any;
  let mockParticipantTwo: any;
  let mockParticipants: any[];
  let mockDocumentOne: any;

  beforeEach(() => {
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
    setActivePinia(createPinia());
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

  it('does not filter key documents by category', async () => {
    const bailKeyDoc = { ...mockDocumentOne, category: 'bail' };
    const otherKeyDoc = {
      issueDate: '2023-03-01',
      documentTypeDescription: 'Type C',
      category: 'other',
      documentPageCount: 2,
      imageId: '789',
    };
    mockParticipants[0].keyDocuments = [bailKeyDoc, otherKeyDoc];

    const wrapper = shallowMount(DocumentsView, {
      props: {
        participants: mockParticipants,
      },
    });

    (wrapper.vm as any).selectedCategory = 'bail';
    await nextTick();

    const documents = (wrapper.vm as any).documents;
    const keyDocuments = (wrapper.vm as any).keyDocuments;

    expect(documents).toHaveLength(1);
    expect(documents[0].category).toBe('bail');

    expect(keyDocuments).toHaveLength(2);
    expect(keyDocuments.some((doc: any) => doc.category === 'bail')).toBe(true);
    expect(keyDocuments.some((doc: any) => doc.category === 'other')).toBe(
      true
    );
  });

  it('filters both documents and key documents by accused', async () => {
    mockParticipants[0].keyDocuments = [mockDocumentOne];
    mockParticipants[1].keyDocuments = [
      {
        issueDate: '2023-03-01',
        documentTypeDescription: 'Type C',
        category: 'other',
        documentPageCount: 2,
        imageId: '789',
      },
    ];

    const wrapper = shallowMount(DocumentsView, {
      props: {
        participants: mockParticipants,
      },
    });

    (wrapper.vm as any).selectedAccused = 'Doe, John';
    await nextTick();

    const documents = (wrapper.vm as any).documents;
    const keyDocuments = (wrapper.vm as any).keyDocuments;

    expect(documents).toHaveLength(1);
    expect(documents[0].fullName).toBe('John Doe');

    expect(keyDocuments).toHaveLength(1);
    expect(keyDocuments[0].fullName).toBe('John Doe');
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

  describe('Category Display Title', () => {
    it('displays "All Documents" when no category is selected', () => {
      const sections = wrapper.findAll('v-card-text .text-h5');
      expect(sections[1].text()).toBe('All Documents (2)');
    });

    it('displays formatted category name when category is selected', async () => {
      wrapper.vm.selectedCategory = 'PSR';
      await nextTick();

      const sections = wrapper.findAll('v-card-text .text-h5');
      expect(sections[1].text()).toBe('Report (0)');
    });

    it('displays "ROP" when rop category is selected', async () => {
      wrapper.vm.selectedCategory = 'rop';
      await nextTick();

      const sections = wrapper.findAll('v-card-text .text-h5');
      expect(sections[1].text()).toBe('ROP (0)');
    });

    it('displays original category name when no special formatting applies', async () => {
      wrapper.vm.selectedCategory = 'bail';
      await nextTick();

      const sections = wrapper.findAll('v-card-text .text-h5');
      expect(sections[1].text()).toBe('bail (1)');
    });
  });
});
