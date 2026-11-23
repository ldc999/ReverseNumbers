import historyApi from '../../api/history'

export default {
  namespaced: true,
  state: {
    historyList: []
  },
  mutations: {
    SET_HISTORY_LIST(state, list) {
      state.historyList = list
    }
  },
  actions: {
    async fetchHistory({ commit }, issueId) {
      const res = await historyApi.getByIssueId(issueId)
      commit('SET_HISTORY_LIST', res.data)
    },
    async saveToHistory({ dispatch }, data) {
      await historyApi.save(data)
      await dispatch('fetchHistory', data.issueId)
    },
    async deleteHistory({ dispatch }, { id, issueId }) {
      await historyApi.delete(id)
      await dispatch('fetchHistory', issueId)
    }
  },
  getters: {
  }
}
