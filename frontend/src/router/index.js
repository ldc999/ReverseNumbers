import Vue from 'vue'
import VueRouter from 'vue-router'
import MainView from '../views/MainView.vue'
import SsqView from '../views/SsqView.vue'

Vue.use(VueRouter)

const routes = [
  { path: '/dlt', name: 'Dlt', component: MainView },
  { path: '/ssq', name: 'Ssq', component: SsqView },
  { path: '/', redirect: '/dlt' }
]

const router = new VueRouter({ mode: 'hash', routes })
export default router
