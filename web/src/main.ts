import LoadingSpinner from '@components/LoadingSpinner.vue';
import '@styles/index.scss';
import { createBootstrap } from 'bootstrap-vue-next';
import 'bootstrap-vue-next/dist/bootstrap-vue-next.css';
import 'bootstrap/dist/css/bootstrap.css';
import 'intersection-observer';
import { createApp } from 'vue';
import App from './App.vue';
import './filters';
import router from './router/index';
import { registerRouter } from './services';
import { registerPinia } from './stores';

const app = createApp(App);

registerPinia(app);
app.use(router);
app.use(createBootstrap());
//Vue.config.productionTip = true
app.component('loading-spinner', LoadingSpinner);
app.component(
  // the registered name
  'MyComponent',
  // the implementation
  {
    LoadingSpinner,
  }
);

registerRouter(app);

app.mount('#app');

// Redirect from / to /jasper/
if (location.pathname == '/') {
  history.pushState({ page: 'home' }, '', import.meta.env.BASE_URL);
}
