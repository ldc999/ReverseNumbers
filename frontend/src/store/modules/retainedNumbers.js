import retainedNumberApi from '../../api/retainedNumber'

export default {
  namespaced: true,
  state: {
    retainedList: []
  },
  mutations: {
    SET_RETAINED_LIST(state, list) {
      state.retainedList = list
    }
  },
  actions: {
    async fetchRetained({ commit }, issueId) {
      const res = await retainedNumberApi.getByIssueId(issueId)
      commit('SET_RETAINED_LIST', res.data)
    },
    async saveRetained({ dispatch }, { issueId, frontNumbers, backNumbers }) {
      await retainedNumberApi.save(issueId, { frontNumbers, backNumbers })
      await dispatch('fetchRetained', issueId)
    },
    async deleteRetained({ dispatch }, { id, issueId }) {
      await retainedNumberApi.delete(id)
      await dispatch('fetchRetained', issueId)
    }
  }
}
