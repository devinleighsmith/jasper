import 'core-js/stable'
import 'regenerator-runtime/runtime'
import 'intersection-observer'
import Vue from 'vue'
import VueResource from 'vue-resource'
import VueRouter from 'vue-router'
import { BootstrapVue, BootstrapVueIcons } from 'bootstrap-vue'
import "@styles/index.scss";
import App from './App.vue';
import routes from './router/index'
import store from './store/index'
import "./filters"
import LoadingSpinner from "@components/LoadingSpinner.vue";

Vue.use(VueResource);
Vue.use(VueRouter);
Vue.use(BootstrapVue);
Vue.use(BootstrapVueIcons);
Vue.config.productionTip = true;
Vue.component('loading-spinner', LoadingSpinner);

Vue.http.interceptors.push(function () {
	return function (response) {
		if (response.status == 401) {
			location.replace(`${process.env.BASE_URL}api/auth/login?redirectUri=${window.location}`);
		}
	};
});

Vue.http.options.root = process.env.BASE_URL;

// Redirect from / to /jasper/
if (location.pathname == "/")
	history.pushState({ page: "home" }, "", process.env.BASE_URL);

const router = new VueRouter({
	mode: 'history',
	base: process.env.BASE_URL,
	routes: routes
});

new Vue({
	router,
	store,
	render: h => h(App)
}).$mount('#app');
