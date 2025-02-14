import { describe, it, expect } from 'vitest';
import { mount } from '@vue/test-utils';
import CourtListCard from 'CMP/courtList/CourtListCard.vue';

describe('CourtListCard.vue', () => {
  it('renders court list location', () => {
    const wrapper = mount(CourtListCard, {
      props: {
        courtListLocation: 'Court A',
        courtListLocationID: '1',
        courtListRoom: 'Room 101',
        activity: 'Hearing',
        amPM: 'AM',
        fileCount: 5,
        presider: 'Judge Smith',
        courtClerk: 'John Doe',
        email: 'court@example.com',
      },
    });
    expect(wrapper.text()).toContain('Court A');
  });

  it('renders court list room and AM/PM', () => {
    const wrapper = mount(CourtListCard, {
      props: {
        courtListLocation: 'Court A',
        courtListLocationID: '1',
        courtListRoom: 'Room 101',
        activity: 'Hearing',
        amPM: 'AM',
        fileCount: 5,
        presider: 'Judge Smith',
        courtClerk: 'John Doe',
        email: 'court@example.com',
      },
    });
    expect(wrapper.text()).toContain('Rooms: Room 101 (AM)');
  });

  it('renders presider', () => {
    const wrapper = mount(CourtListCard, {
      props: {
        courtListLocation: 'Court A',
        courtListLocationID: '1',
        courtListRoom: 'Room 101',
        activity: 'Hearing',
        amPM: 'AM',
        fileCount: 5,
        presider: 'Judge Smith',
        courtClerk: 'John Doe',
        email: 'court@example.com',
      },
    });
    expect(wrapper.text()).toContain('Presider: Judge Smith');
  });

  it('renders court clerk', () => {
    const wrapper = mount(CourtListCard, {
      props: {
        courtListLocation: 'Court A',
        courtListLocationID: '1',
        courtListRoom: 'Room 101',
        activity: 'Hearing',
        amPM: 'AM',
        fileCount: 5,
        presider: 'Judge Smith',
        courtClerk: 'John Doe',
        email: 'court@example.com',
      },
    });
    expect(wrapper.text()).toContain('Court clerk: John Doe');
  });

  it('renders activity', () => {
    const wrapper = mount(CourtListCard, {
      props: {
        courtListLocation: 'Court A',
        courtListLocationID: '1',
        courtListRoom: 'Room 101',
        activity: 'Hearing',
        amPM: 'AM',
        fileCount: 5,
        presider: 'Judge Smith',
        courtClerk: 'John Doe',
        email: 'court@example.com',
      },
    });
    expect(wrapper.text()).toContain('Activity: Hearing');
  });

  it('renders file count', () => {
    const wrapper = mount(CourtListCard, {
      props: {
        courtListLocation: 'Court A',
        courtListLocationID: '1',
        courtListRoom: 'Room 101',
        activity: 'Hearing',
        amPM: 'AM',
        fileCount: 5,
        presider: 'Judge Smith',
        courtClerk: 'John Doe',
        email: 'court@example.com',
      },
    });
    expect(wrapper.text()).toContain('Scheduled: 5 files');
  });

  it('renders email', () => {
    const wrapper = mount(CourtListCard, {
      props: {
        courtListLocation: 'Court A',
        courtListLocationID: '1',
        courtListRoom: 'Room 101',
        activity: 'Hearing',
        amPM: 'AM',
        fileCount: 5,
        presider: 'Judge Smith',
        courtClerk: 'John Doe',
        email: 'court@example.com',
      },
    });
    expect(wrapper.text()).toContain('court@example.com');
  });
});