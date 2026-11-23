import {
  ssqIssueApi, ssqPlayerNumberApi, ssqGenerateApi,
  ssqHistoryApi, ssqConfigApi, ssqRetainedNumberApi, ssqFavoriteNumberApi, ssqTicketApi
} from '../../api/ssq'

// SSQ: red 6/33, blue 1/16
const RED_COUNT = 6, BLUE_COUNT = 1

function emptyRed() { return Array(RED_COUNT).fill('') }
function emptyBlue() { return Array(BLUE_COUNT).fill('') }

export const ssqIssue = {
  namespaced: true,
  state: { currentIssue: null, issueList: [] },
  mutations: {
    SET_ISSUE_LIST(s, v) { s.issueList = v },
    SET_CURRENT_ISSUE(s, v) { s.currentIssue = v }
  },
  actions: {
    async fetchIssues({ commit }) { const r = await ssqIssueApi.getAll(); commit('SET_ISSUE_LIST', r.data) },
    selectIssue({ commit }, issue) { commit('SET_CURRENT_ISSUE', issue) },
    async createIssue({ dispatch }, n) { await ssqIssueApi.create(n); dispatch('fetchIssues') },
    async updateIssue({ dispatch }, { id, issueNumber }) { await ssqIssueApi.update(id, issueNumber); dispatch('fetchIssues') },
    async deleteIssue({ commit, dispatch, state }, id) { await ssqIssueApi.delete(id); if (state.currentIssue?.id === id) commit('SET_CURRENT_ISSUE', null); dispatch('fetchIssues') }
  },
  getters: {
    currentIssueId: s => s.currentIssue?.id || null,
    currentIssueNumber: s => s.currentIssue?.issueNumber || ''
  }
}

export const ssqPlayerNumbers = {
  namespaced: true,
  state: { playerSets: [] },
  mutations: { SET_PLAYER_SETS(s, v) { s.playerSets = v } },
  actions: {
    async fetchPlayerNumbers({ commit }, issueId) { const r = await ssqPlayerNumberApi.getByIssueId(issueId); commit('SET_PLAYER_SETS', r.data) },
    async addPlayerSet({ dispatch }, { issueId, frontNumbers, backNumbers }) { await ssqPlayerNumberApi.add(issueId, frontNumbers, backNumbers); dispatch('fetchPlayerNumbers', issueId) },
    async batchAddPlayerSets({ dispatch }, { issueId, sets }) { await ssqPlayerNumberApi.batchAdd(issueId, sets); dispatch('fetchPlayerNumbers', issueId) },
    async deletePlayerSet({ dispatch }, { issueId, id }) { await ssqPlayerNumberApi.delete(issueId, id); dispatch('fetchPlayerNumbers', issueId) },
    async clearAll({ dispatch }, issueId) { await ssqPlayerNumberApi.clearAll(issueId); dispatch('fetchPlayerNumbers', issueId) }
  },
  getters: { playerSetCount: s => s.playerSets.length }
}

export const ssqGenerator = {
  namespaced: true,
  state: {
    highlightResult: null, maxFrontHighlight: 4, maxBackHighlight: 1,
    highlightConstraints: [{ f: 4, b: 0 },{ f: 3, b: 1 } ],
    blindMode: true,
    histEnable: false, histStart: '', histEnd: '',
    histConstraints: [{ f: 3, b: 1 }],
    trendSelFront: [], trendSelBack: [],
    currentFrontNumbers: emptyRed(), currentBackNumbers: emptyBlue()
  },
  mutations: {
    SET_HIGHLIGHT_RESULT(s, v) { s.highlightResult = v },
    SET_MAX_FRONT(s, v) { s.maxFrontHighlight = v },
    SET_MAX_BACK(s, v) { s.maxBackHighlight = v },
    SET_HIGHLIGHT_CONSTRAINTS(s, v) { s.highlightConstraints = v },
    SET_BLIND_MODE(s, v) { s.blindMode = v },
    SET_TREND_SEL_FRONT(s, v) { s.trendSelFront = [...v] },
    SET_TREND_SEL_BACK(s, v) { s.trendSelBack = [...v] },
    SET_HIST_ENABLE(s, v) { s.histEnable = v },
    SET_HIST_START(s, v) { s.histStart = v },
    SET_HIST_END(s, v) { s.histEnd = v },
    SET_HIST_CONSTRAINTS(s, v) { s.histConstraints = v },
    SET_CURRENT_FRONT(s, v) { s.currentFrontNumbers = [...v] },
    SET_CURRENT_BACK(s, v) { s.currentBackNumbers = [...v] }
  },
  actions: {
    async generateRandom({ commit, state }, { issueId, excludedFront, excludedBack, requiredFront, requiredBack, ticketConstraints }) {
      const r = await ssqGenerateApi.generateRandom({
        issueId, maxFrontHighlight: state.maxFrontHighlight, maxBackHighlight: state.maxBackHighlight,
        excludedFrontNumbers: excludedFront || [], excludedBackNumbers: excludedBack || [],
        requiredFrontNumbers: requiredFront || [], requiredBackNumbers: requiredBack || [],
        ticketConstraints: ticketConstraints || [],
        highlightConstraints: state.highlightConstraints || [],
        enableHistoryConstraint: state.histEnable || false,
        historyStart: state.histStart || null,
        historyEnd: state.histEnd || null,
        historyConstraints: state.histConstraints || []
      })
      return r.data
    },
    async highlightManual({ commit }, { issueId, frontNumbers, backNumbers }) {
      const r = await ssqGenerateApi.highlight({ issueId, frontNumbers, backNumbers })
      commit('SET_HIGHLIGHT_RESULT', r.data)
    },
    async loadConfig({ commit }) {
      const r = await ssqConfigApi.getAll(); const d = r.data
      if (d.max_front_highlight) commit('SET_MAX_FRONT', parseInt(d.max_front_highlight) || 3)
      if (d.max_back_highlight) commit('SET_MAX_BACK', parseInt(d.max_back_highlight) || 1)
    },
    async updateConfig(_, { key, value }) { await ssqConfigApi.update(key, value) }
  }
}

