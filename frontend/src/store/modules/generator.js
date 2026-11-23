import generateApi from '../../api/generate'
import configApi from '../../api/config'

export default {
  namespaced: true,
  state: {
    highlightResult: null,
    maxFrontHighlight: 3,
    maxBackHighlight: 1,
    highlightConstraints: [{ f: 3, b: 1 },{ f: 1, b: 2 }],
    blindMode: true,
    histEnable: false, histStart: '', histEnd: '',
    histConstraints: [{ f: 3, b: 1 }],
    // 走势图选号（跨弹窗开关保留）
    trendSelFront: [], trendSelBack: [],
    // shared 5+2 input squares
    currentFrontNumbers: ['', '', '', '', ''],
    currentBackNumbers: ['', '']
  },
  mutations: {
    SET_HIGHLIGHT_RESULT(state, result) {
      state.highlightResult = result
    },
    CLEAR_HIGHLIGHT_RESULT(state) {
      state.highlightResult = null
    },
    SET_MAX_FRONT(state, value) {
      state.maxFrontHighlight = value
    },
    SET_MAX_BACK(state, value) {
      state.maxBackHighlight = value
    },
    SET_HIGHLIGHT_CONSTRAINTS(state, value) {
      state.highlightConstraints = value
    },
    SET_BLIND_MODE(state, value) {
      state.blindMode = value
    },
    SET_TREND_SEL_FRONT(state, v) { state.trendSelFront = [...v] },
    SET_TREND_SEL_BACK(state, v) { state.trendSelBack = [...v] },
    SET_HIST_ENABLE(state, v) { state.histEnable = v },
    SET_HIST_START(state, v) { state.histStart = v },
    SET_HIST_END(state, v) { state.histEnd = v },
    SET_HIST_CONSTRAINTS(state, v) { state.histConstraints = v },
    SET_CURRENT_FRONT(state, numbers) {
      state.currentFrontNumbers = [...numbers]
    },
    SET_CURRENT_BACK(state, numbers) {
      state.currentBackNumbers = [...numbers]
    }
  },
  actions: {
    async generateRandom({ commit, state }, { issueId, excludedFront, excludedBack, requiredFront, requiredBack, ticketConstraints }) {
      const res = await generateApi.generateRandom({
        issueId,
        maxFrontHighlight: state.maxFrontHighlight,
        maxBackHighlight: state.maxBackHighlight,
        excludedFrontNumbers: excludedFront || [],
        excludedBackNumbers: excludedBack || [],
        requiredFrontNumbers: requiredFront || [],
        requiredBackNumbers: requiredBack || [],
        ticketConstraints: ticketConstraints || [],
        highlightConstraints: state.highlightConstraints || [],
        enableHistoryConstraint: state.histEnable || false,
        historyStart: state.histStart || null,
        historyEnd: state.histEnd || null,
        historyConstraints: state.histConstraints || []
      })
      return res.data
    },
    async highlightManual({ commit }, { issueId, frontNumbers, backNumbers }) {
      const res = await generateApi.highlight({
        issueId,
        frontNumbers,
        backNumbers
      })
      commit('SET_HIGHLIGHT_RESULT', res.data)
    },
    async loadConfig({ commit }) {
      const res = await configApi.getAll()
      const data = res.data
      if (data.max_front_highlight) {
        commit('SET_MAX_FRONT', parseInt(data.max_front_highlight) || 3)
      }
      if (data.max_back_highlight) {
        commit('SET_MAX_BACK', parseInt(data.max_back_highlight) || 1)
      }
    },
    async updateConfig({ commit }, { key, value }) {
      await configApi.update(key, value)
    }
  },
  getters: {
  }
}
