import '@mdi/font/css/materialdesignicons.css';
import * as components from 'vuetify/components';
import { VBtn } from 'vuetify/components';
import * as directives from 'vuetify/directives';
import 'vuetify/styles';
import { createVuetify } from 'vuetify';

// https://vuetifyjs.com/en/introduction/why-vuetify/#feature-guides

export default createVuetify({
  components,
  directives,
  aliases: {
    VBtnSecondary: VBtn,
    VBtnTertiary: VBtn,
  },
  defaults: {
    VBtn: {
      rounded: true,
      variant: 'flat',
      class: 'text-none',
    },
    VBtnSecondary: {
      rounded: true,
      variant: 'outlined',
      class: 'text-none',
    },
    VBtnTertiary: {
      rounded: true,
      variant: 'outlined',
      class: 'text-none',
    },
  },
});