export const ssqHistory = {
  namespaced: true,
  state: { historyList: [] },
  mutations: { SET_HISTORY_LIST(s, v) { s.historyList = v } },
  actions: {
    async fetchHistory({ commit }, issueId) { const r = await ssqHistoryApi.getByIssueId(issueId); commit('SET_HISTORY_LIST', r.data) },
    async saveToHistory({ dispatch }, data) { await ssqHistoryApi.save(data); dispatch('fetchHistory', data.issueId) },
    async deleteHistory({ dispatch }, { id, issueId }) { await ssqHistoryApi.delete(id); dispatch('fetchHistory', issueId) }
  }
}

export const ssqRetained = {
  namespaced: true,
  state: { retainedList: [] },
  mutations: { SET_RETAINED_LIST(s, v) { s.retainedList = v } },
  actions: {
    async fetchRetained({ commit }, issueId) { const r = await ssqRetainedNumberApi.getByIssueId(issueId); commit('SET_RETAINED_LIST', r.data) },
    async saveRetained({ dispatch }, { issueId, frontNumbers, backNumbers }) { await ssqRetainedNumberApi.save(issueId, { frontNumbers, backNumbers }); dispatch('fetchRetained', issueId) },
    async deleteRetained({ dispatch }, { id, issueId }) { await ssqRetainedNumberApi.delete(id); dispatch('fetchRetained', issueId) }
  }
}

export const ssqFavorites = {
  namespaced: true,
  state: { favoriteList: [] },
  mutations: { SET_FAVORITE_LIST(s, v) { s.favoriteList = v } },
  actions: {
    async fetchFavorites({ commit }) { const r = await ssqFavoriteNumberApi.getAll(); commit('SET_FAVORITE_LIST', r.data) },
    async saveFavorite({ dispatch }, { frontNumbers, backNumbers, label }) { await ssqFavoriteNumberApi.save({ frontNumbers, backNumbers }, label); dispatch('fetchFavorites') },
    async deleteFavorite({ dispatch }, id) { await ssqFavoriteNumberApi.delete(id); dispatch('fetchFavorites') }
  }
}

export const ssqTickets = {
  namespaced: true,
  state: { ticketList: [] },
  mutations: { SET_TICKET_LIST(s, v) { s.ticketList = v } },
  actions: {
    async fetchTickets({ commit }, issueId) { const r = await ssqTicketApi.getByIssueId(issueId); commit('SET_TICKET_LIST', r.data) },
    async saveTicket({ dispatch }, { issueId, frontPool, backPool, constraints, bets, ticketType, addToPlayer }) { await ssqTicketApi.save(issueId, { frontPool, backPool, constraints, bets: bets || [], ticketType: ticketType || 'fushi', addToPlayer: addToPlayer !== false }); dispatch('fetchTickets', issueId) },
    async updateConstraints({ dispatch }, { id, constraints, issueId }) { await ssqTicketApi.updateConstraints(id, constraints); dispatch('fetchTickets', issueId) },
    async deleteTicket({ dispatch }, { id, issueId }) { await ssqTicketApi.delete(id); dispatch('fetchTickets', issueId) }
  }
}
