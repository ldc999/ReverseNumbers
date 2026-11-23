import playerNumberApi from '../../api/playerNumber'

export default {
  namespaced: true,
  state: {
    playerSets: []
  },
  mutations: {
    SET_PLAYER_SETS(state, sets) {
      state.playerSets = sets
    }
  },
  actions: {
    async fetchPlayerNumbers({ commit }, issueId) {
      const res = await playerNumberApi.getByIssueId(issueId)
      commit('SET_PLAYER_SETS', res.data)
    },
    async addPlayerSet({ dispatch }, { issueId, frontNumbers, backNumbers }) {
      await playerNumberApi.add(issueId, frontNumbers, backNumbers)
      await dispatch('fetchPlayerNumbers', issueId)
    },
    async batchAddPlayerSets({ dispatch }, { issueId, sets }) {
      await playerNumberApi.batchAdd(issueId, sets)
      await dispatch('fetchPlayerNumbers', issueId)
    },
    async deletePlayerSet({ dispatch }, { issueId, id }) {
      await playerNumberApi.delete(issueId, id)
      await dispatch('fetchPlayerNumbers', issueId)
    },
    async clearAll({ dispatch }, issueId) {
      await playerNumberApi.clearAll(issueId)
      await dispatch('fetchPlayerNumbers', issueId)
    }
  },
  getters: {
    playerSetCount(state) {
      return state.playerSets.length
    }
  }
}
