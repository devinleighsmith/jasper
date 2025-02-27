import { describe, it, expect } from 'vitest';
import { mount } from '@vue/test-utils';
import CourtListCard from 'CMP/courtlist/CourtListCard.vue';
import { CourtListCardInfo } from '@/types/courtlist';

const createWrapper = () => {
    const card: CourtListCardInfo = {
        courtListLocation: 'Court A',
        courtListLocationID: 1,
        courtListRoom: 'Room 101',
        activity: 'Hearing',
        amPM: 'AM',
        fileCount: 5,
        presider: 'Judge Smith',
        courtClerk: 'John Doe',
        email: 'court@example.com'
    };
    return mount(CourtListCard, {
        props: {
            cardInfo: card
        },
    });
};

describe('CourtListCard.vue', () => {
    it('renders all court list details correctly', () => {
        const wrapper = createWrapper();
        expect(wrapper.text()).toContain('Court A');
        expect(wrapper.text()).toContain('Rooms: Room 101 (AM)');
        expect(wrapper.text()).toContain('Presider: Judge Smith');
        expect(wrapper.text()).toContain('Court clerk: John Doe');
        expect(wrapper.text()).toContain('Activity: Hearing');
        expect(wrapper.text()).toContain('Scheduled: 5 files');
        expect(wrapper.text()).toContain('court@example.com');
    });

    it('renders default values when props are nullish', () => {
        const wrapper = mount(CourtListCard, {
          props: {
            cardInfo: {
              courtListLocation: 'Location B',
              courtListRoom: 'Room 2',
              amPM: null,
              presider: null,
              courtClerk: null,
              activity: 'Activity B',
              fileCount: 10,
              email: 'email2@example.com',
            },
          },
        });
    
        expect(wrapper.text()).toContain('Location B');
        expect(wrapper.text()).toContain('Rooms: Room 2');
        expect(wrapper.text()).toContain('Presider: Unassigned');
        expect(wrapper.text()).toContain('Court clerk: Not scheduled');
        expect(wrapper.text()).toContain('Activity: Activity B');
        expect(wrapper.text()).toContain('Scheduled: 10 files');
        expect(wrapper.text()).toContain('email2@example.com');
      });
});
