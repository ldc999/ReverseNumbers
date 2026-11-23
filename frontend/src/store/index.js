import Vue from 'vue'
import Vuex from 'vuex'
import issue from './modules/issue'
import playerNumbers from './modules/playerNumbers'
import generator from './modules/generator'
import history from './modules/history'
import retainedNumbers from './modules/retainedNumbers'
import favorites from './modules/favorites'
import tickets from './modules/tickets'
import { ssqIssue, ssqPlayerNumbers, ssqGenerator, ssqHistory, ssqRetained, ssqFavorites, ssqTickets } from './modules/ssq'

Vue.use(Vuex)

export default new Vuex.Store({
  modules: {
    issue, playerNumbers, generator, history, retainedNumbers, favorites, tickets,
    ssq: {
      namespaced: true,
      modules: { ssqIssue, ssqPlayerNumbers, ssqGenerator, ssqHistory, ssqRetained, ssqFavorites, ssqTickets }
    }
  }
})
