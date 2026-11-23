import ticketApi from '../../api/ticket'

export default {
  namespaced: true,
  state: { ticketList: [] },
  mutations: { SET_TICKET_LIST(s, v) { s.ticketList = v } },
  actions: {
    async fetchTickets({ commit }, issueId) {
      const r = await ticketApi.getByIssueId(issueId)
      commit('SET_TICKET_LIST', r.data)
    },
    async saveTicket({ dispatch }, { issueId, frontPool, backPool, constraints, bets, ticketType, addToPlayer }) {
      await ticketApi.save(issueId, { frontPool, backPool, constraints, bets: bets || [], ticketType: ticketType || 'fushi', addToPlayer: addToPlayer !== false })
      await dispatch('fetchTickets', issueId)
    },
    async updateConstraints({ dispatch }, { id, constraints, issueId }) {
      await ticketApi.updateConstraints(id, constraints)
      await dispatch('fetchTickets', issueId)
    },
    async deleteTicket({ dispatch }, { id, issueId }) {
      await ticketApi.delete(id)
      await dispatch('fetchTickets', issueId)
    }
  }
}
