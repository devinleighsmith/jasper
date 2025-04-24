import { describe, it, expect } from 'vitest';
import { shallowMount } from '@vue/test-utils';
import EllipsesMenu from 'CMP/shared/EllipsesMenu.vue';

describe('EllipsesMenu.vue', () => {
    it('renders the correct number of menu items', () => {
        const menuItems = [
            { title: 'Item 1' },
            { title: 'Item 2' },
            { title: 'Item 3' }
        ];

        const wrapper = shallowMount(EllipsesMenu, {
            props: {
                menuItems,
            },
        });

        const listItems = wrapper.findAll('v-list-item');
        expect(listItems.length).toBe(menuItems.length);
    });

    it('displays the correct titles for menu items', () => {
        const menuItems = [
            { title: 'Item 1' },
            { title: 'Item 2' }
        ];

        const wrapper = shallowMount(EllipsesMenu, {
            props: {
                menuItems,
            },
        });

        const listItemTitles = wrapper.findAll('v-list-item-title');
        listItemTitles.forEach((title, index) => {
            expect(title.text()).toBe(menuItems[index].title);
        });
    });
});