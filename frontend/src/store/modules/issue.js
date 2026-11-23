import issueApi from '../../api/issue'

export default {
  namespaced: true,
  state: {
    currentIssue: null,
    issueList: []
  },
  mutations: {
    SET_ISSUE_LIST(state, list) {
      state.issueList = list
    },
    SET_CURRENT_ISSUE(state, issue) {
      state.currentIssue = issue
    }
  },
  actions: {
    async fetchIssues({ commit }) {
      const res = await issueApi.getAll()
      commit('SET_ISSUE_LIST', res.data)
    },
    selectIssue({ commit }, issue) {
      commit('SET_CURRENT_ISSUE', issue)
    },
    async createIssue({ dispatch }, issueNumber) {
      await issueApi.create(issueNumber)
      await dispatch('fetchIssues')
    },
    async updateIssue({ dispatch }, { id, issueNumber }) {
      await issueApi.update(id, issueNumber)
      await dispatch('fetchIssues')
    },
    async deleteIssue({ dispatch, state }, id) {
      await issueApi.delete(id)
      if (state.currentIssue && state.currentIssue.id === id) {
        commit('SET_CURRENT_ISSUE', null)
      }
      await dispatch('fetchIssues')
    }
  },
  getters: {
    currentIssueId(state) {
      return state.currentIssue ? state.currentIssue.id : null
    },
    currentIssueNumber(state) {
      return state.currentIssue ? state.currentIssue.issueNumber : ''
    }
  }
}
